using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iExchange.Common;

namespace AsyncSslServer.Bll
{
    public class CommandManager
    {
        private List<Command> _CommandList = new List<Command>();
        private CommandManager() { }
        public static readonly CommandManager Default = new CommandManager();
        public void Add(Command command)
        {
            this._CommandList.Add(command);
        }
    }
}
