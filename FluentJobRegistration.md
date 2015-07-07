# Registering jobs with NCron #

Assuming that you are already aware of the different approaches to [scheduling using the fluent API](FluentScheduling.md), the article will show you a few different ways specify the jobs to be executed according to the schedules.

## The `generics` helper extension ##

In all examples you have seen this far, jobs have been referenced using a generic `Run<TJob>()` method. This extension method lives in the `NCron.Fluent.Generics` namespace, and is usually the easiest and most readable way to express job registrations.

```
service.Hourly().Run<DrinkCoffee>();
service.At("0 9 * * 1").Run<EatBaconAndEggs>();
```

Using the generic `Run<TJob>()` method requires the `TJob` type to implement `ICronJob` and to have a default (parameterless) constructor. The latter is a problem if you need to define a job that needs to be run with different arguments. Example:

```
class DeleteDiskCacheJob : NCron.CronJob
{
    readonly string _filePath;

    public DeleteDiskCacheJob(string filePath)
    {
        _filePath = filePath;
    }

    public override void Execute()
    {
        System.IO.File.Delete(_filePath);
    }
}
```

This is a really simply job, but the generic method has no clue what to pass as `filePath` to the constructor. Instead of using the generic helper method, you can use the more explicit `Run(Func<ICronJob>)` which is available directly in the `NCron.Fluent` namespace.

```
service.Hourly().Run(() => new DeleteDiskCacheJob("@c:\tmp\feed.xml"));
service.Daily().Run(() => new DeleteDiskCacheJob("@c:\tmp\report.pdf"));
```

Tip: In this example, lambda expression are used to declare new methods directly within the job registrations. If you have never used lambdas before, you might want to dig around the subject about to get an understanding of whats going on, but it really is quite simple (at least in this example).

If most of your jobs take constructor arguments, and/or some of the arguments are more complex than strings or numbers, you might want to look into the [dependency injection](DependencyInjection.md) features of NCron.

## The `reflection` helper extension ##

The `NCron.Fluent.Reflection` namespace contains a two helper methods, which are meant to be used in applications where job schedules are loaded from configuration files or databases. While somewhat similar to the generic helper method, they do not provide the same safety, as job types are late-bound using reflection.

```
service.Hourly().Run("AcmeCronJobs.CleanupJob");
service.Weekly().Run("ThirdParty.CronJobs.SpookyJob, ThirdParty");
service.Hourly().Run(concreteJob.GetType());
```

Tip: When using a type name as parameter, remember that it has to be fully namespace qualified. Assembly qualification is also required, if the job resides outside of the main service assembly.