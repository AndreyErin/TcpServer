using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
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

namespace TcpClientWPF
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Canvas cnvItem = new Canvas() { Width = 40, Height = 40, Background = Brushes.Black };
        bool connect = false;

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

            Canvas.SetTop(cnvItem, 10);
            Canvas.SetLeft(cnvItem, 10);
            cnvMainClient.Children.Add(cnvItem);
            //using
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketGlobal = socket; 
                
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(cnvMainClient);
            if (point.Y >= 0)
                Canvas.SetTop(cnvItem, point.Y - 20);
            if (point.X >= 0)
                Canvas.SetLeft(cnvItem, point.X - 20);

            StringBuilder sb = new StringBuilder();
            sb.Append(point);
            lblPos.Content = sb.ToString();

            if (connect) 
            {
                SetDataOfServer(point);
            }
        }

        //получение данных
        private async void GetDataOfServer()
        {
//            List<byte> bufferForGet = new List<byte>();
//            // буфер для считывания одного байта
//            var bytesRead = new byte[1];
//            // считываем данные до конечного символа
//            while (true)
//            {
//                //ПОЛУЧЕНИЕ----------------------------------------
//                var count = await socketGlobal.ReceiveAsync(bytesRead, SocketFlags.None);
//                // txtStatConnectClient.Text += "ПОЛУЧЕНИЕ ДАННЫХ\n";
//
//                // смотрим, если считанный байт представляет конечный символ, выходим
//                if (count == 0 || bytesRead[0] == '\n') break;
//                // иначе добавляем в буфер
//                bufferForGet.Add(bytesRead[0]);
//            }
        }

        //отправка данных
        private async void SetDataOfServer(Point point)
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
                await socketGlobal.ConnectAsync("127.0.0.1", 8888);
                txtStatConnectClient.Text += $"Подключено";
                connect = true;
            }
            catch (SocketException ex)
            {
                txtStatConnectClient.Text += (ex.Message + "\n");
            }

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

        }
    }
}
