namespace VirtualQueueApi.Utils.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public int ErrorCode { get; }
        public UnauthorizedException(string message) : base(message) { }
        public UnauthorizedException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
        public UnauthorizedException(string message, int errorCode, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
