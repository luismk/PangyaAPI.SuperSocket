using PangyaAPI.SuperSocket.Interface;

namespace PangyaAPI.SuperSocket.SocketBase
{
   public class PangyaPacketReceive : IReceiveFilter<IRequestInfo>
    {
        protected IReceiveFilter<IRequestInfo> _nextReceiveFilter;
        IReceiveFilter<IRequestInfo> IReceiveFilter<IRequestInfo>.NextReceiveFilter => _nextReceiveFilter;

        public PangyaPacketReceive(IReceiveFilter<IRequestInfo> nextReceiveFilter)
        {
            _nextReceiveFilter = nextReceiveFilter;
        }
        /// <summary>
        /// implemente a leitura do pacote aqui:
        /// </summary>
        /// <param name="readBuffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="toBeCopied"></param>
        /// <param name="rest"></param>
        /// <returns></returns>
        public IRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            rest = 0;
            return default(IRequestInfo); // Substitua pela instância de TRequestInfo filtrada
        }

        public int LeftBufferSize
        {
            get { return 0; } // Defina o tamanho do buffer restante
        }

        public void Reset()
        {
            // Implemente a reinicialização do filtro, se necessário
        }

        public FilterState State => _state;
        protected FilterState _state
        {
            get; set;
        }
    }
    /// <summary>
    /// Implementação de um filtro de recebimento usando a interface IRequestInfo.
    /// </summary>
    /// <typeparam name="TRequestInfo">O tipo de informação de requisição.</typeparam>
    public class PangyaPacketReceive<TRequestInfo> : IReceiveFilter<TRequestInfo>
        where TRequestInfo : IRequestInfo
    {
        private IReceiveFilter<TRequestInfo> _nextReceiveFilter;

        public PangyaPacketReceive(IReceiveFilter<TRequestInfo> nextReceiveFilter)
        {
            _nextReceiveFilter = nextReceiveFilter;
        }

        public TRequestInfo Filter(byte[] readBuffer, int offset, int length, bool toBeCopied, out int rest)
        {
            rest = 0;
            return default(TRequestInfo); // Substitua pela instância de TRequestInfo filtrada
        }

        public int LeftBufferSize
        {
            get { return 0; } // Defina o tamanho do buffer restante
        }

        public IReceiveFilter<TRequestInfo> NextReceiveFilter
        {
            get { return _nextReceiveFilter; }
        }

        public void Reset()
        {
            // Implemente a reinicialização do filtro, se necessário
        }

        public FilterState State
        {
            get { return FilterState.Normal; } // Defina o estado do filtro
        }
    }
}
