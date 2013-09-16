namespace Trader.Common
{
    public struct ReceiveData
    {
        private readonly Session _ClientID;
        private readonly byte[] _Data;
        public Session ClientID
        {
            get { return _ClientID; }
        }
        public byte[] Data
        {
            get { return _Data; }
        }
        public ReceiveData(Session clientId, byte[] data)
        {
            _ClientID = clientId;
            _Data = data;
        }
    }
}
