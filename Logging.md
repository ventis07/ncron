# Writing log messages from an NCron service #

As scheduling service are run in non-interactive mode, logging becomes the primary communication channel from the service to the develop/system administrator. NCron provides an easy to use log interface, encouraging its users to output detailed log information from their scheduled jobs.

When extending the `CronJob` class, use the `Log` property to write messages:

```
public override void Execute()
{
    int answer = 7 * 6;
    Log.Info(() => "It appears that the answer is " + answer);
}
```

You might now be wondering what the `() =>` is all about. By constructing the log message within a lambda expression, it is possible to avoid costly string operations if the `ILog` implementation decides not to use the message for anything. This design encourages users to bulid extensive logging into their jobs, knowing that the cost of debug messages is virtually zero in the production environment, if configured to ignore such messages.

Refer to the [`ILog` interface](http://code.google.com/p/ncron/source/browse/trunk/src/NCron/Logging/ILog.cs) (or Visual Studio IntelliSense) for a complete list of methods provided by the `Log` property.

## What/when should I write to the log? ##

NCron does some logging out-of the box, including:

  * `Info` messages just before any job is executed
  * `Error` messages when unhandled exceptions escapes a job execution

There is no reason for you to duplicate this logging. If your jobs are _really_ simple (delete a single file, run a single command against a database, etc.), there really is no reason for you to do any manual logging. As soon as your job is just a bit more involved (eg if it contains a loop), you _should_ add additional log messages. This extra messages will help you when hunting bugs in an environment with no debugger available.

Tip: use the log levels consistently. Refer to the following descriptions, from the Apache Logging Services, or define you guidelines:

  * The `Debug` level designates fine-grained informational events that are most useful to debug an application.
  * The `Info` level designates informational messages that highlight the progress of the application at coarse-grained level.
  * The `Warn` level designates potentially harmful situations.
  * The `Error` level designates error events that might still allow the application to continue running.

## Where do the log messages go? ##

The default log implementation writes all messages - excluding `Debug` message, which are ignored - to the Application event log. Use the Event Viewer (Control Panel > Administrative Tools) to review the log.

Is is generally recommended to replace the default implementation with something more configurable, preferably using a third-party logging framework such as [log4net](http://logging.apache.org/log4net/) or [NLog](http://nlog-project.org/).

## How do I replace the log implementaion? ##

In your `ServiceSetup` method, assign a new value to the `SchedulingService.LogFactory` property:

```
public void ServiceSetup(SchedulingService service)
{
    service.LogFactory = new CustomLogFactory();
    // Add job schedules as usual.
}
```

A log factory must implement the [`ILogFactory`](http://code.google.com/p/ncron/source/browse/trunk/src/NCron/Logging/ILogFactory.cs) interface, which supports creation of logs implementing the [`ILog` interface](http://code.google.com/p/ncron/source/browse/trunk/src/NCron/Logging/ILog.cs).

An [log4net implementation](http://code.google.com/p/ncron/source/browse/trunk/src/NCron.Integration.log4net/LogFactory.cs) is available in the SVN repository. Copy the source file to your scheduling project, and set `service.LogFactory = new NCron.Integration.log4net.LogFactory()`. Integrating by source code, rather than referencing a pre-built assembly, allows you to pick the version og log4net. This avoids "DLL hell" when other components in your project has dependencies on a specific version of the logging framework.