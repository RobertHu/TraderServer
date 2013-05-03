using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using iExchange.Common;

namespace Trader.Server.Service
{
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface ICommandCollectService
    {
        [OperationContract]
        [XmlSerializerFormat]
        void AddCommand(Token token, Command command);


        [OperationContract]
        [XmlSerializerFormat]
        void KickoutPredecessor(Guid userId);

    }
}
