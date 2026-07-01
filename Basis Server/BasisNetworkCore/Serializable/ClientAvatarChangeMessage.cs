using Basis.Network.Core;

/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    /// <summary>
    /// クライアントアバターChangeメッセージの責務をまとめる構造体です。
    /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public struct ClientAvatarChangeMessage
    {
        // Downloading - URL から download を試みる。hash も存在することを確認する。
        // BuiltIn - Unity 内の addressable として load する。
        public byte loadMode;
        /// <summary>
        /// byteArrayを保持します。型は byte[] で、関連処理から共有される値です。
        /// </summary>
        public byte[] byteArray;
        // これを increment し、255 を超えたら wrap する。
        public byte LocalAvatarIndex;
        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader Writer)
        {
            // load mode を読む。
            loadMode = Writer.GetByte();
            // 指定 length で byte array を初期化する。
            ushort Length = Writer.GetUShort();
            if (Length == 0)
            {
                byteArray = null;
            }
            else
            {
                if (Length > Writer.AvailableBytes)
                {
                    byteArray = null;
                    throw new System.ArgumentException($"Avatar change length {Length} exceeds available data ({Writer.AvailableBytes} bytes).");
                }
                if (byteArray == null || byteArray.Length != Length)
                {
                    byteArray = new byte[Length];
                }

                // 各 byte を手動で array へ読む。
                Writer.GetBytes(byteArray, 0, byteArray.Length);
            }
            LocalAvatarIndex = Writer.GetByte();
        }
        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter Writer)
        {
            // load mode を書く。
            Writer.Put(loadMode);
            if (byteArray == null)
            {
                Writer.Put((ushort)0);
            }
            else
            {
                Writer.Put((ushort)byteArray.Length);
                Writer.Put(byteArray);
            }
            Writer.Put(LocalAvatarIndex);
        }
    }
}
