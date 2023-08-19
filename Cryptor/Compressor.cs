using PangyaAPI.Utilities.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.SuperSocket.Cryptor
{
    public class Compressor
    {

        public Compressor()
        {
        }

        ~Compressor() { /* Implement cleanup if necessary */ }

        public byte[] CompressData(byte[] uncompress, uint sizeUncompress, ref uint @sizeCompress)
        {

            if (uncompress == null)
                throw new Exception("Erro, uncompress é nulo. Compressor::CompressData()");

            if (sizeUncompress <= 0)
                throw new Exception("Erro, sizeUncompress é menor ou igual a 0. Compressor::CompressData()");

            try
            {
              var @compress =  MiniLzo.Compress(uncompress, out sizeCompress);
                if (@compress.Length != sizeCompress)
                    throw new Exception($"Erro ao comprimir dados. Compressor::CompressData().");

                else
                    return compress;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + $" Compressor::CompressData().");
            }
        }
        public void DecompressData(byte[] compress, uint sizeCompress, byte[] uncompress, ref uint sizeUncompress, uint sizeDecompress)
        {
            if (uncompress == null)
                throw new Exception("Erro, uncompress é nulo. Compressor::DecompressData()");

            if (compress == null)
                throw new Exception("Erro, compress é nulo. Compressor::DecompressData()");

            if (sizeUncompress <= 0)
                throw new Exception("Erro, sizeUncompress é menor ou igual a 0. Compressor::DecompressData()");

            if (sizeCompress <= 0)
                throw new Exception("Erro, sizeCompress é menor ou igual a 0. Compressor::DecompressData()");

            try
            {
                if (!MiniLzo.Lzo1XDecompress(compress, uncompress, ref sizeUncompress))
                    throw new Exception($"Erro ao descomprimir dados. Compressor::DecompressData().");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + $" Compressor::DecompressData().");
            }

            if (sizeUncompress != sizeDecompress && Math.Abs(sizeUncompress - sizeDecompress) != 1)
                throw new Exception($"Erro, tamanho descomprimido não corresponde ao tamanho desejado. Tamanho descomprimido: {sizeUncompress}, Tamanho desejado: {sizeDecompress}. Compressor::DecompressData()");
        }

    }
}
