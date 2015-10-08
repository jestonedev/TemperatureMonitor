using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TemperatureMonitor
{
    class HttpClient
    {
        // Конструктор класса. Ему нужно передавать принятого клиента от TcpListener
        public HttpClient(TcpClient Client)
        {
            var Request = "";
            // Буфер для хранения принятых от клиента данных
            var Buffer = new byte[1024];
            // Переменная для хранения количества байт, принятых от клиента
            int Count;
            // Читаем из потока клиента до тех пор, пока от него поступают данные
            try
            {
                while ((Count = Client.GetStream().Read(Buffer, 0, Buffer.Length)) > 0)
                {
                    // Преобразуем эти данные в строку и добавим ее к переменной Request
                    Request += Encoding.ASCII.GetString(Buffer, 0, Count);
                    // Запрос должен обрываться последовательностью \r\n\r\n
                    // Либо обрываем прием данных сами, если длина строки Request превышает 4 килобайта
                    if (Request.IndexOf("\r\n\r\n") >= 0 || Request.Length > 4096)
                        break;
                }
            }
            catch (IOException)
            {
                return;
            }
            // Парсим строку запроса с использованием регулярных выражений
            // При этом отсекаем все переменные GET-запроса
            var ReqMatch = Regex.Match(Request, @"^\w+\s+([^\s]+)[^\s]*\s+HTTP/.*|");

            // Если запрос не удался
            if (ReqMatch == Match.Empty)
            {
                // Передаем клиенту ошибку 400 - неверный запрос
                SendError(Client, 400);
                return;
            }

            var temperatureRow = @"<div class=""{2}""><span class=""time"">{0}</span><span class=""temperature"">{1:F2} <sup>o</sup>C</span></div>";
            var temperatureTable = "";
            foreach (var temperature in Program.Temperatures)
            {
                var row_class = "row";
                if (((float) temperature["temperature"]) >= Configuration.CriticalTemperatureForAlert)
                    row_class = "row alert";
                temperatureTable += string.Format(temperatureRow, temperature["date"], temperature["temperature"], row_class);
            }
            var style = ".time { padding-right: 10px; font-weight: bold; } .alert { color: red; }";
            var body = string.Format("<html><style>{1}</style><body>{0}</body></html>", temperatureTable, style);

            // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
            var Str = "HTTP/1.1 200 OK\nAccess-Control-Allow-Origin: *\nContent-type: text/html\nContent-Length:" + body.Length.ToString() + "\n\n" + body;
            // Приведем строку к виду массива байт
            Buffer = Encoding.ASCII.GetBytes(Str);
            try
            {
                // Отправим его клиенту
                Client.GetStream().Write(Buffer, 0, Buffer.Length);
                // Закроем соединение
                Client.Close();
            }
            catch (IOException)
            {
                return;
            }
        }

        // Отправка страницы с ошибкой
        private void SendError(TcpClient Client, int Code)
        {
            // Получаем строку вида "200 OK"
            // HttpStatusCode хранит в себе все статус-коды HTTP/1.1
            var CodeStr = Code.ToString() + " " + ((HttpStatusCode)Code).ToString();
            // Код простой HTML-странички
            var Html = "<html><body><h1>" + CodeStr + "</h1></body></html>";
            // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
            var Str = "HTTP/1.1 " + CodeStr + "\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;
            // Приведем строку к виду массива байт
            var Buffer = Encoding.ASCII.GetBytes(Str);
            try
            {
                // Отправим его клиенту
                Client.GetStream().Write(Buffer, 0, Buffer.Length);
                // Закроем соединение
                Client.Close();
            }
            catch (IOException)
            {
            }
        }
    }
}
