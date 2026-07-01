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
        /// サーバーNetIDメッセージの責務をまとめる構造体です。
        /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
        /// </summary>
        public struct ServerNetIDMessage
        {
            /// <summary>
            /// NetIDメッセージを保持します。型は NetIDMessage で、関連処理から共有される値です。
            /// </summary>
            public NetIDMessage NetIDMessage;
            /// <summary>
            /// UshortUniqueIDメッセージを保持します。型は UshortUniqueIDMessage で、関連処理から共有される値です。
            /// </summary>
            public UshortUniqueIDMessage UshortUniqueIDMessage;
            /// <summary>
            /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
            /// </summary>
            public void Deserialize(NetDataReader reader)
            {
                NetIDMessage.Deserialize(reader);
                UshortUniqueIDMessage.Deserialize(reader);
            }

            /// <summary>
            /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
            /// </summary>
            public void Serialize(NetDataWriter writer)
            {
                NetIDMessage.Serialize(writer);
                UshortUniqueIDMessage.Serialize(writer);
            }
        }
    }
}
