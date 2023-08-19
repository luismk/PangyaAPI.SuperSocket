using PangyaAPI.Player.Data;
using PangyaAPI.SuperSocket.Cryptor;
using PangyaAPI.Utilities;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PangyaAPI.SuperSocket.SocketBase
{
    public class Packet : AppPacketBase
    {
        public Packet()
        {

        }
        public Packet(ushort ID) : base(ID)
        {
        }

        public void ReadObject(object @value)
        {
            ReadPlain(value);
        }
    }
    //public class ConversionByte
    //{
    //    private byte[] mBuffer;
    //    private readonly ushort[] mPow256;

    //    public ConversionByte(byte[] buffer)
    //    {
    //        mBuffer = buffer;
    //        mPow256 = new ushort[6];

    //        ushort pow = 1;
    //        for (int i = 0; i < 6; i++)
    //        {
    //            mPow256[i] = pow;
    //            pow *= 256;
    //        }
    //    }

    //    public int GetNumberNS()
    //    {
    //        int value = 0;
    //        for (int i = 0; i < 3; i++)
    //        {
    //            value += mBuffer[i] * mPow256[i];
    //        }
    //        return value;
    //    }

    //    public int GetNumberBase255()
    //    {
    //        int value = 0;
    //        for (int i = 0; i < 6; i++)
    //        {
    //            value += mBuffer[i] * mPow256[i];
    //        }
    //        return value;
    //    }
    //}

    //// Other classes and code


    //public class Packet
    //{
    //    private const int CHUNCK_ALLOC = 1024;
    //    private const uint CB_BASE_256 = 10;
    //    private const uint CB_BASE_255 = 20;
    //    private const uint CB_SEQ_NORMAL = 1;
    //    private const uint CB_SEQ_INVERTIDA = 2;
    //    private const uint CB_PARAM_DEFAULT = 0;

   
    //    public class OffsetIndex
    //    {
    //        public byte[] Buffer;
    //        public int IndexR;
    //        public int IndexW;
    //        public int Size;
    //        public int SizeAllocated;

    //        public void Clear()
    //        {
    //            Array.Clear(Buffer, 0, Buffer.Length);
    //            IndexR = 0;
    //            IndexW = 0;
    //            Size = 0;
    //        }

    //        public void ResetRead()
    //        {
    //            IndexR = 0;
    //        }

    //        public void ResetWrite()
    //        {
    //            IndexR = 0;
    //            IndexW = 0;
    //        }

    //        public void Reset()
    //        {
    //            ResetRead();
    //            ResetWrite();
    //        }
    //    }

    //    private OffsetIndex mPlain = new OffsetIndex();
    //    private OffsetIndex mMaked = new OffsetIndex();
    //    private ushort mTipo;

    //    public Packet()
    //    {
    //        mPlain.Buffer = new byte[0];
    //        mMaked.Buffer = new byte[0];
    //        mTipo = ushort.MaxValue;
    //    }

    //    public Packet(ushort tipo)
    //    {
    //        mPlain.Buffer = new byte[0];
    //        mMaked.Buffer = new byte[0];
    //        mTipo = tipo;

    //        AddPlain(BitConverter.GetBytes(mTipo), sizeof(ushort));
    //    }

    //    private void AddAlloc(OffsetIndex index, byte[] buf, int size)
    //    {
    //        Alloc(ref index, size);
    //        Buffer.BlockCopy(buf, 0, index.Buffer, index.IndexW, size);
    //        index.IndexW += size;
    //    }

    //    public void AddPlain(byte[] buf, int size)
    //    {
    //        if (buf == null)
    //            throw new ArgumentNullException(nameof(buf));

    //        Alloc(ref mPlain, size);
    //        Add(ref mPlain, buf, size);
    //    }

    //    public void ReadPlain(byte[] buf, int size)
    //    {
    //        if (buf == null)
    //            throw new ArgumentNullException(nameof(buf));

    //        if (mPlain.IndexW < (size + mPlain.IndexR))
    //            throw new Exception("Error not enough for read in Packet.ReadPlain()");

    //        Buffer.BlockCopy(mPlain.Buffer, mPlain.IndexR, buf, 0, size);
    //        mPlain.IndexR += size;
    //    }

    //    public void AddMaked(byte[] buf, int size)
    //    {
    //        if (buf == null)
    //            throw new ArgumentNullException(nameof(buf));

    //        AddAlloc(mMaked, buf, size);
    //    }

    //    public void ReadMaked(byte[] buf, int size)
    //    {
    //        if (buf == null)
    //            throw new ArgumentNullException(nameof(buf));

    //        if (mMaked.IndexW < (size + mMaked.IndexR))
    //            throw new Exception("Error not enough for read in Packet.ReadMaked()");

    //        Buffer.BlockCopy(mMaked.Buffer, mMaked.IndexR, buf, 0, size);
    //        mMaked.IndexR += size;
    //    }
    //    private void Destroy()
    //    {
    //        if (mPlain.Buffer != null)
    //            mPlain.Buffer = null;

    //        if (mMaked.Buffer != null)
    //            mMaked.Buffer = null;
    //    }

    //    private void InitMaked(int size)
    //    {
    //        mMaked.SizeAllocated = size;
    //        mMaked.Buffer = new byte[size];
    //    }

    //    private void InitPlain(ushort tipo)
    //    {
    //        mTipo = tipo;
    //        mPlain.Buffer = new byte[0];
    //        mMaked.Buffer = new byte[0];

    //        AddPlain(BitConverter.GetBytes(mTipo), sizeof(ushort));
    //    }

    //    private void Reset()
    //    {
    //        mPlain.IndexR = 0;
    //        mPlain.IndexW = 0;
    //        mMaked.IndexR = 0;
    //        mMaked.IndexW = 0;
    //    }

    //    private void Add(byte[] buf, int size, bool _)
    //    {
    //        Array.Copy(buf, 0, mMaked.Buffer, mMaked.IndexW, size);
    //        mMaked.IndexW += size;
    //    }

    //    private void Add(ref OffsetIndex index, byte[] buf, int size)
    //    {
    //        if (index.IndexW + size > index.SizeAllocated)
    //        {
    //            throw new Exception("Exceeded allocated buffer size in packet::add()");
    //        }

    //        Buffer.BlockCopy(buf, 0, index.Buffer, index.IndexW, size);
    //        if (index.IndexW == 14)
    //        {
    //            throw new Exception("Exceeded allocated buffer size in packet::add()");

    //        }
    //        index.IndexW += size;
    //        if (index.IndexW == 10)
    //        {
    //            throw new Exception("Exceeded allocated buffer size in packet::add()");

    //        }
    //    }
    //    private void Read(byte[] buf, int size)
    //    {
    //        Array.Copy(mMaked.Buffer, mMaked.IndexR, buf, 0, size);
    //        mMaked.IndexR += size;
    //    }
    //    private void Read(ref OffsetIndex index, byte[] buf, int size)
    //    {
    //        Array.Copy(index.Buffer, index.IndexR, buf, 0, size);
    //        index.IndexR += size;
    //    }

    //    public ulong ReadUInt64()
    //    {
    //        byte[] bytes = new byte[sizeof(ulong)];
    //        ReadPlain(bytes, sizeof(ulong));
    //        return BitConverter.ToUInt64(bytes, 0);
    //    }

    //    public uint ReadUInt32()
    //    {
    //        byte[] bytes = new byte[sizeof(uint)];
    //        ReadPlain(bytes, sizeof(uint));
    //        return BitConverter.ToUInt32(bytes, 0);
    //    }

    //    public ushort ReadUInt16()
    //    {
    //        byte[] bytes = new byte[sizeof(ushort)];
    //        ReadPlain(bytes, sizeof(ushort));
    //        return BitConverter.ToUInt16(bytes, 0);
    //    }

    //    public byte ReadUInt8()
    //    {
    //        byte[] bytes = new byte[sizeof(byte)];
    //        ReadPlain(bytes, sizeof(byte));
    //        return bytes[0];
    //    }

    //    public void AddString(string str)
    //    {
    //        byte[] strBytes = Encoding.UTF8.GetBytes(str);
    //        AddUInt16((ushort)strBytes.Length);
    //        AddPlain(strBytes, strBytes.Length);
    //    }

    //    public string ReadString()
    //    {
    //        ushort len = ReadUInt16();
    //        byte[] strBytes = new byte[len];
    //        ReadPlain(strBytes, len);
    //        return Encoding.UTF8.GetString(strBytes);
    //    }
    //    public void AddBuffer(byte[] buffer)
    //    {
    //        if (buffer == null)
    //            throw new ArgumentNullException(nameof(buffer), "Buffer cannot be null.");

    //        AddUInt32((uint)buffer.Length);
    //        AddPlain(buffer, buffer.Length);
    //    }

    //    public void ReadBuffer(out byte[] buffer)
    //    {
    //        uint len = ReadUInt32();
    //        buffer = new byte[len];
    //        ReadPlain(buffer, (int)len);
    //    }

    //    public void AddZeroByte(int size)
    //    {
    //        if (size <= 0)
    //            throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than zero.");

    //        byte[] zeros = new byte[size];
    //        AddPlain(zeros, size);
    //    }

    //    public void AddQWord(ulong qword)
    //    {
    //        byte[] bytes = BitConverter.GetBytes(qword);
    //        AddPlain(bytes, sizeof(ulong));
    //    }

    //    public void ReadQWord(out ulong qword)
    //    {
    //        byte[] bytes = new byte[sizeof(ulong)];
    //        ReadPlain(bytes, sizeof(ulong));
    //        qword = BitConverter.ToUInt64(bytes, 0);
    //    }

    //    public void AddDWord(uint dword)
    //    {
    //        byte[] bytes = BitConverter.GetBytes(dword);
    //        AddPlain(bytes, sizeof(uint));
    //    }

    //    public void ReadDWord(out uint dword)
    //    {
    //        byte[] bytes = new byte[sizeof(uint)];
    //        ReadPlain(bytes, sizeof(uint));
    //        dword = BitConverter.ToUInt32(bytes, 0);
    //    }

    //    public void AddWord(ushort word)
    //    {
    //        byte[] bytes = BitConverter.GetBytes(word);
    //        AddPlain(bytes, sizeof(ushort));
    //    }

    //    public void ReadWord(out ushort word)
    //    {
    //        byte[] bytes = new byte[sizeof(ushort)];
    //        ReadPlain(bytes, sizeof(ushort));
    //        word = BitConverter.ToUInt16(bytes, 0);
    //    }

    //    public void AddByte(byte b)
    //    {
    //        AddPlain(new byte[] { b }, 1);
    //    }

    //    public void ReadByte(out byte b)
    //    {
    //        byte[] bytes = new byte[1];
    //        ReadPlain(bytes, 1);
    //        b = bytes[0];
    //    }

    //    public void AddInt64(long value)
    //    {
    //        byte[] bytes = BitConverter.GetBytes(value);
    //        AddPlain(bytes, sizeof(long));
    //    }

    //    public void ReadInt64(out long value)
    //    {
    //        byte[] bytes = new byte[sizeof(long)];
    //        ReadPlain(bytes, sizeof(long));
    //        value = BitConverter.ToInt64(bytes, 0);
    //    }

    //    public void AddInt32(int value)
    //    {
    //        byte[] bytes = BitConverter.GetBytes(value);
    //        AddPlain(bytes, sizeof(int));
    //    }

    //    public void ReadInt32(out int value)
    //    {
    //        byte[] bytes = new byte[sizeof(int)];
    //        ReadPlain(bytes, sizeof(int));
    //        value = BitConverter.ToInt32(bytes, 0);
    //    }
    //    public void AddFloat(float value)
    //    {
    //        byte[] bytes = BitConverter.GetBytes(value);
    //        AddPlain(bytes, sizeof(float));
    //    }

    //    public void ReadFloat(out float value)
    //    {
    //        byte[] bytes = new byte[sizeof(float)];
    //        ReadPlain(bytes, sizeof(float));
    //        value = BitConverter.ToSingle(bytes, 0);
    //    }

    //    public void AddDouble(double value)
    //    {
    //        byte[] bytes = BitConverter.GetBytes(value);
    //        AddPlain(bytes, sizeof(double));
    //    }

    //    public void ReadDouble(out double value)
    //    {
    //        byte[] bytes = new byte[sizeof(double)];
    //        ReadPlain(bytes, sizeof(double));
    //        value = BitConverter.ToDouble(bytes, 0);
    //    }

    //    public void ReadString(out string str)
    //    {
    //        short length;
    //        ReadInt16(out length);

    //        byte[] bytes = new byte[length];
    //        ReadPlain(bytes, length);

    //        str = Encoding.ASCII.GetString(bytes);
    //    }

    //    public void AddWString(string str)
    //    {
    //        byte[] bytes = Encoding.Unicode.GetBytes(str);
    //        AddInt16((short)bytes.Length);
    //        AddPlain(bytes, bytes.Length);
    //    }

    //    public void ReadWString(out string str)
    //    {
    //        short length;
    //        ReadInt16(out length);

    //        byte[] bytes = new byte[length];
    //        ReadPlain(bytes, length);

    //        str = Encoding.Unicode.GetString(bytes);
    //    }
    //    public void AddUInt16(ushort value)
    //    {
    //        byte[] bytes = BitConverter.GetBytes(value);
    //        AddPlain(bytes, sizeof(ushort));
    //    }
    //    public void AddUInt32(uint value)
    //    {
    //        byte[] bytes = BitConverter.GetBytes(value);
    //        AddPlain(bytes, sizeof(uint));
    //    }

    //    public void AddInt16(short value)
    //    {
    //        byte[] bytes = BitConverter.GetBytes(value);
    //        AddPlain(bytes, sizeof(short));
    //    }

    //    public void ReadUInt16(out ushort value)
    //    {
    //        byte[] bytes = new byte[sizeof(ushort)];
    //        ReadPlain(bytes, sizeof(ushort));
    //        value = BitConverter.ToUInt16(bytes, 0);
    //    }
    //    public void ReadInt16(out short value)
    //    {
    //        byte[] bytes = new byte[sizeof(short)];
    //        ReadPlain(bytes, sizeof(short));
    //        value = BitConverter.ToInt16(bytes, 0);
    //    }

    //    public void AddUInt8(byte value)
    //    {
    //        AddPlain(new byte[] { value }, sizeof(byte));
    //    }

    //    public void ReadUInt8(out byte value)
    //    {
    //        byte[] bytes = new byte[sizeof(byte)];
    //        ReadPlain(bytes, sizeof(byte));
    //        value = bytes[0];
    //    }
    //    //public void AddCompressed(byte[] data)
    //    //{
    //    //    int compressedSize;
    //    //    byte[] compressedData = CompressData(data, out compressedSize);
    //    //    AddInt32(compressedSize);
    //    //    AddPlain(compressedData, compressedSize);
    //    //}


    //    private void AddPlain(byte[] buf, int index, int size)
    //    {
    //        if (buf == null)
    //            throw new ArgumentException("Error arguments invalid, _buf is null in packet.AddPlain()");

    //        Alloc(ref mPlain, size);
    //        Add(ref mPlain, buf, size);
    //    }

    //    private void AddMaked(byte[] buf, int index, int size)
    //    {
    //        if (buf == null)
    //            throw new ArgumentException("Error arguments invalid, _buf is null in packet.AddMaked()");

    //        Alloc(ref mMaked, size);
    //        Add(ref mMaked, buf, size);
    //    }

    //    public void Alloc(ref OffsetIndex index, int size)
    //    {
    //        if (size > (index.SizeAllocated - index.IndexW))
    //        {
    //            int ant = index.SizeAllocated;

    //            if (size < 0)
    //            {
    //                throw new Exception("Negative size. packet::alloc()");
    //            }

    //            index.SizeAllocated += ((size - (index.SizeAllocated - index.IndexW)) / CHUNCK_ALLOC + 1) * CHUNCK_ALLOC;

    //            try
    //            {
    //                byte[] tmp = new byte[index.SizeAllocated];

    //                if (index.Buffer != null)
    //                {
    //                    Buffer.BlockCopy(index.Buffer, 0, tmp, 0, index.IndexW);
    //                }

    //                if (index.Buffer != null)
    //                {
    //                    index.Buffer = null; // Liberar memória do buffer anterior
    //                }

    //                index.Buffer = tmp;
    //            }
    //            catch (Exception e)
    //            {
    //                if (index.Buffer != null)
    //                {
    //                    index.Buffer = null; // Liberar memória do buffer anterior
    //                }

    //                throw new Exception("Error ao alocar memoria. size_ant: " + ant +
    //                                    "\r\nsize_alloc: " + index.SizeAllocated +
    //                                    "\r\nsize_request: " + size + ". " + e.Message + ". packet::alloc()");
    //            }

    //            if (index.Buffer == null)
    //            {
    //                throw new Exception("Error ao alocar memoria para o buffer em packet::alloc()");
    //            }
    //        }
    //    }
    //    private void AllocMaked(int size)
    //    {
    //        if (size > (mMaked.SizeAllocated - mMaked.IndexW))
    //        {
    //            int ant = mMaked.SizeAllocated;

    //            if (size < 0)
    //                throw new Exception("Negative size. AllocMaked()");

    //            mMaked.SizeAllocated += ((size - (mMaked.SizeAllocated - mMaked.IndexW)) / CHUNCK_ALLOC + 1) * CHUNCK_ALLOC;

    //            try
    //            {
    //                if (mMaked.Buffer != null)
    //                {
    //                    byte[] tmp = new byte[mMaked.SizeAllocated];
    //                    Array.Copy(mMaked.Buffer, tmp, mMaked.IndexW);
    //                    mMaked.Buffer = tmp;
    //                }
    //                else
    //                    mMaked.Buffer = new byte[mMaked.SizeAllocated];
    //            }
    //            catch (Exception e)
    //            {
    //                if (mMaked.Buffer != null)
    //                    mMaked.Buffer = null;

    //                throw new Exception($"Error allocating memory. size_ant: {ant}\r\nsize_alloc: {mMaked.SizeAllocated}\r\nsize_request: {size}. {e.Message}. AllocMaked()");
    //            }

    //            if (mMaked.Buffer == null)
    //                throw new Exception("Error allocating memory for the buffer in AllocMaked()");
    //        }
    //    }

    //    private void AllocPlain(int size)
    //    {
    //        if (size > (mPlain.Buffer.Length - mPlain.IndexW))
    //        {
    //            int ant = mPlain.Buffer.Length;

    //            if (size < 0)
    //                throw new Exception("Negative size. AllocPlain()");

    //            Array.Resize(ref mPlain.Buffer, mPlain.Buffer.Length + size);

    //            if (mPlain.Buffer == null)
    //                throw new Exception("Error allocating memory for the buffer in AllocPlain()");
    //        }
    //    }

    //    public ushort GetTipo()
    //    {
    //        return mTipo;
    //    }

    //    public int GetSize()
    //    {
    //        return mPlain.IndexW;
    //    }
    //    public WSABuf GetPlainBuf()
    //    {
    //        return new WSABuf { Length = (uint)mPlain.IndexW, Buffer = mPlain.Buffer };
    //    }

    //    public WSABuf GetMakedBuf()
    //    {
    //        return new WSABuf { Length = (uint)mMaked.IndexW, Buffer = mMaked.Buffer.CopyBytes(mMaked.IndexW) };
    //    }

    //    public int GetSizePlain()
    //    {
    //        return mPlain.SizeAllocated;
    //    }

    //    public int GetSizeMaked()
    //    {
    //        return mMaked.SizeAllocated;
    //    }
    //    public byte[] GetBuffer()
    //    {
    //        return mPlain.Buffer;
    //    }
    //    public int SizePlain()
    //    {
    //        return mPlain.Size;
    //    }

    //    public int SizeMaked()
    //    {
    //        return mMaked.Size;
    //    }
    //    ~Packet()
    //    {
    //        Destroy();
    //    }

    //    // Resto do código...
    //}
}
