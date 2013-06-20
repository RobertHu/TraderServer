using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using System.Threading;
using System.Collections.Concurrent;
using log4net;
using System.Threading.Tasks;
using System.Collections;
namespace Trader.Server.Service
{
    public class QuotationDispatcher
    {
        private List<QuotationCommand> _QuotationQueue = new List<QuotationCommand>(20);
        public static readonly QuotationDispatcher Default = new QuotationDispatcher();
        private volatile bool _IsStoped = false;
        private  int _ProcessPeriodMilliseconds;
        private object _Lock = new object();
        private ILog _Logger = LogManager.GetLogger(typeof(QuotationDispatcher));
        private QuotationDispatcher()
        {
            
        }

        public void Initialize(int priceProcessPeriod)
        {
            this._ProcessPeriodMilliseconds = priceProcessPeriod;
            try
            {
                Thread thread = new Thread(this.Process);
                thread.IsBackground = true;
                thread.Start();
            }
            catch (Exception ex)
            {
                this._Logger.Error(ex);
            }
        }


        public void Stop()
        {
            this._IsStoped = true;
        }

        public void Add(QuotationCommand quotation)
        {
            lock (this._Lock)
            {
                this._QuotationQueue.Add(quotation);
            }
        }

        private void Process()
        {
            while (true)
            {
                if (this._IsStoped)
                {
                    break;
                }
                Thread.Sleep(_ProcessPeriodMilliseconds);
                var qotation = new QuotationCommand();
                lock (this._Lock)
                {
                    if (this._QuotationQueue.Count == 0)
                    {
                        continue;
                    }
                    qotation.Merge(this._QuotationQueue);
                    this._QuotationQueue.Clear();
                }
                if (qotation.OverridedQs == null || qotation.OverridedQs.Length==0)
                {
                    continue;
                }
                CommandManager.Default.AddQuotation(qotation);
            }
        }

    }
}
