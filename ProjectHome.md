NCron is a light-weight library for building and deploying scheduled background jobs on the .NET server platform. The goal is to hide the details about scheduling, timers and threads, letting you focus on implementing the actual jobs/tasks. NCron allows you to:

  * Write your jobs in any .NET programming language.
  * Define an unlimited number jobs in the same .NET assembly.
  * Schedule your jobs for execution using a compellingly flexible [fluent API](FluentScheduling.md).
  * Embed the scheduling plans within the assembly, or keep the plans in custom configuration files or database.
  * Write log message using a [simple logging API](Logging.md), optionally integrated with your favorite logging framework.
  * Use any IoC container to perform [dependency injection](DependencyInjection.md) for your jobs.

Implementing a scheduled job with NCron is as easy as implementing a single method:

```
public override void Execute()
{
    System.IO.File.Copy(@"\\corp\sales\export.dat", @"e:\data\sales.dat");
}
```

... and wiring it up for execution according to the desired schedule:

```
service.Hourly().Run<DataUpdateJob>();
```

See the [introductory wiki article](Introduction.md) for details.