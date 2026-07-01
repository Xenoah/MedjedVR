using Basis.Network.Core;
/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    /// <summary>
    /// サーバーSceneDataメッセージの責務をまとめる構造体です。
    /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public struct ServerSceneDataMessage
    {
        /// <summary>
        /// playerIdメッセージを保持します。型は PlayerIdMessage で、関連処理から共有される値です。
        /// </summary>
        public PlayerIdMessage playerIdMessage;
        /// <summary>
        /// sceneDataメッセージを保持します。型は RemoteSceneDataMessage で、関連処理から共有される値です。
        /// </summary>
        public RemoteSceneDataMessage sceneDataMessage;

        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader Writer)
        {
            // playerIdMessage を読む。
            playerIdMessage.Deserialize(Writer);
            sceneDataMessage.Deserialize(Writer);
        }
        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter Writer)
        {
            // playerIdMessage と sceneDataMessage を書く。
            playerIdMessage.Serialize(Writer);
            sceneDataMessage.Serialize(Writer);
        }
    }
}
