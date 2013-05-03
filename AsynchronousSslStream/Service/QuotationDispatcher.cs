using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using System.Threading;
using System.Collections.Concurrent;
using log4net;
namespace Trader.Server.Service
{
    public class QuotationDispatcher
    {
        private List<QuotationCommand> _QuotationQueue = new List<QuotationCommand>();
        public static readonly QuotationDispatcher Default = new QuotationDispatcher();
        private object _DispatchLock = new object();
        private volatile bool _IsStoped = false;
        private const int _ProcessPeriodMilliseconds = 800;
        private ILog _Logger = LogManager.GetLogger(typeof(QuotationDispatcher));
        private QuotationDispatcher()
        {
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
            lock (this._DispatchLock)
            {
                if (quotation == null)
                {
                    return;
                }
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
                lock (this._DispatchLock)
                {
                    if (this._QuotationQueue.Count == 0)
                    {
                        continue;
                    }
                    var qotation = new QuotationCommand();
                    qotation.Merge(new System.Collections.ArrayList(this._QuotationQueue));
                    this._QuotationQueue.Clear();
                    if (qotation.OverridedQs == null)
                    {
                        continue;
                    }
                    CommandManager.Default.AddCommand(null, qotation);
                }
            }
        }

        



    }
}
