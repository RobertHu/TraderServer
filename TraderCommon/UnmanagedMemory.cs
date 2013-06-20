using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Trader.Common
{
    public unsafe class UnmanagedMemory : IDisposable
    {
        public int Count { get; private set; }
        public int Length { get; set; }
        public byte* Handle{get;private set;}
        private bool _Disposed = false;

        public UnmanagedMemory(byte[] keepAliveData)
        {
            this.Data = keepAliveData;
        }

        public UnmanagedMemory(int count)
        {
            this.Handle = (byte*)System.Runtime.InteropServices.Marshal.AllocHGlobal(count);
            this.Count = count;
            this.Length = count;
        }

        public byte[] Data
        {
            get;
            private set;
        }


        public UnmanagedMemory Expand(int count,int length)
        {
            UnmanagedMemory mem = new UnmanagedMemory(count);
            for (int i = 0; i < length; i++)
            {
                mem.Handle[i] = this.Handle[i];
            }
            this.Dispose();
            return mem;
        }


        public void Dispose()
        {
            this.Dispose(true);
        }


        protected virtual void Dispose(bool isDisposing)
        {
            if (this._Disposed)
            {
                return;
            }
            if (isDisposing)
            {
                if (this.Handle != null)
                {
                    System.Runtime.InteropServices.Marshal.FreeHGlobal((IntPtr)this.Handle);
                }
            }
            this._Disposed = true;
        }
        
    }
}
