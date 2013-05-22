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
        private Stack<QuotationCommand> _QuotationQueue = new Stack<QuotationCommand>(20);
        public static readonly QuotationDispatcher Default = new QuotationDispatcher();
        private volatile bool _IsStoped = false;
        private const int _ProcessPeriodMilliseconds = 800;
        private object _Lock = new object();
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
            if (quotation == null)
            {
                return;
            }
            lock (this._Lock)
            {
                this._QuotationQueue.Push(quotation);
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
                if (qotation.OverridedQs == null)
                {
                    continue;
                }
                CommandManager.Default.AddCommand(null, qotation);
            }
        }

    }
}
