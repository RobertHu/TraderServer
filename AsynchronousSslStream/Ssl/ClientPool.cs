using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trader.Server.Ssl
{
    public class ClientPool
    {
        private const int CAPACITY = 5000;
        private Stack<ClientRelation> _Pool = new Stack<ClientRelation>(CAPACITY);
        private object _Lock = new object();
        private ClientPool() { }
        public static readonly ClientPool Default = new ClientPool();
        public void Push(ClientRelation client)
        {
            lock (this._Lock)
            {
                this._Pool.Push(client);
            }
        }


        public ClientRelation Pop()
        {
            lock (this._Lock)
            {
                if (this._Pool.Count > 0)
                {
                    ClientRelation relation = this._Pool.Pop();
                    relation.Receiver.Reset();
                }
                return null;
            }
        }

        
    }
}
