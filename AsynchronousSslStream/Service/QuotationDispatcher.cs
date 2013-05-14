using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using System.Threading;
using System.Collections.Concurrent;
using log4net;
using System.Threading.Tasks;
namespace Trader.Server.Service
{
    public class QuotationDispatcher
    {
        private ConcurrentQueue<QuotationCommand> _QuotationQueue = new ConcurrentQueue<QuotationCommand>();
        public static readonly QuotationDispatcher Default = new QuotationDispatcher();
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
            if (quotation == null)
            {
                return;
            }
            this._QuotationQueue.Enqueue(quotation);
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
                if (this._QuotationQueue.Count == 0)
                {
                    continue;
                }
                var qotation = new QuotationCommand();
                int count = this._QuotationQueue.Count;
                qotation.Merge(new System.Collections.ArrayList(this._QuotationQueue));
                Parallel.For(0, count, i =>
                {
                    QuotationCommand command;
                    this._QuotationQueue.TryDequeue(out command);
                });
                if (qotation.OverridedQs == null)
                {
                    continue;
                }
                CommandManager.Default.AddCommand(null, qotation);
            }
        }

    }
}
