using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trader.Common
{
    public class StringConstants
    {
        public const char ArrayItemSeparator=',';
        public const char Array2DItemSeparator= ';';
        public const string OK_RESULT = "1";
    }

    public class InstrumentConstants
    {
        public const string INSTRUMENT_TABLE_NAME = "Instrument";
        public const string INT_FOR_INTRUMENT_ID_COLUMN_NAME = "SequenceForQuotatoin";
        public const string INSTRUMENT_ID_COLUMN_NAME = "ID";
        public const string COMMAND_SEQUENCE_TABLE_NAME = "CommandSequence";
        public const string COMMAND_SEQUENCE_COLUMN_NAME = "CommandSequenceCol";
    }
}
