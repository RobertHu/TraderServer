using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using log4net;
using Trader.Server.Bll;
using iExchange.Common;
using Trader.Server.Session;

namespace Trader.Server.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class CommandCollectService:ICommandCollectService
    {
        private ILog _Logger = LogManager.GetLogger(typeof(CommandCollectService));
        public void AddCommand(iExchange.Common.Token token, iExchange.Common.Command command)
        {
            if (command is QuotationCommand)
            {
                var quotation = command as QuotationCommand;
                QuotationDispatcher.Default.Add(quotation);
            }
            else
            {
                CompositeCommand compositeCommand = command as CompositeCommand;
                if (compositeCommand != null)
                {
                    foreach (var cmd in compositeCommand.Commands)
                    {
                        CommandManager.Default.AddCommand(token, cmd);
                    }
                }
                else
                {
                    CommandManager.Default.AddCommand(token, command);
                }
            }
        }

        public void KickoutPredecessor(Guid userId)
        {
            _Logger.Info(userId);
        }
    }
}
