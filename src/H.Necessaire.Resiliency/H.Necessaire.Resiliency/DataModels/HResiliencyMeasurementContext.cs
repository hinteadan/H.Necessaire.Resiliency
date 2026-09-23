using H.Necessaire.Resiliency.Abstractions.DataModels;
using System;
using System.Collections;
using System.Collections.Generic;

namespace H.Necessaire.Resiliency.DataModels
{
    public class HResiliencyMeasurementContext : EphemeralTypeBase, ImAnHResiliencyMeasurementContext
    {
        readonly IReadOnlyDictionary<string, object> contextDictionary = new Dictionary<string, object>();
        public HResiliencyMeasurementContext() => DoNotExpire();
        public HResiliencyMeasurementContext(IReadOnlyDictionary<string, object> contextDictionary) : this()
        {
            this.contextDictionary = contextDictionary ?? new Dictionary<string, object>();
        }

        public static implicit operator HResiliencyMeasurementContext(Dictionary<string, object> contextDictionary)
            => new HResiliencyMeasurementContext(contextDictionary);

        public object this[string key] => contextDictionary[key];

        public string ID { get; set; }

        public Note[] Notes { get; set; } = null;

        public OperationResult<TValue> GetValueFor<TValue>(string key)
        {
            return HSafe.Run(() =>
            {
                if (!contextDictionary.TryGetValue(key, out var rawValue))
                    return OperationResult.Fail(string.Join("", key, " doesn't exist")).WithoutPayload<TValue>();

                if (rawValue is null && CanBeNull<TValue>())
                    return default(TValue).ToWinResult();

                if (!(rawValue is TValue value))
                    return OperationResult.Fail(string.Join("", key, " exists but it's not of type ", typeof(TValue).FullName, ", it's of type ", rawValue.GetType().FullName)).WithoutPayload<TValue>();

                return value.ToWinResult();
            }).UnwrapToFirstFailOrLastWin();
        }

        public TValue GetValueOrDefaultFor<TValue>(string key, TValue defaultTo = default)
        {
            if (!GetValueFor<TValue>(key).Ref(out var res, out var value))
                return defaultTo;

            return value;
        }

        static bool CanBeNull<T>()
        {
            Type type = typeof(T);

            // Reference types can always be null
            if (!type.IsValueType) return true;

            // Value types are only nullable if they are Nullable<T>
            return Nullable.GetUnderlyingType(type) != null;
        }

        public IEnumerable<string> Keys => contextDictionary.Keys;

        public IEnumerable<object> Values => contextDictionary.Values;

        public int Count => contextDictionary.Count;

        public bool ContainsKey(string key) => contextDictionary.ContainsKey(key);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => contextDictionary.GetEnumerator();

        public bool TryGetValue(string key, out object value) => contextDictionary.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => contextDictionary.GetEnumerator();
    }
}
