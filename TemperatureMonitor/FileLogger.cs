using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TemperatureMonitor
{
    internal sealed class FileLogger: Logger
    {
        public override void Write(DateTime date, float temperature)
        {
            try
            {
                using (var sw = new StreamWriter(Configuration.LogFileName, true))
                    sw.WriteLine("{0}: {1}", date.ToString("dd.MM.yyyy hh:mm:ss"), temperature);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}: Не удалось записать лог. {1}", DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss"), e.Message);
            }
        }
    }
}
