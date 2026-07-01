using Basis.Network.Core;
using System;
/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    /// <summary>
    /// RemoteアバターDataメッセージの責務をまとめる構造体です。
    /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public struct RemoteAvatarDataMessage
    {
        /// <summary>
        /// プレイヤーIdメッセージを保持します。型は PlayerIdMessage で、関連処理から共有される値です。
        /// </summary>
        public PlayerIdMessage PlayerIdMessage;
        /// <summary>
        /// messageIndexを保持します。型は byte で、関連処理から共有される値です。
        /// </summary>
        public byte messageIndex;
        /// <summary>
        /// payloadを保持します。型は byte[] で、関連処理から共有される値です。
        /// </summary>
        public byte[] payload;
        /// <summary>
        /// アバターLinkIndexを保持します。型は byte で、関連処理から共有される値です。
        /// </summary>
        public byte AvatarLinkIndex;
        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader Writer)
        {
            PlayerIdMessage.Deserialize(Writer);
            // AvatarLinkIndex を安全に読む。
            if (!Writer.TryGetByte(out AvatarLinkIndex))
            {
                throw new ArgumentException("Failed to read AvatarLinkIndex.");
            }
            if (!Writer.TryGetByte(out messageIndex))
            {
                throw new ArgumentException("Failed to read messageIndex.");
            }
            if (Writer.AvailableBytes != 0)
            {
                if (payload != null && payload.Length == Writer.AvailableBytes)
                {
                    Writer.GetBytes(payload, Writer.AvailableBytes);
                }
                else
                {
                    payload = Writer.GetRemainingBytes();
                }
            }
            else
            {
                payload = null;
            }
        }
        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter Writer)
        {
            PlayerIdMessage.Serialize(Writer);
            // AvatarLinkIndex を書く。
            Writer.Put(AvatarLinkIndex);
            // messageIndex を書く。
            Writer.Put(messageIndex);
            // payload があれば書く。
            if (payload != null && payload.Length != 0)
            {
                Writer.Put(payload);
            }
        }
    }
}
