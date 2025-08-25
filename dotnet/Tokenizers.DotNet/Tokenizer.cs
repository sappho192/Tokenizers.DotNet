using CsBindgen;
using System.Text;

namespace Tokenizers.DotNet
{
    public unsafe sealed class Tokenizer : IDisposable
    {
        private readonly string sessionId;

        /// <summary>
        /// Throws exception if error occured during tokenizer initialization.
        /// </summary>
        /// <param name="vocabPath"></param>
        public Tokenizer(string vocabPath)
        {
            fixed (char* p = vocabPath)
            {
                var tokenizerResult = NativeMethods.tokenizer_initialize((ushort*)p, vocabPath.Length);
                ValidateErrorCode(tokenizerResult.error_code);
                try
                {
                    sessionId = Encoding.UTF8.GetString(tokenizerResult.data->ptr, tokenizerResult.data->length);
                }
                finally
                {
                    NativeMethods.free_u8_string(tokenizerResult.data);
                }
            }
        }

        ~Tokenizer()
        {
            if (sessionId is null)
            {
                return;
            }

            fixed (char* cp = sessionId)
            {
                NativeMethods.tokenizer_cleanup((ushort*)cp, sessionId.Length);
            }
        }

        /// <summary>
        /// Throws exception if error occured from native library.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <exception cref="TokenizerException"></exception>
        public uint[] Encode(string text)
        {
            fixed (char* p = sessionId)
            {
                fixed (char* pt = text)
                {
                    var tokenizerResult = NativeMethods.tokenizer_encode((ushort*)p, sessionId.Length, (ushort*)pt, text.Length);
                    ValidateErrorCode(tokenizerResult.error_code);
                    try
                    {
                        return tokenizerResult.data->ToArray<uint>();
                    }
                    finally
                    {
                        NativeMethods.free_u8_string(tokenizerResult.data);
                    }
                }
            }
        }

        /// <summary>
        /// Throws exception if error occured from native library.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public string Decode(uint[] tokens)
        {
            fixed (uint* p = tokens)
            {
                fixed (char* cp = sessionId)
                {
                    var tokenizerResult = NativeMethods.tokenizer_decode(
                        (ushort*)cp, sessionId.Length,
                        p, tokens.Length);
                    ValidateErrorCode(tokenizerResult.error_code);
                    try
                    {
                        return Encoding.UTF8.GetString(tokenizerResult.data->ptr, tokenizerResult.data->length);
                    }
                    finally
                    {
                        NativeMethods.free_u8_string(tokenizerResult.data);
                    }
                }
            }
        }

        /// <summary>
        /// Throws exception if error occured from native library.
        /// </summary>
        /// <returns></returns>
        public string GetVersion()
        {
            fixed (char* cp = sessionId)
            {
                var tokenizerResult = NativeMethods.get_version((ushort*)cp, sessionId.Length);
                ValidateErrorCode(tokenizerResult.error_code);
                try
                {
                    return Encoding.UTF8.GetString(tokenizerResult.data->ptr, tokenizerResult.data->length);
                }
                finally
                {
                    NativeMethods.free_u8_string(tokenizerResult.data);
                }
            }
        }

        public void Dispose()
        {
            fixed (char* cp = sessionId)
            {
                var errorCode = NativeMethods.tokenizer_cleanup((ushort*)cp, sessionId.Length);
                if (errorCode == TokenizerErrorCode.InvalidSessionId)
                {
                    return;
                }

                ValidateErrorCode(errorCode);
            }

            GC.SuppressFinalize(this);
        }

        private string GetLastError()
        {
            var errorBytes = NativeMethods.get_last_error_message();
            try
            {
                return Encoding.UTF8.GetString(errorBytes->ptr, errorBytes->length);
            }
            finally
            {
                NativeMethods.free_u8_string(errorBytes);
            }
        }

        private void ValidateErrorCode(TokenizerErrorCode errorCode)
        { 
            switch (errorCode)
            {
                case TokenizerErrorCode.Success:
                    return;

                case TokenizerErrorCode.InvalidSessionId:
                    throw new ObjectDisposedException(nameof(Tokenizer));

                default:
                    throw new TokenizerException(GetLastError(), (int)errorCode);
            }
        }
    }
}
