using Basis.Network.Core;
/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    /// <summary>
    /// サーバー準備完了メッセージの責務をまとめる構造体です。
    /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public struct ServerReadyMessage
    {
        /// <summary>
        /// playerIdメッセージを保持します。型は PlayerIdMessage で、関連処理から共有される値です。
        /// </summary>
        public PlayerIdMessage playerIdMessage;//who this came from
        /// <summary>
        /// local準備完了メッセージを保持します。型は ReadyMessage で、関連処理から共有される値です。
        /// </summary>
        public ReadyMessage localReadyMessage;
        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader Writer)
        {
            playerIdMessage.Deserialize(Writer);
            localReadyMessage.Deserialize(Writer);
        }
        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter Writer)
        {
            playerIdMessage.Serialize(Writer);
            localReadyMessage.Serialize(Writer);
        }
    }
}
