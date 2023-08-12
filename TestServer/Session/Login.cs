using PangyaAPI.SuperSocket.SocketBase;
using PangyaAPI.Utilities.BinaryModels;
using ServerConsole.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerConsole.Session
{
    public class LoginMessages
    {
        // Note: PangYa does not use UTF-8.
        public static readonly Encoding StringEncoding = Encoding.UTF8;

        public interface IMessage
        {
            byte[] ToBytes();
        }

        public struct ClientLoginMessage : IMessage
        {
            public const ushort MessageId = 0x0001;

            public readonly string Username;
            public readonly string Password;

            public ClientLoginMessage(string username, string password)
            {
                Username = username;
                Password = password;
            }

            public static ClientLoginMessage FromBytes(byte[] data)
            {
                using (var reader = new PangyaBinaryReader(new MemoryStream(data)))
                {
                    Debug.Assert(reader.ReadUInt16() == MessageId);
                    var username = reader.ReadString();
                    var password = reader.ReadString();
                    return new ClientLoginMessage(username, password);
                }
            }

            public byte[] ToBytes()
            {
                var stream = new MemoryStream();

                using (var writer = new PangyaBinaryWriter(stream))
                {
                    writer.Write(MessageId);
                    writer.WriteStr(Username);
                    writer.WriteStr(Password);
                }

                return stream.GetBuffer();
            }
        }

        public struct ServerSecurity1Message : IMessage
        {
            public const ushort MessageId = 0x0010;

            public readonly string Token;

            public ServerSecurity1Message(string token)
            {
                Token = token;
            }

            public static ServerSecurity1Message FromBytes(byte[] data)
            {
                using (var reader = new PangyaBinaryReader(new MemoryStream(data)))
                {
                    Debug.Assert(reader.ReadUInt16() == MessageId);
                    return new ServerSecurity1Message(reader.ReadString());
                }
            }

            public byte[] ToBytes()
            {
                var stream = new MemoryStream();

                using (var writer = new PangyaBinaryWriter(stream))
                {
                    writer.Write(MessageId);
                    writer.WritePStr(Token);
                }

                return stream.ToArray();
            }
        }

        public struct ServerEntry
        {
            public readonly string ServerName;
            public readonly ushort ServerId;
            public readonly ushort NumUsers;
            public readonly ushort MaxUsers;
            public readonly string IpAddress;
            public readonly ushort Port;
            public readonly ushort Flags;

            public ServerEntry(string serverName, ushort serverId, ushort numUsers, ushort maxUsers, string ipAddress,
                ushort port, ushort flags)
            {
                ServerName = serverName;
                ServerId = serverId;
                NumUsers = numUsers;
                MaxUsers = maxUsers;
                IpAddress = ipAddress;
                Port = port;
                Flags = flags;
            }

            public static ServerEntry FromReader(PangyaBinaryReader reader)
            {
                var serverName = reader.ReadPStr(40);
                var serverId = reader.ReadUInt16();
                var numUsers = reader.ReadUInt16();
                var maxUsers = reader.ReadUInt16();
                reader.ReadUInt32();
                reader.ReadUInt16();
                var ipAddress = reader.ReadPStr(18);
                var port = reader.ReadUInt16();
                reader.ReadUInt16();
                var flags = reader.ReadUInt16();
                reader.ReadBytes(16);
                return new ServerEntry(serverName, serverId, numUsers, maxUsers, ipAddress, port, flags);
            }

            public void ToWriter(PangyaBinaryWriter writer)
            {
                writer.WriteStr(ServerName, 40);
                writer.Write(ServerId);
                writer.Write(NumUsers);
                writer.Write(MaxUsers);
                writer.Write((uint)0);
                writer.Write((ushort)0);
                writer.WriteStr(IpAddress, 18);
                writer.Write(Port);
                writer.Write((ushort)0);
                writer.Write(Flags);
                writer.Write(new byte[16]);
            }
        }

        public struct ServerListMessage : IMessage
        {
            public const ushort MessageId = 0x0002;

            public readonly ServerEntry[] Servers;

            public ServerListMessage(ServerEntry[] servers)
            {
                Servers = servers;
            }

            public static ServerListMessage FromBytes(byte[] data)
            {
                using (var reader = new PangyaBinaryReader(new MemoryStream(data)))
                {
                    Debug.Assert(reader.ReadUInt16() == MessageId);
                    var servers = new ServerEntry[reader.ReadByte()];
                    for (var i = 0; i < servers.Length; i++)
                        servers[i] = ServerEntry.FromReader(reader);
                    return new ServerListMessage(servers);
                }
            }

            public byte[] ToBytes()
            {
                var stream = new MemoryStream();

                using (var writer = new PangyaBinaryWriter(stream))
                {
                    writer.Write(MessageId);
                    writer.Write((byte)Servers.Length);
                    for (var i = 0; i < Servers.Length; i++)
                        Servers[i].ToWriter(writer);
                }

                return stream.ToArray();
            }
        }
    }
}
