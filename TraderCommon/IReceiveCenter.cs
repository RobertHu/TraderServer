using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trader.Common
{
    public interface IReceiveCenter
    {
        void Send(ReceiveData data);
    }
}
