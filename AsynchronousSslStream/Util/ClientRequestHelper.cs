using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncSslServer;
using Trader.Common;
using Serialization;
using System.Xml;
using Trader.Server.Bll;
using Trader.Server.Service;
using Trader.Server.TypeExtension;

using Trader.Helper;
using Mobile = iExchange3Promotion.Mobile;
using System.Xml.Linq;

namespace Trader.Server.Util
{
    public static class ClientRequestHelper
    {
        public static JobItem Process(SerializedObject request)
        {
            JobItem result;
            try
            {
                XmlNode content = ProcessHelper(request);
                request.Content = content;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);                
                request.Content = XmlResultHelper.NewErrorResult(ex.ToString());
            }
            finally
            {
                result = new JobItem(request);
                Application.Default.SessionMonitor.Update(request.Session);
            }
            return result;

        }

        private static XmlNode ProcessHelper(SerializedObject request)
        {
            XmlNode result = XmlResultHelper.ErrorResult;
            XmlNode content = request.Content;
            if (content.Name == RequestConstants.RootNodeName)
            {
                XmlNode methodNode = content.SelectSingleNode(string.Format("//{0}/{1}", content.Name, RequestConstants.MethodNodeName));
                if (methodNode.Name == RequestConstants.MethodNodeName)
                {
                    result = ProcessMethodReqeust(request, methodNode.InnerText);
                }
            }
            return result;
        }

        private static void WhenSessionNotExistRecoverSessionToCurrentSession(SerializedObject request)
        {
            if (!string.IsNullOrEmpty(request.CurrentSession))
            {
                request.Session = request.CurrentSession;
            }
        }

        private static XmlNode ProcessMethodReqeust(SerializedObject request,string methodName)
        {
            iExchange.Common.Token token = Trader.Server.Session.SessionManager.Default.GetToken(request.Session);
            XmlNode result = XmlResultHelper.ErrorResult;
            XmlNode content = request.Content;
            if (!Application.Default.SessionMonitor.Exist(request.Session))
            {
                if (methodName == "Login")
                {
                    result = RequestTable.Default.Execute(methodName, request, token);
                }
                WhenSessionNotExistRecoverSessionToCurrentSession(request);
            }
            else
            {
                result = RequestTable.Default.Execute(methodName, request, token);
            }
            return result;
        }
    }
}
