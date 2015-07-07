# Dependency injection in NCron #

When building a scheduling service for an enterprise application, your jobs are likely to depend on other components/services from your application infrastructure. A job that updates order states might need a `OrderRepository`, and a job that send mail notifications might need an `IMailer`.

As you have seen in the [basic samples](FluentJobRegistration.md), NCron knows how to instantiate jobs using lambdas, allowing to resolve dependencies manually when setting up the `SchedulingService`. However, as soon as your dependency graph gets more than one layer deep - or when your dependencies need explicit disposal - this will get messy, and you should consider using automatic dependency injection.

## Using a third-party IoC library ##

NCron allows you to use any brand of IoC container to resolve and perform dependency injection for your jobs. Currently, the NCron source code repository only contains helper methods for [Autofac](http://code.google.com/p/autofac/), but do expect more helpers to be added in the near future. To use the Autofac helpers, simply copy the [helper extensions](http://code.google.com/p/ncron/source/browse/trunk/src/NCron.Integration.Autofac/AutofacIntegration.cs) to your project, and add a reference to the Autofac assembly.

Here is an example of a service setup, using Autofac for job instantiation:

```
using Autofac.Builder;
using NCron.Service;
using NCron.Fluent.Crontab;
using NCron.Integration.Autofac;

static void ServiceSetup(SchedulingService service)
{
    // Register components in container.
    var builder = new ContainerBuilder();
    builder.Register<SmtpMailer>().As<IMailer>().SingletonScoped();
    builder.Register<SqlDatabase>().As<IDatabase>().ContainerScoped();
    builder.Register<NotificationJob>().ContainerScoped();

    // Hook up integration with the root container.
    AutofacIntegration.RootContainer = builder.Build();

    // Schedule your jobs for execution as usual.
    service.At("0 2 * * *").Run<NotificationJob>();
}
```

Notice the namespaces included by `using` statements in the sample code. `NCron.Fluent.Generics` is left out in favor of `NCron.Integration.Autofac` which provides similar extension methods on the fluent API. This means that the `Run<TJob>()` method call in the last line uses Autofac, rather than the default instantiation technique.

By using the `ContainerScoped()` modifier for the job, we tell Autofac to create a new instance of the component for each container. The integration package ensures that each job execution is done in a fresh container, nested within the `RootContainer`. In the sample, the same `IMailer` is used for each job execution (even for other jobs dependent on the interface), while a new `IDatabase` is instantiated (and released) for each execution.

## Adding helpers for other IoC libraries ##

So, you prefer to use Castle Windsor, StructureMap, Unity, Ninject or some other IoC library which does not currently have helpers available in the NCron source repository? Do not fear - it is really easy to write such helpers.

Below is the _entire_ source code for the Autofac helpers:

```
public static class AutofacIntegration
{
    public static IContainer RootContainer { private get; set; }

    public static JobPart Run<TJob>(this SchedulePart part)
        where TJob : ICronJob
    {
        return part.With(() => RootContainer.CreateInnerContainer()).Run(c => c.Resolve<TJob>());
    }

    public static JobPart Run(this SchedulePart part, string serviceName)
    {
        return part.With(() => RootContainer.CreateInnerContainer()).Run(c => c.Resolve<ICronJob>(serviceName));
    }
}
```

Notice the `With()` calls within the fluent registrations. This method allows you to tell NCron how to create a new IoC container, which is then made available in the subsequent `Run()`, where you tell NCron how to resolve jobs from the container.

Whenever a job is up for execution, the container creating lambda will be called and its return value passed to the job resolving lambda. After executing the job, both the job and the container will be disposed. Depending on the API of your favorite IoC, you might have to write a wrapper around its container concept, supporting the `IDisposable` interface.

Please do submit helpers for your favorite IoC container as a patch, and it is likely to make it into the NCron source repository. If you need help building the helpers, throw some source code as a comment below along with a description of the problem you are facing.