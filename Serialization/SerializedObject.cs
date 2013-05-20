using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Trader.Common;

namespace Serialization
{
    public class SerializedObject
    {
        
        public SerializedObject(byte[] price) : this(true, null, price,string.Empty) { }
        public SerializedObject(bool isPrice, byte[] price) : this(isPrice, null, price,string.Empty) { }

        public SerializedObject(bool isPrice,long? session,byte[] price,string clientInvokeID)
        {
            this.IsPrice = isPrice;
            this.Session = session;
            this.Price = price;
            this.ClientInvokeID = clientInvokeID;
        }

        public SerializedObject(XElement content, string clientInvokeId):this(clientInvokeId:clientInvokeId,content:content,session:null) { }
        public SerializedObject(byte[] contentInBytes, string clientInvokeId) : this(contentInBytes: contentInBytes, clientInvokeId: clientInvokeId,session:null) { }


        public SerializedObject(byte[] contentInBytes = null, string clientInvokeId = "", long? session = null, XElement content = null, bool isPrice = false)
        {
            this.ContentInByte = contentInBytes;
            this.ClientInvokeID = clientInvokeId;
            this.Session = session;
            this.Content = content;
            this.IsPrice = isPrice;
        }
        protected SerializedObject() { }
        public static SerializedObject Create(long? session, bool isKeepAlive, byte[] keepAlivePacket)
        {
            SerializedObject target = new SerializedObject();
            target.IsKeepAlive = isKeepAlive;
            target.KeepAlivePacket = keepAlivePacket;
            target.Session = session;
            return target;
        }
        public bool IsPrice { get; set; }
        public byte[] Price { get; set; }
        public XElement Content { get;  set; }
        public byte[] ContentInByte { get; set; }
        public string ClientInvokeID { get; private set; }
        public long? CurrentSession { get; set; }
        public bool IsKeepAlive { get; private set; }
        public bool IsKeepAliveSuccess { get;set; }
        public byte[] KeepAlivePacket { get; private set; }
        public long? Session { get; set; }

    }
}
