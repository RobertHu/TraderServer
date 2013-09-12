using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime;
using System.Security;
using System.Runtime.InteropServices;
using Trader.Common;

namespace Trader.Common
{
    public class TraderMemoryStream : Stream
    {
        private int _capacity;
        private bool _expandable;
        private bool _exposable;
        private bool _isOpen;
        private int _length;
        private int _origin;
        private int _position;
        private bool _writable;
        private const int MemStreamMaxLength = 0x7fffffff;
        private UnmanagedMemory _buffer;
        
        
        public TraderMemoryStream() : this(0)
        {
        }
        
        public TraderMemoryStream(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException("capacity", "ArgumentOutOfRange_NegativeCapacity");
            }
            this._buffer = new UnmanagedMemory(capacity);
            this._capacity = capacity;
            this._expandable = true;
            this._writable = true;
            this._exposable = true;
            this._origin = 0;
            this._isOpen = true;
        }
        
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this._isOpen = false;
                    this._writable = false;
                    this._expandable = false;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        
        private bool EnsureCapacity(int value)
        {
            if (value < 0)
            {
                throw new IOException("IO.IO_StreamTooLong");
            }
            if (value <= this._capacity)
            {
                return false;
            }
            int num = value;
            if (num < 0x100)
            {
                num = 0x100;
            }
            if (num < (this._capacity * 2))
            {
                num = this._capacity * 2;
            }
            this.Capacity = num;
            return true;
        }
        
        public override void Flush()
        {
        }
        
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual byte[] GetBuffer()
        {
            throw new NotImplementedException();
        }

        internal int InternalEmulateRead(int count)
        {
            if (!this._isOpen)
            {
                //__Error.StreamIsClosed();
            }
            int num = this._length - this._position;
            if (num > count)
            {
                num = count;
            }
            if (num < 0)
            {
                num = 0;
            }
            this._position += num;
            return num;
        }
        
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal byte[] InternalGetBuffer()
        {
            throw new NotImplementedException(); ;
        }
        
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal void InternalGetOriginAndLength(out int origin, out int length)
        {
            if (!this._isOpen)
            {
               // __Error.StreamIsClosed();
            }
            origin = this._origin;
            length = this._length;
        }
        
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal int InternalGetPosition()
        {
            if (!this._isOpen)
            {
               // __Error.StreamIsClosed();
            }
            return this._position;
        }
        
        internal int InternalReadInt32()
        {
            throw new NotImplementedException(); ;
          
        }
        
        protected override void ObjectInvariant()
        {
        }
        
        [SecuritySafeCritical]
        public override int Read([In, Out] byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException(); ;
        }
        
        public override int ReadByte()
        {
            throw new NotImplementedException(); ;
        }
        
        public override long Seek(long offset, SeekOrigin loc)
        {
            throw new NotImplementedException(); ;
        }
        
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
        
        [SecuritySafeCritical]
        public virtual byte[] ToArray()
        {
            long count = this.Length - this._origin;
            byte[] dst = new byte[count];
            unsafe
            {
                Marshal.Copy((IntPtr)(this._buffer.Handle + this._origin), dst, 0, (int)count); 
            }
            return dst;
        }
        
        [SecuritySafeCritical]
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", "ArgumentNull_Buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset","ArgumentOutOfRange_NeedNonNegNum");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "ArgumentOutOfRange_NeedNonNegNum");
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException("Argument_InvalidOffLen");
            }
            if (!this._isOpen)
            {
                //__Error.StreamIsClosed();
            }
            if (!this.CanWrite)
            {
                //__Error.WriteNotSupported();
            }
            int num = this._position + count ;
            if (num < 0)
            {
                throw new IOException("IO.IO_StreamTooLong");
            }
            if (num > this._length)
            {
                bool flag = this._position > this._length;
                if ((num > this._capacity) && this.EnsureCapacity(num))
                {
                    flag = false;
                }
                if (flag)
                {
                    //Array.Clear(this._buffer, this._length, num - this._length);
                }
                this._length = num;
            }
            unsafe
            {
                Marshal.Copy(buffer, offset, (IntPtr)(this._buffer.Handle + this._position), count);
            }
            this._position = num;
        }
        
        public override void WriteByte(byte value)
        {
            if (!this._isOpen)
            {
                //__Error.StreamIsClosed();
            }
            if (!this.CanWrite)
            {
               // __Error.WriteNotSupported();
            }
            if (this._position >= this._length)
            {
                int num = this._position + 1;
                bool flag = this._position > this._length;
                if ((num >= this._capacity) && this.EnsureCapacity(num))
                {
                    flag = false;
                }
                if (flag)
                {
                    //Array.Clear(this._buffer, this._length, this._position - this._length);
                }
                this._length = num;
            }
            unsafe
            {
                this._buffer.Handle[this._position++] = value;
            }
        }
        
        public virtual void WriteTo(Stream stream)
        {
            throw new NotImplementedException();
        }
        
        public override bool CanRead
        {
            get
            {
                return this._isOpen;
            }
        }
        
        public override bool CanSeek
        {
            get
            {
                return this._isOpen;
            }
        }
        
        public override bool CanWrite
        {
            get
            {
                return this._writable;
            }
        }
        
        public virtual int Capacity
        {
            get
            {
                if (!this._isOpen)
                {
                    //__Error.StreamIsClosed();
                }
                return (this._capacity - this._origin);
            }
            [SecuritySafeCritical]
            set
            {
                if (value < this.Length)
                {
                    throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_SmallCapacity");
                }
                if (!this._isOpen)
                {
                   // __Error.StreamIsClosed();
                }
                if (!this._expandable && (value != this.Capacity))
                {
                    //__Error.MemoryStreamNotExpandable();
                }
                if (this._expandable && (value != this._capacity))
                {
                    if (value > 0)
                    {
                        if (this._length > 0)
                        {
                           this._buffer.Expand(value);
                        }
                    }
                    else
                    {
                        this._buffer = null;
                    }
                    this._capacity = value;
                }
            }
        }
        
        public override long Length
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (!this._isOpen)
                {
                    //__Error.StreamIsClosed();
                }
                return (long) (this._length - this._origin);
            }
        }

        public UnmanagedMemory Buffer
        {
            get
            {
                this._buffer.SetLength((int)this.Length);
                return this._buffer;
            }
        }
        
        public override long Position
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (!this._isOpen)
                {
                   // __Error.StreamIsClosed();
                }
                return (long) (this._position - this._origin);
            }
            set
            {
                if (value < 0L)
                {
                    throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_NeedNonNegNum");
                }
                if (!this._isOpen)
                {
                    //__Error.StreamIsClosed();
                }
                if (value > 0x7fffffffL)
                {
                    throw new ArgumentOutOfRangeException("value", "ArgumentOutOfRange_StreamLength");
                }
                this._position = this._origin + ((int) value);
            }
        }
    }
}
