using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;
using Trader.Server.Session;
using System.Collections.Concurrent;
using Trader.Server.Service;
using System.Xml;
using Trader.Common;
using System.Runtime.InteropServices;
namespace Trader.Server._4BitCompress
{
    public class Quotation4Bit
    {
        private ConcurrentDictionary<AppType, ConcurrentDictionary<long, byte[]>> _Dict = new ConcurrentDictionary<AppType, ConcurrentDictionary<long, byte[]>>();
        private readonly static DateTime OrginTime = new DateTime(2011, 4, 1, 0, 0, 0);
        private const int CAPACITY = 512;
        private QuotationCommand _QuotationCommand;
        private const char _InnerSeparator = ':';
        private const char _StartSeparator = '/';
        private const char _OutterSeparator = ';';
        private const char _SequenceSeparator = '-';
        private static object _Lock = new object();
        public Quotation4Bit(QuotationCommand command)
        {
            this._QuotationCommand = command;
        }

        public QuotationCommand QuotationCommand { get { return this._QuotationCommand; } }


        public static byte[] GetPriceInBytes(Token token,TraderState state,QuotationCommand command,int beginSequence,int endSequence)
        {
            if (token.AppType == AppType.TradingConsole)
            {
                return GetDataForJavaTrader(state, command,beginSequence,endSequence);
            }
            return GetDataForMobile(token, state, command,beginSequence,endSequence);
        }


        public byte[] GetPriceInBytes(Token token,TraderState state)
        {
            byte[] price;
            ConcurrentDictionary<long,byte[]>  dict;
            if (!this._Dict.TryGetValue(token.AppType, out dict))
            {
                dict = new ConcurrentDictionary<long, byte[]>();
                this._Dict.TryAdd(token.AppType,dict);
            }
            if (dict.TryGetValue(state.SignMapping, out price))
            {
                return price;
            }
            if (token.AppType == AppType.TradingConsole)
            {
                price = GetDataForJavaTrader(state,this._QuotationCommand);
            }
            else
            {
                price = GetDataForMobile(token, state,this._QuotationCommand);
            }
            dict.TryAdd(state.SignMapping, price);
            return price;
        }

        private static byte[] GetDataForJavaTrader(TraderState state,QuotationCommand command,int? beginSequence=null,int? endSequence=null)
        {
            StringBuilder stringBuilder = new StringBuilder(CAPACITY);
            string seqenceStr = string.Format("{0}{1}{2}", (beginSequence ?? command.Sequence).ToString(), _SequenceSeparator.ToString(), (endSequence ?? command.Sequence).ToString());
            stringBuilder.Append(seqenceStr);
            stringBuilder.Append(_StartSeparator);
            OverridedQuotation[] overridedQuotations = command.OverridedQs;

            if (overridedQuotations != null && overridedQuotations.Length > 0)
            {
                bool addSeprator = false;
                foreach (OverridedQuotation overridedQuotation in overridedQuotations)
                {
                    if (!state.Instruments.ContainsKey(overridedQuotation.InstrumentID))
                    {
                        continue;
                    }
                    if (overridedQuotation.QuotePolicyID != (Guid)state.Instruments[overridedQuotation.InstrumentID])
                    {
                        continue;
                    }

                    if (addSeprator)
                    {
                        stringBuilder.Append(_OutterSeparator);
                    }
                    else
                    {
                        addSeprator = true;
                    }

                    stringBuilder.Append(GuidMapping.Get(overridedQuotation.InstrumentID));
                    stringBuilder.Append(_InnerSeparator);
                    stringBuilder.Append(overridedQuotation.Ask);
                    stringBuilder.Append(_InnerSeparator);
                    stringBuilder.Append(overridedQuotation.Bid);
                    stringBuilder.Append(_InnerSeparator);
                    stringBuilder.Append(overridedQuotation.High);
                    stringBuilder.Append(_InnerSeparator);
                    stringBuilder.Append(overridedQuotation.Low);
                    stringBuilder.Append(_InnerSeparator);
                    stringBuilder.Append(string.Empty);
                    stringBuilder.Append(_InnerSeparator);
                    stringBuilder.Append((long)(overridedQuotation.Timestamp - OrginTime).TotalSeconds);
                    stringBuilder.Append(_InnerSeparator);
                    stringBuilder.Append(overridedQuotation.Volume);
                    stringBuilder.Append(_InnerSeparator);
                    stringBuilder.Append(overridedQuotation.TotalVolume);
                }
            }
            stringBuilder.Append(_StartSeparator);
            return Quotation4BitEncoder.Encode(stringBuilder.ToString());
        }


        private static byte[] GetDataForMobile(Token token, TraderState state,QuotationCommand command,int? beginSequence=null,int? endSequence=null)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement commands = xmlDoc.CreateElement("Commands");
            xmlDoc.AppendChild(commands);
            XmlNode commandNode = command.ToXmlNode(token, state);
            if (commandNode == null)
            {
                return null;
            }
            XmlNode commandNode2 = commands.OwnerDocument.ImportNode(commandNode, true);
            commands.AppendChild(commandNode2);
            commands.SetAttribute("FirstSequence", (beginSequence ?? command.Sequence).ToString());
            commands.SetAttribute("LastSequence", (endSequence ?? command.Sequence).ToString());
            return Constants.ContentEncoding.GetBytes(commands.OuterXml);
        }


        public static byte[] GetDataForCommand(Token token,TraderState state,Command command)
        {
            if (string.IsNullOrEmpty(state.QuotationFilterSign))
            {
                return null;
            }
            return GetDataBytesInUtf8Format(token, state, command);
        }


        private static byte[] GetDataBytesInUtf8Format(Token token, TraderState state, Command command)
        {
            var node = ConvertCommand(token, state, command);
            if (node == null)
            {
                return null;
            }
            string xml = node.OuterXml;
            if (string.IsNullOrEmpty(xml))
            {
                return null;
            }
            return Constants.ContentEncoding.GetBytes(xml);
        }

        private static XmlNode ConvertCommand(Token token, State state, Command command)
        {
            if (token != null && token.AppType == AppType.Mobile)
            {
                return iExchange3Promotion.Mobile.Manager.ConvertCommand(token, command);
            }
            else
            {
                XmlNode commandNode = command.ToXmlNode(token, state);
                XmlElement commandElement = commandNode as XmlElement;
                if (commandElement == null)
                {
                    return null;
                }
                lock (_Lock)
                {
                    if (!commandElement.HasAttribute(ResponseConstants.CommandSequence))
                    {
                        commandElement.SetAttribute(ResponseConstants.CommandSequence, command.Sequence.ToString());
                    }
                }
                return commandElement;
            }
        }

    }
}
