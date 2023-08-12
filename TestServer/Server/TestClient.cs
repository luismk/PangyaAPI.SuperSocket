using PangyaAPI.SuperSocket.Interface;
using PangyaAPI.SuperSocket.SocketBase;
using PangyaAPI.Utilities;
using ServerConsole.Session;
using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace ServerConsole.Server
{
    /// <summary>
    /// Vai ser a class usada no Program.cs
    /// </summary>
    public class TestClient

    {
       public static void StartAsync()
        {

            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                client.Connect("127.0.0.1", 20201);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            while (true)
            {
                //try
                //{
                //    client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //    client.Connect("127.0.0.1", 2020);
                //}
                //catch (Exception ex)
                //{
                //    throw ex;
                //}

                try
                {
                    client.Send(System.Text.Encoding.UTF8.GetBytes("hello world!"));
                }
                catch (Exception)
                {
                    Console.WriteLine("send error.");
                }
                Console.WriteLine("sent message.");
                var buffer = new byte[128];
                try
                {
                    client.Receive(buffer);
                }
                catch (Exception)
                {
                    Console.WriteLine("receive error.");
                }
                Console.WriteLine("received message.");
                Console.WriteLine(buffer.HexDump());

                var key = Console.ReadKey();

                if (key.KeyChar.Equals('q'))
                    break;

                Console.WriteLine("any key to continue, press q to exit.");
                //try
                //{
                //    Console.WriteLine("---Close Client.---");
                //    client.Shutdown(SocketShutdown.Both);
                //    client.Dispose();
                //}
                //catch (Exception)
                //{
                //    Console.WriteLine("Shundown Error");
                //}

            }
        }
    }
}
