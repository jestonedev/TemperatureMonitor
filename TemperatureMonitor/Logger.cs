using System;
using System.Collections.Generic;
using System.Text;

namespace TemperatureMonitor
{
    internal class Logger
    {
        public virtual void Write(DateTime date, float temperature)
        {
            // do nothing
        }
    }
}
