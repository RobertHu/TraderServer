using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trader.Server.Ssl;

namespace Trader.Server.ValueObjects
{
    public struct ClientRelation
    {
        private ReceiveAgent _Receiver;
        private Client _Sender;
        public ClientRelation(Client sender, ReceiveAgent receiver)
        {
            this._Receiver = receiver;
            this._Sender = sender;
        }
        public ReceiveAgent Receiver { get { return this._Receiver; } }
        public Client Sender { get { return this._Sender; } }
    }
}
