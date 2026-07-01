using Basis.Network.Core;

namespace BasisNetworkCore.Serializable
{
    /// <summary>
    /// SerializableBasisの責務をまとめるクラスです。
    /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static partial class SerializableBasis
    {
        /// <summary>
        /// server/client stats の snapshot。wire format を扱いやすくするため fixed layout。
        /// </summary>
        public struct ServerStatisticMessage
        {
            /// <summary>
            /// Dataを保持します。型は byte[] で、関連処理から共有される値です。
            /// </summary>
            public byte[] Data;
            /// <summary>
            /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
            /// </summary>
            public void Serialize(NetDataWriter w)
            {
                w.Put(Data);
            }

            /// <summary>
            /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
            /// </summary>
            public void Deserialize(NetDataReader r)
            {
                Data = r.GetRemainingBytes();
            }
        }
    }
}
