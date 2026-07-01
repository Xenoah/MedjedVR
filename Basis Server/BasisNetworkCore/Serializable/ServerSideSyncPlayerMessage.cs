using Basis.Network.Core;
/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    /// <summary>
    /// サーバーSide同期プレイヤーメッセージの責務をまとめる構造体です。
    /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public struct ServerSideSyncPlayerMessage
    {
        /// <summary>
        /// playerIdメッセージを保持します。型は PlayerIdMessage で、関連処理から共有される値です。
        /// </summary>
        public PlayerIdMessage playerIdMessage;
        /// <summary>
        /// intervalを保持します。型は byte で、関連処理から共有される値です。
        /// </summary>
        public byte interval;
        /// <summary>
        /// sequenceを保持します。型は byte で、関連処理から共有される値です。
        /// </summary>
        public byte sequence;
        /// <summary>
        /// avatarSerializationを保持します。型は LocalAvatarSyncMessage で、関連処理から共有される値です。
        /// </summary>
        public LocalAvatarSyncMessage avatarSerialization;
        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader Writer)
        {
            playerIdMessage.Deserialize(Writer);//2bytes
            Writer.Get(out interval);//1 byte
            Writer.Get(out sequence);//1 byte
            avatarSerialization.Deserialize(Writer);
        }
        /// <summary>
        /// quality と additional-data の有無が channel から derive される場合に deserialize する (server->client path)。
        /// </summary>
        public void Deserialize(NetDataReader Writer, byte channelDerivedQuality, bool hasAdditionalData)
        {
            playerIdMessage.Deserialize(Writer);//2bytes
            Writer.Get(out interval);//1 byte
            Writer.Get(out sequence);//1 byte
            avatarSerialization.Deserialize(Writer, channelDerivedQuality, hasAdditionalData);
        }
        /// <summary>
        /// channel variant に基づき、byte / ushort playerID で deserialize する。
        /// </summary>
        public void Deserialize(NetDataReader Writer, byte channelDerivedQuality, bool hasAdditionalData, bool largeId)
        {
            playerIdMessage.Deserialize(Writer, largeId);
            Writer.Get(out interval);
            Writer.Get(out sequence);
            avatarSerialization.Deserialize(Writer, channelDerivedQuality, hasAdditionalData);
        }
        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter Writer)
        {
            playerIdMessage.Serialize(Writer);
            Writer.Put(interval);
            Writer.Put(sequence);
            avatarSerialization.Serialize(Writer, (Basis.Network.Core.Compression.BasisAvatarBitPacking.BitQuality)avatarSerialization.DataQualityLevel);
        }
    }
}
