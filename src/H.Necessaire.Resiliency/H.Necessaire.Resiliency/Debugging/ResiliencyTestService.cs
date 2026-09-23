using System;
using System.Threading;

namespace H.Necessaire.Resiliency.Debugging
{
    internal class ResiliencyTestService : IStringIdentity
    {
        static readonly Random random = new Random();
        static int instanceCount = 0;
        readonly string id;
        public ResiliencyTestService(string id)
        {
            this.id = id;
        }
        public ResiliencyTestService()
            : this (string.Join("", nameof(ResiliencyTestService), "_", Interlocked.Increment(ref instanceCount)))
        { }

        public string ID => id;

        public void RandomlyThrowError()
        {
            if (random.Next(0, 1000) % 17 == 0)
                throw new OperationResultException("Just a random exception for testing");
        }
    }
}
