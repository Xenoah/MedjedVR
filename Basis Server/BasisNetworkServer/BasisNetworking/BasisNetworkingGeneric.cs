using Basis.Network.Core;
using System;
using System.Collections.Generic;
using static SerializableBasis;

namespace Basis.Network.Server.Generic
{
    /// <summary>
    /// BasisNetworkingGenericの責務をまとめるクラスです。
    /// ing領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static class BasisNetworkingGeneric
    {
        [ThreadStatic]
        private static List<NetPeer> _targetedClients;
        /// <summary>
        /// GetTargetedListを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        private static List<NetPeer> GetTargetedList()
        {
            if (_targetedClients == null) _targetedClients = new List<NetPeer>();
            else _targetedClients.Clear();
            return _targetedClients;
        }

        [ThreadStatic]
        private static HashSet<ushort> _seenRecipients;
        /// <summary>
        /// GetSeenSetを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        private static HashSet<ushort> GetSeenSet()
        {
            if (_seenRecipients == null) _seenRecipients = new HashSet<ushort>();
            else _seenRecipients.Clear();
            return _seenRecipients;
        }

        /// <summary>
        /// 処理Sceneを処理します。受信データを検証し、必要な状態更新や再配信を行います。
        /// </summary>
        public static void HandleScene(NetPacketReader Reader, DeliveryMethod DeliveryMethod, NetPeer sender, byte broadcastChannel = BasisNetworkCommons.SceneChannel)
        {
            SceneDataMessage SceneDataMessage = new SceneDataMessage();
            SceneDataMessage.Deserialize(Reader);
            Reader.Recycle();

            byte[] payload = SceneDataMessage.payload;
            int payloadLength = (payload != null) ? payload.Length : 0;

            ServerSceneDataMessage serverSceneDataMessage = new ServerSceneDataMessage
            {
                sceneDataMessage = new RemoteSceneDataMessage()
                {
                    messageIndex = SceneDataMessage.messageIndex,
                    payload = payload,
                    payloadLength = payloadLength
                },
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = (ushort)sender.Id,
                }
            };

            byte Channel = broadcastChannel;
            NetDataWriter Writer = NetworkServer.RentWriter();
            serverSceneDataMessage.Serialize(Writer);
            if (SceneDataMessage.recipientsSize != 0)
            {
                List<NetPeer> targetedClients = GetTargetedList();
                HashSet<ushort> seen = GetSeenSet();

                int recipientsLength = SceneDataMessage.recipientsSize;
                for (int index = 0; index < recipientsLength; index++)
                {
                    ushort recipient = SceneDataMessage.recipients[index];
                    if (!seen.Add(recipient))
                    {
                        continue;
                    }
                    if (NetworkServer.AuthenticatedPeers.TryGetValue(recipient, out NetPeer client))
                    {
                        targetedClients.Add(client);
                    }
                    else
                    {
                        BNL.Log("Missing Peer! " + recipient);
                    }
                }

                if (targetedClients.Count > 0)
                {
                    NetworkServer.BroadcastMessageToClients(Writer, Channel, ref targetedClients, DeliveryMethod);
                }
            }
            else
            {
                NetworkServer.BroadcastMessageToClients(Writer, Channel, sender, NetworkServer.PeerSnapshot, DeliveryMethod);
            }
            NetworkServer.ReturnWriter(Writer);
            serverSceneDataMessage.sceneDataMessage.Release();
        }
        /// <summary>
        /// 処理アバターを処理します。受信データを検証し、必要な状態更新や再配信を行います。
        /// </summary>
        public static void HandleAvatar(NetPacketReader Reader, DeliveryMethod DeliveryMethod, NetPeer sender, byte broadcastChannel = BasisNetworkCommons.AvatarChannel)
        {
            AvatarDataMessage avatarDataMessage = new AvatarDataMessage();
            avatarDataMessage.Deserialize(Reader);
            Reader.Recycle();
            ServerAvatarDataMessage serverAvatarDataMessage = new ServerAvatarDataMessage
            {
                avatarDataMessage = new RemoteAvatarDataMessage()
                {
                    messageIndex = avatarDataMessage.messageIndex,
                    payload = avatarDataMessage.payload,
                    PlayerIdMessage = avatarDataMessage.PlayerIdMessage,
                    AvatarLinkIndex = avatarDataMessage.AvatarLinkIndex,
                },
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = (ushort)sender.Id
                }
            };
            byte Channel = broadcastChannel;
            NetDataWriter Writer = NetworkServer.RentWriter();
            serverAvatarDataMessage.Serialize(Writer);
            if (avatarDataMessage.recipientsSize != 0)
            {
                List<NetPeer> targetedClients = GetTargetedList();
                HashSet<ushort> seen = GetSeenSet();

                int recipientsLength = avatarDataMessage.recipientsSize;
                for (int index = 0; index < recipientsLength; index++)
                {
                    ushort recipient = avatarDataMessage.recipients[index];
                    if (!seen.Add(recipient))
                    {
                        continue;
                    }
                    if (NetworkServer.AuthenticatedPeers.TryGetValue(recipient, out NetPeer client))
                    {
                        targetedClients.Add(client);
                    }
                    else
                    {
                        BNL.Log("Missing Peer! " + recipient);
                    }
                }

                if (targetedClients.Count > 0)
                {
                    NetworkServer.BroadcastMessageToClients(Writer, Channel, ref targetedClients, DeliveryMethod);
                }
            }
            else
            {
                NetworkServer.BroadcastMessageToClients(Writer, Channel, sender, NetworkServer.PeerSnapshot, DeliveryMethod);
            }
            NetworkServer.ReturnWriter(Writer);
        }
    }
}
