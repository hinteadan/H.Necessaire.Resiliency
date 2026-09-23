# H.Necessaire.Resiliency

Building blocks to facilitate dependencies resiliency.



## How to?


### Scenario

Say I have a dependency that I want to add resiliency to.

Let's call it `MyResilientDep`.

At app level (C#) this translates into a `class` registered as a dependency.




### Steps to add resiliency to it

1. We **wrap** it in a `ImAnHResilientServiceProvider<T>` and register it as such in the deps registry
    1. How?
        1. We implement `internal class MyResilientDepProvider : HResilientServiceProviderBase<MyResilientDep>`
        1. Which internally will have
            1. `ImAnHServiceResiliencyProvider<MyResilientDep>[] resilientServices = null;`
            1. `ImAnHResiliencyMeasurer measurer = null;`
        1. Refering the `resilientServices`
            1. Like such:
            ```csharp
            resilientServices = new ImAnHServiceResiliencyProvider<MyResilientDep>[] {
                "A".Morph(id => deps.NewHServiceResiliencyProvider(id, x => new MyResilientDep(id), HExecutionTimeMeasurer.NewContext(id,...))),
                "B".Morph(id => deps.NewHServiceResiliencyProvider(id, x => new MyResilientDep(id), HExecutionTimeMeasurer.NewContext(id,...))),
            }
            ```
        1. Refering the `measurer = dependencyProvider.Get<HExecutionTimeMeasurer>();`
            1. More measurers are to come out of the box
            1. A custom measurer can also be implemented: `class MyCustomMeasurer : ImAnHResiliencyMeasurer`
        1. Implementing the abstract class
            ```csharp
            protected override ImAnHServiceResiliencyProvider<ResiliencyTestService>[] ResilientServices => resilientServices;
            protected override ImAnHResiliencyMeasurer ResiliencyMeasurer => measurer;
            ```
    1. Measurement context can be built using a common dictionary via the concrete class `HResiliencyMeasurementContext`
        1. Existing measurers expose an explicit static method, e.g. `HExecutionTimeMeasurer.NewContext(...)` to help with context creation
        1. When implementing a custom measurer we should keep the same pattern
        1. The `HExecutionTimeMeasurer` should be fine for most scearios and it can also be used internally in a custom measurer
        1. Contexts can be merged together via a couple of extension methods defined in `H.Necessaire.Resiliency.Concretes.DataModels.Xtnx`

1. We register it: `deps.Register<ImAnHResilientServiceProvider<MyResilientDep>>(() => new MyResilientDepProvider())`

1. We use it in our conusming code
```csharp
internal class HNecessaireResiliencyDebugger : ImADependency, IDebug
{
    ImAnHResilientServiceProvider<MyResilientDepProvider> resiliencyTestServiceProvider;
    public void ReferDependencies(ImADependencyProvider dependencyProvider)
    {
        resilientDepProvider = dependencyProvider.Get<ImAnHResilientServiceProvider<MyResilientDepProvider>>();
    }

    public async Task Debug()
    {
        (await resilientDepProvider.GetAllServiceInstancesOrderedByResiliency()).Ref(out var res, out var service);

        //OR

        ImAnHServiceResiliencyRunner<MyResilientDepProvider> runner = await resilientDepProvider.GetResiliencyRunner().ThrowOnFailOrReturn();

        //OR

        MyResilientDepProvider service = await resilientDepProvider.GetBestKnownServiceInstance().ThrowOnFailOrReturn();

    }
}
```
> ⚠️ The preferred way is to use `await resilientDepProvider.GetResiliencyRunner()` because this will internally try to fallback to the other instances on the spot in case the latest select one fails in between measurements.
     