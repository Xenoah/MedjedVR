using Basis.Network.Core;
using System;

/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    /// <summary>
    /// SceneDataメッセージの責務をまとめる構造体です。
    /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public struct SceneDataMessage
    {
        /// <summary>
        /// messageIndexを保持します。型は ushort で、関連処理から共有される値です。
        /// </summary>
        public ushort messageIndex;
        /// <summary>
        /// recipientsSizeを保持します。型は ushort で、関連処理から共有される値です。
        /// </summary>
        public ushort recipientsSize;
        /// <summary>
        /// null の場合は全員宛て。そうでなければ listed entry のみに送る。
        /// </summary>
        public ushort[] recipients;
        /// <summary>
        /// payloadを保持します。型は byte[] で、関連処理から共有される値です。
        /// </summary>
        public byte[] payload;

        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader Writer)
        {
            // messageIndex を安全に読む。
            if (!Writer.TryGetUShort(out messageIndex))
            {
                throw new ArgumentException("Failed to read messageIndex.");
            }
            // recipientsSize を安全に読む。
            if (Writer.TryGetUShort(out recipientsSize))
            {
                // 負値相当や異常な size を防ぐ。
                if (recipientsSize > Writer.AvailableBytes / sizeof(ushort))
                {
                    throw new ArgumentException($"Invalid recipientsSize: {recipientsSize}");
                }
                if (recipients == null || recipients.Length != recipientsSize)
                {
                    recipients = new ushort[recipientsSize];
                }
                for (int index = 0; index < recipientsSize; index++)
                {
                    if (!Writer.TryGetUShort(out recipients[index]))
                    {
                        throw new ArgumentException($"Failed to read recipient at index {index}.");
                    }
                }

                // 残り byte を payload として読む。
                if (Writer.AvailableBytes > 0)
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
            }
            else
            {
                recipients = null;
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

            // recipientsSize を決めて書く。
            if (recipients == null || recipients.Length == 0)
            {
                recipientsSize = 0;
            }
            else
            {
                recipientsSize = (ushort)recipients.Length;
            }
            Writer.Put(recipientsSize);

            // recipients array があれば書く。
            if (recipients != null && recipients.Length > 0)
            {
                for (int index = 0; index < recipientsSize; index++)
                {
                    Writer.Put(recipients[index]);
                }
            }

            // payload があれば書く。
            if (payload != null && payload.Length > 0)
            {
                Writer.Put(payload);
            }
        }
    }
}
