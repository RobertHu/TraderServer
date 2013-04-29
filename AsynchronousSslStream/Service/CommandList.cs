using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;

namespace AsyncSslServer.Service
{
    public class CommandList : List<Command>
    {
        public CommandList(int capacity)
            :base(capacity)
        {

        }
        public DateTime LastTime { get; set; }
    }
   
}
