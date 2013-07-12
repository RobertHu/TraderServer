using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Trader.Common;
namespace Trader.Server.Serialization
{
    public class SerializeManager
    {
        private SerializeManager() { }

        public static readonly SerializeManager Default = new SerializeManager();

        public UnmanagedMemory Serialize(SerializedObject target)
        {
            return PacketBuilder.Build(target);
        }


        public UnmanagedMemory SerializePrice(byte[] price)
        {
            return PacketBuilder.BuildPrice(price);
        }

        public UnmanagedMemory SerializeCommand(byte[] content)
        {
            return PacketBuilder.BuildForContentInBytesCommand(content);
        }

        public SerializedObject Deserialze(byte[] packet)
        {
            return PacketParser.Parse(packet);
        }

    }
}
