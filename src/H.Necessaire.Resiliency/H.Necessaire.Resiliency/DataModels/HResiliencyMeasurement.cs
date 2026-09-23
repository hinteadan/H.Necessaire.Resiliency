using H.Necessaire.Resiliency.Abstractions.DataModels;
using System;

namespace H.Necessaire.Resiliency.DataModels
{
    public class HResiliencyMeasurement : EphemeralTypeBase, ImAnHResiliencyMeasurement
    {
        public static readonly NumberInterval Myriad = new NumberInterval(0.0, 10000.0);
#if DEBUG
        const bool isDebug = true;
#else
        const bool isDebug = false;
#endif
        static readonly TimeSpan defaultValidity = isDebug ? TimeSpan.FromSeconds(17) : TimeSpan.FromMinutes(17);
        public HResiliencyMeasurement(string id, short resiliencyScoreInPerMyriad, Note[] notes = null)
        {
            ID = id;
            ResiliencyScoreInPerMyriad = resiliencyScoreInPerMyriad;
            Notes = notes.NullIfEmpty();
            ExpireIn(defaultValidity);
        }

        public string ID { get; }

        public short ResiliencyScoreInPerMyriad { get; set; } = 0;

        public Note[] Notes { get; }
    }
}
