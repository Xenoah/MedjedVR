using Basis.Network.Core;

/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    /// <summary>
    /// クライアントMetaDataメッセージの責務をまとめる構造体です。
    /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public struct ClientMetaDataMessage
    {
        /// <summary>
        /// playerUUIDを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string playerUUID;
        /// <summary>
        /// playerDisplayNameを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string playerDisplayName;
        /// <summary>
        /// playerPlatformを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string playerPlatform;
        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader Writer)
        {
            Writer.Get(out playerUUID);
            Writer.Get(out playerDisplayName);
            Writer.Get(out playerPlatform);

        }
        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter Writer)
        {
            if (string.IsNullOrEmpty(playerUUID) == false)
            {
                Writer.Put(playerUUID);
            }
            else
            {
                Writer.Put("Failure");
            }
            if (string.IsNullOrEmpty(playerDisplayName) == false)
            {
                Writer.Put(playerDisplayName);
            }
            else
            {
                Writer.Put("Failure");
            }
            if (string.IsNullOrEmpty(playerPlatform) == false)
            {
                Writer.Put(playerPlatform);
            }
            else
            {
                Writer.Put("Failure");
            }
        }
    }
}
