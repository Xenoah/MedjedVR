using Basis.Network.Core;
using System.Collections.Concurrent;
using System.Net;
using static BasisNetworkCore.Serializable.SerializableBasis;
using static SerializableBasis;
using LiteNatPunchListener = LiteNetLib.EventBasedNatPunchListener;

namespace BasisNetworkServer
{
    /// <summary>
    /// BasisサーバーP2PBrokerの責務をまとめるクラスです。
    /// Server領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static class BasisServerP2PBroker
    {
        /// <summary>
        /// Session状態の責務をまとめる列挙型です。
        /// Server領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
        /// </summary>
        private enum SessionState : byte { Awaiting, ReadyForPunch, Punched }

        /// <summary>
        /// Sessionの責務をまとめるクラスです。
        /// Server領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
        /// </summary>
        private sealed class Session
        {
            /// <summary>
            /// Tokenを保持します。型は string で、関連処理から共有される値です。
            /// </summary>
            public string Token;
            /// <summary>
            /// InitiatorピアIdを保持します。型は int で、関連処理から共有される値です。
            /// </summary>
            public int InitiatorPeerId;
            /// <summary>
            /// TargetピアIdを保持します。型は int で、関連処理から共有される値です。
            /// </summary>
            public int TargetPeerId;
            /// <summary>
            /// 状態を保持します。型は SessionState で、関連処理から共有される値です。
            /// </summary>
            public SessionState State;

            /// <summary>
            /// EndpointAInternalを保持します。型は IPEndPoint で、関連処理から共有される値です。
            /// </summary>
            public IPEndPoint EndpointA_Internal;
            /// <summary>
            /// EndpointAExternalを保持します。型は IPEndPoint で、関連処理から共有される値です。
            /// </summary>
            public IPEndPoint EndpointA_External;
            /// <summary>
            /// EndpointBInternalを保持します。型は IPEndPoint で、関連処理から共有される値です。
            /// </summary>
            public IPEndPoint EndpointB_Internal;
            /// <summary>
            /// EndpointBExternalを保持します。型は IPEndPoint で、関連処理から共有される値です。
            /// </summary>
            public IPEndPoint EndpointB_External;
            /// <summary>
            /// HasAを保持します。型は bool で、関連処理から共有される値です。
            /// </summary>
            public bool HasA;
            /// <summary>
            /// HasBを保持します。型は bool で、関連処理から共有される値です。
            /// </summary>
            public bool HasB;

            /// <summary>
            /// InitiatorLinkUpを保持します。型は bool で、関連処理から共有される値です。
            /// </summary>
            public bool InitiatorLinkUp;
            /// <summary>
            /// TargetLinkUpを保持します。型は bool で、関連処理から共有される値です。
            /// </summary>
            public bool TargetLinkUp;
        }

        private static readonly ConcurrentDictionary<string, Session> _sessions = new();
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<string, byte>> _peerSessions = new();
        private static readonly ConcurrentDictionary<long, byte> _offloadedPairs = new();

        /// <summary>
        /// PackPairを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private static long PackPair(int a, int b)
        {
            int lo = a < b ? a : b;
            int hi = a < b ? b : a;
            return ((long)lo << 32) | (uint)hi;
        }

        /// <summary>
        /// IsP2POffloadedを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static bool IsP2POffloaded(int a, int b)
        {
            if (a == b) return false;
            return _offloadedPairs.ContainsKey(PackPair(a, b));
        }

        private static LiteNatPunchListener _natListener;

        /// <summary>
        /// Initializeを初期化します。設定、永続化ファイル、実行時キャッシュを起動時の状態へ整えます。
        /// </summary>
        public static void Initialize()
        {
            if (_natListener != null) return;

            var manager = (NetworkServer.Server as LNLNetManager)?.manager;
            if (manager == null)
            {
                BNL.LogError("[P2P] NetManager not initialised or active stack is not LiteNetLib, cannot start P2P broker.");
                return;
            }

            if (!manager.NatPunchEnabled)
            {
                BNL.LogWarning("[P2P] NatPunchEnabled=false in server config — direct peer connections will not work. Set NatPunchEnabled=true to enable.");
            }

            _natListener = new LiteNatPunchListener();
            _natListener.NatIntroductionRequest += OnNatIntroductionRequest;
            manager.NatPunchModule.Init(_natListener);
            manager.NatPunchModule.UnsyncedEvents = true;

            BNL.Log("[P2P] Broker initialised.");
        }

        /// <summary>
        /// 処理P2Pメッセージを処理します。受信データを検証し、必要な状態更新や再配信を行います。
        /// </summary>
        public static void HandleP2PMessage(NetPacketReader reader, NetPeer peer)
        {
            byte sub = reader.GetByte();
            BasisP2PSignalMessage msg = default;
            msg.Deserialize(reader);
            reader.Recycle();

            switch (sub)
            {
                case BasisNetworkCommons.P2PSub_Request:
                    HandleRequest(peer, msg);
                    break;
                case BasisNetworkCommons.P2PSub_Accept:
                    HandleAccept(peer, msg);
                    break;
                case BasisNetworkCommons.P2PSub_Decline:
                    ForwardAndDrop(peer, msg, BasisNetworkCommons.P2PSub_Decline);
                    break;
                case BasisNetworkCommons.P2PSub_Cancel:
                    ForwardAndDrop(peer, msg, BasisNetworkCommons.P2PSub_Cancel);
                    break;
                case BasisNetworkCommons.P2PSub_LinkLost:
                    HandleLinkLost(peer, msg);
                    break;
                case BasisNetworkCommons.P2PSub_LinkUp:
                    HandleLinkUp(peer, msg);
                    break;
                default:
                    BNL.LogError($"[P2P] Unknown sub-type {sub} from peer {peer.Id}.");
                    break;
            }
        }

        /// <summary>
        /// 処理LinkUpを処理します。受信データを検証し、必要な状態更新や再配信を行います。
        /// </summary>
        private static void HandleLinkUp(NetPeer sender, BasisP2PSignalMessage msg)
        {
            if (!_sessions.TryGetValue(msg.sessionToken, out Session s)) return;

            if (sender.Id == s.InitiatorPeerId) s.InitiatorLinkUp = true;
            else if (sender.Id == s.TargetPeerId) s.TargetLinkUp = true;
            else return;

            BNL.Log($"[P2P] LinkUp from peer {sender.Id} (token {Preview(s.Token)}); flags InitiatorUp={s.InitiatorLinkUp} TargetUp={s.TargetLinkUp}.");
            if (s.InitiatorLinkUp && s.TargetLinkUp)
            {
                _offloadedPairs[PackPair(s.InitiatorPeerId, s.TargetPeerId)] = 0;
                BNL.Log($"[P2P] OFFLOADED pair ({s.InitiatorPeerId},{s.TargetPeerId}) — server will skip relaying voice + avatar between them.");
            }
        }

        /// <summary>
        /// 処理Requestを処理します。受信データを検証し、必要な状態更新や再配信を行います。
        /// </summary>
        private static void HandleRequest(NetPeer sender, BasisP2PSignalMessage msg)
        {
            if (string.IsNullOrEmpty(msg.sessionToken))
            {
                BNL.LogError($"[P2P] Empty session token from peer {sender.Id}, dropping Request.");
                return;
            }
            // admin 管理の instance lockout: 非 admin は direct (P2P) connection を確立できない。
            // admin (basis.moderation.globallock) は moderation のため接続できるよう除外する。
            if (BasisNetworkServer.Security.BasisGlobalLockManager.DirectConnectLocked &&
                !BasisPermissions.PermissionManager.PermissionIntegration.HasValidRequirement(sender, BasisPermissions.PermNodes.ModerationGlobalLock))
            {
                BNL.Log($"[P2P] DirectConnectLocked: rejecting Request from non-admin peer {sender.Id}.");
                SendSub(sender, BasisNetworkCommons.P2PSub_Cancel, msg.sessionToken, msg.otherPlayerId);
                return;
            }
            if (msg.otherPlayerId == sender.Id)
            {
                BNL.LogError($"[P2P] Peer {sender.Id} tried to request a session with itself.");
                return;
            }
            if (!NetworkServer.AuthenticatedPeers.TryGetValue(msg.otherPlayerId, out NetPeer target))
            {
                SendSub(sender, BasisNetworkCommons.P2PSub_Cancel, msg.sessionToken, msg.otherPlayerId);
                return;
            }

            var session = new Session
            {
                Token = msg.sessionToken,
                InitiatorPeerId = sender.Id,
                TargetPeerId = msg.otherPlayerId,
                State = SessionState.Awaiting,
            };
            _sessions[msg.sessionToken] = session;
            TrackPeerSession(sender.Id, msg.sessionToken);
            TrackPeerSession(msg.otherPlayerId, msg.sessionToken);

            BNL.Log($"[P2P] Forwarding Request from peer {sender.Id} to peer {msg.otherPlayerId} (token {msg.sessionToken}).");
            SendSub(target, BasisNetworkCommons.P2PSub_Request, msg.sessionToken, (ushort)sender.Id, msg.ephemeralPublicKey);

            // ServerArmed はどちらかが punching を始める前に registration を確認し、race を避ける。
            SendSub(sender, BasisNetworkCommons.P2PSub_ServerArmed, msg.sessionToken, msg.otherPlayerId);
        }

        /// <summary>
        /// 処理Acceptを処理します。受信データを検証し、必要な状態更新や再配信を行います。
        /// </summary>
        private static void HandleAccept(NetPeer sender, BasisP2PSignalMessage msg)
        {
            if (!_sessions.TryGetValue(msg.sessionToken, out Session s))
            {
                BNL.LogError($"[P2P] Accept for unknown token from peer {sender.Id}.");
                return;
            }
            if (s.TargetPeerId != sender.Id || s.InitiatorPeerId != msg.otherPlayerId)
            {
                BNL.LogError($"[P2P] Accept from peer {sender.Id} doesn't match session pair ({s.InitiatorPeerId},{s.TargetPeerId}).");
                return;
            }

            s.State = SessionState.ReadyForPunch;

            if (NetworkServer.AuthenticatedPeers.TryGetValue(s.InitiatorPeerId, out NetPeer initiator))
            {
                BNL.Log($"[P2P] Accept from peer {sender.Id} (token {Preview(s.Token)}); session armed, forwarding to initiator {s.InitiatorPeerId}.");
                SendSub(initiator, BasisNetworkCommons.P2PSub_Accept, s.Token, (ushort)sender.Id, msg.ephemeralPublicKey);
            }
            else
            {
                BNL.LogWarning($"[P2P] Accept arrived but initiator {s.InitiatorPeerId} already gone; dropping session {Preview(s.Token)}.");
                RemoveSession(s.Token);
            }
        }

        /// <summary>
        /// 処理LinkLostを処理します。受信データを検証し、必要な状態更新や再配信を行います。
        /// </summary>
        private static void HandleLinkLost(NetPeer sender, BasisP2PSignalMessage msg)
        {
            // session を再 arm し、offload を clear して re-punch window 中に relay を再開する。
            if (_sessions.TryGetValue(msg.sessionToken, out Session s))
            {
                bool wasOffloaded = _offloadedPairs.ContainsKey(PackPair(s.InitiatorPeerId, s.TargetPeerId));
                s.HasA = false;
                s.HasB = false;
                s.InitiatorLinkUp = false;
                s.TargetLinkUp = false;
                s.State = SessionState.ReadyForPunch;
                _offloadedPairs.TryRemove(PackPair(s.InitiatorPeerId, s.TargetPeerId), out _);
                BNL.Log($"[P2P] LinkLost from peer {sender.Id} (token {Preview(s.Token)}); re-armed for punch, offload {(wasOffloaded ? "cleared (relay resumed)" : "already cleared")}.");
            }
            ForwardAndDrop(sender, msg, BasisNetworkCommons.P2PSub_LinkLost, dropSession: false);
        }

        /// <summary>
        /// ForwardAndDropを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private static void ForwardAndDrop(NetPeer sender, BasisP2PSignalMessage msg, byte sub, bool dropSession = true)
        {
            if (NetworkServer.AuthenticatedPeers.TryGetValue(msg.otherPlayerId, out NetPeer other))
            {
                SendSub(other, sub, msg.sessionToken, (ushort)sender.Id);
            }
            if (dropSession && !string.IsNullOrEmpty(msg.sessionToken))
            {
                RemoveSession(msg.sessionToken);
            }
        }

        /// <summary>
        /// OnNatIntroductionRequestイベントを受け取り、関連するサーバー状態や送信処理へ反映します。
        /// </summary>
        private static void OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token)
        {
            if (string.IsNullOrEmpty(token)) return;
            if (!_sessions.TryGetValue(token, out Session s))
            {
                BNL.LogWarning($"[P2P] NatIntroduceRequest with unknown token {Preview(token)} from {remoteEndPoint} — dropping.");
                return;
            }
            if (s.State < SessionState.ReadyForPunch)
            {
                BNL.LogWarning($"[P2P] NatIntroduceRequest for token {Preview(token)} in state {s.State} — not ready, dropping.");
                return;
            }
            BNL.Log($"[P2P] NatIntroduceRequest token={Preview(token)} from internal={localEndPoint} external={remoteEndPoint}; HasA={s.HasA} HasB={s.HasB}.");

            // arrival order で slot にラベルを付ける。NatIntroduce は対称なのでどちらでも構わない。
            lock (s)
            {
                if (!s.HasA)
                {
                    s.EndpointA_Internal = localEndPoint;
                    s.EndpointA_External = remoteEndPoint;
                    s.HasA = true;
                }
                else if (!s.HasB)
                {
                    s.EndpointB_Internal = localEndPoint;
                    s.EndpointB_External = remoteEndPoint;
                    s.HasB = true;
                }

                if (s.HasA && s.HasB)
                {
                    bool firstFire = s.State != SessionState.Punched;
                    bool sameNat = s.EndpointA_External != null &&
                                   s.EndpointB_External != null &&
                                   s.EndpointA_External.Address.Equals(s.EndpointB_External.Address);
                    string lanTag = sameNat ? " [SAME-NETWORK]" : "";

                    // 両側に predicted port を spray する (A/B は arrival order であり、特定 peer への
                    // mapping ではない)。same-network pair では internal punch が処理済みなので除外する。
                    int spray = (firstFire && !sameNat) ? GetPredictionRange() : 0;

                    BNL.Log($"[P2P] Both NAT endpoints collected for token {Preview(token)}: A={s.EndpointA_External} (int {s.EndpointA_Internal}), B={s.EndpointB_External} (int {s.EndpointB_Internal}). Firing NatIntroduce (spray={spray}).{lanTag}");
                    LiteNetLib.NetManager lnlManager = (NetworkServer.Server as LNLNetManager)?.manager;
                    if (lnlManager == null) return;
                    lnlManager.NatPunchModule.NatIntroduce(
                        s.EndpointA_Internal,
                        s.EndpointA_External,
                        spray,
                        s.EndpointB_Internal,
                        s.EndpointB_External,
                        spray,
                        token);
                    s.State = SessionState.Punched;
                }
            }
        }

