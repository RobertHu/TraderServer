using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using log4net;
using Trader.Server.Bll;
using iExchange.Common;
using Trader.Server.SessionNamespace;

namespace Trader.Server.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class CommandCollectService:ICommandCollectService
    {
        private ILog _Logger = LogManager.GetLogger(typeof(CommandCollectService));
        public void AddCommand(iExchange.Common.Token token, iExchange.Common.Command command)
        {
           CommandManager.Default.Send(command);
        }

        public void KickoutPredecessor(Guid userId)
        {
            if (!SettingManager.Default.IsTest)
            {
                var session = SessionManager.Default.GetSession(userId);
                var sender = AgentController.Default.GetSender(session);
                if (sender != null)
                {
                    sender.Send(new ValueObjects.CommandForClient(data: NamedCommands.GetKickoutPacket()));
                }
                Application.Default.SessionMonitor.Remove(session);
            }
        }
    }
}
