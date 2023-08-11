using PangyaAPI.SuperSocket.Interface;
using PangyaAPI.SuperSocket.SocketBase;
using ServerConsole.Server;
using ServerConsole.Session;
using System;

namespace ServerConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var serverApp = new MainServer();//chama a class com servidor imbutido

            serverApp.StartingServer();//inicializa o server
            serverApp.NewSessionConnected += Handle_NewSessionConnected;
            serverApp.NewRequestReceived += Handle_NewRequestReceived;
            //faz um laço para o servidor fica sempre correndo
            while (true)
            {
                System.Threading.Thread.Sleep(50);

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.KeyChar == 'q')
                    {
                        Console.WriteLine("Server Terminate ~~~");
                        serverApp.Stop();//faço o servidor parar de rodar ou simplesmente não ira mais receber conexao!
                        break;
                    }
                }

            }
        }

        /// <summary>
        /// metodo que ira lidar com conexao do player recebida
        /// </summary>
        /// <param name="session">Jogador ou conexao</param>
        /// <exception cref="NotImplementedException">@! não implementado(ainda)</exception>
        private static void Handle_NewSessionConnected(Player session)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// metodo que ira lidar com os pacotes
        /// </summary>
        /// <param name="session">Jogador ou conexao</param>
        /// <param name="requestInfo">mensgem ou packet</param>
        /// <exception cref="NotImplementedException">@! não implementado(ainda)</exception>
        private static void Handle_NewRequestReceived(Player session, IRequestInfo requestInfo)
        {
            throw new NotImplementedException();
        }
    }
}
