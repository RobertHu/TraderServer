//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using iExchange.Common;
//using System.Xml;
//using System.Collections.Concurrent;
//using Trader.Server.Session;
//using Serialization;
//using log4net;
//using Trader.Server._4BitCompress;
//using Mobile = iExchange3Promotion.Mobile;
//using Trader.Common;
//using System.Web;

//namespace Trader.Server.Bll
//{
//    public class Quotation
//    {
//        private static ILog _Logger = LogManager.GetLogger(typeof(Quotation));
//        private  ConcurrentDictionary<AppType, ConcurrentDictionary<long, byte[]>> _QuotationFilterByAppTypeDict = new ConcurrentDictionary<AppType, ConcurrentDictionary<long, byte[]>>();
//        private object _Lock = new object();
//        private Quotation() { }
//        public static readonly Quotation Default = new Quotation();

      
//        public byte[] ToBytesForGeneral(Token token, TraderState state,Command command)
//        {
//            byte[] result = null;
//            try
//            {
//                if (state.QuotationFilterSign == null)
//                {
//                    return result;
//                }
//                return GetDataBytesInUtf8Format(token, state,command);
//            }
//            catch(Exception ex)
//            {
//                _Logger.Error(ex);
//                result = null;
//            }
//            return result;
//        }


//        public byte[] ToBytesForQuotation(Token token,TraderState state,QuotationCommand command)
//        {
//            byte[] result = null;
//            try
//            {
//                if (token.AppType == AppType.Mobile)
//                {
//                    result = GetQuotationWhenMobile(token, state, command);
//                }
//                else if (token.AppType == AppType.TradingConsole)
//                {
//                    var quotation = Quotation4Bit.TryAddQuotation(command.OverridedQs, state, command.Sequence);
//                    if (quotation == null)
//                    {
//                        return null;
//                    }
//                    result = quotation.GetData(); 
//                }
//            }
//            catch (Exception ex)
//            {
//                _Logger.Error(ex);
//            }
//            return result;
//        }


//        private byte[] GetQuotationWhenMobile(Token token, TraderState state,QuotationCommand command)
//        {
//            byte[] result = null;
//            result = GetQuotationCommon(token, state);
//            if (result == null)
//            {
//                XmlDocument xmlDoc = new XmlDocument();
//                XmlElement commands = xmlDoc.CreateElement("Commands");
//                xmlDoc.AppendChild(commands);
//                XmlNode commandNode = command.ToXmlNode(token, state);
//                if (commandNode == null)
//                {
//                    return null;
//                }
//                XmlNode commandNode2 = commands.OwnerDocument.ImportNode(commandNode, true);
//                commands.AppendChild(commandNode2);
//                commands.SetAttribute("FirstSequence", command.Sequence.ToString());
//                commands.SetAttribute("LastSequence", command.Sequence.ToString());
//                result = Constants.ContentEncoding.GetBytes(commands.OuterXml);
//                CacheQuotationCommon(token.AppType, state.SignMapping, result);
//            }
//            return result;
//        }

//        private void CacheQuotationCommon(AppType appType, long filterSign, byte[] quotation)
//        {
//            ConcurrentDictionary<long, byte[]> dict;
//            if (this._QuotationFilterByAppTypeDict.TryGetValue(appType,out dict))
//            {
//                dict.TryAdd(filterSign, quotation);
//            }
//        }


//        private byte[] GetQuotationCommon(Token token, TraderState state)
//        {
//            byte[] result = null;
//            ConcurrentDictionary<long, byte[]> dict = null;
//            if (!this._QuotationFilterByAppTypeDict.TryGetValue(token.AppType, out dict))
//            {
//                dict = new ConcurrentDictionary<long, byte[]>();
//                this._QuotationFilterByAppTypeDict.TryAdd(token.AppType,dict);
//            }
//            dict.TryGetValue(state.SignMapping,out result);
//            return result;
//        }

//        private byte[] GetDataBytesInUtf8Format(Token token, TraderState state,Command command)
//        {
//            var node = ConvertCommand(token, state, command);
//            if (node == null)
//            {
//                return null;
//            }
//            string xml = node.OuterXml;
//            if (string.IsNullOrEmpty(xml))
//            {
//                return null;
//            }
//            return Constants.ContentEncoding.GetBytes(xml);
//        }

//        private XmlNode ConvertCommand( Token token, State state,Command command)
//        {
//            if (token != null && token.AppType == AppType.Mobile)
//            {
//                return Mobile.Manager.ConvertCommand(token, command);
//            }
//            else
//            {
//                XmlNode commandNode = command.ToXmlNode(token, state);
//                XmlElement commandElement = commandNode as XmlElement;
//                if (commandElement == null)
//                {
//                    return null;
//                }
//                lock (_Lock)
//                {
//                    if (!commandElement.HasAttribute(ResponseConstants.CommandSequence))
//                    {
//                        commandElement.SetAttribute(ResponseConstants.CommandSequence, command.Sequence.ToString());
//                    }
//                }
//                return commandElement;
//            }
//        }

//        public void Clear()
//        {
//            this._QuotationFilterByAppTypeDict.Clear();
//        }
//    }
//}
