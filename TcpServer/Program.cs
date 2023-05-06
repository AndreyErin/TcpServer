using System.Net;
using System.Net.Sockets;

//создаем сокет
Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


//создаем точку подключения которая будет адресом этого сервера
// адрес будет любой из доступных на этой машине
IPEndPoint ep = new IPEndPoint(IPAddress.Any, 8888);

//привязываем точку к сокету
socket.Bind(ep);

//запускаем прослушивание. максимальное количество сообщений в очереди будет 1000
socket.Listen(1000);

//в данном случае сокет будет прослушивать порть 8888 на любых локальных адресах
Console.WriteLine("Сокет запущен. Ожидание подключения");

//для подключенного клиента создается отдельный сокет
using Socket client = await socket.AcceptAsync();

Console.WriteLine("Адрес клиента {0}", client.RemoteEndPoint);
