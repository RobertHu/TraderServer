namespace Trader.Common
{
    public static class FirstHeadByteBitConstants
    {
        public const byte IsPricevValue = 0x01;
        public const byte IsKeepAliveMask = 0x02;
        public const byte IsKeepAliveAndSuccessValue = 0x06;
        public const byte IsKeepAliveAndFailedValue = 0x02;
        public const byte IsPlainString = 0x08;
    }
}