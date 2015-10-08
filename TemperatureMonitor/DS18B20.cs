using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TemperatureMonitor
{
    public class DS18B20 : IDisposable
    {
        private byte[] _ID = new byte[8];
        private IPort _port;

        public byte[] ID
        {
            get { return _ID; }
        }

        public DS18B20(IPort port)
        {
            _port = port;
            ConvertT();
        }

        public DS18B20(byte[] id, IPort port)
        {
            _ID = id;
            _port = port;
            ConvertT();
        }

        ///<summary>
        /// Only when there is one slave on the bus
        ///</summary>
        public void ReadID()
        {
            lock (_port)
            {
                if (!_port.InitializePort()) return;
                if (!_port.Reset()) return;
                WriteCommand(Command.ReadROM);
                _ID = Read(8);
            }
        }

        private float ConvertToTemperature(byte MS, byte LS, byte CR, byte CP)
        {
            var negative = false;
            if ((MS & 128) > 0)
            {
                MS = (byte)(0xFF - MS);
                LS = (byte)(0x00 - LS);
                negative = true;
            }
            var temp = ((float)(MS << 8) + LS) / 2 - 0.25f + (float)(CP - CR) / CP;
            temp = negative ? -temp : temp;
            return temp;
        }

        public bool WriteCommand(Command cmd)
        {
            return Write((byte)cmd);
        }

        public bool Write(params byte[] b)
        {
            lock (_port)
            {
                return _port.Write(b);
            }
        }

        public byte[] Read(int num)
        {
            lock (_port)
            {
                return _port.Read(num);
            }
        }

        private bool ConvertT()
        {
            var flag = true;
            lock (_port)
            {
                if (_port.Reset())
                {
                    if (WriteCommand(Command.SkipROM))
                    {
                        flag = WriteCommand(Command.ConvertTemperatureFUN);
                    }
                }
            }
            if (!flag) return false;
            Thread.Sleep(750);
            return true;
        }

        public float ReadTemperature()
        {
            lock (_port)
            {
                if (!ConvertT()) return -999.999f;
                if (!AddressByID()) return -999.999f;
                WriteCommand(Command.ReadScratchPadFUN);
                var buf = Read(9);
                var tmp = ConvertToTemperature(buf[1], buf[0], buf[6], buf[7]);
                return tmp;
            }
        }

        public bool AddressByID()
        {
            lock (_port)
            {
                if (!_port.Reset()) return false;
                if (WriteCommand(Command.MatchROM))
                {
                    return Write(_ID);
                }
            }
            return false;
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            _port = null;
        }

        #endregion
    }

}
