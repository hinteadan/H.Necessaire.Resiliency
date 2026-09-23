using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H.Necessaire.Resiliency.Abstractions
{
    public interface ImAnHServiceResiliencyRunner<TService>
    {
        Task<OperationResult<TResult>> ResilientlyRun<TResult>(Func<TService, Task<TResult>> action);
        Task<OperationResult<TResult>> ResilientlyRun<TResult>(Func<TService, Task<OperationResult<TResult>>> action);
        OperationResult<TResult> ResilientlyRun<TResult>(Func<TService, TResult> action);
        OperationResult<TResult> ResilientlyRun<TResult>(Func<TService, OperationResult<TResult>> action);

        Task<OperationResult> ResilientlyRun(Func<TService, Task> action);
        Task<OperationResult> ResilientlyRun(Func<TService, Task<OperationResult>> action);
        OperationResult ResilientlyRun(Action<TService> action);
        OperationResult ResilientlyRun(Func<TService, OperationResult> action);
    }

    internal class HServiceResiliencyRunner<TService> : ImAnHServiceResiliencyRunner<TService>
    {
        const string noteIdForServiceRef = "H.Necessaire.Resiliency::ServiceID";
        readonly Func<KeyValuePair<string, TService>[]> servicesOrderedByResiliencyGrabber;
        public HServiceResiliencyRunner(Func<KeyValuePair<string, TService>[]> servicesOrderedByResiliencyGrabber)
        {
            this.servicesOrderedByResiliencyGrabber = servicesOrderedByResiliencyGrabber;
        }

        public async Task<OperationResult<TResult>> ResilientlyRun<TResult>(Func<TService, Task<TResult>> action)
        {
            TResult result = default;
            OperationResult res = await ResilientlyRunTask(async service => (await action(service)).RefTo(out result));
            return res.WithPayload(result);
        }

        public async Task<OperationResult<TResult>> ResilientlyRun<TResult>(Func<TService, Task<OperationResult<TResult>>> action)
        {
            OperationResult<TResult> result = "Not yet started";
            var res = await ResilientlyRunTask(async service => (await action(service)).RefTo(out result));
            return !res ? res.WithoutPayload<TResult>() : result;
        }

        public OperationResult<TResult> ResilientlyRun<TResult>(Func<TService, TResult> action)
        {
            TResult result = default;
            OperationResult res = ResilientlyRunAction(service => action(service).RefTo(out result));
            return res.WithPayload(result);
        }

        public OperationResult<TResult> ResilientlyRun<TResult>(Func<TService, OperationResult<TResult>> action)
        {
            OperationResult<TResult> result = "Not yet started";
            var res = ResilientlyRunAction(service => action(service).RefTo(out result));
            return !res ? res.WithoutPayload<TResult>() : result;
        }

        public Task<OperationResult> ResilientlyRun(Func<TService, Task> action)
            => ResilientlyRunTask(action);

        public async Task<OperationResult> ResilientlyRun(Func<TService, Task<OperationResult>> action)
        {
            OperationResult result = "Not yet started";
            var res = await ResilientlyRunTask(async service => (await action(service)).RefTo(out result));
            return !res ? res : result;
        }

        public OperationResult ResilientlyRun(Action<TService> action)
            => ResilientlyRunAction(action);

        public OperationResult ResilientlyRun(Func<TService, OperationResult> action)
        {
            OperationResult result = "Not yet started";
            var res = ResilientlyRunAction(service => action(service).RefTo(out result));
            return !res ? res : result;
        }

        OperationResult ResilientlyRunAction(Action<TService> action)
        {
            if (action is null)
                return "Action to run is unspecified";

            return (HSafe.Run<OperationResult>(() => {

                KeyValuePair<string, TService>[] services = servicesOrderedByResiliencyGrabber?.Invoke();
                if (services.IsEmpty())
                    return "servicesOrderedByResiliencyGrabber returned no services";

                List<OperationResult> opResults = new List<OperationResult>(services.Length);
                foreach (KeyValuePair<string, TService> service in services)
                {
                    if (!HSafe.Run(() => action(service.Value)).WithComment(string.Join(Note.IDSeparator, noteIdForServiceRef, service.Key)).Ref(out var runRes))
                    {
                        opResults.Add(runRes);
                        continue;
                    }

                    return runRes;
                }

                return opResults.Merge();

            })).UnwrapToFirstFailOrLastWin();
        }

        async Task<OperationResult> ResilientlyRunTask(Func<TService, Task> action)
        {
            if (action is null)
                return "Action to run is unspecified";

            return (await HSafe.Run<OperationResult>(async () => {

                KeyValuePair<string, TService>[] services = servicesOrderedByResiliencyGrabber?.Invoke();
                if (services.IsEmpty())
                    return "servicesOrderedByResiliencyGrabber returned no services";

                List<OperationResult> opResults = new List<OperationResult>(services.Length);
                foreach (KeyValuePair<string, TService> service in services)
                {
                    if (!(await HSafe.Run(async () => await action(service.Value))).WithComment(string.Join(Note.IDSeparator, noteIdForServiceRef, service.Key)).Ref(out var runRes))
                    {
                        opResults.Add(runRes);
                        continue;
                    }

                    return runRes;
                }

                return opResults.Merge();

            })).UnwrapToFirstFailOrLastWin();
        }
    }
}
