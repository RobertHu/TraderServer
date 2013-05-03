using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using iExchange.Common;

namespace Trader.Server.Bll
{
    public class MarketDepthManager
    {
        private Dictionary<Guid, MatchInfoCommand> _MatchInfoCommands;

        public MarketDepthManager()
        {
            this._MatchInfoCommands = new Dictionary<Guid, MatchInfoCommand>();
        }

        public List<MatchInfoCommand> GetCommands(IEnumerable<Guid> instrumentIds)
        {
            List<MatchInfoCommand> commands = new List<MatchInfoCommand>();
            foreach (Guid instrumentId in instrumentIds)
            {
                lock (this)
                {
                    if (this._MatchInfoCommands.ContainsKey(instrumentId))
                    {
                        commands.Add(this._MatchInfoCommands[instrumentId]);
                    }
                }
            }
            return commands;
        }

        public void UpdateCache(MatchInfoCommand command)
        {
            lock (this)
            {
                this._MatchInfoCommands[command.InstrumentId] = command;
            }
        }
    }
}