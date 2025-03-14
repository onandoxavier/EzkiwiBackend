namespace VirtualQueueApi.Utils.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public int ErrorCode { get; }
        public EntityNotFoundException(string message) : base(message) { }
        public EntityNotFoundException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
        public EntityNotFoundException(string message, int errorCode, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
