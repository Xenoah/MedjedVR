using Basis.Network.Core;
using System;

/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    /// <summary>
    /// RemoteSceneDataメッセージの責務をまとめる構造体です。
    /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public struct RemoteSceneDataMessage
    {
        /// <summary>
        /// messageIndexを保持します。型は ushort で、関連処理から共有される値です。
        /// </summary>
        public ushort messageIndex;
        /// <summary>
        /// payloadを保持します。型は byte[] で、関連処理から共有される値です。
        /// </summary>
        public byte[] payload;
        /// <summary>
        /// payloadLengthを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public int payloadLength;

        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            // messageIndex を安全に読む。
            if (!reader.TryGetUShort(out messageIndex))
            {
                throw new ArgumentException("Failed to read messageIndex.");
            }

            int payloadSize = reader.AvailableBytes;

            if (payloadSize > 0)
            {
                payload = new byte[payloadSize];
                reader.GetBytes(payload, payloadSize);
                payloadLength = payloadSize;
            }
        }

        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(messageIndex);

            int len = payloadLength > 0 ? payloadLength : (payload != null ? payload.Length : 0);
            if (payload != null && len > 0)
            {
                writer.Put(payload, 0, len);
            }
        }

        /// <summary>
        /// Releaseを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public void Release()
        {
            if (payload != null)
            {
                payload = null;
                payloadLength = 0;
            }
        }
    }
}
