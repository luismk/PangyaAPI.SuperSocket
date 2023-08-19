using PangyaAPI.SuperSocket.Ext;
using PangyaAPI.Utilities.Cryptography;
using System;

namespace PangyaAPI.SuperSocket.Cryptor
{
    public class Crypt : IDisposable
    {
        private readonly uint[] m_key = new uint[2];
        private bool disposedValue;

        public Crypt()
        {
            Array.Clear(m_key, 0, m_key.Length);
        }

        public byte InitKey(byte _keyHigh, byte _keyLow)
        {
            ushort posDic = (ushort)(_keyHigh << 8 | _keyLow);

            m_key[0] = KeyDictionary.Keys[posDic];
            m_key[1] = KeyDictionary.Keys[4096 + posDic];

            return (byte)m_key[1];
        }

        public void Decrypt(byte[] _cipher, int size, byte[] @plain)
        {
            if (_cipher == null)
                throw new AppException("Error _plain is nullptr, crypt::decrypt()", Tools.STDA_MAKE_ERROR(STDA_ERROR_TYPE.CRYPT, 1, 0));

            if (plain == null)
                throw new AppException("Error _plain is nullptr, crypt::decrypt()", Tools.STDA_MAKE_ERROR(STDA_ERROR_TYPE.CRYPT, 1, 0));

            
            if (_cipher.Length < 5)
                throw new AppException($"Packet too small ({_cipher.Length} < 5), crypt::decrypt()", Tools.STDA_MAKE_ERROR(STDA_ERROR_TYPE.CRYPT, 1, 0));

            if (size > 0)
            {
                int i;
                for (i = 0; i < (size >= 4 ? 4 : size); ++i)//decripta o header packet
                    plain[i] = ((byte)(_cipher[i] ^ BitConverter.GetBytes(m_key[0])[i]));

                for (i = 4; i < size; ++i)//decripta o resto do pacote
                {
                    plain[i] = (byte)(_cipher[i] ^ plain[i - 4]);
                }

                //checagem do packet
                if (!CheckKey(plain[0]))
                    throw new AppException($"Error nao conseguiu decrypt, crypt::decrypt()", Tools.STDA_MAKE_ERROR(STDA_ERROR_TYPE.CRYPT, 3, 0));

            }
            else
            {
                throw new AppException($"Error size is 0, Crypt.Decrypt()", Tools.STDA_MAKE_ERROR(STDA_ERROR_TYPE.CRYPT, 4, 0));
            }
        }

        public void Encrypt(byte[] plain, int size, byte[] @cipher)
        {
            if (cipher == null)
                throw new AppException("Error _plain is nullptr, crypt::decrypt()", Tools.STDA_MAKE_ERROR(STDA_ERROR_TYPE.CRYPT, 1, 0));

            if (plain == null)
                throw new AppException("Error _plain is nullptr, crypt::decrypt()", Tools.STDA_MAKE_ERROR(STDA_ERROR_TYPE.CRYPT, 1, 0));

            if (cipher.Length < 5)
                throw new AppException($"Packet too small ({cipher.Length} < 5), crypt::decrypt()", Tools.STDA_MAKE_ERROR(STDA_ERROR_TYPE.CRYPT, 1, 0));
            if (size > 0)
            {

                int i;
                for (i = 0; i < (size >= 4 ? 4 : size); ++i)
                    cipher[i] = (byte)(plain[i] ^ BitConverter.GetBytes(m_key[0])[i]);

                for (i = 4; i < size; ++i)
                    cipher[i] = (byte)(plain[i] ^ plain[i - 4]);

                if (!CheckKey((byte)(cipher[0] ^ m_key[0])))
                    throw new AppException($"Error nao conseguiu encrypt, crypt::encrypt()", Tools.STDA_MAKE_ERROR(STDA_ERROR_TYPE.CRYPT, 2, 0));
            }
            else
            {
                throw new AppException($"Error size is 0, Crypt.Encrypt()", Tools.STDA_MAKE_ERROR(STDA_ERROR_TYPE.CRYPT, 4, 0));
            }
        }

        public byte[] Encrypt(byte[] plain)
        {
            var buffer = new byte[plain.Length + 8];
            var pLen = buffer.Length - 3;

            var u = plain.Length;
            var x = (u + u / 255) & 0xff;
            var v = (u - x) / 255;
            var y = (v + v / 255) & 0xff;
            var w = (v - y) / 255;
            var z = (w + w / 255) & 0xff;
            //packet header
            buffer[0] = 0;
            buffer[1] = (byte)((pLen >> 0) & 0xFF);
            buffer[2] = (byte)((pLen >> 8) & 0xFF);
            //seq
            buffer[3] = (byte)(m_key[0] ^ m_key[1]);

            buffer[5] = (byte)z;
            buffer[6] = (byte)y;
            buffer[7] = (byte)x;

            Array.Copy(plain, 0, buffer, 8, plain.Length);

            for (var i = buffer.Length - 1; i >= 10; i--) buffer[i] ^= buffer[i - 4];

            buffer[7] ^= (byte)m_key[1];

            return buffer;
        }
        private bool CheckKey(byte key)
        {
            return key == (byte)m_key[1];
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Array.Clear(m_key, 0, m_key.Length);
                }

                disposedValue = true;
            }
        }

        ~Crypt()
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
