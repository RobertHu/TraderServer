using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Serialization
{
    public class SerializedObject
    {

        public SerializedObject(byte[] price) : this(true, null, price,string.Empty) { }
        public SerializedObject(bool isPrice, byte[] price) : this(isPrice, null, price,string.Empty) { }

        public SerializedObject(bool isPrice,Guid? session,byte[] price,string clientInvokeID)
        {
            this.IsPrice = isPrice;
            this.Session = session;
            this.Price = price;
            this.ClientInvokeID = clientInvokeID;
        }

        public SerializedObject(XElement content, string clientInvokeId):this(clientInvokeId:clientInvokeId,content:content,session:null) { }
        public SerializedObject(byte[] contentInBytes, string clientInvokeId) : this(contentInBytes: contentInBytes, clientInvokeId: clientInvokeId,session:null) { }


        public SerializedObject(byte[] contentInBytes = null, string clientInvokeId = "", Guid? session = null, XElement content = null, bool isPrice = false)
        {
            this.ContentInByte = contentInBytes;
            this.ClientInvokeID = clientInvokeId;
            this.Session = session ?? Guid.Empty;
            this.Content = content;
            this.IsPrice = isPrice;
        }
       
        public Guid? Session { get;  set; }
        public bool IsPrice { get; private set; }
        public byte[] Price { get; private set; }
        public XElement Content { get;  set; }
        public byte[] ContentInByte { get; set; }
        public string ClientInvokeID { get; private set; }
        public Guid? CurrentSession { get; set; }

    }
}
