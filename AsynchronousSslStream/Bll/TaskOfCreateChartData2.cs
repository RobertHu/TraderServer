using System.Threading;

namespace Trader.Server.Bll
{
    class TaskOfCreateChartData2
    {
        public ChartDataArgument2 ArgumentOfCreateChartData;
        public WaitCallback FuncToCreateChartData;
        public TaskOfCreateChartData2(ChartDataArgument2 argument, WaitCallback funcToCreateChartData)
        {
            this.ArgumentOfCreateChartData = argument;
            this.FuncToCreateChartData = funcToCreateChartData;
        }
    }
}