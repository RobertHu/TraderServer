using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Trader.Common
{
    public unsafe class UnmanagedMemory
    {
        public int Count { get; private set; }
        public int Length { get; private set; }
        public byte* Handle{get;private set;}
        private bool _Disposed;
        public byte[] Data { get; private set; }

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


        public void Expand(int count)
        {
            this.Handle = (byte*)Marshal.ReAllocHGlobal((IntPtr)this.Handle, (IntPtr)count);
        }

        public void SetLength(int length)
        {
            this.Length = length;
        }

        public byte[] ToArray()
        {
            byte[] target = new byte[this.Length];
            Marshal.Copy((IntPtr)this.Handle, target, 0, this.Length);
            return target;
        }

        public void Dispose()
        {
            if (this._Disposed)
            {
                return;
            }
            if (this.Handle != null)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal((IntPtr)this.Handle);
            }
            this._Disposed = true;
        }

    }
}
