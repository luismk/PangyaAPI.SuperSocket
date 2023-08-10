using PangyaAPI.Cryptor.HandlePacket;
using PangyaAPI.SuperSocket.Interface;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PangyaAPI.SuperSocket.SocketBase
{
    public class AppPacketBase : IRequest, IDisposable
    {
        private MemoryStream _stream;
        private PangyaBinaryReader Reader;
        private PangyaBinaryWriter Writer;

        public byte[] Message { get; set; }
        public byte[] MessageCrypted { get; set; }

        public byte[] GetRemainingData => this.Reader.GetRemainingData();

        public byte[] GetBytes => this.Writer.GetBytes();
        public short Id { get; set; }
        private bool disposedValue = false;
        public AppPacketBase()
        {
            Writer = new PangyaBinaryWriter();
        }
        public AppPacketBase(byte[] message, bool IsClient = false)
        {
            if (IsClient)
            {

            }
            else
            {
                Id = BitConverter.ToInt16(new byte[] { message[5], message[6] }, 0);

                MessageCrypted = new byte[message.Length];
                Buffer.BlockCopy(message, 0, MessageCrypted, 0, message.Length); //Copia mensagem recebida criptografada
            }
        }
        public AppPacketBase(byte key, byte[] message, bool IsClient = false)
        {
            if (IsClient)
            {

            }
            else
            {
                Id = BitConverter.ToInt16(new byte[] { message[5], message[6] }, 0);

                MessageCrypted = new byte[message.Length];
                Buffer.BlockCopy(message, 0, MessageCrypted, 0, message.Length); //Copia mensagem recebida criptografada

                Message = Pang.ClientDecrypt(message, key);

                _stream = new MemoryStream(Message);

                _stream.Seek(2, SeekOrigin.Current); //Seek Inicial
                Reader = new PangyaBinaryReader(_stream);
                Writer = new PangyaBinaryWriter();
            }
        }

        public void SetKey(byte key)
        {
            Message = Pang.ClientDecrypt(MessageCrypted, key);

            _stream = new MemoryStream(Message);

            _stream.Seek(2, SeekOrigin.Current); //Seek Inicial
            Reader = new PangyaBinaryReader(_stream);
            Writer = new PangyaBinaryWriter();
        }
        public string StringLog()
        {
            return Message.HexDump();
        }
        public double ReadDouble()
        {
            return Reader.ReadDouble();
        }

        public byte ReadByte()
        {
            return Reader.ReadByte();
        }
        public short ReadInt16()
        {
            return Reader.ReadInt16();
        }
        public ushort ReadUInt16()
        {
            return Reader.ReadUInt16();
        }



        public uint ReadUInt32()
        {
            return Reader.ReadUInt32();
        }
        public int ReadInt32()
        {
            return Reader.ReadInt32();
        }

        public ulong ReadUInt64()
        {
            return Reader.ReadUInt64();
        }

        public long ReadInt64()
        {
            return Reader.ReadInt64();
        }

        public float ReadSingle()
        {
            return Reader.ReadSingle();
        }

        public string ReadPStr()
        {
            return Reader.ReadPStr();
        }
        public void Skip(int count)
        {
            Reader.Skip(count);
        }


        public void Seek(int offset, int origin)
        {
            Reader.Seek(offset, origin);
        }

        public object ReadObject(object obj)
        {
            return Reader.ReadObject(obj);
        }

        public bool ReadObject(out object obj)
        {
            Reader.ReadObject(out obj);
            return true;
        }

        public T Read<T>() where T : struct
        {
            return Reader.Read<T>();
        }
        public IEnumerable<uint> Read(uint count)
        {
            return Reader.Read(count);
        }
        public object Read(object value, int Count)
        {
            return Reader.Read(value, Count);
        }

        public object Read(object value)
        {
            return Reader.Read(value);
        }

        public string ReadPStr(uint Count)
        {
            var data = new byte[Count];
            //ler os dados
            Reader.BaseStream.Read(data, 0, (int)Count);
            var value = Encoding.ASCII.GetString(data);
            return value;
        }

        public bool ReadPStr(out string value, uint Count)
        {
            return Reader.ReadPStr(out value, Count);
        }
        public bool ReadPStr(out string value)
        {
            return Reader.ReadPStr(out value);
        }
        public bool ReadDouble(out Double value)
        {
            return Reader.ReadDouble(out value);
        }
        public bool ReadBytes(out byte[] value)
        {
            return Reader.ReadBytes(out value);
        }
        public bool ReadByte(out byte value)
        {
            return Reader.ReadByte(out value);
        }
        public bool ReadInt16(out short value)
        {
            return Reader.ReadInt16(out value);
        }
        public bool ReadUInt16(out ushort value)
        {
            return Reader.ReadUInt16(out value);
        }

        public bool ReadUInt32(out uint value)
        {
            return Reader.ReadUInt32(out value);
        }

        public bool ReadInt32(out int value)
        {
            return Reader.ReadInt32(out value);
        }

        public bool ReadUInt64(out ulong value)
        {
            return Reader.ReadUInt64(out value);
        }

        public bool ReadInt64(out long value)
        {
            return Reader.ReadInt64(out value);
        }

        public bool ReadSingle(out float value)
        {
            return Reader.ReadSingle(out value);
        }

        public byte[] ReadBytes(int count)
        {
            return Reader.ReadBytes(count);
        }

        public void Write(int value)
        {
            Writer.Write(value);
        }

        public void Write(uint value)
        {
            Writer.Write(value);
        }

        public void Write(byte value)
        {
            Writer.Write(value);
        }

        public void Write(short value)
        {
            Writer.Write(value);
        }

        public void Write(ulong value)
        {
            Writer.Write(value);
        }

        public void Write(long value)
        {
            Writer.Write(value);
        }


        public void Write(byte[] data)
        {
            try
            {
                Writer.Write(data);
            }
            catch
            {
            }
            return;
        }

        public void WriteStruct(object data)
        {
            try
            {
                Writer.WriteStruct(data);
            }
            catch
            {
            }
            return;
        }

        public void WriteStruct(object ex, object original)
        {
            try
            {
                Writer.WriteStruct(ex, original);
            }
            catch
            {
            }
            return;
        }


        public void WriteStr(string message, int length)
        {

            try
            {
                Writer.WriteStr(message, length);
            }
            catch
            {
            }
            return;
        }

        public bool WriteStr(string message)
        {
            try
            {
                WriteStr(message, message.Length);
            }
            catch
            {
                return false;
            }
            return true;

        }

        public void WritePStr(string value)
        {

            try
            {
                Writer.WritePStr(value);

            }
            catch
            {
                return;
            }
        }

        public void WriteZero(int count)
        {
            try
            {
                Writer.WriteZero(count);
            }
            catch
            {

            }

        }
        public void WriteUInt16(ushort value)
        {
            try
            {
                Writer.Write(value);
            }
            catch
            {

            }

        }

        public void WriteUInt16(int value)
        {
            try
            {
                Writer.WriteUInt16(value);
            }
            catch
            {

            }

        }


        public void WriteUInt16(uint value)
        {
            try
            {
                Writer.WriteUInt16(value);
            }
            catch
            {

            }

        }
        public void WriteUInt16(short value)
        {
            try
            {
                Writer.Write(value);
            }
            catch
            {

            }

        }

        public void WriteInt16(short value)
        {
            try
            {
                Writer.Write(value);
            }
            catch
            {

            }

        }
        public void WriteByte(int value)
        {
            try
            {
                WriteByte((byte)value);
            }
            catch
            {

            }

        }
        public void WriteByte(byte value)
        {
            try
            {
                Writer.Write(value);
            }
            catch
            {

            }

        }

        public void WriteSingle(float value)
        {
            try
            {
                Writer.Write(value);
            }
            catch
            {

            }

        }

        public void WriteUInt32(uint value)
        {
            try
            {
                Writer.Write(value);
            }
            catch
            {

            }

        }

        public void WriteInt32(int value)
        {
            try
            {
                Writer.Write(value);
            }
            catch
            {

            }

        }

        public void WriteUInt64(ulong value)
        {
            try
            {
                Writer.Write(value);
            }
            catch
            {

            }

        }

        public void WriteInt64(long value)
        {
            try
            {
                Writer.Write(value);
            }
            catch
            {

            }

        }

        public void WriteDouble(double value)
        {
            try
            {
                Writer.Write(value);
            }
            catch
            {

            }

        }

        /// <summary>
        /// Write Pangya Time
        /// </summary>
        /// <returns></returns>
        public void WriteTime(DateTime? date)
        {
            Writer.WriteTime(date);
        }

        /// <summary>
        /// Write Pangya Time
        /// </summary>
        /// <returns></returns>
        public void WriteTime()
        {
            Writer.WriteTime();
        }
        public void Clear()
        {
            Writer.Clear();
        }

        protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Reader?.Close();
                    Reader?.Dispose();
                    Writer?.Close();
                    Writer?.Dispose();
                    _stream.Close();
                    _stream.Dispose();
                }
                disposedValue = true;
            }
        }

        ~AppPacketBase()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
