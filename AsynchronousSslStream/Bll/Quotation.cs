using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using System.Xml;
using System.Collections.Concurrent;
using AsyncSslServer.Session;
using Serialization;
using log4net;
using AsyncSslServer._4BitCompress;
namespace AsyncSslServer.Bll
{
    public class Quotation
    {
        private WeakReference _Command;
        private ILog _Logger = LogManager.GetLogger(typeof(Quotation));
        private ConcurrentDictionary<AppType, ConcurrentDictionary<string, byte[]>> _QuotationFilterByAppTypeDict = new ConcurrentDictionary<AppType, ConcurrentDictionary<string, byte[]>>();
        public Quotation(Command command)
        {
            this._Command = new WeakReference(command);
        }

        public Tuple<bool,byte[]> ToBytes(Token token, TraderState state)
        {
            byte[] result = null;
            bool isQuotation =false;
            try
            {
                if (state.QuotationFilterSign == null)
                {
                    return Tuple.Create(false,result);
                }
                isQuotation = IsQuotationCommand();
                if (!isQuotation)
                {
                    return Tuple.Create(isQuotation, GetDataBytesInUtf8Format(token, state));
                }
                if (token.AppType == AppType.Mobile)
                {
                    result = GetQuotationWhenMobile(token, state);
                }
                else if (token.AppType == AppType.TradingConsole)
                {
                    result = GetQuotationWhenJavaTrader(token, state);
                }
            }
            catch(Exception ex)
            {
                this._Logger.Error(ex);
                result = null;
            }
            return Tuple.Create(isQuotation, result);
        }


        private byte[] GetQuotationWhenJavaTrader(Token token, TraderState state)
        {
            byte[] result = null;
            result = GetQuotationCommon(token, state);
            if (result == null)
            {
                QuotationCommand command = (QuotationCommand)this._Command.Target;
                var target = ConvertQuotation(command,state);
                result = target.DataInBytes.Value;
            }
            return result;
        }

        private Quotation4Bit ConvertQuotation(QuotationCommand quotationCommand,TraderState state)
        {
            var overridedQuotationList = new List<_4BitCompress.OverridedQuotation>();
            for (int index = 0; index < quotationCommand.OverridedQs.Length; index++)
            {
                var originOverridedQuotation = quotationCommand.OverridedQs[index];
                if (!state.Instruments.ContainsKey(originOverridedQuotation.InstrumentID))
                {
                    continue;
                }
                if (originOverridedQuotation.QuotePolicyID != (Guid)state.Instruments[originOverridedQuotation.InstrumentID])
                {
                    continue;
                }
                AsyncSslServer._4BitCompress.OverridedQuotation overridedQuotation = new AsyncSslServer._4BitCompress.OverridedQuotation();
                overridedQuotationList.Add(overridedQuotation);
                iExchange.Common.OverridedQuotation overridedQ = quotationCommand.OverridedQs[index];
                overridedQuotation.InstrumentId = overridedQ.InstrumentID;
                overridedQuotation.InstrumentSequence = GuidMapping.InstrumentIdMapping.AddOrGetExisting(overridedQuotation.InstrumentId);
                overridedQuotation.QuotePolicyId = overridedQ.QuotePolicyID;
                overridedQuotation.Timestamp = new DateTime(overridedQ.Timestamp.Ticks, DateTimeKind.Utc);
                overridedQuotation.Ask = overridedQ.Ask;
                overridedQuotation.Bid = overridedQ.Bid;
                overridedQuotation.High = overridedQ.High;
                overridedQuotation.Low = overridedQ.Low;
                overridedQuotation.TotalVolume = overridedQ.TotalVolume;
                overridedQuotation.Volume = overridedQ.Volume;
            }
            Quotation4Bit quotation = new Quotation4Bit();
            quotation.Sequence = quotationCommand.Sequence;
            quotation.OverridedQuotations =overridedQuotationList.ToArray() ;
            return quotation;
        }




        private byte[] GetQuotationWhenMobile(Token token, TraderState state)
        {
            byte[] result = null;
            result = GetQuotationCommon(token, state);
            if (result == null)
            {
                result = GetDataBytesInUtf8Format(token, state);
                CacheQuotationCommon(token.AppType, state.QuotationFilterSign, result);
            }

            return result;
        }

        private void CacheQuotationCommon(AppType appType, string filterSign, byte[] quotation)
        {
            this._QuotationFilterByAppTypeDict[appType][filterSign] = quotation;
        }


        private byte[] GetQuotationCommon(Token token, TraderState state)
        {
            byte[] result = null;
            if (!this._QuotationFilterByAppTypeDict.ContainsKey(token.AppType))
            {
                this._QuotationFilterByAppTypeDict[token.AppType] = new ConcurrentDictionary<string, byte[]>();
            }

            if (this._QuotationFilterByAppTypeDict[token.AppType].ContainsKey(state.QuotationFilterSign))
            {
                result = this._QuotationFilterByAppTypeDict[token.AppType][state.QuotationFilterSign];
            }
            return result;
        }

        private byte[] GetDataBytesInUtf8Format(Token token, TraderState state)
        {
            var node = GetXmlNodeHelper(token, state);
            return Constants.ContentEncoding.GetBytes(node.OuterXml);
        }


        private XmlNode GetXmlNodeHelper(Token token,TraderState state)
        {
            Command command = (Command)this._Command.Target;
            return ConvertCommand(command,token,state);
        }


        private bool IsQuotationCommand()
        {
            bool isQuotation = false;
            if (this._Command.IsAlive)
            {
                if (this._Command.Target is QuotationCommand)
                {
                    isQuotation = true;
                }
            }
            return isQuotation;
        }


        private XmlNode ConvertCommand(Command command, Token token, State state)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement commands = xmlDoc.CreateElement("Commands");
            xmlDoc.AppendChild(commands);
            this.AppendChild(commands, command, token, state);
            commands.SetAttribute("FirstSequence", command.Sequence.ToString());
            commands.SetAttribute("LastSequence", command.Sequence.ToString());
            return commands;
        }


        private void AppendChild(XmlElement commands, Command command, Token token, State state)
        {
            if (command == null) return;
            XmlNode commandNode = command.ToXmlNode(token, state);
            if (commandNode != null)
            {
                XmlNode commandNode2 = commands.OwnerDocument.ImportNode(commandNode, true);
                commands.AppendChild(commandNode2);
            }
        }
       
        
    }
}
