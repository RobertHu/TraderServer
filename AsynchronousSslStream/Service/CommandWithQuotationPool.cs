using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trader.Server.Service
{
    public class CommandWithQuotationPool
    {
        private CommandWithQuotationPool() { }
        public static readonly CommandWithQuotationPool Default = new CommandWithQuotationPool();
        private object _Lock = new object();
        private const int CAPACITY = 500;
        private Stack<CommandWithQuotation> _Pool = new Stack<CommandWithQuotation>(CAPACITY);
        public void Push(CommandWithQuotation item)
        {
            lock (this._Lock)
            {
                this._Pool.Push(item);
            }
        }

        public CommandWithQuotation Pop()
        {
            lock (this._Lock)
            {
                if (this._Pool.Count > 0)
                {
                    return this._Pool.Pop();
                }
                return new CommandWithQuotation();
            }
        }
    }
}
