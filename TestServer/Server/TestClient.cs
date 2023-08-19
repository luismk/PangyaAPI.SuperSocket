//using PangyaAPI.Cryptor.HandlePacket;
//using PangyaAPI.SuperSocket.Interface;
//using PangyaAPI.SuperSocket.SocketBase;
//using PangyaAPI.Utilities;
//using ServerConsole.Session;
//using System;
//using System.Diagnostics;
//using System.Net.Sockets;
//using System.Text;

//namespace ServerConsole.Server
//{
//    /// <summary>
//    /// Vai ser a class usada no Program.cs
//    /// </summary>
//    public class TestClient

//    {
//       public static void StartAsync()
//        {
//            var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//            try
//            {
//                client.Connect("127.0.0.1", 20201);
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//            var r_buff = new byte[1024];
//            var size = client.Receive(r_buff, 0);
//            var n_buff = new byte[size];
//            if (size > 0)
//            {
//                Buffer.BlockCopy(r_buff, 0, n_buff, 0, size);
//            }
//            n_buff.DebugDump();
//            client.Close();
//        }
//    }
//}
