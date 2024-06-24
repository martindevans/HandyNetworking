using HandySerialization;

namespace HandyNetworking.SyncVar
{
    public abstract class SyncVar<TRequestChange, TSetValue, T>
        : IDisposable
        where TRequestChange : struct, IByteSerializable<TRequestChange>
        where TSetValue : struct, IByteSerializable<TSetValue>
    {
        private readonly bool _isServer;
        private readonly ISender _sender;
        private readonly IReceiver _receiver;

        private readonly byte _channel;
        private readonly PacketReliability _reliability;

        private IDisposable? _requestSub;

        public SyncVar(bool isServer, ISender sender, IReceiver receiver, byte channel, PacketReliability reliability = PacketReliability.ReliableSequenced)
        {
            _isServer = isServer;
            _sender = sender;
            _receiver = receiver;
            _channel = channel;
            _reliability = reliability;

            _requestSub = receiver.Subscribe<TRequestChange>(ReceiveRequest);
        }

        private void ReceiveRequest(PeerId sender, TRequestChange request)
        {
            //if (!_isServer)
            //    return;

            //if (TryCreateStateChange(request, out var change))
            //{
            //    ApplyStateChange(change);
            //    _sender.Send(, change, _channel, _reliability);
            //}
        }

        public void Dispose()
        {
            _requestSub?.Dispose();
            _requestSub = null;
        }
    }
}
