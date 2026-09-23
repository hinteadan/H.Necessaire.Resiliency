using H.Necessaire.Resiliency.Abstractions.DataModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace H.Necessaire.Resiliency.Abstractions.Bases
{
    public abstract class HResilientServiceProviderBase<TService> : ImAnHResilientServiceProvider<TService>, ImADependency
    {
#if DEBUG
        const bool isDebug = true;
#else
        const bool isDebug = false;
#endif
        protected static readonly TimeSpan defaultMeasurementStartDelay = isDebug ? TimeSpan.FromSeconds(2) : TimeSpan.FromSeconds(17);
        protected static readonly TimeSpan defaultMeasurementInterval = isDebug ? TimeSpan.FromSeconds(17) : TimeSpan.FromMinutes(17);
        protected abstract ImAnHServiceResiliencyProvider<TService>[] ResilientServices { get; }
        protected abstract ImAnHResiliencyMeasurer ResiliencyMeasurer { get; }

        readonly HServiceResiliencyRunner<TService> serviceResiliencyRunner;
        readonly ConcurrentDictionary<string, ServiceWithMeasurement> latestMeasurements
            = new ConcurrentDictionary<string, ServiceWithMeasurement>();
        readonly SemaphoreSlim measurementsSemaphore = new SemaphoreSlim(1, 1);
        static readonly TimeSpan measurementsSemaphoreMaxWaitTime = TimeSpan.FromSeconds(60);
        readonly bool isMeasurementDaemonDisabled = false;
        readonly TimeSpan measurementStartDelay = defaultMeasurementStartDelay;
        readonly TimeSpan measurementInterval = defaultMeasurementInterval;
        protected HResilientServiceProviderBase(bool isMeasurementDaemonDisabled, TimeSpan? measurementStartDelay = null, TimeSpan? measurementInterval = null)
        {
            this.isMeasurementDaemonDisabled = isMeasurementDaemonDisabled;
            this.measurementStartDelay = measurementStartDelay ?? defaultMeasurementStartDelay;
            this.measurementInterval = measurementInterval ?? defaultMeasurementInterval;
            serviceResiliencyRunner = new HServiceResiliencyRunner<TService>(() => currentServiceInstancesOrderedByResiliency);
        }
        protected HResilientServiceProviderBase() : this(isMeasurementDaemonDisabled: false, measurementStartDelay: null, measurementInterval: null) { }
        ImALogger log;
        ImACancellationManager cancelMgr;
        ImAPeriodicAction measurementDaemon;
        public virtual void ReferDependencies(ImADependencyProvider dependencyProvider)
        {
            log = dependencyProvider.GetLogger<HResilientServiceProviderBase<TService>>();
            cancelMgr = dependencyProvider.Get<ImACancellationManager>();
            if (!isMeasurementDaemonDisabled)
            {
                measurementDaemon = dependencyProvider.Get<ImAPeriodicAction>();
                measurementDaemon.StartDelayed(measurementStartDelay, measurementInterval, async () => await SafelyMeasureAndUpdateServicesResiliencyIfNecessary());
                cancelMgr.Token.Register(() => { measurementDaemon.Stop(); });
            }

        }

        KeyValuePair<string, TService>[] currentServiceInstancesOrderedByResiliency;
        TService[] CurrentServiceInstancesOrderedByResiliency { get; set; }

        public async Task<OperationResult<TService[]>> GetAllServiceInstancesOrderedByResiliency()
        {
            if (!(await SafelyMeasureAndUpdateServicesResiliencyIfNecessary()).Ref(out var upRes))
                return upRes.WithoutPayload<TService[]>();

            return CurrentServiceInstancesOrderedByResiliency;
        }

        public async Task<OperationResult<KeyValuePair<string, TService>[]>> GetAllServiceInstancesOrderedByResiliencyWithIDs()
        {
            if (!(await SafelyMeasureAndUpdateServicesResiliencyIfNecessary()).Ref(out var upRes))
                return upRes.WithoutPayload<KeyValuePair<string, TService>[]>();

            return currentServiceInstancesOrderedByResiliency;
        }

        public async Task<OperationResult<TService>> GetBestKnownServiceInstance()
        {
            if (!(await SafelyMeasureAndUpdateServicesResiliencyIfNecessary()).Ref(out var upRes))
                return upRes.WithoutPayload<TService>();

            return
                CurrentServiceInstancesOrderedByResiliency.IsEmpty()
                ? "No available services"
                : CurrentServiceInstancesOrderedByResiliency[0].ToWinResult()
                ;
        }

        public async Task<OperationResult<KeyValuePair<string, TService>>> GetBestKnownServiceInstanceWithID()
        {
            if (!(await SafelyMeasureAndUpdateServicesResiliencyIfNecessary()).Ref(out var upRes))
                return upRes.WithoutPayload<KeyValuePair<string, TService>>();

            return
                currentServiceInstancesOrderedByResiliency.IsEmpty()
                ? "No available services"
                : currentServiceInstancesOrderedByResiliency[0].ToWinResult()
                ;
        }

        public async Task<OperationResult<ImAnHServiceResiliencyRunner<TService>>> GetResiliencyRunner()
        {
            if (!(await SafelyMeasureAndUpdateServicesResiliencyIfNecessary()).Ref(out var upRes))
                return upRes.WithoutPayload<ImAnHServiceResiliencyRunner<TService>>();

            return serviceResiliencyRunner;
        }

        async Task<OperationResult> MeasureAndUpdateServicesResiliencyIfNecessary()
        {
            if (!(await HSafe.Run(async () => await measurementsSemaphore.WaitAsync(measurementsSemaphoreMaxWaitTime, cancelMgr.Token))).Ref(out var waitRes, out bool waitIsOK) || !waitIsOK)
                return !waitRes ? waitRes : string.Join("", "Timed out while waiting for current running measurments to finish after a waiting time of ", measurementsSemaphoreMaxWaitTime);

            using (new ScopedRunner(null, _ => measurementsSemaphore.Release()))
            {
                if (AreLatestMeasurementsStillValid())
                    return true;

                ImAnHServiceResiliencyProvider<TService>[] services = ResilientServices;
                if (services.IsEmpty())
                    return "ResilientServices is empty";

                ImAnHResiliencyMeasurer measurer = ResiliencyMeasurer;
                if (measurer is null)
                    return "ResiliencyMeasurer is undefined";

                await Task.WhenAll(services.Select(async service => await MeasureAndUpdateServiceResiliencyMeasurement(service, measurer)));

                currentServiceInstancesOrderedByResiliency
                    = services
                    .Where(svc => svc != null)
                    .OrderByDescending(svc => !latestMeasurements.TryGetValue(svc.ID, out var svcmsr) ? -1 : (svcmsr?.Measurement?.ResiliencyScoreInPerMyriad ?? -1))
                    .Select(svc => new KeyValuePair<string, TService>(svc.ID, svc.GetServiceInstance()))
                    .ToNoNullsArray()
                    ;

                CurrentServiceInstancesOrderedByResiliency
                    = currentServiceInstancesOrderedByResiliency
                    ?.Select(x => x.Value)
                    ?.ToArray()
                    ;

                return true;
            }
        }

        async Task MeasureAndUpdateServiceResiliencyMeasurement(ImAnHServiceResiliencyProvider<TService> service, ImAnHResiliencyMeasurer measurer)
        {
            await HSafe.Run<OperationResult>(async () =>
            {

                if (!(await service.GetLatestResiliencyMeasurementContext()).Ref(out var ctxRes, out var ctx))
                    return ctxRes;

                if (!(await measurer.MeasureResiliency(ctx)).Ref(out var msrRes, out var msr))
                    return msrRes;

                ServiceWithMeasurement entry = (service, msr);

                latestMeasurements.AddOrUpdate(entry.ID, entry, (id, ex) => entry);

                return true;

            })
            .UnwrapToFirstFailOrLastWin()
            .LogError(log, string.Join("", nameof(MeasureAndUpdateServiceResiliencyMeasurement), " for ", service.ID))
            ;
        }

        OperationResult AreLatestMeasurementsStillValid()
        {
            if (latestMeasurements.IsEmpty())
                return "Nothing measured yet";

            ImAnHServiceResiliencyProvider<TService>[] services = ResilientServices;
            if (services.IsEmpty())
                return "ResilientServices is empty";

            return
                services.All(s =>
                    s != null
                    && latestMeasurements.TryGetValue(s.ID, out var svcmsr)
                    && svcmsr?.Measurement != null
                    && svcmsr.Measurement.IsActive()
                );
        }

        async Task<OperationResult> SafelyMeasureAndUpdateServicesResiliencyIfNecessary()
            => await HSafe.Run(MeasureAndUpdateServicesResiliencyIfNecessary)
            .UnwrapToFirstFailOrLastWin()
            .LogError(log, nameof(MeasureAndUpdateServicesResiliencyIfNecessary))
            ;

        class ServiceWithMeasurement : IStringIdentity
        {
            public ServiceWithMeasurement(ImAnHServiceResiliencyProvider<TService> service, ImAnHResiliencyMeasurement measurement)
            {
                this.Service = service;
                this.Measurement = measurement;
            }

            public string ID => Service?.ID ?? Measurement?.ID;
            public ImAnHServiceResiliencyProvider<TService> Service { get; }
            public ImAnHResiliencyMeasurement Measurement { get; }

            public static implicit operator ServiceWithMeasurement((ImAnHServiceResiliencyProvider<TService> service, ImAnHResiliencyMeasurement measurement) parts)
                => new ServiceWithMeasurement(parts.service, parts.measurement);
        }
    }
}
