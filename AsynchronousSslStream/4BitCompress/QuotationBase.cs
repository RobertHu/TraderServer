using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Trader.Server._4BitCompress
{
    public delegate int InstrumentIdToSequence(Guid id);
    public delegate Guid SequenceToInstrumentId(int sequence);
    
    public class QuotationBase
    {
        public static InstrumentIdToSequence InstrumentIdToSequence;
        public static SequenceToInstrumentId SequenceToInstrumentId;

        public static DateTime OrginTime = new DateTime(2011, 4, 1, 0, 0, 0);
        
        [IgnoreDataMember]
        public Guid InstrumentId
        {
            get;
            set;
        }

        public int InstrumentSequence
        {
            get;
            set;
        }
        
        public string Ask
        {
            get;
            set;
        }

        public string Bid
        {
            get;
            set;
        }
                
        public string High
        {
            get;
            set;
        }
                
        public string Low
        {
            get;
            set;
        }
                
        public DateTime Timestamp
        {
            get;
            set;
        }
                
        public string Open
        {
            get;
            set;
        }

        [DataMember]
        public string PrevClose
        {
            get;
            set;
        }
                
        public string Volume
        {
            get;
            set;
        }
                
        public string TotalVolume
        {
            get;
            set;
        }

        public override string ToString()
        {
            return string.Format("InstrumentId={0}, Ask={1}, Bid={2}, High={3}, Low={4}, Timestamp={5}, Open={6}, PrevClose={7}, Volume={8}, TotalVolume={9}",
                InstrumentId, Ask, Bid, High, Low, Timestamp, Open, PrevClose, Volume, TotalVolume);
        }
    }
}