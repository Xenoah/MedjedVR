using Basis.Network.Core;

using static SerializableBasis;
namespace DarkRift.Basis_Common.Serializable
{
    /// <summary>
    /// SerializableBasisの責務をまとめるクラスです。
    /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static partial class SerializableBasis
    {
        /// <summary>
        /// OwnershipTransferメッセージの責務をまとめる構造体です。
        /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
        /// </summary>
        public struct OwnershipTransferMessage
        {
            /// <summary>
            /// playerIdメッセージを保持します。型は PlayerIdMessage で、関連処理から共有される値です。
            /// </summary>
            public PlayerIdMessage playerIdMessage;
            /// <summary>
            /// ownershipIDを保持します。型は string で、関連処理から共有される値です。
            /// </summary>
            public string ownershipID;
            /// <summary>
            /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
            /// </summary>
            public void Deserialize(NetDataReader Writer)
            {
                playerIdMessage.Deserialize(Writer);
                Writer.Get(out ownershipID, 256);
            }
            /// <summary>
            /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
            /// </summary>
            public void Serialize(NetDataWriter Writer)
            {
                playerIdMessage.Serialize(Writer);
                Writer.Put(ownershipID);
            }
        }
    }
}
