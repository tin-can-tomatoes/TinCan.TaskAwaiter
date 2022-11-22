using System;
using System.Collections.Generic;
using System.Text;

namespace TinCan.TaskAwaiter
{


    public class TaskAwaiter<T,P>
    {
        private class CallbackPair
        {
            public readonly TaskCompleteCallback complete;
            public readonly TaskFailedCallback failed;
            public CallbackPair(TaskCompleteCallback c, TaskFailedCallback f)
            {
                complete = c;
                failed = f;
            }
        }
        public delegate void TaskCompleteCallback(T result);
        public delegate void TaskFailedCallback(TaskStatus status, Exception error);
        
        Dictionary<Task<T,P>, CallbackPair> results = new Dictionary<Task<T,P>, CallbackPair>();


        public int PendingTasks
        {
            get
            {
                return results.Count;
            }
        }
        public void Await(Task<T,P> task, TaskCompleteCallback c, TaskFailedCallback f)
        {
            CallbackPair pair = new CallbackPair(c, f);
            results.Add(task, pair);
        }

        public void Check()
        {
            Dictionary<Task<T,P>, CallbackPair>.KeyCollection tasks = results.Keys;
            List<Task<T,P>> completions = new List<Task<T,P>>();

            foreach (Task<T,P> task in tasks)
            {
                if (task.Finished)
                {
                    completions.Add(task);
                }
            }
            foreach (Task<T,P> task in completions)
            {
                CallbackPair cb = results[task];
                results.Remove(task);
                if (task.Status == TaskStatus.Success)
                {
                    cb.complete(task.Result);
                }
                else
                {
                    cb.failed(task.Status, task.Error);
                }
            }
        }
        public void Cleanup()
        {
            foreach (Task<T,P> task in results.Keys)
            {
                task.Cancel();
            }
            results.Clear();
        }
    }
}
