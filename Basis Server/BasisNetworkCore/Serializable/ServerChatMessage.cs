using Basis.Network.Core;

/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    /// <summary>
    /// server-to-client chat message。chat payload を sender の player ID で包む。
    /// </summary>
    public struct ServerChatMessage
    {
        /// <summary>
        /// playerIdメッセージを保持します。型は PlayerIdMessage で、関連処理から共有される値です。
        /// </summary>
        public PlayerIdMessage playerIdMessage;
        /// <summary>
        /// chatメッセージを保持します。型は ChatMessage で、関連処理から共有される値です。
        /// </summary>
        public ChatMessage chatMessage;

        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            playerIdMessage.Deserialize(reader);
            chatMessage.Deserialize(reader);
        }

        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            playerIdMessage.Serialize(writer);
            chatMessage.Serialize(writer);
        }
    }
}
