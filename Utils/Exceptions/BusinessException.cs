namespace VirtualQueueApi.Utils.Exceptions
{
    public class BusinessException : Exception
    {
        public int ErrorCode { get; }
        public BusinessException(string message) : base(message) { }
        public BusinessException(string message, int errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
        public BusinessException(string message, int errorCode, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
