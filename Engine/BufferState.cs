namespace PangyaAPI.SuperSocket.Engine
{
    sealed class BufferState : BufferBaseState
    {
        public byte[] Buffer { get; private set; }

        public BufferState(byte[] buffer)
        {
            Buffer = buffer;
        }
    }
}