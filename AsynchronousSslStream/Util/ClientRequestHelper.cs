using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trader.Common;
using System.Xml;
using Trader.Server.Bll;
using Trader.Server.Service;
using Trader.Server.TypeExtension;
using Mobile = iExchange3Promotion.Mobile;
using System.Xml.Linq;
using Trader.Server.Serialization;
using Trader.Server.SessionNamespace;
using iExchange.Common;

namespace Trader.Server.Util
{
    public static class ClientRequestHelper
    {
        public static void Process(SerializedObject request)
        {
            XElement content = null;
            try
            {
                if (!request.IsKeepAlive)
                {
                    content = ProcessForNormal(request);
                }
                else
                {
                    ProcessForKeepAlive(request);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                content = XmlResultHelper.NewErrorResult(ex.ToString());
            }
            finally
            {
                Application.Default.SessionMonitor.Update(request.Session);
                if (content != null)
                {
                    request.Content = content;
                    SendCenter.Default.Send(request);
                }
            }
        }

        private static void ProcessForKeepAlive(SerializedObject request)
        {
            if (Application.Default.SessionMonitor.Exist(request.Session))
            {
                request.IsKeepAliveSuccess = true;
            }
            else
            {
                request.IsKeepAliveSuccess = false;
            }
            SendCenter.Default.Send(request);
        }

        private static XElement  ProcessForNormal(SerializedObject request)
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

        private static XElement ProcessMethodReqeust(SerializedObject request,string methodName)
        {
           Token token = SessionManager.Default.GetToken(request.Session);
           XElement result;
            if (!Application.Default.SessionMonitor.Exist(request.Session))
            {
                ExecuteRequestWhenSessionNotExist(methodName, request, token, out result);
            }
            else
            {
                result = RequestTable.Default.Execute(methodName, request, token);
            }
            return result;
        }

        private static void ExecuteRequestWhenSessionNotExist(string methodName, SerializedObject request, Token token,out XElement result)
        {
            result = XmlResultHelper.ErrorResult;
            if (methodName == "Login")
            {
                result = RequestTable.Default.Execute(methodName, request, token);
            }
            WhenSessionNotExistRecoveSessionToCurrentSession(request);
        }

        private static void WhenSessionNotExistRecoveSessionToCurrentSession(SerializedObject request)
        {
            if (request.ClientID != Session.INVALID_VALUE)
            {
                request.Session = request.ClientID;
            }
        }
    }
}
