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
using Trader.Common;

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

        public Command Command { get { return _Command; } }

        public byte[] ToBytes(Token token, TraderState state,out bool isQuotation)
        {
            byte[] result = null;
            isQuotation =false;
            try
            {
                if (state!=null && state.QuotationFilterSign == null &&!string.IsNullOrEmpty(state.SessionId))
                {
                    return result;
                }
                isQuotation = this._Command is QuotationCommand;
                if (!isQuotation)
                {
                    return GetDataBytesInUtf8Format(token, state);
                }
                if (token.AppType == AppType.Mobile)
                {
                    result = GetQuotationWhenMobile(token, state);
                }
                else if (token.AppType == AppType.TradingConsole)
                {
                    result = GetQuotationForJavaTrader(token, state);
                }
            }
            catch(Exception ex)
            {
                _Logger.Error(ex);
                result = null;
            }
            return result;
        }


        private byte[] GetQuotationForJavaTrader(Token token, TraderState state)
        {
            byte[] result = null;
            result = GetQuotationCommon(token, state);
            if (result == null)
            {
                QuotationCommand command = (QuotationCommand)this._Command;
                var quotation = Quotation4Bit.TryAddQuotation(command.OverridedQs, state,command.Sequence);
                result = quotation.GetData();
                CacheQuotationCommon(token.AppType, state.SignMapping, result);
            }
            return result;
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

        private XmlNode ConvertCommand( Token token, State state)
        {
            if (token != null && token.AppType == AppType.Mobile)
            {
                return Mobile.Manager.ConvertCommand(token, this._Command);
            }
            else
            {
                XmlNode commandNode = this._Command.ToXmlNode(token, state);
                XmlElement commandElement = commandNode as XmlElement;
                if (commandElement == null)
                {
                    return null;
                }
                lock (this._Command)
                {
                    if (!commandElement.HasAttribute(ResponseConstants.CommandSequence))
                    {
                        commandElement.SetAttribute(ResponseConstants.CommandSequence, this._Command.Sequence.ToString());
                    }
                  
                }
                return commandElement;
            }
        }
    }
}