        /// <summary>
        /// Previewを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private static string Preview(string token)
        {
            if (string.IsNullOrEmpty(token)) return "(empty)";
            return token.Length <= 8 ? token : token.Substring(0, 8);
        }

        /// <summary>
        /// GetPredictionRangeを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        private static int GetPredictionRange()
        {
            try
            {
                var cfg = BasisTransportConfigStore.Get<LNLTransportConfig>(BasisNetworkStackRegistry.LiteNetLibId);
                return cfg != null ? cfg.NatPortPredictionRange : 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Removeピアを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void RemovePeer(int peerId)
        {
            if (!_peerSessions.TryRemove(peerId, out var tokens)) return;
            BNL.Log($"[P2P] Peer {peerId} disconnected; closing out {tokens.Count} P2P session(s).");
            foreach (var kv in tokens)
            {
                string token = kv.Key;
                if (!_sessions.TryGetValue(token, out Session s)) continue;
                int otherId = s.InitiatorPeerId == peerId ? s.TargetPeerId : s.InitiatorPeerId;
                if (NetworkServer.AuthenticatedPeers.TryGetValue(otherId, out NetPeer other))
                {
                    BNL.Log($"[P2P] Notifying peer {otherId} via Cancel that peer {peerId} is gone (token {Preview(token)}).");
                    SendSub(other, BasisNetworkCommons.P2PSub_Cancel, token, (ushort)peerId);
                }
                RemoveSession(token);
            }
        }

        /// <summary>
        /// RemoveSessionを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private static void RemoveSession(string token)
        {
            if (!_sessions.TryRemove(token, out Session s)) return;
            UntrackPeerSession(s.InitiatorPeerId, token);
            UntrackPeerSession(s.TargetPeerId, token);
            _offloadedPairs.TryRemove(PackPair(s.InitiatorPeerId, s.TargetPeerId), out _);
        }

        /// <summary>
        /// TrackピアSessionを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private static void TrackPeerSession(int peerId, string token)
        {
            var inner = _peerSessions.GetOrAdd(peerId, _ => new ConcurrentDictionary<string, byte>());
            inner[token] = 0;
        }

        /// <summary>
        /// UntrackピアSessionを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private static void UntrackPeerSession(int peerId, string token)
        {
            if (_peerSessions.TryGetValue(peerId, out var inner))
            {
                inner.TryRemove(token, out _);
            }
        }

        /// <summary>
        /// SendSubを送信します。対象ピア、チャンネル、配送方式に合わせてパケット化します。
        /// </summary>
        private static void SendSub(NetPeer to, byte sub, string token, ushort otherPlayerId, byte[] ephemeralPublicKey = null)
        {
            NetDataWriter writer = NetworkServer.RentWriter();
            writer.Put(sub);
            var body = new BasisP2PSignalMessage
            {
                otherPlayerId = otherPlayerId,
                sessionToken = token ?? string.Empty,
                ephemeralPublicKey = ephemeralPublicKey,
            };
            body.Serialize(writer);
            NetworkServer.TrySend(to, writer, BasisNetworkCommons.P2PChannel, DeliveryMethod.ReliableOrdered);
            NetworkServer.ReturnWriter(writer);
        }
    }
}
