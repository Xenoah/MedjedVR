using Basis.Network.Core;
using BasisNetworkServer.BasisNetworking;
using static SerializableBasis;

namespace BasisNetworkServer
{
    /// <summary>
    /// BasisサーバーイベントRouterの責務をまとめるクラスです。
    /// Server領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static class BasisServerEventsRouter
    {
        /// <summary>
        /// 処理イベントを処理します。受信データを検証し、必要な状態更新や再配信を行います。
        /// </summary>
        public static void HandleEvent(NetPacketReader reader, NetPeer peer)
        {
            byte eventType = reader.GetByte();

            switch (eventType)
            {
                case BasisNetworkCommons.EventType_CameraShutterSound:
                    HandleCameraShutterSound(peer, eventType);
                    reader.Recycle();
                    break;

                case BasisNetworkCommons.EventType_CameraCountdown:
                    HandleCameraCountdown(reader, peer, eventType);
                    break;

                case BasisNetworkCommons.EventType_PlayerTempBlock:
                    BasisNetworkHandleTempBlock.HandleEvent(reader, peer, eventType);
                    break;

                case BasisNetworkCommons.EventType_AvatarRateChange:
                    HandleAvatarRateChange(reader, peer, eventType);
                    break;

                case BasisNetworkCommons.EventType_PlayerChatTyping:
                    BasisNetworkHandleChatTyping.HandleEvent(reader, peer, eventType);
                    break;
                case BasisNetworkCommons.EventType_TalkModeChanged:
                    HandleTalkModeChanged(reader, peer, eventType);
                    break;

                case BasisNetworkCommons.EventType_MuteStateChanged:
                    HandleMuteStateChanged(reader, peer, eventType);
                    break;

                case BasisNetworkCommons.EventType_ErrorReport:
                    BasisNetworkHandleErrorReport.HandleEvent(reader, peer, eventType);
                    break;

                default:
                    BNL.LogError($"Unknown EventsChannel event type: {eventType}");
                    reader.Recycle();
                    break;
            }
        }

        /// <summary>
        /// 処理カメラShutterSoundを処理します。受信データを検証し、必要な状態更新や再配信を行います。
        /// </summary>
        private static void HandleCameraShutterSound(NetPeer peer, byte eventType)
        {
            ushort peerId = (ushort)peer.Id;

            NetDataWriter writer = NetworkServer.RentWriter();
            writer.Put(eventType);

            CameraShutterSoundMessage outMsg = new CameraShutterSoundMessage
            {
                PlayerID = peerId,
            };
            outMsg.Serialize(writer);

            NetworkServer.BroadcastMessageToClients(writer, BasisNetworkCommons.EventsChannel, peer, NetworkServer.PeerSnapshot, DeliveryMethod.Sequenced);
            NetworkServer.ReturnWriter(writer);
        }

        // wire (in):  [eventType:1][intervalMs:2]
        // wire (out): [eventType:1][senderId:2][intervalMs:2]
        private static void HandleAvatarRateChange(NetPacketReader reader, NetPeer peer, byte eventType)
        {
            ushort intervalMs = reader.GetUShort();
            reader.Recycle();

            ushort senderId = (ushort)peer.Id;

            NetDataWriter writer = NetworkServer.RentWriter();
            writer.Put(eventType);
            writer.Put(senderId);
            writer.Put(intervalMs);

            NetworkServer.BroadcastMessageToClients(writer, BasisNetworkCommons.EventsChannel, peer, NetworkServer.PeerSnapshot, DeliveryMethod.ReliableOrdered);
            NetworkServer.ReturnWriter(writer);
        }

        // wire (in):  [eventType:1][modeByte:1]
        // wire (out): [eventType:1][senderId:2][modeByte:1]
        private static void HandleTalkModeChanged(NetPacketReader reader, NetPeer peer, byte eventType)
        {
            byte mode = reader.GetByte();
            reader.Recycle();

            ushort senderId = (ushort)peer.Id;

            NetDataWriter writer = NetworkServer.RentWriter();
            writer.Put(eventType);
            writer.Put(senderId);
            writer.Put(mode);

            NetworkServer.BroadcastMessageToClients(writer, BasisNetworkCommons.EventsChannel, peer, NetworkServer.PeerSnapshot, DeliveryMethod.ReliableOrdered);
            NetworkServer.ReturnWriter(writer);
        }

        // wire (in):  [eventType:1][muted:1]
        // wire (out): [eventType:1][senderId:2][muted:1]
        private static void HandleMuteStateChanged(NetPacketReader reader, NetPeer peer, byte eventType)
        {
            byte muted = reader.GetByte();
            reader.Recycle();

            ushort senderId = (ushort)peer.Id;

            NetDataWriter writer = NetworkServer.RentWriter();
            writer.Put(eventType);
            writer.Put(senderId);
            writer.Put(muted);

            NetworkServer.BroadcastMessageToClients(writer, BasisNetworkCommons.EventsChannel, peer, NetworkServer.PeerSnapshot, DeliveryMethod.ReliableOrdered);
            NetworkServer.ReturnWriter(writer);
        }

        /// <summary>
        /// 処理カメラCountdownを処理します。受信データを検証し、必要な状態更新や再配信を行います。
        /// </summary>
        private static void HandleCameraCountdown(NetPacketReader reader, NetPeer peer, byte eventType)
        {
            ClientCameraCountdownMessage clientMsg = new ClientCameraCountdownMessage();
            clientMsg.Deserialize(reader);
            reader.Recycle();

            ushort peerId = (ushort)peer.Id;

            NetDataWriter writer = NetworkServer.RentWriter();
            writer.Put(eventType);

            CameraCountdownMessage outMsg = new CameraCountdownMessage
            {
                PlayerID = peerId,
                Seconds = clientMsg.Seconds,
            };
            outMsg.Serialize(writer);

            NetworkServer.BroadcastMessageToClients(writer, BasisNetworkCommons.EventsChannel, peer, NetworkServer.PeerSnapshot, DeliveryMethod.Sequenced);
            NetworkServer.ReturnWriter(writer);
        }
    }
}
