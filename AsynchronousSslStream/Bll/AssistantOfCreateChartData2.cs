using System.Collections.Generic;
using System.Threading;
using iExchange.Common;

namespace Trader.Server.Bll
{
    public class AssistantOfCreateChartData2
    {
        private readonly Queue<TaskOfCreateChartData2> _Tasks = new Queue<TaskOfCreateChartData2>();
        private readonly object _TaskLock = new object();
        private int _WorkItemNumber;
        private const int MaxThreadCount = 3;
        public void AddTask(AsyncResult asyncResult, ChartDataArgument2 argument, WaitCallback funcToCreateChartData)
        {
            lock (this._TaskLock)
            {
                var task = new TaskOfCreateChartData2(argument, funcToCreateChartData);
                bool needQueueWorkItem = this._WorkItemNumber <= MaxThreadCount;
                this._Tasks.Enqueue(task);
                if (needQueueWorkItem)
                {
                    this._WorkItemNumber++;
                    ThreadPool.QueueUserWorkItem(this.CreateChartData, null);
                }
            }
        }

        private void CreateChartData(object state)
        {
            while (true)
            {
                TaskOfCreateChartData2 task = null;
                lock (this._TaskLock)
                {
                    if (this._Tasks.Count > 0)
                    {
                        task = this._Tasks.Dequeue();
                    }
                    else
                    {
                        this._WorkItemNumber--;
                        break;
                    }
                }
                task.FuncToCreateChartData(task.ArgumentOfCreateChartData);
            }
        }
    }
}