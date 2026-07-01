using Basis.Network.Core;
/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    /// <summary>
    /// 準備完了メッセージの責務をまとめる構造体です。
    /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public struct ReadyMessage
    {
        /// <summary>
        /// playerMetaDataメッセージを保持します。型は ClientMetaDataMessage で、関連処理から共有される値です。
        /// </summary>
        public ClientMetaDataMessage playerMetaDataMessage;
        /// <summary>
        /// clientアバターChangeメッセージを保持します。型は ClientAvatarChangeMessage で、関連処理から共有される値です。
        /// </summary>
        public ClientAvatarChangeMessage clientAvatarChangeMessage;
        /// <summary>
        /// localアバター同期メッセージを保持します。型は LocalAvatarSyncMessage で、関連処理から共有される値です。
        /// </summary>
        public LocalAvatarSyncMessage localAvatarSyncMessage;
        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader Writer)
        {
            playerMetaDataMessage.Deserialize(Writer);
            clientAvatarChangeMessage.Deserialize(Writer);
            localAvatarSyncMessage.Deserialize(Writer);
        }
        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter Writer)
        {
            playerMetaDataMessage.Serialize(Writer);
            clientAvatarChangeMessage.Serialize(Writer);
            localAvatarSyncMessage.Serialize(Writer, (Basis.Network.Core.Compression.BasisAvatarBitPacking.BitQuality)localAvatarSyncMessage.DataQualityLevel);
        }
        /// <summary>
        /// WasDeserializedCorrectlyを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public bool WasDeserializedCorrectly()
        {
            if(clientAvatarChangeMessage.byteArray == null)
            {
                return false;
            }
            if(localAvatarSyncMessage.array == null)
            {
                return false;
            }
            return true;
        }
    }
}
