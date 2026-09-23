using System;
using System.Collections;
using System.Collections.Generic;

namespace H.Necessaire.Resiliency.Abstractions.DataModels
{
    public interface ImAnHResiliencyMeasurementContext : IEphemeralType, IStringIdentity, IReadOnlyDictionary<string, object>
    {
        Note[] Notes { get; }
        OperationResult<TValue> GetValueFor<TValue>(string key);
        TValue GetValueOrDefaultFor<TValue>(string key, TValue defaultTo = default(TValue));
    }
}
