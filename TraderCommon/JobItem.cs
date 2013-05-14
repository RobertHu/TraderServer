using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Serialization;
using System.Xml.Linq;

namespace Trader.Common
{
    public enum JobType
    {
        None,
        Price,
        Transaction
    }

    public class JobItem
    {
        public JobItem() { }
        public JobItem(SerializedObject target)
        {
            this.Type = target.IsPrice ? JobType.Price : JobType.Transaction;
            this.Price = target.Price;
            this.SessionID = target.Session;
            this.ClientInvokeID = target.ClientInvokeID;
            this.Content = target.Content;
            this.ContentInByte = target.ContentInByte;
        }
        public static JobItem Empty = new JobItem();
        public JobType Type { get; set; }
        public byte[] Price{ get; set; }
        public XElement Content{ get; set; }
        public byte[] ContentInByte { get; set; }
        public Guid? SessionID{ get; set; }
        public string ClientInvokeID{ get; set; }
    }
}
