# Building a scheduling service with NCron #

Building a scheduling service with NCron is supposed to be as frictionless as possible, allowing you to focus on the real business logic in your project.

This instructions on this page assumes you have access to Visual Studio 2008 or newer. It is most certainly possible to use NCron without Visual Studio, and the following guide should still give you all the hints you need to get started.

## Step 1 - Create a new "Console Application" project ##

Remember to set choose .NET Framework 3.5 or higher. Choose C#, VB.NET or whatever language you are most comfortable with - NCron does not care, but the code samples on this website are mostly in C#.

![http://ncron.googlecode.com/svn/wiki/images/intro-newproj.png](http://ncron.googlecode.com/svn/wiki/images/intro-newproj.png)

## Step 2 - Add a reference to the NCron assembly ##

Make sure you have the newest public release of NCron (or your own build if you prefer) somewhere on your file system. Use the "Add Reference..." dialog to reference the assembly from your console application:

![http://ncron.googlecode.com/svn/wiki/images/intro-addref.png](http://ncron.googlecode.com/svn/wiki/images/intro-addref.png)

## Step 3 - Add one of more job types to your project ##

Simply create classes in your project (or in seperate library projects referenced by the console application), and let your classes extend the abstract `CronJob` class. The class has an abstract method `Execute()` which you will need to implement. This is where you place the logic of your job:

```
class DataUpdateJob : NCron.CronJob
{
    public override void Execute()
    {
        System.IO.File.Copy(@"\\corp\sales\export.dat", @"e:\data\sales.dat");
    }
}
```

Tip: If your job implementation uses unmanaged resources, you should also override the `Dispose(bool)` method, ensuring that everything gets cleaned up after use.

## Step 4 - Bootstrap the application entry point ##

When you created your project, Visual Studio created a Program.cs file for you. This files contains a class with a static method `Main()`, serving as the entry point of your application. From this method you need to make a call to the static method `Bootstrap.Init()`, giving NCron control over the actual service execution:

```
using NCron.Service;
using NCron.Fluent.Crontab;
using NCron.Fluent.Generics;

class Program
{
    static void Main(string[] args)
    {
        Bootstrap.Init(args, ServiceSetup);
    }

    static void ServiceSetup(SchedulingService service)
    {
    }
}
```

The `using` statements and the empty `ServiceSetup` method is included in the listing above, as they are needed in the next step.

Tip: It is _highly_ recommended that you do not do anything in the Main() method, except from calling `Bootstrap()`. Exceptions thrown in the `Main()` method is extremely hard to debug when your application is running as a Windows service. Do whatever you have to do in the `ServiceSetup()` method instead.

## Step 5 - Schedule your task for execution ##

We are now ready to add our first schedule to the service. Add the following to the `SerivceSetup` method body:

```
service.Hourly().Run<DataUpdateJob>();
```

This tells NCron to execute your job once every hour. The fluent API is extremely flexible, and it enables you to specify (almost) any [schedule](FluentScheduling.md) for several flavors of [job registration](FluentJobRegistration.md).

## Step 6 - Compile, deploy and start service ##

Compilation is no different than with any other Visual Studio project. The output is an EXE file and a bunch of referenced DLL files. Copy the output to the server where the job is required to run, and install the service by opening a command line in the installation folder and running this command (replacing `AcmeCronJobs` with your assembly name):

```
AcmeCronJobs install
```

Tip: In this short tutorial, we let NCron use default values for all installation options. Before you rush to install your service in a production environment - or if you hit exceptions related to user permissions - you should review the [deployment documentation](Deployment.md).

In the command prompt, type the following to start the service:

```
net start AcmeCronJobs
```

Your service is now be running, and it will automatically be started again if the server gets rebooted. Watch the Application event log for messages from the service. Each job invocation will be logged, along with any unhandled exceptions from your jobs.

If you prefer logging in a text file, email, or something else, it is perfectly possible to replace the log implementation used by NCron. It is also possible (and encouraged) to write additional custom messages to the log from your jobs. Proceed to read more about [logging](Logging.md) when you are ready.