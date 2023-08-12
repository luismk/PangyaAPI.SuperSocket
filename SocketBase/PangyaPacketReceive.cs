using PangyaAPI.SuperSocket.Interface;
using System;

namespace PangyaAPI.SuperSocket.SocketBase
{
    public class PangyaRequestInfo : IRequestInfo
    {
        public short PacketID { get; set; }
        public byte[] Message { get; set; }

        public uint m_oid { get; set; }

        public PangyaRequestInfo()
        {

        }
    }
    /// <summary>
    /// Implementação de um filtro de recebimento usando a interface IRequestInfo.
    /// </summary>
    /// <typeparam name="TRequestInfo">O tipo de informação de requisição.</typeparam>
    public class PangyaReceiveFilter : IReceiveFilter<PangyaRequestInfo>
    {

        /// <summary>
        /// implemente a leitura do pacote aqui:
        /// </summary>
        /// <param name="readBuffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="toBeCopied"></param>
        /// <param name="rest"></param>
        /// <returns></returns>
        public PangyaRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            rest = 0;

            try
            {
                var body = new byte[length];
                Buffer.BlockCopy(readBuffer, offset, body, 0, length);

                var deviceRequest = new PangyaRequestInfo()
                {
                    PacketID = BitConverter.ToInt16(new byte[] { body[5], body[6] }, 0),
                    Message = body,
                };

                return deviceRequest;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public void Reset()
        {
            // Implemente a reinicialização do filtro, se necessário
        }

        public int LeftBufferSize { get; }
        private IReceiveFilter<PangyaRequestInfo> _nextReceiveFilter;


        public IReceiveFilter<PangyaRequestInfo> NextReceiveFilter
        {
            get { return _nextReceiveFilter; }
        }

        public FilterState State => _state;
        protected FilterState _state
        {
            get; set;
        }
    }
}
