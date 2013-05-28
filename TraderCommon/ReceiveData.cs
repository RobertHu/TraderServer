using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trader.Common
{
    public class ReceiveData
    {

        public long Session
        {
            get;
            set;
        }


        public byte[] Data
        {
            get;
            set;
        }
        public ReceiveData(long session, byte[] data)
        {
            this.Session = session;
            this.Data = data;
        }
    }
}
