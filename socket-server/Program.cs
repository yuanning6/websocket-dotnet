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

        using Socket listener = new(
            localEndPoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp
        );

        listener.Bind(localEndPoint);
        // Listening for incoming connections with a backlog of 100 pending connections.
        listener.Listen(100);
        Console.WriteLine($"Server listening on {localEndPoint}.");

        try
        {
            while (true)
            {
                Socket handler = await listener.AcceptAsync();
                // Allows multiple client connections to be handled concurrently
                // The main thread remains free to accept new connections instead of being blocked by handling client communication.
                ThreadPool.QueueUserWorkItem(state => HandleConnection(handler));
            }
        }
        catch (Exception e)
        {
            
            Console.WriteLine(e.ToString());
        }
        finally
        {
            listener.Shutdown(SocketShutdown.Both);
        }
        
        async void HandleConnection(Socket handler)
        {
            try
            {
                // Prints a message when a client connects.
                Console.WriteLine($"Client connected: {handler.RemoteEndPoint}.");
                while (true)
                {
                    var buffer = new byte[1_024];
                    // Receives data asynchronously into a buffer
                    var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, received);
                    
                    Console.WriteLine(
                        $"Socket server received message: \"{receivedMessage}\"");
                    if (String.IsNullOrEmpty(receivedMessage))
                    {
                        Console.WriteLine($"Disconnect from client: {handler.RemoteEndPoint}.");
                        handler.Shutdown(SocketShutdown.Both);
                        break;
                    }
                    
                    var ackMessage = "<|ACK|>";
                    var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                    await handler.SendAsync(echoBytes, 0);
                    Console.WriteLine(
                        $"Socket server sent acknowledgment: \"{ackMessage}\"");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
