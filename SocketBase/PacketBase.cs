using PangyaAPI.Cryptor.HandlePacket;
using PangyaAPI.SuperSocket.Cryptor;
using PangyaAPI.SuperSocket.Ext;
using PangyaAPI.SuperSocket.Interface;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static PangyaAPI.SuperSocket.Ext.Tools;
namespace PangyaAPI.SuperSocket.SocketBase
{

    public class OffsetIndex
    {
        public byte[] Buffer;
        public int IndexR;
        public int IndexW;
        public int Size;
        public int SizeAllocated;

        public void Clear()
        {
            if (Buffer != null)
            {
                Array.Clear(Buffer, 0, Buffer.Length);
            }
            else
            {
                Buffer = new byte[1024];
            }
            IndexR = 0;
            IndexW = 0;
            Size = 0;
        }

        public void ResetRead()
        {
            IndexR = 0;
        }

        public void ResetWrite()
        {
            IndexR = 0;
            IndexW = 0;
        }

        public void Reset()
        {
            ResetRead();
            ResetWrite();
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PacketHead
    {
        public byte LowKey;
        public ushort Size;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PacketHeadClient : PacketHead
    {
        public byte Seq;
    }
    public class ConversionByte
    {
        private const byte CB_BASE_256 = 10;
        private const byte CB_BASE_255 = 20;
        private const byte CB_SEQ_NORMAL = 1;
        private const byte CB_SEQ_INVERTIDA = 2;
        private const byte CB_PARAM_DEFAULT = 0;

        private U unionConvertido;
        private byte m_flag;

        public ConversionByte(uint dwConvertido, byte flag = CB_PARAM_DEFAULT)
        {
            unionConvertido.dwConvertido = dwConvertido;
            m_flag = flag;

            if (m_flag != CB_PARAM_DEFAULT)
                Invert();
        }

        public ConversionByte(byte[] ucpConvertido, byte flag = CB_PARAM_DEFAULT)
        {
            unionConvertido.dwConvertido = ucpConvertido != null && ucpConvertido.Length >= 4 ? BitConverter.ToUInt32(ucpConvertido, 0) : 0;
            m_flag = flag;

            if (m_flag != CB_PARAM_DEFAULT)
                Invert();
        }

        private void Invert()
        {
            if ((m_flag & CB_BASE_255) != 0)
            {
                unionConvertido.dwConvertido = GetNumberIS();
                unionConvertido.dwConvertido = GetNumberBase256();
            }
            else
            {
                unionConvertido.dwConvertido = GetNumberBase255();
                unionConvertido.dwConvertido = GetNumberIS();
            }
        }

        public uint GetNumberNS() => unionConvertido.dwConvertido;

        public uint GetNumberIS()
        {
            uint ulNumber = 0;

            ulNumber = unionConvertido.stConvertido.d;
            ulNumber |= (uint)(unionConvertido.stConvertido.c << 8);
            ulNumber |= (uint)(unionConvertido.stConvertido.b << 16);
            ulNumber |= (uint)(unionConvertido.stConvertido.a << 24);

            return ulNumber;
        }

        private uint ulNumber_temp;

        public byte[] GetLPUCNS()
        {
            ulNumber_temp = GetNumberNS();

            return BitConverter.GetBytes(ulNumber_temp);
        }

        public byte[] GetLPUCIS()
        {
            ulNumber_temp = GetNumberIS();

            return BitConverter.GetBytes(ulNumber_temp);
        }

        public uint GetNumberBase256() => GetNumberNS() * 255 / 256 + 1;

        public uint GetNumberBase255() => ((unionConvertido.dwConvertido / 255) << 8) | (unionConvertido.dwConvertido % 255);

        public uint GetISNumberBase256() => (uint)(unionConvertido.dwConvertido * 255L / 256 + 1);

        public uint GetISNumberBase255() => ((GetNumberIS() / 255) << 8) | (GetNumberIS() % 255);

        public int PutNumberBuffer(ref byte[] buffer)
        {
            if (buffer == null || buffer.Length < sizeof(uint))
                return -1;

            //errado aqui, alguma coisa modificou para zerar tudo

            Buffer.BlockCopy(buffer, 4+1, BitConverter.GetBytes(unionConvertido.dwConvertido), 0, sizeof(uint));

            return sizeof(uint);
        }

        private struct U
        {
            public uint dwConvertido;

            public struct Convertido
            {
                public byte a;
                public byte b;
                public byte c;
                public byte d;
            }

            public Convertido stConvertido;
        }
    }
    public class AppPacketBase 
    {
        private OffsetIndex mPlain = new OffsetIndex();
        private OffsetIndex mMaked = new OffsetIndex();
        public ushort m_Tipo;
        int CHUNCK_ALLOC = 1024;
        public AppPacketBase()
        {
            mPlain.Buffer = new byte[0];
            mMaked.Buffer = new byte[0];
            m_Tipo = ushort.MaxValue;
        }
        public AppPacketBase(ushort ID)
        {
            m_Tipo = ID;
            mPlain.Clear();
            mMaked.Clear();

            AddPlain(BitConverter.GetBytes(m_Tipo), sizeof(ushort));
        }


        //public void SetKey(byte key)
        //{
        //    Message = Pang.ClientDecrypt(MessageCrypted, key);

        //    _stream = new MemoryStream(Message);

        //    _stream.Seek(2, SeekOrigin.Current); //Seek Inicial
        //    Reader = new PangyaBinaryReader(_stream);
        //    Writer = new PangyaBinaryWriter();
        //}

        /// <summary>
        /// Decripta o pacote cliente
        /// </summary>
        public void UnMake(byte _Key)
        {
            using (Crypt _crypt = new Crypt())
            {
                PacketHeadClient phc = new PacketHeadClient();

                int index = 0;

                phc = mMaked.Buffer.ByteArrayToStructure<PacketHeadClient>(index);
                index += Marshal.SizeOf(phc);

                if (phc.Size > mMaked.IndexW)
                    throw new Exception("Erro: Unknown Packet. AppPacketBase::UnMake()");

                _crypt.InitKey(_Key, phc.LowKey);

                byte[] decrypt = new byte[phc.Size];
                try
                {
                    _crypt.Decrypt(mMaked.Buffer.Clone(index), phc.Size, decrypt);
                }
                catch (Exception e)
                {
                    if (decrypt != null)
                        decrypt = null;

                    throw;
                }
                // Reset Plain
                mPlain.Reset();

                AddPlain(decrypt.Clone(1), phc.Size - 1);
            }
        }
        ///// <summary>
        ///// não testado ainda
        ///// </summary>
        ///// <param name="_key"></param>
        ///// <exception cref="AppException"></exception>
        //public void Make(byte _key)
        //{
        //    var encrypt = GetBytes.ServerEncrypt(Key);

        //    Clear();

        //    Write(encrypt);

        //    /// ACRISIO REF ///
        //    //if (GetBytes == null)
        //    //{
        //    //  throw new  AppException("Error buf is nullptr em packet::makeFull()", 15);
        //    //}

        //    //Crypt _crypt = null;
        //    //PacketHead ph = new PacketHead();

        //    //ConversionByte cb = new ConversionByte((uint)GetBytes.Length, 10);

        //    //byte[] tmp = new byte[cb.GetNumberIS() + 5 + 5];
        //    //uint compressOut = 0;

        //    //Compressor _compress = new Compressor();
        //    //try
        //    //{
        //    //   tmp = _compress.CompressData(GetBytes, (uint)GetBytes.Length, ref compressOut);
        //    //}
        //    //catch (Exception e)
        //    //{
        //    //    // Clean
        //    //    tmp.CleanUp(_compress, _crypt);
        //    //    throw;
        //    //}

        //    //// Make Packet Head
        //    //ph.Size = (byte)(compressOut + 5); // key low and size raw decompressed

        //    //Random rand = new Random(((int)DateTime.UtcNow.Ticks * 7) * ph.Size);
        //    //ph.LowKey = (byte)(rand.Next() & 255);

        //    //_crypt = new Crypt();

        //    //tmp[0] = _crypt.InitKey(_key, ph.LowKey);

        //    //cb.PutNumberBuffer(ref tmp);

        //    //// Maked Reset
        //    //Clear();
        //    //// Convert PacketHead to byte array
        //    //byte[] phBytes = ph.ConvertArray();
        //    //Write(phBytes);//escreve, porem com tamanho, assim o pacote fica correto na escrita
        //    //WriteZero(ph.Size);
        //    //var m_marked = GetBytes;
        //    //try
        //    //{
        //    //    _crypt.Encrypt(tmp, ph.Size - 8, m_marked );
        //    //}
        //    //catch (Exception e)
        //    //{
        //    //    // Clean
        //    //    tmp.CleanUp(_compress, _crypt);
        //    //    throw;
        //    //}


        //    //// Clean
        //    //tmp.CleanUp(_compress, _crypt);
        //    //m_marked.DebugDump();
        //    //var decrypt = Pang.ServerDecrypt(m_marked, _key);
        //    //decrypt.DebugDump();
        //}

        /// <summary>
        /// Create Packet Hello!
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void MakeRaw()
        {
            if (mPlain == null || mPlain.Buffer == null)
            {
                throw new Exception("Erro, Message é nulo em AppPacketBase::MakeRaw()");
            }

            PacketHead ph = new PacketHead
            {
                LowKey = 0, // low part of key random - 0 nesse pacote porque ele é o primeiro que passa a chave
                Size = (ushort)(mPlain.IndexW + 1)
            };
            switch (m_Tipo)
            {
                case 0x0B00:// Pacote Raw Login
                    mPlain.Buffer[1] = 0;
                    break;
                case 0x2E:      // Pacote Raw MSN
                case 0x3F:      // Pacote Raw Game
                case 0x1388:	// Pacote Raw Rank
                    break;
                default:
                    break;
            }
            mMaked.Reset();
            AddMaked(ph);
            AddByte(0);// byte com valor 0 para dizer que é um pacote raw
            AddMaked(mPlain.Buffer, mPlain.IndexW);
            mMaked.Buffer.DebugDump();
        }
        private void AddAlloc(OffsetIndex index, byte[] @buf, int size)
        {
            Alloc(ref index, size);
            Buffer.BlockCopy(@buf, 0, index.Buffer, index.IndexW, size);
            index.IndexW += size;
        }

        public void AddPlain(byte[] @buf, int size)
        {
            if (@buf == null)
                throw new ArgumentNullException(nameof(@buf));

            Alloc(ref mPlain, size);
            Add(ref mPlain, @buf, size);
        }

        public void ReadPlain(byte[] @buf, int size)
        {
            if (buf == null)
                throw new ArgumentNullException(nameof(@buf));

            if (mPlain.IndexW < (size + mPlain.IndexR))
                throw new Exception("Error not enough for read in Packet.ReadPlain()");

            Buffer.BlockCopy(mPlain.Buffer, mPlain.IndexR, @buf, 0, size);
            mPlain.IndexR += size;
        }
        public void ReadPlain(object @value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));


            int size = Marshal.SizeOf(value);
            byte[] buf = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, ptr, true);
            Marshal.Copy(ptr, buf, 0, size);
            Marshal.FreeHGlobal(ptr);

            if (buf == null)
                throw new ArgumentNullException(nameof(buf));

            Buffer.BlockCopy(mPlain.Buffer, mPlain.IndexR, @buf, 0, size);
            mPlain.IndexR += size;
        }

