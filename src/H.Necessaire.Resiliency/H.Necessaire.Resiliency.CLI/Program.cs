using H.Necessaire.CLI;
using H.Necessaire.Runtime.CLI;

namespace H.Necessaire.Resiliency.CLI
{
    internal class Program
    {
        public static async Task Main()
        {
            await new CliApp()
                .WithEverything()
                .WithDefaultRuntimeConfig()
                .With(x => x.Register<Deps>(() => new Deps()))
                .Run()
                ;
        }
    }
}
