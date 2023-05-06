using System.Net.Sockets;

using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
try
{
    await socket.ConnectAsync("127.0.0.1", 8888);
    Console.WriteLine($"Подключение к {socket.RemoteEndPoint} установлено");
}
catch (SocketException)
{
    Console.WriteLine($"Не удалось установить подключение с {socket.RemoteEndPoint}");
}
