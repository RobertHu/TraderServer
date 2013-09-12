using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trader.Common
{
    public struct ReceiveData
    {
        private Session _ClientID;
        private byte[] _Data;
        public Session ClientID
        {
            get { return _ClientID; }
        }
        public byte[] Data
        {
            get { return _Data; }
        }
        public ReceiveData(Session clientId, byte[] data)
        {
            this._ClientID = clientId;
            this._Data = data;
        }
    }
}
