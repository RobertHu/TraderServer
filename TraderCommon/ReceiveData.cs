using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trader.Common
{
    public struct ReceiveData
    {
        private long _ClientID;
        private byte[] _Data;
        public long ClientID
        {
            get { return _ClientID; }
        }
        public byte[] Data
        {
            get { return _Data; }
        }
        public ReceiveData(long clientId, byte[] data)
        {
            this._ClientID = clientId;
            this._Data = data;
        }
    }
}
