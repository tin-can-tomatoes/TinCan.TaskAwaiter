# Tin-Can Task Awaiter

An extremely simple .NET Framework 2.0 library for executing long-running tasks off the UI thread.

For more recent versions of .NET, you *really* should use the built-in async functionality. This library was only intended for developing quick & dirty projects for legacy systems that need some hands-off multithreading support

Built in Visual Studio 2005.

## Usage

Tasks can be created from any method that takes one parameter and returns a value.

Examples will use the following method as our long running task.

```cs
private string LongRunningTask(int millisecondsDelay)
```

To create the task, simply create the task with the first type parameter being the Type of the result (string), and the second type parameter being the type of the long running task's parameter (in this case, int).
Pass the parameter that will be used by the method in the Task's constructor:
```cs
Task<string, int> task = new Task(LongRunningTask, 10000);
```

Watch out! Creating a task doesn't start it. Make sure to run task.Start()! 

As a convention, functions returning tasks should only return started tasks.

 Each task will run in its own thread, that is created when the task starts, and exits as the task finishes. This isn't exactly efficient, so be aware of this drawback when designing your software.

```cs
task.Start()
```


Great! so we've started the task. Now, how do we get the results back on the UI thread?

That's where the TaskAwaiter comes into play.

The TaskAwaiter can be used with just about any UI library, but Windows Forms will be used as an example here.


In your form's class, create a TaskAwaiter with the same type parameters as the Tasks you'll be awaiting:

```cs
private TaskAwaiter<string, int> awaiter = new TaskAwaiter<string, int>();
```

Create a couple callback methods, one for success, and one for failure. These will be called from the UI thread, so no need to mess around with invoking components.
The success handler should accept one parameter of the type specified as the task's result.
The failure handler should accept two parameters: the first a TaskStatus, the second an Exception (which may be null)
Both should not return a value:

```cs
private void handleTaskResult(string result)
{
    // Do something with the result
}

private void handleTaskFailure(TaskStatus status, Exception ex)
{
    // Do something with the status and/or exception
}
```

Now, alone this will do nothing. Something on the UI thread needs to incite the TaskAwaiter to check for completed/failed tasks.

Since we're using Windows Forms for this example, we can use a timer!

Create a timer on your form, and enable it. You can modify its interval to increase/decrease the frequency tasks are checked.

Then, for the tick handler, simply call (TaskAwaiter).Check()

```cs

private void tmrCheckTasks_Tick(object sender, EventArgs e)
{
    awaiter.Check();
}

```


Now, your task awaiter is ready to go! Just need to await some tasks:

```
awaiter.Await(task,handleTaskResult, handleTaskFailure);
```

The tasks will run in the background, with the TaskAwaiter checking on each timer tick to determine if a task has completed or failed.
When a task completes, the appropriate handler supplied to the awaiter with the task will be called from the thread that called (TaskAwaiter).Check().

See the example project for more information.