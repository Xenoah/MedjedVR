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
        /// UshortUniqueIDメッセージの責務をまとめる構造体です。
        /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
        /// </summary>
        public struct UshortUniqueIDMessage
        {
            /// <summary>
            /// UniqueIDUshortを保持します。型は ushort で、関連処理から共有される値です。
            /// </summary>
            public ushort UniqueIDUshort;

            /// <summary>
            /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
            /// </summary>
            public void Deserialize(NetDataReader reader)
            {
                int bytes = reader.AvailableBytes;
                if (bytes != 0)
                {
                    UniqueIDUshort = reader.GetUShort();
                }
                else
                {
                    BNL.LogError($"Unable to read remaining bytes: {bytes}");
                }
            }

            /// <summary>
            /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
            /// </summary>
            public void Serialize(NetDataWriter writer)
            {
                writer.Put(UniqueIDUshort);
            }
        }
    }
}
