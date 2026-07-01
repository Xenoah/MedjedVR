using Basis.Network.Core;
using System;

namespace BasisNetworkCore.Serializable
{
    /// <summary>
    /// SerializableBasisの責務をまとめるクラスです。
    /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static partial class SerializableBasis
    {
        /// <summary>
        /// アバター読み込みDataメッセージの責務をまとめる構造体です。
        /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
        /// </summary>
        public struct AvatarLoadDataMessage
        {
            /// <summary>
            /// messageIndexを保持します。型は byte で、関連処理から共有される値です。
            /// </summary>
            public byte messageIndex;
            /// <summary>
            /// payloadSizeを保持します。型は ushort で、関連処理から共有される値です。
            /// </summary>
            public ushort payloadSize;
            /// <summary>
            /// payloadを保持します。型は byte[] で、関連処理から共有される値です。
            /// </summary>
            public byte[] payload;
            /// <summary>
            /// WhoSentUsThisを保持します。型は ushort で、関連処理から共有される値です。
            /// </summary>
            public ushort WhoSentUsThis;
            /// <summary>
            /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
            /// </summary>
            public void Deserialize(NetDataReader Writer)
            {
                // messageIndex を安全に読む。
                if (!Writer.TryGetByte(out messageIndex))
                {
                    throw new ArgumentException("Failed to read messageIndex.");
                }
                if (Writer.TryGetUShort(out WhoSentUsThis))
                {
                    throw new ArgumentException("Failed to read who sent us this!");
                }
                // recipientsSize を安全に読む。
                if (Writer.TryGetUShort(out payloadSize))
                {
                    // 負値相当や異常な size を防ぐ。
                    if (payloadSize > Writer.AvailableBytes / sizeof(ushort))
                    {
                        throw new ArgumentException($"Invalid recipientsSize: {payloadSize}");
                    }
                    if (payload == null || payload.Length != payloadSize)
                    {
                        payload = new byte[payloadSize];
                    }
                    if (!Writer.TryGetBytesWithLength(out payload))
                    {
                        throw new ArgumentException($"Failed to read payload!.");
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
                // messageIndex を書く。
                Writer.Put(messageIndex);
                Writer.Put(WhoSentUsThis);
                // recipientsSize を決めて書く。
                if (payload == null || payload.Length == 0)
                {
                    payloadSize = 0;
                }
                else
                {
                    payloadSize = (ushort)payload.Length;
                }
                Writer.Put(payloadSize);
                // recipients array があれば書く。
                if (payload != null && payload.Length > 0)
                {
                    Writer.Put(payload);
                }
            }
        }
    }
}
