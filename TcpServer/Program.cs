using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;

//создаем сокет
Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


//создаем точку подключения которая будет адресом этого сервера
// адрес будет любой из доступных на этой машине
IPEndPoint ep = new IPEndPoint(IPAddress.Any, 8888);



try
{
    //привязываем точку к сокету
    socket.Bind(ep);

    //запускаем прослушивание. максимальное количество сообщений в очереди будет 1000
    socket.Listen(1000);

    //в данном случае сокет будет прослушивать порть 8888 на любых локальных адресах
    Console.WriteLine("Сокет запущен. Ожидание подключения");

    //отправляем данные

    while(true) 
    {
        //для подключенного клиента создается отдельный сокет
        using Socket client = await socket.AcceptAsync();
        // определяем данные для отправки - текущее время
        byte[] data = Encoding.UTF8.GetBytes(DateTime.Now.ToLongTimeString());
        // отправляем данные
        await client.SendAsync(data, SocketFlags.None);
        Console.WriteLine($"Клиенту {client.RemoteEndPoint} отправлены данные");
    }
}
catch (SocketException ex)
{

    Console.WriteLine(ex);
}



Console.ReadLine();
