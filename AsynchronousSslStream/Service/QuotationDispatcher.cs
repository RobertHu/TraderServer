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
        private ConcurrentQueue<QuotationCommand> _Queue = new ConcurrentQueue<QuotationCommand>();
        private AutoResetEvent _WaitEvent = new AutoResetEvent(false);
        private AutoResetEvent _StopEvent = new AutoResetEvent(false);
        private AutoResetEvent[] _Events;
        private bool _IsSendPriceImmediately = false;
         private List<QuotationCommand> _QuotationQueue = new List<QuotationCommand>(20);
         private  int _ProcessPeriodMilliseconds;
         private object _Lock = new object();
        private volatile bool _IsStoped = false;
        private ILog _Logger = LogManager.GetLogger(typeof(QuotationDispatcher));
        private QuotationDispatcher()
        {      
            this._Events = new AutoResetEvent[] { this._WaitEvent,this._StopEvent};
        }

        public static readonly QuotationDispatcher Default = new QuotationDispatcher();

        public void Initialize(int priceProcessPeriod, bool isSendPriceImmediately= false)
        {
            this._IsSendPriceImmediately = isSendPriceImmediately;
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
            if (this._IsSendPriceImmediately)
            {
                this._StopEvent.Set();
            }
        }

        public void Add(QuotationCommand quotation)
        {
            if (this._IsSendPriceImmediately)
            {
                this._Queue.Enqueue(quotation);
                this._WaitEvent.Set();
            }
            else
            {
                lock (this._Lock)
                {
                    this._QuotationQueue.Add(quotation);
                }
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
                if (this._IsSendPriceImmediately)
                {
                    WaitHandle.WaitAny(this._Events);
                    QuotationCommand quotation;
                    while (this._Queue.TryDequeue(out quotation))
                    {
                        CommandManager.Default.AddQuotation(quotation);
                    }
                }
                else
                {
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
                    if (qotation.OverridedQs == null || qotation.OverridedQs.Length == 0)
                    {
                        continue;
                    }
                    CommandManager.Default.AddQuotation(qotation);
                }
            }
        }

    }
}
