using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;

namespace Trader.Server.Service
{
    public class QuotationPool
    {
        private const int CAPACITY = 3000;
        private Stack<QuotationCommand> _Pool = new Stack<QuotationCommand>(CAPACITY);
        private object _Lock = new object();
        private QuotationPool() { }
        public static readonly QuotationPool Default=new QuotationPool();
        public void Push(QuotationCommand quotation)
        {
            lock (this._Lock)
            {
                this._Pool.Push(quotation);
            }
        }

        public QuotationCommand Pop()
        {
            lock (this._Lock)
            {
                if (this._Pool.Count > 0)
                {
                    return this._Pool.Pop();
                }
                return new QuotationCommand();
            }
        }
    }
}
