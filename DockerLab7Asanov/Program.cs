using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;

namespace SyrkinLab3ConsoleServer
{
    internal class Program
    {
        static string ipAdress = "127.0.0.1";
        static int port = 11223;

        static Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static IPEndPoint endPoint;

        static List<Socket> clientSockets = new List<Socket>();



        static string username;

        static async Task Main(string[] args)
        {
            endPoint = new IPEndPoint(IPAddress.Parse(ipAdress), port);

            //List<string> users = new List<string>();

            serverSocket.Bind(endPoint);
            serverSocket.Listen(10);

            Console.WriteLine("Listening...");

            SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();

            acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(acceptEventArg_Completed);

            while (true)
            {
                Socket clientSocket = await AcceptAsync();
                _ = HandleClientAsync(clientSocket);
            }
            //
            //byte[] buffer = new byte[1024];
            //int bytesRead = clientSocket.Receive(buffer);
            //string receivedString = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            //username = receivedString;


            //while (true)
            //{
            //    //Console.WriteLine("Listening");

            //    if (clientSocket.Available > 0)
            //    {
            //        string username = "";
            //        string text = "";

            //        buffer = new byte[1024];
            //        bytesRead = clientSocket.Receive(buffer);
            //        text = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            //        Console.WriteLine("Accepted");

            //        if (text=="Connection")
            //        { 
            //            clientSocket = serverSocket.Accept();
            //        }
            //        else
            //        {
            //            buffer = Encoding.UTF8.GetBytes(text);

            //            clientSocket.Send(buffer);

            //            Console.WriteLine("Sent");
            //        }
            //    }
            //}
        }

        static Task<Socket> AcceptAsync()
        {
            var tcs = new TaskCompletionSource<Socket>();

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += (sender, e) =>
            {
                tcs.SetResult(e.AcceptSocket);
            };

            if (!serverSocket.AcceptAsync(args))
            {
                // Completed synchronously
                tcs.SetResult(args.AcceptSocket);
            }



            return tcs.Task;
        }

        static async Task HandleClientAsync(Socket client)
        {
            clientSockets.Add(client);

            Console.WriteLine($"Подключен клиент по IP: {client.RemoteEndPoint}");

            // TODO: Implement your logic for handling the connected client

            // Example: Receive data asynchronously
            var buffer = new byte[1024];
            while (true)
            {
                int bytesRead = await ReceiveAsync(client, buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    Console.WriteLine($"Клиент по IP {client.RemoteEndPoint} разорвал соединение.");
                    break;
                }

                string username = "";
                string text = "";

                //buffer = new byte[1024];

                text = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine($"Accepted: {text}");

                buffer = Encoding.UTF8.GetBytes(text);

                foreach (Socket theClient in clientSockets)
                    theClient.Send(buffer);

                Console.WriteLine($"Sent: {text}");
                //string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                //Console.WriteLine($"Received data from {client.RemoteEndPoint}: {receivedData}");
            }

            client.Close();
        }

        private static Task<int> ReceiveAsync(Socket socket, byte[] buffer, int offset, int count)
        {
            var tcs = new TaskCompletionSource<int>();

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(buffer, offset, count);
            args.Completed += (sender, e) =>
            {
                tcs.SetResult(e.BytesTransferred);
            };

            if (!socket.ReceiveAsync(args))
            {
                // Completed synchronously
                tcs.SetResult(args.BytesTransferred);
            }

            return tcs.Task;
        }

        //static void StartAccept(SocketAsyncEventArgs acceptEventArg)
        //{
        //    bool willRaiseEvent = false;
        //    while (!willRaiseEvent)
        //    {
        //        m_maxNumberAcceptedClients.WaitOne();

        //        // socket must be cleared since the context object is being reused
        //        acceptEventArg.AcceptSocket = null;
        //        willRaiseEvent = listenSocket.AcceptAsync(acceptEventArg);
        //        if (!willRaiseEvent)
        //        {
        //            ProcessAccept(acceptEventArg);
        //        }
        //    }
        //}

        public static void acceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {

        }

        ~Program()
        {
            serverSocket.Close();
        }

        static string GetUsername(string _string)
        {
            string result = "";

            for (int i = 0; i < _string.Length; i++)
            {
                if (_string[i] != '|')
                    result += _string[i];
                else
                    break;
            }

            return result;
        }

        static string GetMessage(string _string)
        {
            string result = "";

            bool read = false;

            for (int i = 0; i < _string.Length; i++)
            {
                if (read)
                    result += _string[i];

                if (_string[i] == '|')
                    read = true;
            }

            return result;
        }
    }
}