        public void AddMaked(byte[] buf, int size)
        {
            if (buf == null)
                throw new ArgumentNullException(nameof(buf));

            Alloc(ref mMaked, size);
            Add(ref mMaked, buf, size);
        }
        public void AddMaked(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));


            int size = Marshal.SizeOf(value);
            byte[] buf = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, ptr, true);
            Marshal.Copy(ptr, buf, 0, size);
            Marshal.FreeHGlobal(ptr);

            if (buf == null)
                throw new ArgumentNullException(nameof(buf));

            Alloc(ref mMaked, size);
            Add(ref mMaked, buf, size);
        }

        public void ReadMaked(byte[] @buf, int size)
        {
            if (@buf == null)
                throw new ArgumentNullException(nameof(@buf));

            if (mMaked.IndexW < (size + mMaked.IndexR))
                throw new Exception("Error not enough for read in Packet.ReadMaked()");

            Buffer.BlockCopy(mMaked.Buffer, mMaked.IndexR, @buf, 0, size);
            mMaked.IndexR += size;
        }
        private void Destroy()
        {
            if (mPlain.Buffer != null)
                mPlain.Buffer = null;

            if (mMaked.Buffer != null)
                mMaked.Buffer = null;
        }

        private void InitMaked(int size)
        {
            mMaked.SizeAllocated = size;
            mMaked.Buffer = new byte[size];
        }

        private void InitPlain(ushort tipo)
        {
            m_Tipo = tipo;
            mPlain.Buffer = new byte[0];
            mMaked.Buffer = new byte[0];

            AddPlain(BitConverter.GetBytes(m_Tipo), sizeof(ushort));
        }

        private void Reset()
        {
            mPlain.IndexR = 0;
            mPlain.IndexW = 0;
            mMaked.IndexR = 0;
            mMaked.IndexW = 0;
        }

        private void Add(byte[] buf, int size, bool _)
        {
            Array.Copy(buf, 0, mMaked.Buffer, mMaked.IndexW, size);
            mMaked.IndexW += size;
        }

        private void Add(ref OffsetIndex index, byte[] buf, int size)
        {
            Buffer.BlockCopy(buf, 0, index.Buffer, index.IndexW, size);            
            index.IndexW += size;
        }
        private void Read(byte[] buf, int size)
        {
            Array.Copy(mMaked.Buffer, mMaked.IndexR, buf, 0, size);
            mMaked.IndexR += size;
        }
        private void Read(ref OffsetIndex index, byte[] buf, int size)
        {
            Array.Copy(index.Buffer, index.IndexR, buf, 0, size);
            index.IndexR += size;
        }

        public ulong ReadUInt64()
        {
            byte[] bytes = new byte[sizeof(ulong)];
            ReadPlain(bytes, sizeof(ulong));
            return BitConverter.ToUInt64(bytes, 0);
        }

        public uint ReadUInt32()
        {
            byte[] bytes = new byte[sizeof(uint)];
            ReadPlain(bytes, sizeof(uint));
            return BitConverter.ToUInt32(bytes, 0);
        }

        public ushort ReadUInt16()
        {
            byte[] bytes = new byte[sizeof(ushort)];
            ReadPlain(bytes, sizeof(ushort));
            return BitConverter.ToUInt16(bytes, 0);
        }

        public byte ReadUInt8()
        {
            byte[] bytes = new byte[sizeof(byte)];
            ReadPlain(bytes, sizeof(byte));
            return bytes[0];
        }

        public void AddString(string str)
        {
            byte[] strBytes = Encoding.UTF8.GetBytes(str);
            AddUInt16((ushort)strBytes.Length);
            AddPlain(strBytes, strBytes.Length);
        }

        public string ReadString()
        {
            ushort len = ReadUInt16();
            byte[] strBytes = new byte[len];
            ReadPlain(strBytes, len);
            return Encoding.UTF8.GetString(strBytes);
        }
        public void AddBuffer(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer), "Buffer cannot be null.");

            AddUInt32((uint)buffer.Length);
            AddPlain(buffer, buffer.Length);
        }

        public void ReadBuffer(out byte[] buffer)
        {
            uint len = ReadUInt32();
            buffer = new byte[len];
            ReadPlain(buffer, (int)len);
        }

        public void AddZeroByte(int size)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than zero.");

            byte[] zeros = new byte[size];
            AddPlain(zeros, size);
        }

        public void AddQWord(ulong qword)
        {
            byte[] bytes = BitConverter.GetBytes(qword);
            AddPlain(bytes, sizeof(ulong));
        }

        public void ReadQWord(out ulong qword)
        {
            byte[] bytes = new byte[sizeof(ulong)];
            ReadPlain(bytes, sizeof(ulong));
            qword = BitConverter.ToUInt64(bytes, 0);
        }

        public void AddDWord(uint dword)
        {
            byte[] bytes = BitConverter.GetBytes(dword);
            AddPlain(bytes, sizeof(uint));
        }

        public void ReadDWord(out uint dword)
        {
            byte[] bytes = new byte[sizeof(uint)];
            ReadPlain(bytes, sizeof(uint));
            dword = BitConverter.ToUInt32(bytes, 0);
        }

        public void AddWord(ushort word)
        {
            byte[] bytes = BitConverter.GetBytes(word);
            AddPlain(bytes, sizeof(ushort));
        }

        public void ReadWord(out ushort word)
        {
            byte[] bytes = new byte[sizeof(ushort)];
            ReadPlain(bytes, sizeof(ushort));
            word = BitConverter.ToUInt16(bytes, 0);
        }

        public void AddByte(byte b)
        {
            AddPlain(new byte[] { b }, 1);
        }

        public void ReadByte(out byte b)
        {
            byte[] bytes = new byte[1];
            ReadPlain(bytes, 1);
            b = bytes[0];
        }

        public void AddInt64(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            AddPlain(bytes, sizeof(long));
        }

        public void ReadInt64(out long value)
        {
            byte[] bytes = new byte[sizeof(long)];
            ReadPlain(bytes, sizeof(long));
            value = BitConverter.ToInt64(bytes, 0);
        }

        public void AddInt32(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            AddPlain(bytes, sizeof(int));
        }

        public void ReadInt32(out int value)
        {
            byte[] bytes = new byte[sizeof(int)];
            ReadPlain(bytes, sizeof(int));
            value = BitConverter.ToInt32(bytes, 0);
        }
        public void AddFloat(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            AddPlain(bytes, sizeof(float));
        }

        public void ReadFloat(out float value)
        {
            byte[] bytes = new byte[sizeof(float)];
            ReadPlain(bytes, sizeof(float));
            value = BitConverter.ToSingle(bytes, 0);
        }

        public void AddDouble(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            AddPlain(bytes, sizeof(double));
        }

        public void ReadDouble(out double value)
        {
            byte[] bytes = new byte[sizeof(double)];
            ReadPlain(bytes, sizeof(double));
            value = BitConverter.ToDouble(bytes, 0);
        }

        public void ReadString(out string str)
        {
            short length;
            ReadInt16(out length);

            byte[] bytes = new byte[length];
            ReadPlain(bytes, length);

            str = Encoding.ASCII.GetString(bytes);
        }

        public void AddWString(string str)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(str);
            AddInt16((short)bytes.Length);
            AddPlain(bytes, bytes.Length);
        }

        public void ReadWString(out string str)
        {
            short length;
            ReadInt16(out length);

            byte[] bytes = new byte[length];
            ReadPlain(bytes, length);

            str = Encoding.Unicode.GetString(bytes);
        }
        public void AddUInt16(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            AddPlain(bytes, sizeof(ushort));
        }
        public void AddUInt32(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            AddPlain(bytes, sizeof(uint));
        }

        public void AddInt16(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            AddPlain(bytes, sizeof(short));
        }

        public void ReadUInt16(out ushort value)
        {
            byte[] bytes = new byte[sizeof(ushort)];
            ReadPlain(bytes, sizeof(ushort));
            value = BitConverter.ToUInt16(bytes, 0);
        }
        public void ReadInt16(out short value)
        {
            byte[] bytes = new byte[sizeof(short)];
            ReadPlain(bytes, sizeof(short));
            value = BitConverter.ToInt16(bytes, 0);
        }

        public void AddUInt8(byte value)
        {
            AddPlain(new byte[] { value }, sizeof(byte));
        }

        public void ReadUInt8(out byte value)
        {
            byte[] bytes = new byte[sizeof(byte)];
            ReadPlain(bytes, sizeof(byte));
            value = bytes[0];
        }
        //public void AddCompressed(byte[] data)
        //{
        //    int compressedSize;
        //    byte[] compressedData = CompressData(data, out compressedSize);
        //    AddInt32(compressedSize);
        //    AddPlain(compressedData, compressedSize);
        //}


        private void AddPlain(byte[] buf, int index, int size)
        {
            if (buf == null)
                throw new ArgumentException("Error arguments invalid, _buf is null in packet.AddPlain()");

            Alloc(ref mPlain, size);
            Add(ref mPlain, buf, size);
        }

        private void AddMaked(byte[] buf, int index, int size)
        {
            if (buf == null)
                throw new ArgumentException("Error arguments invalid, _buf is null in packet.AddMaked()");

            Alloc(ref mMaked, size);
            Add(ref mMaked, buf, size);
        }

        public void Alloc(ref OffsetIndex index, int size)
        {
            if (size > (index.SizeAllocated - index.IndexW))
            {
                int ant = index.SizeAllocated;

                if (size < 0)
                {
                    throw new Exception("Negative size. packet::alloc()");
                }

                index.SizeAllocated += ((size - (index.SizeAllocated - index.IndexW)) / CHUNCK_ALLOC + 1) * CHUNCK_ALLOC;

                try
                {
                    byte[] tmp = new byte[index.SizeAllocated];

                    if (index.Buffer != null)
                    {
                        Buffer.BlockCopy(index.Buffer, 0, tmp, 0, index.IndexW);
                    }

                    if (index.Buffer != null)
                    {
                        index.Buffer = null; // Liberar memória do buffer anterior
                    }

                    index.Buffer = tmp;
                }
                catch (Exception e)
                {
                    if (index.Buffer != null)
                    {
                        index.Buffer = null; // Liberar memória do buffer anterior
                    }

                    throw new Exception("Error ao alocar memoria. size_ant: " + ant +
                                        "\r\nsize_alloc: " + index.SizeAllocated +
                                        "\r\nsize_request: " + size + ". " + e.Message + ". packet::alloc()");
                }

                if (index.Buffer == null)
                {
                    throw new Exception("Error ao alocar memoria para o buffer em packet::alloc()");
                }
            }
        }
        private void AllocMaked(int size)
        {
            if (size > (mMaked.SizeAllocated - mMaked.IndexW))
            {
                int ant = mMaked.SizeAllocated;

                if (size < 0)
                    throw new Exception("Negative size. AllocMaked()");

                mMaked.SizeAllocated += ((size - (mMaked.SizeAllocated - mMaked.IndexW)) / CHUNCK_ALLOC + 1) * CHUNCK_ALLOC;

                try
                {
                    if (mMaked.Buffer != null)
                    {
                        byte[] tmp = new byte[mMaked.SizeAllocated];
                        Array.Copy(mMaked.Buffer, tmp, mMaked.IndexW);
                        mMaked.Buffer = tmp;
                    }
                    else
                        mMaked.Buffer = new byte[mMaked.SizeAllocated];
                }
                catch (Exception e)
                {
                    if (mMaked.Buffer != null)
                        mMaked.Buffer = null;

                    throw new Exception($"Error allocating memory. size_ant: {ant}\r\nsize_alloc: {mMaked.SizeAllocated}\r\nsize_request: {size}. {e.Message}. AllocMaked()");
                }

                if (mMaked.Buffer == null)
                    throw new Exception("Error allocating memory for the buffer in AllocMaked()");
            }
        }

        private void AllocPlain(int size)
        {
            if (size > (mPlain.Buffer.Length - mPlain.IndexW))
            {
                int ant = mPlain.Buffer.Length;

                if (size < 0)
                    throw new Exception("Negative size. AllocPlain()");

                Array.Resize(ref mPlain.Buffer, mPlain.Buffer.Length + size);

                if (mPlain.Buffer == null)
                    throw new Exception("Error allocating memory for the buffer in AllocPlain()");
            }
        }

        public ushort GetTipo()
        {
            return m_Tipo;
        }

        public int GetSize()
        {
            return mPlain.IndexW;
        }
        public WSABuf GetPlainBuf()
        {
            return new WSABuf { Length = (uint)mPlain.IndexW, Buffer = mPlain.Buffer };
        }

        public WSABuf GetMakedBuf()
        {
            return new WSABuf { Length = (uint)mMaked.IndexW, Buffer = mMaked.Buffer };
        }

        public int GetSizePlain()
        {
            return mPlain.SizeAllocated;
        }

        public int GetSizeMaked()
        {
            return mMaked.SizeAllocated;
        }
        public byte[] GetBuffer()
        {
            return mPlain.Buffer;
        }
        public int SizePlain()
        {
            return mPlain.Size;
        }

        public int SizeMaked()
        {
            return mMaked.Size;
        }
       
    }
}
