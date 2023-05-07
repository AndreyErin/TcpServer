using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace TcpClientWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // слова для отправки для получения перевода
            var words = new string[] { "red", "yellow", "blue" };

            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                //ПОДКЛЮЧЕНИЕ-------------------------------------
                await socket.ConnectAsync("127.0.0.1", 8888);
                // буфер для входящих данных
                var bufferForGet = new List<byte>();
                foreach (var word in words)
                {
                    // считыванием строку в массив байт
                    // при отправке добавляем маркер завершения сообщения - \n
                    byte[] data = Encoding.UTF8.GetBytes(word + '\n');
                    // отправляем данные
                    //ОТПРАВКА-----------------------------------------------
                    await socket.SendAsync(data, SocketFlags.None);

                   // txtStatConnectClient.Text += "ОТПРАВКА ДАННЫХ\n";

                    // буфер для считывания одного байта
                    var bytesRead = new byte[1];
                    // считываем данные до конечного символа
                    while (true)
                    {
                        //ПОЛУЧЕНИЕ----------------------------------------
                        var count = await socket.ReceiveAsync(bytesRead, SocketFlags.None);
                       // txtStatConnectClient.Text += "ПОЛУЧЕНИЕ ДАННЫХ\n";

                        // смотрим, если считанный байт представляет конечный символ, выходим
                        if (count == 0 || bytesRead[0] == '\n') break;
                        // иначе добавляем в буфер
                        bufferForGet.Add(bytesRead[0]);
                    }
                    var translation = Encoding.UTF8.GetString(bufferForGet.ToArray());
                    txtStatConnectClient.Text += $"Слово {word}: {translation}\n";
                    bufferForGet.Clear();
                    // имитируем долговременную работу, чтобы одновременно несколько клиентов обрабатывались
                    //await Task.Delay(2000);
                }
                //ОТКЛЮЧАЕМСЯ ОТ СЕРВЕРА-------------------------
                // отправляем маркер завершения подключения - END
                await socket.SendAsync(Encoding.UTF8.GetBytes("END\n"), SocketFlags.None);

            }
            catch (SocketException ex)
            {
                txtStatConnectClient.Text += (ex.Message + "\n");
            }
        }
    }
}
