using System;
using System.Collections.Generic;
using System.Text;

namespace TemperatureMonitor
{
    public interface IPort
    {
        bool InitializePort();
        bool Reset();
        byte[] ByteToBits(byte b);
        byte[] BitsToBytes(byte[] bits);
        bool Write(params byte[] bytes);
        bool WriteBit(byte bit);
        byte[] Read(int length);
        byte ReadBit();
        void Dispose();
    }

}
