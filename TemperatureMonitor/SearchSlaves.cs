using System;
using System.Collections.Generic;
using System.Text;

namespace TemperatureMonitor
{
    public class SearchSlaves : IDisposable
    {
        private List<byte[]> IDs = new List<byte[]>();
        private IPort port;
        private int lastDiscrepancy;
        private byte lastSelectedBit;
        private bool done;
        private byte[] currentID = new byte[512];

        public List<byte[]> IdList
        {
            get { return IDs; }
        }

        ///<summary>
        /// Search slaves
        ///</summary>
        ///<param name="p">Port name, example: COM1</param>
        ///<returns></returns>
        public bool Search(IPort p)
        {
            port = p;
            lock (port)
            {
                if (!port.InitializePort())
                    return false;
                lastDiscrepancy = -1;
                lastSelectedBit = 0x00;
                done = true;
                do
                {
                    Run(lastSelectedBit);
                    IDs.Add(port.BitsToBytes(currentID));
                } while (!done);
            }
            return true;
        }

        private void Run(byte selectedBit)
        {
            if (!port.Reset())
                return;
            if (!port.Write((byte)Command.SearchROM))
                return;
            for (var i = 0; i < 512; i++)
            {
                var ab = (byte)(port.ReadBit() == 0xFF ? 0x01 : 0x00);
                ab ^= (byte)(port.ReadBit() == 0xFF ? 0x02 : 0x00);
                if (ab != 0x00)
                {
                    if (ab == 0x01)
                        currentID[i] = 0xFF;
                    if (ab == 0x02)
                        currentID[i] = 0x00;
                    if (!port.WriteBit(currentID[i]))
                        return;
                }
                else
                {
                    done = lastDiscrepancy == i;
                    if (lastDiscrepancy == i)
                    {
                        switch (lastSelectedBit)
                        {
                            case 0x00:
                                selectedBit = 0xFF;
                                break;
                            case 0xFF:
                                selectedBit = 0x00;
                                break;
                        }
                    }
                    currentID[i] = selectedBit;
                    lastDiscrepancy = i;
                    lastSelectedBit = selectedBit;
                    if (!port.WriteBit(selectedBit))
                        return;
                }
            }
        }

        void IDisposable.Dispose()
        {
            port = null;
        }
    }
}
