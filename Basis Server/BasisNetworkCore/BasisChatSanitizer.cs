using System.Text;

namespace Basis.Network.Core
{
    /// <summary>
    /// 不正な UTF-16 / UTF-8 を作らずに、Basis chat transport の制限を適用する。
    /// </summary>
    public static class BasisChatSanitizer
    {
        /// <summary>
        /// MaxメッセージCharactersを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public const int MaxMessageCharacters = 256;
        /// <summary>
        /// MaxメッセージBytesを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public const int MaxMessageBytes = SerializableBasis.ChatMessage.MaxPayloadBytes;

        /// <summary>
        /// Sanitizeを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static string Sanitize(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return string.Empty;
            }

            string sanitized = ClampUtf16Length(message, MaxMessageCharacters);
            while (sanitized.Length > 0 && Encoding.UTF8.GetByteCount(sanitized) > MaxMessageBytes)
            {
                sanitized = TrimLastScalar(sanitized);
            }

            return sanitized;
        }

        /// <summary>
        /// ClampUtf16Lengthを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private static string ClampUtf16Length(string text, int maxLength)
        {
            if (text.Length <= maxLength)
            {
                return text;
            }

            int length = maxLength;
            if (length > 0 && char.IsHighSurrogate(text[length - 1]))
            {
                length--;
            }

            return text.Substring(0, length);
        }

        /// <summary>
        /// TrimLastScalarを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private static string TrimLastScalar(string text)
        {
            int length = text.Length;
            // length == 0 は caller 側で起こらないよう保証するため、ここでは確認しない。

            if (length >= 2 &&
                char.IsHighSurrogate(text[length - 2]) &&
                char.IsLowSurrogate(text[length - 1]))
            {
                return text.Substring(0, length - 2);
            }

            return text.Substring(0, length - 1);
        }
    }
}
