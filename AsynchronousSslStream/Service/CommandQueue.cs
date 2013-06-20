using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using iExchange.Common;
using System.Xml;
using System.Collections;
using log4net;
namespace Trader.Server.Service
{
    public class CommandQueue
    {
        private ILog _Logger = LogManager.GetLogger(typeof(CommandQueue));
        private ReaderWriterLockSlim _ReaderWriterLock = new ReaderWriterLockSlim();
        private static readonly int MessageListCapacity = 2048;
        private List<CommandList> _Commands = new List<CommandList>(MessageListCapacity);
        private Timer _Timer;
        private int _LastSequence = 0;
        public int LastSequence
        {
            get
            {
                return this._LastSequence;
            }
        }
        public CommandQueue(TimeSpan interval)
        {
            this._Timer = new Timer(this.CleanExpiredCommandHandle, interval, interval, interval);
        }
        public void Add(Command command)
        {
            this._ReaderWriterLock.EnterWriteLock();
            try
            {
                command.Sequence = Interlocked.Increment(ref this._LastSequence);
                if (this._Commands.Count == 0)
                {
                    this._Commands.Add(new CommandList(MessageListCapacity));
                    this._Commands[0].Add(command);
                    this._Commands[0].LastTime = DateTime.Now;
                }
                else if (this._Commands[0].Count == 0)
                {
                    this._Commands[0].Add(command);
                    this._Commands[0].LastTime = DateTime.Now;
                }
                else
                {
                    long minSequence = this._Commands[0][0].Sequence;
                    int indexOfMessageList = (int)Math.Floor((double)(command.Sequence - minSequence) / MessageListCapacity);
                    if (indexOfMessageList >= this._Commands.Count)
                    {
                        this._Commands.Add(new CommandList(MessageListCapacity));
                    }
                    this._Commands[indexOfMessageList].Add(command);
                    this._Commands[indexOfMessageList].LastTime = DateTime.Now;
                }
            }
            finally
            {
                this._ReaderWriterLock.ExitWriteLock();
            }
        }


        public XmlNode GetCommands(Token token, State state, int firstSequence, int lastSequence)
        {
            this._ReaderWriterLock.EnterReadLock();
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement commands = xmlDoc.CreateElement("Commands");
                xmlDoc.AppendChild(commands);
                List<Command> commandList = this.InnerGet(firstSequence, lastSequence);
                if (commandList == null)
                {
                    this._Logger.ErrorFormat("get lost command {0} -------{1} return null", firstSequence, lastSequence);
                    return null;
                }
                TradingConsoleState tradingConsoleState = state as TradingConsoleState;
                ArrayList quotationCommands = new ArrayList();
                foreach (var command in commandList)
                {
                    QuotationCommand quotation = command as QuotationCommand;
                    if (quotation != null)
                    {
                        quotationCommands.Add(quotation);
                    }
                    else
                    {
                        if (quotationCommands.Count > 0)
                        {
                            QuotationCommand mergingCommand = this.Merge(quotationCommands);
                            quotationCommands.Clear();
                            this.AppendChild(commands, mergingCommand, token, state);
                        }
                        this.AppendChild(commands, command, token, state);
                    }
                }
                if (quotationCommands.Count > 0)
                {
                    QuotationCommand mergingCommand = this.Merge(quotationCommands);
                    quotationCommands.Clear();
                    this.AppendChild(commands, mergingCommand, token, state);
                }

                commands.SetAttribute("FirstSequence", firstSequence.ToString());
                commands.SetAttribute("LastSequence", lastSequence.ToString());

                return commands;
            }
            finally
            {
                this._ReaderWriterLock.ExitReadLock();
            }
        }


        private List<Command> InnerGet(long beginSequence, long endSequence)
        {
            if (beginSequence >= this._LastSequence || this._Commands.Count == 0 || this._Commands[0].Count == 0) return null;
            long minSequence = this._Commands[0][0].Sequence;
            if (beginSequence < minSequence) return null;
            int indexOfMessageList = (int)Math.Floor((double)(beginSequence - minSequence) / MessageListCapacity);
            int beginIndex = (int)(beginSequence - this._Commands[indexOfMessageList][0].Sequence);
            int count = Math.Min((int)(endSequence - beginSequence + 1), (int)(this._LastSequence - beginSequence));
            List<Command> commandList = new List<Command>();
            for (int index = 0; index < count; index++)
            {
                int indexInMessageList = beginIndex++;
                if (indexInMessageList >= MessageListCapacity)
                {
                    indexOfMessageList++;
                    indexInMessageList = beginIndex = 0;
                }
                Command command = this._Commands[indexOfMessageList][indexInMessageList];
                commandList.Add(command);
            }
            return commandList;
        }


        private QuotationCommand Merge(ArrayList quotationCommands)
        {
            if (quotationCommands == null || quotationCommands.Count == 0)
            {
                return null;
            }
            else if (quotationCommands.Count == 1)
            {
                return (QuotationCommand)quotationCommands[0];
            }
            else
            {
                QuotationCommand lastQuotationCommand = (QuotationCommand)(quotationCommands[quotationCommands.Count - 1]);
                QuotationCommand mergingCommand = new QuotationCommand(lastQuotationCommand.Sequence);
                mergingCommand.Merge(quotationCommands);
                return mergingCommand;
            }
        }

        private void AppendChild(XmlElement commands, Command command, Token token, State state)
        {
            if (command == null) return;
            XmlNode commandNode = command.ToXmlNode(token, state);
            if (commandNode != null)
            {
                XmlNode commandNode2 = commands.OwnerDocument.ImportNode(commandNode, true);
                if (token.UserType == UserType.TradingProxy)
                {
                    ((XmlElement)commandNode2).SetAttribute("CommandSequenceForTradingProxy", command.Sequence.ToString());
                }
                commands.AppendChild(commandNode2);
            }
        }

        private void CleanExpiredCommandHandle(object state)
        {
            this._ReaderWriterLock.EnterWriteLock();
            try
            {
                TimeSpan messageTTL = (TimeSpan)state;
                List<CommandList> expiredCommands = new List<CommandList>(32);
                foreach (CommandList commandList in this._Commands)
                {
                    if (commandList.Count == MessageListCapacity
                        && (DateTime.Now - commandList.LastTime) >= messageTTL)
                    {
                        expiredCommands.Add(commandList);
                    }
                    else
                    {
                        break;
                    }
                }
                foreach (CommandList cmds in expiredCommands)
                {
                    cmds.Clear();
                    this._Commands.Remove(cmds);
                }
                expiredCommands.Clear();
            }
            finally
            {
                this._ReaderWriterLock.ExitWriteLock();
            }
            
        }
    }
}
