namespace Tokenizers.DotNet
{
    public class TokenizerException : Exception
    {
        public int ErrorCode { get; }

        public TokenizerException(string message, int errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
