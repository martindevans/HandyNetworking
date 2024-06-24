namespace HandyNetworking.SyncVar
{
    /// <summary>
    /// A SyncVar is a value with a single owner. Only the owner can change the value.
    /// Anyone else can try to set the value, and the owner will receive a request.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SyncVar<T>
    {
        private readonly ISender _sender;
        private readonly IReceiver _receiver;
        private readonly PacketReliability _reliability;

        public SyncVar(T value, ISender sender, IReceiver receiver, PacketReliability reliability = PacketReliability.ReliableSequenced)
        {
            _sender = sender;
            _receiver = receiver;
            _reliability = reliability;
        }

        /// <summary>
        /// Indicates if this sync var is owned by the local peer and can be changed
        /// </summary>
        public bool IsLocallyOwned { get; private set; }

        /// <summary>
        /// Get the latest value from this sync var
        /// </summary>
        public T Value
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
