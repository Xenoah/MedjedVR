using Basis.Network.Core;
using System;

namespace Basis.Network.Core.Serializable
{
    /// <summary>
    /// SerializableBasisの責務をまとめるクラスです。
    /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static partial class SerializableBasis
    {
        /// <summary>
        /// ushort length と、それに続く同じ length の byte array で構成される。
        /// </summary>
        [System.Serializable]
        /// <summary>
        /// Bytesメッセージの責務をまとめる構造体です。
        /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
        /// </summary>
        public struct BytesMessage
        {
            /// <summary>
            /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
            /// </summary>
            public bool Deserialize(NetDataReader reader, out byte[] Data)
            {
                if (!reader.TryGetUShort(out ushort msgLength))
                {
                    BNL.LogError("unable to read the size of the data");
                    Data = null;
                    return false;
                }

                if (reader.AvailableBytes < msgLength)
                {
                    BNL.LogError($"BytesMessage: declared length {msgLength} exceeds available bytes {reader.AvailableBytes}; possible protocol mismatch or truncated packet.");
                    Data = null;
                    return false;
                }

                Data = new byte[msgLength];
                if (msgLength > 0)
                {
                    reader.GetBytes(Data, msgLength);
                }
                return true;
            }

            /// <summary>
            /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
            /// </summary>
            public readonly void Serialize(NetDataWriter writer, byte[] Data)
            {
                ushort Length = (ushort)Data.Length;
                if (Length == 0)
                {
                    BNL.LogError("this data does not belong on the network! was size 0");
                }
                writer.Put(Length);
                writer.Put(Data);
            }
        }
    }
}
