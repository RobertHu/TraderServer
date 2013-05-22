using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
namespace Serialization
{
    public class SerializeManager
    {
        private SerializeManager() { }

        public static readonly SerializeManager Default = new SerializeManager();

        public byte[] Serialize(SerializedObject target)
        {
            return PacketBuilder.Build(target);
        }


        public byte[] SerializePrice(byte[] price)
        {
            return PacketBuilder.BuildPrice(price);
        }

        public byte[] SerializeCommand(byte[] content)
        {
            return PacketBuilder.BuildForContentInBytesCommand(content);
        }

        public SerializedObject Deserialze(byte[] packet)
        {
            return PacketParser.Parse(packet);
        }

    }
}
