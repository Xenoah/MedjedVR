using Basis.Network.Core;
/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    /// <summary>
    /// サーバーアバターChangeメッセージの責務をまとめる構造体です。
    /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public struct ServerAvatarChangeMessage
    {
        /// <summary>
        /// uShortプレイヤーIdを保持します。型は PlayerIdMessage で、関連処理から共有される値です。
        /// </summary>
        public PlayerIdMessage uShortPlayerId;
        /// <summary>
        /// clientアバターChangeメッセージを保持します。型は ClientAvatarChangeMessage で、関連処理から共有される値です。
        /// </summary>
        public ClientAvatarChangeMessage clientAvatarChangeMessage;
        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader Writer)
        {
            uShortPlayerId.Deserialize(Writer);
            clientAvatarChangeMessage.Deserialize(Writer);
        }
        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter Writer)
        {
            uShortPlayerId.Serialize(Writer);
            clientAvatarChangeMessage.Serialize(Writer);
        }
    }
}
