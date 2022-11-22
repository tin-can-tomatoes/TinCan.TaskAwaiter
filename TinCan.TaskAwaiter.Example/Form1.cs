using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TinCan.TaskAwaiter.Example
{
    public partial class Form1 : Form
    {
        private Random rnd = new Random();
        private TaskAwaiter<string, int> awaiter = new TaskAwaiter<string, int>();
        public Form1()
        {
            InitializeComponent();
        }

        private void tmrCheckTasks_Tick(object sender, EventArgs e)
        {
            awaiter.Check();
            lblPendingTasks.Text = string.Format("{0} Task(s) currently in progress", awaiter.PendingTasks);
        }

        private string LongRunningTask(int millisecondsDelay)
        {
            if (millisecondsDelay % 5 == 0)
            {
                throw new ApplicationException("Random error!! Oh no!");
            }

            System.Threading.Thread.Sleep(millisecondsDelay);

            if (millisecondsDelay % 7 == 0)
            {
                throw new ApplicationException("Another error! What a tragedy!");
            }

            return String.Format("Waited {0} milliseconds", millisecondsDelay);
        }

        private Task<string, int> LongRunningTaskAsync(int millisecondsDelay)
        {
            Task<string, int> task = new Task<string, int>(LongRunningTask, millisecondsDelay);
            task.Start();
            return task;
        }

        private void handleTaskResult(string result)
        {
            txtResult.Text += "Task completed: " + result + Environment.NewLine;
        }

        private void handleTaskFailure(TaskStatus status, Exception ex)
        {
            txtResult.Text += "Task failed: " + ex.Message + Environment.NewLine;
        }

        private void btnAddTask_Click(object sender, EventArgs e)
        {
            int r = rnd.Next(0, 10000);
            Task<string, int> task = LongRunningTaskAsync(r);
            awaiter.Await(task, handleTaskResult, handleTaskFailure);
        }
    }
}