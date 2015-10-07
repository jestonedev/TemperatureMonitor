using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TemperatureMonitor
{
    class HttpServer
    {
        TcpListener Listener; // Объект, принимающий TCP-клиентов

        // Запуск сервера
        public HttpServer(int Port)
        {
            // Создаем "слушателя" для указанного порта
            Listener = new TcpListener(IPAddress.Any, Port);
            Listener.Start(); // Запускаем его

            // В бесконечном цикле
            while (true)
            {
                // Принимаем новых клиентов и передаем их на обработку новому экземпляру класса Client
                new HttpClient(Listener.AcceptTcpClient());
            }
        }

        // Остановка сервера
        ~HttpServer()
        {
            // Если "слушатель" был создан
            if (Listener != null)
            {
                // Остановим его
                Listener.Stop();
            }
        }
    }
}
