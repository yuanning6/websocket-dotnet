using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static async Task Main(string[] args)
    {
        var hostName = Dns.GetHostName();
        IPHostEntry localhost = await Dns.GetHostEntryAsync(hostName);
        // This is the IP address of the local machine
        IPAddress localIpAddress = localhost.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(localIpAddress, 11_000);

        using Socket client = new(
            localEndPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp
        );

        await client.ConnectAsync(localEndPoint);
        while (true)
        {
            // Send message
            var message = "Hello from client";
            var messageBytes = Encoding.UTF8.GetBytes(message);
            _ = await client.SendAsync(messageBytes, SocketFlags.None);
            Console.WriteLine($"Socket client sent message: \"{message}\"");

            // Receive ack
            var buffer = new byte[1_024];
            var received = await client.ReceiveAsync(buffer, SocketFlags.None);
            var response = Encoding.UTF8.GetString(buffer, 0, received);
            if (response == "<|ACK|>")
            {
                Console.WriteLine(
                    $"Socket client received acknowledgement: \"{response}\"");
                break;
            }
        }

        client.Shutdown(SocketShutdown.Both);
    }
}
