using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.SuperSocket.CRYPTS
{
    public class Crypt : IDisposable
    {
        private uint[] m_key = new uint[2];
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

        public void Decrypt(byte[] cipher, int size, byte[] plain)
        {
            if (cipher == null)
                throw new Exception("Error cipher is null, Crypt.Decrypt()");

            if (plain == null)
                throw new Exception("Error plain is null, Crypt.Decrypt()");

            int i = 0;

            if (size > 0)
            {
                for (i = 0; i < (size >= 4 ? 4 : size); ++i)
                    plain[i] = (byte)(cipher[i] ^ BitConverter.GetBytes(m_key[0])[i]);

                for (i = 4; i < size; ++i)
                    plain[i] = (byte)(cipher[i] ^ plain[i - 4]);

                if (!CheckKey(plain[0]))
                    throw new Exception("Error failed to decrypt, Crypt.Decrypt()");
            }
            else
            {
                throw new Exception("Error size is 0, Crypt.Decrypt()");
            }
        }

        public void Encrypt(byte[] plain, int size, byte[] cipher)
        {
            if (plain == null)
                throw new Exception("Error plain is null, Crypt.Encrypt()");

            if (cipher == null)
                throw new Exception("Error cipher is null, Crypt.Encrypt()");

            int i = 0;

            if (size > 0)
            {
                for (i = 0; i < (size >= 4 ? 4 : size); ++i)
                    cipher[i] = (byte)(plain[i] ^ BitConverter.GetBytes(m_key[0])[i]);

                for (i = 4; i < size; ++i)
                    cipher[i] = (byte)(plain[i] ^ plain[i - 4]);

                if (!CheckKey((byte)(cipher[0] ^ m_key[0])))
                    throw new Exception("Error failed to encrypt, Crypt.Encrypt()");
            }
            else
            {
                throw new Exception("Error size is 0, Crypt.Encrypt()");
            }
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
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Crypt()
        // {
        //     // Não altere este código. Coloque o código de limpeza no método 'Dispose(bool disposing)'
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Não altere este código. Coloque o código de limpeza no método 'Dispose(bool disposing)'
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
