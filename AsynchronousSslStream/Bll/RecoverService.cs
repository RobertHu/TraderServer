using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trader.Server.Util;
using Trader.Server.TypeExtension;
using System.Xml;
using log4net;
using Trader.Common;
using System.Xml.Linq;
namespace Trader.Server.Bll
{
    public static class RecoverService
    {
        private static ILog _Logger = LogManager.GetLogger(typeof(RecoverService));
        public static XElement Recover(Session originSession,Session currentSession)
        {
            XElement result = XmlResultHelper.ErrorResult;
            try
            {
                if (AgentController.Default.RecoverConnection(originSession, currentSession))
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
