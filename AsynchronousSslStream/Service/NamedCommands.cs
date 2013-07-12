using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trader.Common;
using CommonUtil;
namespace Trader.Server.Service
{
    public class NamedCommands
    {
        private const string KickoutContent = "<KickoutCommand/>";
        private static object _Lock = new object();
        private static UnmanagedMemory _KickoutPacket;
        public static UnmanagedMemory GetKickoutPacket()
        {
            if (_KickoutPacket != null)
            {
                return _KickoutPacket;
            }
            lock (_Lock)
            {
                if (_KickoutPacket != null)
                {
                    return _KickoutPacket;
                }
                byte[] contentBytes = Constants.ContentEncoding.GetBytes(KickoutContent);
                byte[] contentLengthBytes = contentBytes.Length.ToCustomerBytes();
                byte[] packet = new byte[Constants.HeadCount + contentBytes.Length];
                Buffer.BlockCopy(contentLengthBytes, 0, packet, Constants.ContentLengthIndex, contentLengthBytes.Length);
                Buffer.BlockCopy(contentBytes, 0, packet, Constants.HeadCount, contentBytes.Length);
                _KickoutPacket = new UnmanagedMemory(packet);
                return _KickoutPacket;
            }

        }
    }
}
