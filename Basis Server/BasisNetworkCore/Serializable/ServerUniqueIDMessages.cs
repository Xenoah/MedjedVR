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
        /// サーバーUniqueIDMessagesの責務をまとめる構造体です。
        /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
        /// </summary>
        public struct ServerUniqueIDMessages
        {
            /// <summary>
            /// メッセージCountを保持します。型は ushort で、関連処理から共有される値です。
            /// </summary>
            public ushort MessageCount;
            /// <summary>
            /// Messagesを保持します。型は ServerNetIDMessage[] で、関連処理から共有される値です。
            /// </summary>
            public ServerNetIDMessage[] Messages;

            /// <summary>
            /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
            /// </summary>
            public void Deserialize(NetDataReader reader)
            {
                int bytes = reader.AvailableBytes;
                if (bytes >= sizeof(ushort))
                {
                    MessageCount = reader.GetUShort();
                    if (Messages == null || Messages.Length != MessageCount)
                    {
                        Messages = new ServerNetIDMessage[MessageCount];
                    }
                    for (int Index = 0; Index < MessageCount; Index++)
                    {
                        Messages[Index] = new ServerNetIDMessage();
                        Messages[Index].Deserialize(reader);
                    }
                }
                else
                {
                    Messages = null;
                    BNL.LogError($"Unable to read remaining bytes for MessageCount. Available: {bytes}");
                }
            }

            /// <summary>
            /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
            /// </summary>
            public void Serialize(NetDataWriter writer)
            {
                if (Messages != null)
                {
                    MessageCount = (ushort)Messages.Length;
                    writer.Put(MessageCount);
                    for (int Index = 0; Index < MessageCount; Index++)
                    {
                        ServerNetIDMessage message = Messages[Index];
                        message.Serialize(writer);
                    }
                }
                else
                {
                    BNL.LogError("Unable to serialize. Messages array was null.");
                }
            }
        }
    }
}
