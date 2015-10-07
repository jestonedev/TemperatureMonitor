using System;
using System.Configuration;

namespace TemperatureMonitor
{
    class Configuration
    {
        public static int TemperatureReadTimeout
        {
            get
            {
                int temperatureReadTimeout;
                if (!int.TryParse(ConfigurationSettings.AppSettings["temperatureReadTimeout"],
                    out temperatureReadTimeout))
                    temperatureReadTimeout = 5; // По умолчанию 5 секунд
                temperatureReadTimeout = Math.Abs(temperatureReadTimeout);
                return temperatureReadTimeout;
            }
        }

        public static float CriticalTemperatureForAlert
        {
            get
            {
                float criticalTemperatureForAlert;
                if (!float.TryParse(ConfigurationSettings.AppSettings["criticalTemperatureForAlert"],
                        out criticalTemperatureForAlert))
                    criticalTemperatureForAlert = 50; // По умолчанию 50 градусов
                return criticalTemperatureForAlert;
            }
        }

        public static float AlertFrequency
        {
            get
            {
                int alertFrequency;
                if (!int.TryParse(ConfigurationSettings.AppSettings["alertFrequency"],
                        out alertFrequency))
                    alertFrequency = 30; // По умолчанию 30 минут
                alertFrequency = Math.Abs(alertFrequency);
                return alertFrequency;
            }
        }

        public static string SmtpHost
        {
            get { return ConfigurationSettings.AppSettings["smtpHost"]; }
        }

        public static int SmtpPort
        {
            get
            {
                int smtpPort;
                if (!int.TryParse(ConfigurationSettings.AppSettings["smtpPort"],
                        out smtpPort))
                    smtpPort = 25; // По умолчанию 25 порт
                smtpPort = Math.Abs(smtpPort);
                return smtpPort;
            }
        }

        public static string[] EmailsForAlert
        {
            get
            {
                return ConfigurationSettings.AppSettings["emailsForAlert"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public static string SmtpSubject
        {
            get { return ConfigurationSettings.AppSettings["smtpSubject"]; }
        }

        public static string SmtpBody
        {
            get { return ConfigurationSettings.AppSettings["smtpBody"]; }
        }

        public static string SmtpFrom
        {
            get { return ConfigurationSettings.AppSettings["smtpFrom"]; }
        }

        public static bool HasWebMonitor
        {
            get
            {
                bool hasWebMonitor;
                if (!bool.TryParse(ConfigurationSettings.AppSettings["hasWebMonitor"], out hasWebMonitor))
                    hasWebMonitor = false;  // По умолчанию нет
                return hasWebMonitor;
            }
        }

        public static int WebMonitorPort
        {
            get
            {
                int webMonitorPort;
                if (!int.TryParse(ConfigurationSettings.AppSettings["webMonitorPort"],
                    out webMonitorPort))
                    webMonitorPort = 33333; // По умолчанию 33333 порт
                webMonitorPort = Math.Abs(webMonitorPort);
                return webMonitorPort;
            }
        }
    }
}
