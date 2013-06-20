using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trader.Common
{
    public struct ReceiveData
    {

        private long _Session;
        private byte[] _Data;
        public long Session
        {
            get { return _Session; }
        }
        public byte[] Data
        {
            get { return _Data; }
        }
        public ReceiveData(long session, byte[] data)
        {
            this._Session = session;
             this._Data= data;
        }
    }
}
