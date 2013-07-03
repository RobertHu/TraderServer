using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using System.Threading;
namespace Trader.Server.Bll
{
    public class TradeDayChecker
    {
        private DateTime _TradeDayBeginTime;
        private volatile bool _IsStarted = false;
        private volatile bool _IsStoped = false;
        private const int WHEN_RIJIE_SLEEPTIME = 3600 * 1000;
        private const int GENERAL_SLEEPTIME = 10 * 60 * 1000;
        public TradeDayChecker(string connStr)
        {
            string tradeDayTimeInString = DataAccess.ExecuteScalar("select TradeDayBeginTime from dbo.SystemParameter", connStr).ToString();
            this._TradeDayBeginTime = DateTime.Parse(tradeDayTimeInString);
        }

        public void Start()
        {
            if (this._IsStarted)
            {
                return;
            }
            Thread checkThread = new Thread(this.CheckHandle);
            checkThread.IsBackground = true;
            checkThread.Start();
        }

        public void Stop()
        {
            this._IsStoped = true;
        }

        private void CheckHandle()
        {
            while (true)
            {
                if (this._IsStoped)
                {
                    break;
                }
                var dt = DateTime.Now;
                if (dt.Hour == this._TradeDayBeginTime.Hour && dt.Minute > this._TradeDayBeginTime.Minute)
                {
                    AgentController.Default.KickoutAllClient();
                    Thread.Sleep(WHEN_RIJIE_SLEEPTIME);
                }
                else
                {
                    Thread.Sleep(GENERAL_SLEEPTIME);
                }
            }
        }

    }
}
