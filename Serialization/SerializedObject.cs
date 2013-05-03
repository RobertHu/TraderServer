using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Serialization
{
    public class SerializedObject
    {

        public SerializedObject(byte[] price) : this(true, string.Empty, price,string.Empty) { }
        public SerializedObject(bool isPrice, byte[] price) : this(isPrice, string.Empty, price,string.Empty) { }

        public SerializedObject(bool isPrice,string session,byte[] price,string clientInvokeID)
        {
            this.IsPrice = isPrice;
            this.Session = session;
            this.Price = price;
            this.ClientInvokeID = clientInvokeID;
        }

        public SerializedObject(XmlNode content, string clientInvokeId):this(clientInvokeId:clientInvokeId,content:content,session:"") { }
        public SerializedObject(byte[] contentInBytes, string clientInvokeId) : this(contentInBytes: contentInBytes, clientInvokeId: clientInvokeId,session:"") { }


        public SerializedObject(byte[] contentInBytes=null, string clientInvokeId = "", string session = "", XmlNode content = null,bool isPrice=false)
        {
            this.ContentInByte = contentInBytes;
            this.ClientInvokeID = clientInvokeId;
            this.Session = session;
            this.Content = content;
            this.IsPrice = isPrice;
        }
       
        public string Session { get;  set; }
        public bool IsPrice { get; private set; }
        public byte[] Price { get; private set; }
        public XmlNode Content { get;  set; }
        public byte[] ContentInByte { get; set; }
        public string ClientInvokeID { get; private set; }
        public string CurrentSession { get; set; }

    }
}
