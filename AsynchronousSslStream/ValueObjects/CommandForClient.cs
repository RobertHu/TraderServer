using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using Trader.Server._4BitCompress;
using Trader.Common;

namespace Trader.Server.ValueObjects
{
    public enum DataType
    {
        Quotation,
        Command,
        Response
    }

    public class CommandForClient
    {
        private QuotationTranslator _Quotation;
        private Command _Command;
        private DataType _CommandType;
        private UnmanagedMemory _Data;
        public CommandForClient(UnmanagedMemory data = null, QuotationCommand quotationCommand = null, Command command = null)
        {
            if (command != null)
            {
                this._Command = command;
                this._Quotation = null;
                this._Data = null;
                this._CommandType = DataType.Command;
            }
            else if (quotationCommand != null)
            {
                this._Quotation = new QuotationTranslator(quotationCommand);
                this._Command = null;
                this._Data = null;
                this._CommandType = DataType.Quotation;
            }
            else
            {
                this._Data = data;
                this._Command = null;
                this._Quotation = null;
                this._CommandType = DataType.Response;
            }
        }

        public QuotationTranslator Quotation { get { return this._Quotation; } }
        public Command Command { get { return this._Command; } }
        public DataType CommandType { get { return this._CommandType; } }
        public UnmanagedMemory Data { get { return this._Data; } }
    }
}
