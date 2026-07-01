using Basis.Network.Core;

/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    /// <summary>
    /// AdditionalアバターDataの責務をまとめる構造体です。
    /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public struct AdditionalAvatarData
    {
        /// <summary>
        /// PayloadSizeを保持します。型は byte で、関連処理から共有される値です。
        /// </summary>
        public byte PayloadSize;
        /// <summary>
        /// messageIndexを保持します。型は byte で、関連処理から共有される値です。
        /// </summary>
        public byte messageIndex;
        /// <summary>
        /// arrayを保持します。型は byte[] で、関連処理から共有される値です。
        /// </summary>
        public byte[] array;

        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            if (reader.TryGetByte(out PayloadSize))
            {
                if (PayloadSize == 0)
                {
                    return;
                }
                if (reader.TryGetByte(out messageIndex))
                {
                    if (PayloadSize > reader.AvailableBytes)
                    {
                        BNL.LogError("AdditionalAvatarData payload exceeds available data!");
                        return;
                    }
                    if (array == null || array.Length != PayloadSize)
                    {
                        array = new byte[PayloadSize];
                    }
                    reader.GetBytes(array, PayloadSize);
                }
                else
                {
                    BNL.LogError("trying to write data that does not exist! messageIndex");
                }
            }
            else
            {
                BNL.LogError("trying to write data that does not exist! PayloadSize");
            }
        }
        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            if (array == null)
            {
                PayloadSize = 0;
                writer.Put(PayloadSize);
                return;
            }

            if (array.Length > 255)
            {
                BNL.LogError("Larger than 255 cannot send this Additional Avatar Data");
                PayloadSize = 0;
                writer.Put(PayloadSize);
                return;
            }
            PayloadSize = (byte)array.Length;

            writer.Put(PayloadSize);
            writer.Put(messageIndex);

            if (PayloadSize > 0)
            {
                writer.Put(array, 0, PayloadSize);
            }
        }
    }
}
