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
using System.Threading;
//сервер
namespace TcpServerWPF
{

    public partial class MainWindow : Window
    {
        Canvas cnvItem = new Canvas() { Width = 40, Height = 40, Background = Brushes.Black };
        Canvas cnvItemOfClient = new Canvas() { Width = 40, Height = 40, Background = Brushes.Red };
        Point pointMauseCurrent = new Point();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Canvas cnvItem = new Canvas() { Width = 40, Height = 40, Background = Brushes.Brown};
            Canvas.SetTop(cnvItem, 10);
            Canvas.SetLeft(cnvItem, 10);
            cnvMainServer.Children.Add(cnvItem);

            Canvas.SetTop(cnvItemOfClient, 10);
            Canvas.SetLeft(cnvItemOfClient, 100);
            cnvMainServer.Children.Add(cnvItemOfClient);

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
                    Task.Run( async () => await ProcessClientAsync(clientSocket, Dispatcher));
                    Task.Run(async () => await SetDataToClient(clientSocket));
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        // Прием. общение с конкрентым клиентом
        async Task ProcessClientAsync(Socket client, Dispatcher MainDisp)
        {

            // буфер для накопления входящих данных
            var bufferForGet = new List<byte>();

            // буфер для считывания одного байта
            var bytesRead = new byte[1];
            while (true)
            {
                //ПОЛУЧЕНИЕ-----------------------------------------
                // считываем данные до конечного символа
                while (true)
                {
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

                string[] strPos = word.Split(';');

                              
                Action action = () =>
                {
                    Canvas.SetTop(cnvItem, double.Parse(strPos[1]) - 20);
                    Canvas.SetLeft(cnvItem, double.Parse(strPos[0]) - 20);
                    txtStatConnectServer.Text = $"Координаты \n X: {strPos[0]}\n Y: {strPos[1]}";

                };
                MainDisp.Invoke(action);
               bufferForGet.Clear();



            }
            //client.Shutdown(SocketShutdown.Both);
            //client.Close();
        }

        //отправка
        async Task SetDataToClient(Socket client)         
        {
            while (true)
            {
                //ОТПРАВКА
                try
                {
                    StringBuilder sb = new StringBuilder();
                    Point point = pointMauseCurrent;
                    //                    point.Y = Canvas.GetLeft(cnvItemOfClient);
                    //                    point.X = Canvas.GetTop(cnvItemOfClient);
                    sb.Append(point);

                    // считыванием строку в массив байт
                    // при отправке добавляем маркер завершения сообщения - \n
                    byte[] data = Encoding.UTF8.GetBytes(sb.ToString() + '\n');

                    // отправляем данные               
                    await client.SendAsync(data, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    txtStatConnectServer.Text += (ex.Message + "\n");
                }
                Thread.Sleep(1);
            }
        }

        private void cnvMainServer_MouseMove(object sender, MouseEventArgs e)
        {
            Canvas.SetTop(cnvItemOfClient, e.GetPosition(cnvMainServer).Y - 20);
            Canvas.SetLeft(cnvItemOfClient, e.GetPosition(cnvMainServer).X - 20);

            pointMauseCurrent = e.GetPosition(cnvMainServer);
       }
    }
}
