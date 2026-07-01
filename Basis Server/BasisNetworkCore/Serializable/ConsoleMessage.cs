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
        /// ConsoleDataの責務をまとめる構造体です。
        /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
        /// </summary>
        public struct ConsoleData
        {
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
                int bytesAvailable = reader.AvailableBytes;
                if (bytesAvailable > 0)
                {
                    messageIndex = reader.GetByte();

                    ushort payloadSize = reader.GetUShort();

                    if (payloadSize > 0)
                    {
                        if (payloadSize > reader.AvailableBytes)
                        {
                            BNL.LogError($"ConsoleData payload {payloadSize} exceeds available data ({reader.AvailableBytes} bytes).");
                            array = new byte[0];
                            return;
                        }
                        if (array == null || array.Length != payloadSize)
                        {
                            array = new byte[payloadSize];
                        }
                        reader.GetBytes(array, payloadSize);
                    }
                    else
                    {
                        array = new byte[0]; // Handle zero-length array case
                    }
                }
                else
                {
                    BNL.LogError($"Unable to read remaining bytes, available: {bytesAvailable}");
                }
            }

            /// <summary>
            /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
            /// </summary>
            public void Serialize(NetDataWriter writer)
            {
                writer.Put(messageIndex);

                ushort size = (array != null) ? (ushort)array.Length : (ushort)0;
                writer.Put(size);

                if (size > 0)
                {
                    writer.Put(array);
                }
            }
        }
    }
}
