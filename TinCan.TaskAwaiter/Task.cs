using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TinCan.TaskAwaiter
{
    public enum TaskStatus
    {
        Pending,
        InProgress,
        Success,
        Fail,
        Aborted
    }

    public class Task<T,P>
    {
        #region Private Fields

        private T result;
        private Exception error;
        private TaskStatus status;
        private bool fin;

        private TaskOperation operation;
        private P operationParameter;

        private Thread workThread;
        private readonly object _lock = new object();


        #endregion

        public delegate T TaskOperation(P taskParams);

        public T Result { get { return result; } private set { result = value; } }

        public Exception Error { get { return error; } private set { error = value; } }

        public TaskStatus Status { get { return status; } private set { status = value; } }

        public bool Finished { get { return fin; } private set { fin = value; } }

        public Task(TaskOperation task, P taskParams)
        {
            operation = task;
            operationParameter = taskParams;
            workThread = null;
            Status = TaskStatus.Pending;
            Finished = false;
            Error = null;
        }


        public void Start()
        {
            lock (_lock)
            {
                if (workThread != null)
                {
                    throw new NotSupportedException("Work thread already started");
                }
                else if (Status != TaskStatus.Pending)
                {
                    throw new NotSupportedException("Undefined state");
                }

                workThread = new Thread(threadTask);
            }
            workThread.Start();

        }

        public void Cancel()
        {
            lock (_lock)
            {
                workThread.Abort();
                Status = TaskStatus.Aborted;
                Finished = true;
            }
        }

        private void threadTask()
        {
            T res = default(T);
            lock (_lock)
            {
                Status = TaskStatus.InProgress;
            }
            try
            {
                res = operation(operationParameter);
                lock (_lock)
                {
                    Status = TaskStatus.Success;
                }
            }
            catch (Exception ex)
            {
                res = default(T);
                lock (_lock)
                {
                    Error = ex;
                    Status = TaskStatus.Fail;
                }
            }

            lock (_lock)
            {
                Result = res;
                Finished = true;
            }
        }

    }
}
