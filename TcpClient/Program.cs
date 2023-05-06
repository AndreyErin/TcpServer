using System.Net.Sockets;
using System.Text;

/////клиент

using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
try
{
    //устанавливаем соединение
    await socket.ConnectAsync("127.0.0.1", 8888);
    Console.WriteLine($"Подключение к {socket.RemoteEndPoint} установлено");

    //буфер для чтения данных
    byte[] data = new byte[512];

    //получаем данные из потока
    int dataSize = await socket.ReceiveAsync(data, SocketFlags.None);
    // получаем отправленное время
    string time = Encoding.UTF8.GetString(data, 0, dataSize);
    Console.WriteLine($"Текущее время: {time}");

}
catch (SocketException)
{
    Console.WriteLine($"Не удалось установить подключение с {socket.RemoteEndPoint}");
}

Console.ReadLine();