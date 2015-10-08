using System;
using System.Collections.Generic;
using System.Text;

namespace TemperatureMonitor
{
    public enum Command : byte
    {
        ConvertTemperatureFUN = 0x44,

        WriteScratchPadFUN = 0x4E,

        ReadScratchPadFUN = 0xBE,

        CopyScratchPadFUN = 0x48,

        RecallE2FUN = 0xB8,

        ReadPowerSupplyFUN = 0xB4,

        SearchROM = 0xF0,

        ReadROM = 0x33,

        MatchROM = 0x55,

        SkipROM = 0xCC,

        AlarmSearchROM = 0xEC
    }

}
