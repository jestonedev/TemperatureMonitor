using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace TemperatureMonitor
{
    public class COMPort : IPort, IDisposable
    {
        private SerialPort sp;
        private string portName = "COM1";
        private bool init;

        ///<summary>
        /// By default used port is COM1
        ///</summary>
        public COMPort()
        {
        }

        ///<summary>
        /// By default used port is COM1
        ///</summary>
        ///<param name="pName">Port name for example: COM1</param>
        public COMPort(string pName)
        {
            if (!string.IsNullOrEmpty(pName))
                portName = pName;
        }

        private SerialPort Port
        {
            get { return sp ?? (sp = new SerialPort(portName)); }
        }

        private bool IsInitialized
        {
            get { return init; }
        }

        ///<summary>
        /// By default ReadTimeout=3000ms;BaudRate=9600;Dtr=Enable
        ///</summary>
        ///<returns></returns>
        public bool InitializePort()
        {
            try
            {
                if (!Port.IsOpen)
                    Port.Open();
                Port.ReadTimeout = 3000;
                Port.BaudRate = 9600;
                Port.DiscardInBuffer();
                Port.DiscardOutBuffer();
                if (!Port.DtrEnable)
                    Port.DtrEnable = true;
                init = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " : " +portName);
            }
            return false;
        }

        public bool Reset()
        {
            try
            {
                if (!IsInitialized)
                    InitializePort();
                Port.BaudRate = 9600;
                Port.Write(new byte[1] {0xF0}, 0, 1);
                while (Port.ReadByte() == 0xF0) ;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Port.Close();
                return false;
            }
            Port.BaudRate = 115200;
            return true;
        }

        public byte[] ByteToBits(byte b)
        {
            var bits = new byte[8];
            byte and = 128;
            for (var i = 7; i >= 0; i--)
            {
                bits[i] = (b & and) == 0x00 ? (byte) 0x00 : (byte) 0xFF;
                and /= 2;
            }
            return bits;
        }

        public byte[] BitsToBytes(byte[] b)
        {
            var result = new byte[b.Length/8];
            for (var n = 0; n < result.Length; n++)
            {
                byte and = 128;
                for (var i = 7; i >= 0; i--)
                {
                    result[n] ^= b[n*8 + i] == (byte) 0xFF ? and : (byte) 0x00;
                    and /= 2;
                }
            }
            return result;
        }

        public bool Write(params byte[] bytes)
        {
            try
            {
                foreach (var b in bytes)
                {
                    var bufer = ByteToBits(b);
                    for (var i = 0; i < 8; i++)
                    {
                        WriteBit(bufer[i]);
                    }
                }
                return true;
            }
            catch
            {
            }
            return false;
        }

        public bool WriteBit(byte bit)
        {
            try
            {
                var bufer = new byte[1] {bit};
                Port.Write(bufer, 0, 1);
                while (Port.ReadByte() != bufer[0]) ;
                return true;
            }
            catch
            {
            }
            return false;
        }

        public byte[] Read(int length)
        {
            var rbuf = new byte[length*8];
            for (var n = 0; n < length; n++)
            {
                for (var i = 0; i < 8; i++)
                {
                    rbuf[n*8 + i] = ReadBit();
                }
            }
            return BitsToBytes(rbuf);
        }

        public byte ReadBit()
        {
            byte bit;
            Port.Write(new byte[] {0xFF}, 0, 1);
            bit = Port.ReadByte() != 0xFF ? (byte) 0x00 : (byte) 0xFF;
            return bit;
        }

        public void Dispose()
        {
            Port.Close();
            Port.Dispose();
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            Dispose();
        }

        #endregion
    }
}