using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trader.Server.Bll;
using Trader.Server.Session;
using log4net;
using System.Threading;
using Trader.Common;
using Trader.Helper;
using System.Threading.Tasks;
using iExchange.Common;
using System.Xml;
using System.Xml.Linq;
using Trader.Server.TypeExtension;
using System.Collections;

namespace Trader.Server.Service
{
    public sealed class CommandSender
    {
        private ILog _Logger = LogManager.GetLogger(typeof(CommandSender));
        private CommandSender() { }
        public static readonly CommandSender Default = new CommandSender();

        public void Send(Command command)
        {
            try
            {
                //QuotationAgent.Quotation.Default.Send(new Quotation(command), this.SendCommand);
            }
            catch (Exception ex)
            {
                this._Logger.Error(ex);
            }
        }



    }
}
