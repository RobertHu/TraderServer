using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using System.Xml;
using System.Collections.Concurrent;
using Trader.Server.Session;
using Serialization;
using log4net;
using Trader.Server._4BitCompress;
using Mobile = iExchange3Promotion.Mobile;

namespace Trader.Server.Bll
{
    public class Quotation
    {
        private Command  _Command;
        private static ILog _Logger = LogManager.GetLogger(typeof(Quotation));
        private ConcurrentDictionary<AppType, ConcurrentDictionary<long, byte[]>> _QuotationFilterByAppTypeDict = new ConcurrentDictionary<AppType, ConcurrentDictionary<long, byte[]>>();
        public Quotation(Command command)
        {
            this._Command = command;
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
                _Logger.Error(ex);
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
                QuotationCommand command = (QuotationCommand)this._Command;
                var target = ConvertQuotation(command,state);
                result = target.DataInBytes.Value;
                CacheQuotationCommon(token.AppType, state.SignMapping, result);
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
                Trader.Server._4BitCompress.OverridedQuotation overridedQuotation = new Trader.Server._4BitCompress.OverridedQuotation();
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
                CacheQuotationCommon(token.AppType, state.SignMapping, result);
            }

            return result;
        }

        private void CacheQuotationCommon(AppType appType, long filterSign, byte[] quotation)
        {
            ConcurrentDictionary<long, byte[]> dict;
            if (this._QuotationFilterByAppTypeDict.TryGetValue(appType,out dict))
            {
                dict.TryAdd(filterSign, quotation);
            }
        }


        private byte[] GetQuotationCommon(Token token, TraderState state)
        {
            byte[] result = null;
            ConcurrentDictionary<long, byte[]> dict = null;
            if (!this._QuotationFilterByAppTypeDict.TryGetValue(token.AppType, out dict))
            {
                dict = new ConcurrentDictionary<long, byte[]>();
                this._QuotationFilterByAppTypeDict.TryAdd(token.AppType,dict);
            }
            dict.TryGetValue(state.SignMapping,out result);
            return result;
        }

        private byte[] GetDataBytesInUtf8Format(Token token, TraderState state)
        {
            var node = ConvertCommand(token, state);
            return node == null ? null : Constants.ContentEncoding.GetBytes(node.OuterXml);
        }

        private bool IsQuotationCommand()
        {
            bool isQuotation = false;

            if (this._Command is QuotationCommand)
            {
                isQuotation = true;
            }
            return isQuotation;
        }


        private XmlNode ConvertCommand( Token token, State state)
        {
            if (!(this._Command is QuotationCommand) && token.AppType == AppType.Mobile)
            {
                return Mobile.Manager.ConvertCommand(token, this._Command);
            }
            else
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement commands = xmlDoc.CreateElement("Commands");
                xmlDoc.AppendChild(commands);
                this.AppendChild(commands, this._Command, token, state);
                commands.SetAttribute("FirstSequence", this._Command.Sequence.ToString());
                commands.SetAttribute("LastSequence", this._Command.Sequence.ToString());
                return commands;
            }
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
