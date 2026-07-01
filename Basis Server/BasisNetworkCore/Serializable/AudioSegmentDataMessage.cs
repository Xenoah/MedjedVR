using Basis.Network.Core;

/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    [System.Serializable]
    /// <summary>
    /// AudioSegmentDataメッセージの責務をまとめる構造体です。
    /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public struct AudioSegmentDataMessage
    {
        /// <summary>
        /// SequenceNumberを保持します。型は byte で、関連処理から共有される値です。
        /// </summary>
        public byte SequenceNumber;
        /// <summary>
        /// TotalPlayedInSilenceを保持します。型は byte で、関連処理から共有される値です。
        /// </summary>
        public byte TotalPlayedInSilence;
        /// <summary>
        /// bufferを保持します。型は byte[] で、関連処理から共有される値です。
        /// </summary>
        public byte[] buffer;
        /// <summary>
        /// TotalLengthを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public int TotalLength;
        /// <summary>
        /// LengthUsedを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public int LengthUsed;
        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader Writer)
        {
            SequenceNumber = Writer.GetByte();
            TotalPlayedInSilence = Writer.GetByte();
            if (Writer.EndOfData)
            {
                buffer = null;
                TotalLength = 0;
                LengthUsed = 0;
            }
            else
            {
                buffer = Writer.GetRemainingBytes();
                TotalLength = buffer.Length;
                LengthUsed = TotalLength;
            }
        }
        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter Writer)
        {
            Writer.Put(SequenceNumber);
            Writer.Put(TotalPlayedInSilence);
            if (LengthUsed != 0)
            {
                Writer.Put(buffer, 0, LengthUsed);
            }
        }
    }
}