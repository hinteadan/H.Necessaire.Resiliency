using System;

namespace H.Necessaire.Resiliency.Abstractions.DataModels
{
    public interface ImAnHResiliencyMeasurement : IEphemeralType, IStringIdentity
    {
        /// <summary>
        /// For operational service value is [0 - 10000]; 0 - Worst, 10000 - Best
        /// For faulted services (down, unavailable, errored, etc.) the value is negative (< 0), usually -1
        /// </summary>
        short ResiliencyScoreInPerMyriad { get; }
        Note[] Notes { get; }
    }
}
