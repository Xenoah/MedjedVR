using Basis.Network.Core;
using static SerializableBasis;

namespace Basis.Network
{
    /// <summary>
    /// メッセージHandlerの責務をまとめるクラスです。
    /// ClientConsole領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static class MessageHandler
    {
        /// <summary>
        /// OnDisconnectイベントを受け取り、関連するサーバー状態や送信処理へ反映します。
        /// </summary>
        public static void OnDisconnect(NetPeer peer, DisconnectInfo info)
        {
            BNL.LogError($"Peer {peer.Id} disconnected.");
        }

        /// <summary>
        /// OnReceiveイベントを受け取り、関連するサーバー状態や送信処理へ反映します。
        /// </summary>
        public static void OnReceive(ConsoleClientIdentity identity, NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod method)
        {
            if (peer.Id != 0) return;

            switch (channel)
            {
                case BasisNetworkCommons.AuthIdentityChannel:
                    AuthIdentityMessage(identity, peer, reader);
                    return; // 内部ですでに recycled 済み。
                case BasisNetworkCommons.metaDataChannel:
                    if (identity != null)
                    {
                        identity.Authenticated = true;
                    }
                    break;
                case BasisNetworkCommons.PlayerAvatarVeryLowChannel:
                case BasisNetworkCommons.PlayerAvatarVeryLowAdditionalChannel:
                case BasisNetworkCommons.PlayerAvatarLowChannel:
                case BasisNetworkCommons.PlayerAvatarLowAdditionalChannel:
                case BasisNetworkCommons.PlayerAvatarMediumChannel:
                case BasisNetworkCommons.PlayerAvatarMediumAdditionalChannel:
                case BasisNetworkCommons.PlayerAvatarHighChannel:
                case BasisNetworkCommons.PlayerAvatarHighAdditionalChannel:
                    // full avatar update は読み捨てる。
                    break;
                case BasisNetworkCommons.DisconnectionChannel:
                    // disconnection message は読み捨てる。
                    break;
                default:
                    // その他の channel は静かに読み捨てる。
                    break;
            }

            reader.Recycle();
        }

        /// <summary>
        /// 認証識別情報メッセージを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void AuthIdentityMessage(ConsoleClientIdentity identity, NetPeer peer, NetPacketReader reader)
        {
            if (identity != null && identity.TryRespondToChallenge(reader, out NetDataWriter writer))
            {
                peer.Send(writer, BasisNetworkCommons.AuthIdentityChannel, DeliveryMethod.ReliableOrdered);
            }
            else
            {
                BNL.LogError("Failed to respond to auth challenge!");
            }
            reader.Recycle();
        }
    }
}
