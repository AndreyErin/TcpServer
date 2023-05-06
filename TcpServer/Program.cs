using System.Net;
using System.Net.Sockets;
using System.Text;
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
    Console.WriteLine("Сокет запущен. Ожидание подключения");

    
    //слушаем новые подключения
    while(true) 
    {
        //для подключенного клиента создается отдельный сокет
        var clientSocket = await mainSocket.AcceptAsync();
        
        //запускаем общение с клиентом в отдельном потоке и ждем следующего клиента
        Task.Run(async ()=> await ProcessClientAsync(clientSocket));
    }
}
catch (SocketException ex)
{
    Console.WriteLine(ex);
}


//общение с конкрентым клиентом
async Task ProcessClientAsync(Socket client)
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
            Console.WriteLine("111111" + bufferForGet.ToString());
            //ПОЛУЧЕНИЕ-----------------------------------------

            try
            {
                var count = await client.ReceiveAsync(bytesRead);
                // смотрим, если считанный байт представляет конечный символ, выходим
                Console.WriteLine("222222" + bufferForGet.ToString());


                if (count == 0 || bytesRead[0] == '\n')
                {
                    Console.WriteLine("33333" + bufferForGet.ToString());
                    break;
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);

            }


            






            // иначе добавляем в буфер
           // bufferForGet.Add(bytesRead[0]);
            Console.WriteLine("44444" + bufferForGet.ToString());
        }
        var word = Encoding.UTF8.GetString(bufferForGet.ToArray());
        // если прислан маркер окончания взаимодействия,
        // выходим из цикла и завершаем взаимодействие с клиентом
        if (word == "END") break;

        Console.WriteLine($"Запрошен перевод слова {word}");
        // находим слово в словаре и отправляем обратно клиенту
        if (!words.TryGetValue(word, out var translation)) translation = "не найдено в словаре";
        // добавляем символ окончания сообщения 
        translation += '\n';
        // отправляем перевод слова из словаря
        //ОТПРАЛЯЕМ-------------------------------------------
        await client.SendAsync(Encoding.UTF8.GetBytes(translation));
        bufferForGet.Clear();
    }
    client.Shutdown(SocketShutdown.Both);
    client.Close();
}




Console.ReadLine();
