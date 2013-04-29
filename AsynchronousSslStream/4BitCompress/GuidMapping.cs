using System;
using System.Collections.Generic;
using System.Linq;

namespace AsyncSslServer._4BitCompress
{
    internal class GuidMapping
    {
        internal static GuidMapping InstrumentIdMapping = new GuidMapping(256);

        private object _Lock = new object();
        private int _NextSequence = 0;
        private Guid[] _Guids;
        private Dictionary<Guid, int> _Sequenes = new Dictionary<Guid, int>();

        static GuidMapping()
        {
            QuotationBase.InstrumentIdToSequence = delegate(Guid id)
            {
                return GuidMapping.InstrumentIdMapping.AddOrGetExisting(id);
            };
        }

        private GuidMapping(int maxCapacity)
        {
            this._Guids = new Guid[maxCapacity];
        }

        internal int AddOrGetExisting(Guid id)
        {
            lock (this._Lock)
            {
                int sequence = this._NextSequence;
                if (this._Sequenes.TryGetValue(id, out sequence))
                {
                    return sequence;
                }
                else
                {
                    this._Guids[this._NextSequence] = id;
                    this._Sequenes.Add(id, this._NextSequence);
                    return this._NextSequence++;
                }
            }
        }

        internal void Clear()
        {
            lock (this._Lock)
            {
                this._Sequenes.Clear();
                this._NextSequence = 0;
            }
        }

        internal bool TryGetSequence(Guid id, out int sequence)
        {
            lock (this._Lock)
            {
                return this._Sequenes.TryGetValue(id, out sequence);
            }
        }

        internal bool TryGetGuid(int sequence, out Guid? id)
        {
            lock (this._Lock)
            {
                id = null;
                if (sequence >= 0 && sequence < this._Guids.Length)
                {
                    id = this._Guids[sequence];
                }

                return id != null;
            }
        }
    }
}