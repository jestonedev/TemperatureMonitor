using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace TemperatureMonitor
{
    internal sealed class DbLogger : Logger
    {
        public override void Write(DateTime date, float temperature)
        {
            try
            {
                var factory = DbProviderFactories.GetFactory(ParseProviderName(Configuration.LogDbProvider));
                var connection = factory.CreateConnection();
                connection.ConnectionString = Configuration.LogDbConnectionString;
                var command = factory.CreateCommand();
                // Don't use parameters, because they different for different providers
                command.CommandText = Configuration.LogDbCommand;
                var dateParameter = factory.CreateParameter();
                dateParameter.ParameterName = "date";
                dateParameter.Value = date;
                command.Parameters.Add(dateParameter);
                var temperatureParameter = factory.CreateParameter();
                temperatureParameter.ParameterName = "temperature";
                temperatureParameter.Value = temperature;
                command.Parameters.Add(temperatureParameter);
                command.Connection = connection;
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}: Не удалось записать лог. {1}", DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss"), e.Message);
            }
        }

        private static string ParseProviderName(string name)
        {
            var dt = DbProviderFactories.GetFactoryClasses();
            var providers = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                providers.Add(row["InvariantName"].ToString());
            }
            foreach (string provider in providers)
            {
                if (Regex.IsMatch(provider, name, RegexOptions.IgnoreCase))
                    return provider;
            }
            return ParseProviderName("ODBC");
        }
    }
}
