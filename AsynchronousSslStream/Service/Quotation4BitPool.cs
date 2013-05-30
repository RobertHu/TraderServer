using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trader.Server._4BitCompress;

namespace Trader.Server.Service
{
    public class Quotation4BitPool
    {
        private const int CAPACITY = 500;
        private Stack<Quotation4Bit> _Pool = new Stack<Quotation4Bit>(CAPACITY);
        private object _Lock = new object();
        private Quotation4BitPool() { }
        public static readonly Quotation4BitPool Default = new Quotation4BitPool();
        public void Push(Quotation4Bit quotation)
        {
            lock (this._Lock)
            {
                this._Pool.Push(quotation);
            }
        }

        public Quotation4Bit Pop()
        {
            lock (this._Lock)
            {
                if (this._Pool.Count > 0)
                {
                    return this._Pool.Pop();
                }
                return null;
            }
        }
        
    }
}
