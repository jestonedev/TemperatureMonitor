using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Configuration;
using System.Diagnostics;

namespace TemperatureMonitor
{
    class Program
    {
        private static SerialPort port = null;

        public static Queue<Dictionary<string, object>> Temperatures { get; set; }

        static void Main(string[] args)
        {
            using (var com2 = new COMPort(ConfigurationSettings.AppSettings["sensorPort"]))
            {
                try
                {
                    // Device initialization
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
                    // SMTP initialization
                    var smtp = new System.Net.Mail.SmtpClient(Configuration.SmtpHost, Configuration.SmtpPort);
                    // WebMonitor initialization
                    if (Configuration.HasWebMonitor)
                    {
                        var thread = new Thread(() =>
                        {
                            new HttpServer(Configuration.WebMonitorPort);
                        });
                        thread.Start();
                    }
                    // Log initialization
                    var logger = new Logger();
                    switch (Configuration.Log.ToLower())
                    {
                        case "file":
                            logger = new FileLogger();
                            break;
                        case "database":
                            logger = new DbLogger();
                            break;
                    }

                    // Performance counter initialization
                    PerformanceCounter counter = null;
                    try
                    {
                        if (Configuration.UsePerformanceCounter)
                        {
                            if (!PerformanceCounterCategory.Exists("Temperature of DS18B20 sensor"))
                            {
                                var counterDataCollection = new CounterCreationDataCollection();
                                var counterData = new CounterCreationData
                                {
                                    CounterName = "Current temperature",
                                    CounterHelp = "Current temperature of DS18B20 sensor",
                                    CounterType = PerformanceCounterType.NumberOfItems64
                                };
                                counterDataCollection.Add(counterData);
                                PerformanceCounterCategory.Create("Temperature of DS18B20 sensor",
                                    "Temperature of DS18B20 sensor",
                                    PerformanceCounterCategoryType.SingleInstance, counterDataCollection);
                            }
                            counter = new PerformanceCounter
                            {
                                CategoryName = "Temperature of DS18B20 sensor",
                                CounterName = "Current temperature",
                                MachineName = ".",
                                ReadOnly = false
                            };
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Не удалось инициализировать счетчик производительности");
                    }

                    // Other initializations
                    var lastNotification = DateTime.Now.AddMinutes(0 - Configuration.AlertFrequency);
                    Temperatures = new Queue<Dictionary<string, object>>(11);
                    
                    // Reading temperature
                    while (true)
                    {
                        var temperature = (float) Math.Round(ds18b20.ReadTemperature(), 2);
                        var now = DateTime.Now;
                        Temperatures.Enqueue(                           
                                new Dictionary<string, object>()
                                {
                                    { "date", now },
                                    { "temperature", temperature }
                                });
                        if (counter != null)
                            counter.RawValue = (long)temperature;
                        if (Temperatures.Count > 10)
                            Temperatures.Dequeue();
                        Console.WriteLine("{0}: {1}", now.ToString("dd.MM.yyyy hh:mm:ss"), temperature);
                        // Write log
                        logger.Write(now, temperature);
                        // Send email notification
                        if (temperature >= Configuration.CriticalTemperatureForAlert &&
                            lastNotification <= now.AddMinutes(0 - Configuration.AlertFrequency))
                        {
                            lastNotification = now;
                            try
                            {
                                foreach (var email in Configuration.EmailsForAlert)
                                    smtp.Send(Configuration.SmtpFrom, email, Configuration.SmtpSubject,
                                        string.Format(Configuration.SmtpBody, temperature));
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
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
