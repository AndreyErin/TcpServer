using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
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

namespace TcpClientWPF
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dispatcher MainDisp = Dispatcher.CurrentDispatcher;
        Canvas cnvItem = new Canvas() { Width = 40, Height = 40, Background = Brushes.Black };
        Canvas cnvItemOfServer = new Canvas() { Width = 40, Height = 40, Background = Brushes.Red };
        bool connect = false;
        System.Windows.Point pointMouse = new System.Windows.Point();

        public MainWindow()
        {
            InitializeComponent();

        }
        //ФПС
        private void RenderingFPS(object sender, EventArgs e)
        {
            cnvMainClient.InvalidateVisual();
        }

        private Socket socketGlobal { get; set; }

        //загрузка программы
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //ФПС
            System.Windows.Media.CompositionTarget.Rendering += RenderingFPS;
            //управляется мышкой
            Canvas.SetTop(cnvItem, 10);
            Canvas.SetLeft(cnvItem, 10);
            cnvMainClient.Children.Add(cnvItem);
            //управляется сервером
            Canvas.SetTop(cnvItem, 10);
            Canvas.SetLeft(cnvItem, 100);
            cnvMainClient.Children.Add(cnvItemOfServer);

            //using
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketGlobal = socket; 
                
        }
        //---------------------------------------------------
        //получение данных
        private async void GetDataOfServer()
        {
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
                    var count = await socketGlobal.ReceiveAsync(bytesRead, SocketFlags.None);

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
                    Canvas.SetTop(cnvItemOfServer, double.Parse(strPos[1]) - 20);
                    Canvas.SetLeft(cnvItemOfServer, double.Parse(strPos[0]) -20);
                    //txtStatConnectServer.Text = $"Координаты \n X: {strPos[0]}\n Y: {strPos[1]}";
                };
                MainDisp.Invoke(action);

                bufferForGet.Clear();
                //Thread.Sleep(10);
            }
        }
        
        //отправка данных
        private async void SetDataOfServer(System.Windows.Point point)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(point);
                //MessageBox.Show(sb.ToString());
                // считыванием строку в массив байт
                // при отправке добавляем маркер завершения сообщения - \n
                byte[] data = Encoding.UTF8.GetBytes(sb.ToString() + '\n');

                // отправляем данные               
                await socketGlobal.SendAsync(data, SocketFlags.None);
            }
            catch (SocketException ex)
            {
                txtStatConnectClient.Text += (ex.Message + "\n");
            }
        }
        
        //коннект
        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //ПОДКЛЮЧЕНИЕ-------------------------------------
                await socketGlobal.ConnectAsync("192.168.0.34", 8888);
                txtStatConnectClient.Text += $"Подключено";
                connect = true;
            }
            catch (SocketException ex)
            {
                txtStatConnectClient.Text += (ex.Message + "\n");
            }

            //GetDataOfServer();
            Task.Factory.StartNew(() => { GetDataOfServer(); });
        }

        //дисконнект
        private async void btnDisConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //ОТКЛЮЧАЕМСЯ ОТ СЕРВЕРА-------------------------
                // отправляем маркер завершения подключения - END
                await socketGlobal.SendAsync(Encoding.UTF8.GetBytes("END\n"), SocketFlags.None);
                connect = false;
            }
            catch (SocketException ex)
            {
                txtStatConnectClient.Text += (ex.Message + "\n");
            }
            //socketGlobal.Shutdown(SocketShutdown.Both);
            //socketGlobal.Close();
        }

        //передвижение мыши
        private void cnvMainClient_MouseMove(object sender, MouseEventArgs e)
        {
            pointMouse = e.GetPosition(cnvMainClient);
            if (pointMouse.Y >= 0)
                Canvas.SetTop(cnvItem, pointMouse.Y - 20);
            if (pointMouse.X >= 0)
                Canvas.SetLeft(cnvItem, pointMouse.X - 20);

            StringBuilder sb = new StringBuilder();
            sb.Append(pointMouse);
            lblPos.Content = sb.ToString();

            if (connect)
            {
                SetDataOfServer(pointMouse);
            }
        }
    }
}
