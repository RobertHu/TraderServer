using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncSslServer.Bll;
using AsyncSslServer.Session;
using iExchange3Promotion.Net.AsyncSslServer;
using log4net;
using System.Threading;
using Trader.Common;
using Trader.Helper;
using System.Threading.Tasks;
using iExchange.Common;
using System.Xml;
using System.Xml.Linq;
using AsyncSslServer.TypeExtension;
using System.Collections;

namespace AsyncSslServer.Service
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
                QuotationAgent.Quotation.Default.Send(new Quotation(command), this.SendCommand);
            }
            catch (Exception ex)
            {
                this._Logger.Error(ex);
            }
        }

        public void SendCommand(object commandObj,Guid session, Common.ICommunicationAgent sendAgent)
        {
            Quotation command = commandObj as Quotation;
            if (command == null) return;
            string sessionStr = session.ToString();
            var tokenAndState = SessionManager.Default.GetTokenAndState(sessionStr);
            Token token = tokenAndState.Item1;
            TraderState state = tokenAndState.Item2;
            if (token == null || state == null)
            {
                return;
            }
            var result = command.ToBytes(token,state);
            bool isQuotation = result.Item1;
            byte[] quotation = result.Item2;
            if (quotation == null)
            {
                return;
            }
            JobItem job = new JobItem();
            if (token.AppType == AppType.TradingConsole&&isQuotation)
            {
                job.Type = JobType.Price;
                job.Price = quotation;
            }
            else
            {
                job.Type = JobType.Transaction;
                job.ContentInByte = quotation;
                job.SessionID = session;
            }
            byte[] packet = SendManager.SerializeMsg(job);
            sendAgent.Send(packet);
        }

       


    }
}
