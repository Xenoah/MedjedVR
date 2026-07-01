using Basis.Network.Core;

/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    /// <summary>
    /// BasisP2PSignalメッセージの責務をまとめる構造体です。
    /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public struct BasisP2PSignalMessage
    {
        /// <summary>
        /// MaxTokenLengthを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public const int MaxTokenLength = 64;
        /// <summary>
        /// PublicKeySizeを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public const int PublicKeySize = 32;

        /// <summary>
        /// otherプレイヤーIdを保持します。型は ushort で、関連処理から共有される値です。
        /// </summary>
        public ushort otherPlayerId;
        /// <summary>
        /// sessionTokenを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string sessionToken;
        /// <summary>
        /// sender の X25519 ephemeral public key。
        /// server が relay し、2 peer が per-pair key を derive して direct (P2P) link を常に encrypt できるようにする。
        /// </summary>
        public byte[] ephemeralPublicKey;

        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            otherPlayerId = reader.GetUShort();
            sessionToken = reader.GetString(MaxTokenLength);
            byte hasKey = reader.GetByte();
            if (hasKey == 1 && reader.AvailableBytes >= PublicKeySize)
            {
                ephemeralPublicKey = new byte[PublicKeySize];
                reader.GetBytes(ephemeralPublicKey, PublicKeySize);
            }
            else
            {
                ephemeralPublicKey = null;
            }
        }

        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(otherPlayerId);
            writer.Put(sessionToken ?? string.Empty, MaxTokenLength);
            if (ephemeralPublicKey != null && ephemeralPublicKey.Length == PublicKeySize)
            {
                writer.Put((byte)1);
                writer.Put(ephemeralPublicKey);
            }
            else
            {
                writer.Put((byte)0);
            }
        }
    }
}
