using PangyaAPI.Utilities.BinaryModels;
using System;
using System.Collections.Generic;
using System.IO;
namespace PangyaAPI.SuperSocket.Interface
{
    public interface IPacket
    {
        /// <summary>
        /// Mensagem do Packet
        /// </summary>
        byte[] Message { get; set; }

        /// <summary>
        /// Mensagem encriptada
        /// </summary>
        byte[] MessageCrypted { get; set; }

        /// <summary>
        /// obtem o resto da mensagem do reader
        /// </summary>

        byte[] GetRemainingData { get; }

        /// <summary>
        /// obtem os bytes escritos no writer
        /// </summary>
        byte[] GetBytes { get; }

        /// <summary>
        /// id do pacote
        /// </summary>
        short Id { get; set; }
  
        #region Packet Reader

        double ReadDouble();

        byte ReadByte();

        short ReadInt16();

        ushort ReadUInt16();

        uint ReadUInt32();

        int ReadInt32();

        ulong ReadUInt64();

        long ReadInt64();

        float ReadSingle();

        string ReadPStr();

        void Skip(int count);

        void Seek(int offset, int origin);

        T Read<T>() where T : struct;

        IEnumerable<uint> Read(uint count);

        object Read(object value, int Count);

        object Read(object value);

        string ReadPStr(uint Count);

        bool ReadPStr(out string value, uint Count);

        bool ReadPStr(out string value);

        bool ReadDouble(out Double value);

        bool ReadBytes(out byte[] value);

        bool ReadByte(out byte value);

        bool ReadInt16(out short value);

        bool ReadUInt16(out ushort value);

        bool ReadUInt32(out uint value);

        bool ReadInt32(out int value);

        bool ReadUInt64(out ulong value);

        bool ReadInt64(out long value);

        bool ReadSingle(out float value);

        object ReadObject(object obj);

        bool ReadObject(out object obj);

        byte[] ReadBytes(int count);
        #endregion

        #region Packet Writer

        void Write(byte[] data);

        void WriteStruct(object data);

        void WriteStr(string message, int length);

        bool WriteStr(string message);

        void WritePStr(string value);

        void WriteZero(int count);

        void WriteUInt16(ushort value);

        void WriteInt16(short value);

        void WriteByte(byte value);

        void WriteSingle(float value);

        void WriteUInt32(uint value);

        void WriteInt32(int value);

        void WriteUInt64(ulong value);

        void WriteInt64(long value);

        void WriteDouble(double value);
        #endregion
    }

    public interface IPacket<TPacket> : IPacket
    {
    }


    /// <summary>
    /// Request information interface
    /// </summary>
    /// <typeparam name="TRequestHeader">The type of the request header.</typeparam>
    /// <typeparam name="TPacket">The type of the request body.</typeparam>
    public interface IPacket<TRequestHeader, TPacket> : IPacket<TPacket>
    { }
}
