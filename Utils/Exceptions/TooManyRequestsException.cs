namespace VirtualQueueApi.Utils.Exceptions
{
    public class TooManyRequestsException : Exception
    {
        public int ErrorCode { get; }
        public TooManyRequestsException(string message) : base(message) { }
        public TooManyRequestsException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
        public TooManyRequestsException(string message, int errorCode, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
