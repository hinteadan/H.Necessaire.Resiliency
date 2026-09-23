using System;

namespace H.Necessaire.Resiliency.Abstractions
{
    public static class Xtnx
    {
        static TimeSpan EnsureMinMax(this TimeSpan value, TimeSpan min, bool isMinIncludedAsValid, TimeSpan max, bool isMaxIncludedAsValid, TimeSpan valueIfLessThanMin,TimeSpan valueIfGreaterThanMax)
        {
            bool isValueTooLow = !(isMinIncludedAsValid ? value >= min : value > min);
            bool isValueTooHigh = !(isMaxIncludedAsValid ? value <= max : value < max);
            bool isValueValid = !isValueTooLow && !isValueTooHigh;
            if (isValueValid)
                return value;
            if (isValueTooLow)
                return valueIfLessThanMin;
            if (isValueTooHigh)
                return valueIfGreaterThanMax;
            return value;//Should never get here
        }
    }
}
