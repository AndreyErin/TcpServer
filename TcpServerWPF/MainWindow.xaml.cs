using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TcpServerWPF
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            //сервер
            //создаем сокет
            using Socket mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //создаем точку подключения которая будет адресом этого сервера
            // адрес будет любой из доступных на этой машине
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 8888);

            try
            {
                //привязываем точку к сокету
                mainSocket.Bind(ep);

                //запускаем прослушивание. максимальное количество сообщений в очереди будет 1000
                mainSocket.Listen();

                //в данном случае сокет будет прослушивать порть 8888 на любых локальных адресах
                txtStatConnectServer.Text += "Сокет запущен. Ожидание подключения\n";


                //слушаем новые подключения
                while (true)
                {
                    //для подключенного клиента создается отдельный сокет
                    var clientSocket = await mainSocket.AcceptAsync();

                    //запускаем общение с клиентом в отдельном потоке и ждем следующего клиента
                    Task.Run(async () => await ProcessClientAsync(clientSocket, Dispatcher));
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }







        //общение с конкрентым клиентом
        async Task ProcessClientAsync(Socket client, Dispatcher MainDisp)
        {

            // условный словарь
            var words = new Dictionary<string, string>()
    {
        { "red", "красный" },
        { "blue", "синий" },
        { "green", "зеленый" },
    };
            // буфер для накопления входящих данных
            var bufferForGet = new List<byte>();

            // буфер для считывания одного байта
            var bytesRead = new byte[1];
            while (true)
            {
                // считываем данные до конечного символа
                while (true)
                {
                    //ПОЛУЧЕНИЕ-----------------------------------------
                    var count = await client.ReceiveAsync(bytesRead, SocketFlags.None);
                    // смотрим, если считанный байт представляет конечный символ, выходим

                    if (count == 0 || bytesRead[0] == '\n')
                    {
                        break;
                    }

                    // иначе добавляем в буфер
                    bufferForGet.Add(bytesRead[0]);

                }
                var word = Encoding.UTF8.GetString(bufferForGet.ToArray());
                // если прислан маркер окончания взаимодействия,
                // выходим из цикла и завершаем взаимодействие с клиентом
                if (word == "END") break;

                Action action = () =>
                {
                    txtStatConnectServer.Text += $"Запрошен перевод слова {word}\n";
                };
                MainDisp.Invoke(action);

                // находим слово в словаре и отправляем обратно клиенту
                if (!words.TryGetValue(word, out var translation)) translation = "не найдено в словаре";
                // добавляем символ окончания сообщения 
                translation += '\n';
                // отправляем перевод слова из словаря
                //ОТПРАЛЯЕМ-------------------------------------------
                await client.SendAsync(Encoding.UTF8.GetBytes(translation), SocketFlags.None);
                bufferForGet.Clear();
            }
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
    }
}
