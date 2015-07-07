# Scheduling jobs with NCron #

The [introductory article](Introduction.md) have already shown you how to bootstrap your NCron based service and register a job for execution once every hour. In this article you will learn more about the fluent scheduling API, including how to extend it.

## The `crontab` helper extensions ##

In the introductory sample, a helper method `Hourly()` is used to schedule a job for execution every hour. This method belongs to a set of extension methods available from the `NCron.Fluent.Crontab` namespace. Along with its friends `Daily()`, `Weekly()` and `Monthly()`, it allows super simple scheduling. In many applications these might be all the schedules you will ever need.

```
service.Hourly().Run<DrinkCoffee>();
service.Daily().Run<TakeShower>();
service.Weekly().Run<PlayBadminton>();
service.Monthly().Run<VisitParents>();
```

If you want to run at job _twice_ every day, or if you need your job to be executed at a specific time of the day, you will need something more powerful. The recommended solution is to use the `At(string)` extension method, which will accept any [crontab expression](http://code.google.com/p/ncrontab/wiki/CrontabExpression) (external link) as parameter. These expressions are succinct, yet extremely flexible, and they are widely used on Unix/BSD systems.

```
// Every sunday morning (I wish).
service.At("0 9 * * 0").Run<EatBaconAndEggs>();

// At nine in the morning, on the first day of each quarter.
service.At("0 8 1 1,4,7,10 *").Run<SubmitVatDeclaration>();

// Every half hour within usual work hours, Monday through Friday.
service.At("*/30 8-17 * * 1-5").Run<LookAtStackoverflow>();
```

Some people seem to suffer from allergy to strings, and if you belong to this group, the crontab expressions might scare you. Try to think of crontab expressions as a programming language for time schedules - similar to what regular expressions are to strings. If that does not help, remove the `using NCron.Fluent.Crontab` statement and continue reading.

## Adding schedules without using helpers ##

There might be cases where you need (or think you need) a schedule which can not be expressed by a crontab expression. In these cases, you are able to fall back to the `At(Func<DateTime, DateTime>)` method provided directly in the `NCron.Fluent` namespace. This lets you define a method that computes the next occurence in the schedule from a specified base time (last execution or start of service):

```
// Every night at midnight (0 1 * * *)
service.At(dt => dt.AddDays(1).Date).Run<EnterNewDay>();

// Frequent in the beginning of each month, then gradually less frequent.
service.At(dt => dt.AddHours(dt.Day)).Run<BuyNewGadget>();
```

Tip: In this example, lambda expression are used to declare new methods directly within the job registrations. If you have never used lambdas before, you might want to dig around the subject about to get an understanding of whats going on, but it really is quite simple (at least in this example).