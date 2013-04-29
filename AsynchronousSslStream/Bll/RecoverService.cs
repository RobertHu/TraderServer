using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsyncSslServer.Util;
using AsyncSslServer.TypeExtension;
using System.Xml;
using Trader.Helper;
using log4net;
using Trader.Common;
namespace AsyncSslServer.Bll
{
    public static class RecoverService
    {
        private static ILog _Logger = LogManager.GetLogger(typeof(RecoverService));
        public static XmlNode Recover(string originSession,string currentSession)
        {
            XmlNode result = XmlResultHelper.ErrorResult;
            try
            {
                if (AgentController.Default.RecoverConnection(Guid.Parse(originSession), Guid.Parse(currentSession)))
                {
                    result = XmlResultHelper.NewResult(StringConstants.OK_RESULT);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _Logger.Error(ex);
            }
            return result;
        }
    }
}
