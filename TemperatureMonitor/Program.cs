using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Configuration;

namespace TemperatureMonitor
{
    class Program
    {
        private static SerialPort port = null;

        public static Queue<List<Dictionary<string, object>>> Temperatures { get; set; }

        static void Main(string[] args)
        {
            using (var com2 = new COMPort(ConfigurationSettings.AppSettings["sensorPort"]))
            {
                try
                {
                    var ss = new SearchSlaves();
                    ss.Search(com2);
                    DS18B20 ds18b20 = null;
                    var n = 0;
                    foreach (byte[] id in ss.IdList)
                    {
                        var buf = new byte[8];
                        for (var i = 0; i < 8; i++)
                        {
                            buf[i] = id[i];
                        }
                        ds18b20 = new DS18B20(buf, com2);
                    }
                    if (ds18b20 == null)
                    {
                        Console.WriteLine("Не удалось обнаружить датчик температуры DS18B20");
                        return;
                    }
                    var lastNotification = DateTime.Now.AddMinutes(0 - Configuration.AlertFrequency);
                    var smtp = new System.Net.Mail.SmtpClient(Configuration.SmtpHost, Configuration.SmtpPort);
                    if (Configuration.HasWebMonitor)
                    {
                        var thread = new Thread(() =>
                        {
                            new HttpServer(Configuration.WebMonitorPort);
                        });
                        thread.Start();
                    }
                    Temperatures = new Queue<List<Dictionary<string, object>>>(11);
                    while (true)
                    {
                        var temperature = (float) Math.Round(ds18b20.ReadTemperature(), 2);
                        Temperatures.Enqueue(
                            new List<Dictionary<string, object>>()
                            {
                                new Dictionary<string, object>()
                                {
                                    { "date", DateTime.Now },
                                    { "temperature", temperature }
                                }
                            });
                        if (Temperatures.Count > 10)
                            Temperatures.Dequeue();
                        Console.WriteLine(temperature);
                        if (temperature >= Configuration.CriticalTemperatureForAlert &&
                            lastNotification <= DateTime.Now.AddMinutes(0 - Configuration.AlertFrequency))
                        {
                            lastNotification = DateTime.Now;
                            foreach (var email in Configuration.EmailsForAlert)
                                smtp.Send(Configuration.SmtpFrom, email, Configuration.SmtpSubject,
                                    string.Format(Configuration.SmtpBody, temperature));
                        }
                        Thread.Sleep(Configuration.TemperatureReadTimeout*1000 - 750);
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
