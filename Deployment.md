# Deploying an NCron based service #

Applications built using NCron are intended to be installed as Windows services. This allows the application to run in the backround as soon as Windows is started, even without a user being logged on to the system.

## Installing the service ##

After compiling your application, copy the output (`.exe` and `.dll` files) to the server where you want the service to run. Open a command prompt, navigate to the directory where you have placed the files, and do **one** of the following:

  * Run this command to install the service with **default** settings:
> > `YourApplicationName install`
  * Use the `sc` command to install the service with **custom** settings:
> > `sc create YourServiceName binPath= YourApplicationName.exe start= auto`

With the `sc` command you are able to configure the service in several ways:

  * Use `obj=` to specify which account the service should run under
  * Use `start=` to choose automatic or manual service startup
  * Use `DisplayName=` to set the display name of the service

Learn more about the possiblities with `sc` command by typing `sc create` in your console, or by reading the [documentation](http://support.microsoft.com/kb/251192).

Tip: Beware of the spooky syntax of the command line paramters to `sc`. You need to have an equality sign _and_ a space character between the key and the value in each pair.

**Important:** NCron expects to be able to write log messages to the application event log, using a source name equal to the application assembly file name. This source is registered automatically when using the embedded installer, but when installing using `sc` you must create it manually:

```
eventcreate /so YourApplicationName /l Application /id 1 /t information /d "Log source created"
```

## Starting the service ##

If you have used the default installation, or if you specified `start= auto` when installing with `sc`, your service will be started automatically whenever the server boots. However, you will still need to explicitly start it right after installation, unless you plan to restart anyway.

There are several ways to start the newly installed service:

  * Type this in a command prompt, or in the Windows "Run" dialog: `net start YourServiceName`
  * Navigate to Control Panel > Administrative Tools > Services, and right click your service and choose "Start".

## Deploying updates to the service ##

Before you can ovewrite the application binaries on the server, your service must be stopped. Once agin, you have several options:

  * Type this in a command prompt, or in the Windows "Run" dialog: `net stop YourServiceName`
  * Navigate to Control Panel > Administrative Tools > Services, and right click your service and choose "Stop".

Now, unless you need to reconfigure the service installation (eg. change the account under which it is running), you can copy in the updated files, and the service again. If you _do_ need to change installation options, you should uninstall the existing service (see below), copy in new files, reinstall with new options, and then start the service again.

### Scripting the deployment process ###

If you plan to update your scheduling application often, you might want to automate the deployment process. This is quite easy to do, using a simpel batch script. Example:

```
net stop YourServiceName
cd c:/path/to/installed/service
copy /y c:/path/to/build/output/*
net start YourServiceName
```

Expand the script with uninstall/install operations in necessary.

## Uninstalling the service ##

Before uninstalling the service, make sure it is stopped (see above). Then do **one** of the following:

  * If installed using the `YourApplicationName install` command, just do a `YourApplicatioName uninstall`
  * If installed using `sc` (or if on doubt) do this: `sc delete YourServiceName`