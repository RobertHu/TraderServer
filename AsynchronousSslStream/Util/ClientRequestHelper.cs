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
        public static void Process(SerializedObject request)
        {
            JobItem result = null;
            try
            {
                XElement content = ProcessHelper(request);
                request.Content = content;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                request.Content = XmlResultHelper.NewErrorResult(ex.ToString());
            }
            if (request.Content != null)
            {
                result = new JobItem(request);
            }
            Application.Default.SessionMonitor.Update(request.Session);
            if (result != null)
            {
                SendCenter.Default.Send(result);
            }
        }

        private static XElement  ProcessHelper(SerializedObject request)
        {
            XElement  result = XmlResultHelper.ErrorResult;
            XElement  content = request.Content;
            if (content.Name == RequestConstants.RootNodeName)
            {
                XElement methodNode = content.Descendants().Single(m => m.Name == RequestConstants.MethodNodeName);
                if (methodNode.Name == RequestConstants.MethodNodeName)
                {
                    result = ProcessMethodReqeust(request, methodNode.Value);
                }
            }
            return result;
        }

        private static void WhenSessionNotExistRecoverSessionToCurrentSession(SerializedObject request)
        {
            if (request.CurrentSession.HasValue)
            {
                request.Session = request.CurrentSession;
            }
        }

        private static XElement ProcessMethodReqeust(SerializedObject request,string methodName)
        {
            iExchange.Common.Token token = Trader.Server.Session.SessionManager.Default.GetToken(request.Session.Value);
            XElement  result = XmlResultHelper.ErrorResult;
            XElement  content = request.Content;
            if (!Application.Default.SessionMonitor.Exist(request.Session.Value))
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
