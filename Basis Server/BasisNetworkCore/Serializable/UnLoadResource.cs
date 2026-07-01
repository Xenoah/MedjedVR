using Basis.Network.Core;
/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    /// <summary>
    /// Un読み込みリソースの責務をまとめる構造体です。
    /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public struct UnLoadResource
    {
        /// <summary>
        /// 0 = Game object, 1 = Scene,
        /// </summary>
        public byte Mode;
        /// <summary>
        /// LoadedNetIDを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string LoadedNetID;
        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public bool Deserialize(NetDataReader Writer)
        {
            int Bytes = Writer.AvailableBytes;
            if (Writer.TryGetByte(out Mode) == false)
            {
                return false;
            }

            if (Writer.TryGetString(out LoadedNetID) == false)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter Writer)
        {
            Writer.Put(Mode);
            Writer.Put(LoadedNetID);
        }
    }
}
