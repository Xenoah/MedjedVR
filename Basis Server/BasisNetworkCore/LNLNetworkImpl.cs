using System;
using System.Net;
using System.Net.Sockets;

namespace Basis.Network.Core
{

    /// <summary>
    /// イベントBasedNetListenerの責務をまとめるクラスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public partial class EventBasedNetListener : LiteNetLib.INetEventListener
    {
        void LiteNetLib.INetEventListener.OnConnectionRequest(LiteNetLib.ConnectionRequest request)
        {
            ConnectionRequestEvent?.Invoke(new LNLConnectionRequest(request));
        }

        void LiteNetLib.INetEventListener.OnPeerDisconnected(LiteNetLib.NetPeer peer, LiteNetLib.DisconnectInfo disconnectInfo)
        {
            PeerDisconnectedEvent?.Invoke(new LNLNetPeer(peer), new DisconnectInfo(disconnectInfo));
        }

        void LiteNetLib.INetEventListener.OnPeerConnected(LiteNetLib.NetPeer peer)
        {
            PeerConnectedEvent?.Invoke(new LNLNetPeer(peer));
        }

        void LiteNetLib.INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            NetworkErrorEvent?.Invoke(endPoint, socketError);
        }

        void LiteNetLib.INetEventListener.OnNetworkReceive(LiteNetLib.NetPeer peer, LiteNetLib.NetPacketReader reader, byte channelNumber, LiteNetLib.DeliveryMethod deliveryMethod)
        {
            NetPacketReader read = new NetPacketReader(reader);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            read.channel = channelNumber;
            read.method = (DeliveryMethod)(byte)deliveryMethod;
#endif

            NetworkReceiveEvent?.Invoke(new LNLNetPeer(peer), read, channelNumber, (DeliveryMethod)(byte)deliveryMethod);
        }

        void LiteNetLib.INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, LiteNetLib.NetPacketReader reader, LiteNetLib.UnconnectedMessageType messageType)
        {
            // broadcast packet は server-info contract の対象外。direct probe にだけ応答する。
            if (messageType != LiteNetLib.UnconnectedMessageType.BasicMessage) return;

            NetPacketReader read = new NetPacketReader(reader);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            read.channel = 255;
            read.method = DeliveryMethod.Unreliable;
#endif
            NetworkReceiveUnconnectedEvent?.Invoke(remoteEndPoint, read);
        }

        void LiteNetLib.INetEventListener.OnNetworkLatencyUpdate(LiteNetLib.NetPeer peer, int latency)
        {
            // 未使用
        }
    }

    /// <summary>
    /// DisconnectInfoの責務をまとめる構造体です。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public partial struct DisconnectInfo
    {
        /// <summary>
        /// DisconnectInfoを生成し、利用に必要な初期状態を設定します。
        /// </summary>
        internal DisconnectInfo(LiteNetLib.DisconnectInfo info)
        {
            NetPacketReader reader = new NetPacketReader(info.AdditionalData);

            // TODO: よりよい enum 変換にする?
            Reason = (DisconnectReason)(int)info.Reason;
            SocketErrorCode = info.SocketErrorCode;
            AdditionalData = reader;
        }
    }

    /// <summary>
    /// Net統計の責務をまとめるクラスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public sealed partial class NetStatistics
    {
        /// <summary>
        /// Net統計を生成し、利用に必要な初期状態を設定します。
        /// </summary>
        internal NetStatistics(LiteNetLib.NetStatistics stats)
        {
            PacketsSent = stats.PacketsSent;
            PacketsReceived = stats.PacketsReceived;
            BytesSent = stats.BytesSent;
            BytesReceived = stats.BytesReceived;
            PacketLoss = stats.PacketLoss;
        }
    }

    /// <summary>
    /// NetパケットReaderの責務をまとめるクラスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public partial class NetPacketReader
    {
        /// <summary>
        /// NetパケットReaderを生成し、利用に必要な初期状態を設定します。
        /// </summary>
        internal NetPacketReader(LiteNetLib.NetPacketReader reader) : base((LiteNetLib.Utils.NetDataReader)reader)
        {
            RecycleInternal = () => reader.Recycle();
        }
    }

    /// <summary>
    /// LNL接続Requestの責務をまとめるクラスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public class LNLConnectionRequest : ConnectionRequest
    {
        readonly LiteNetLib.ConnectionRequest request;
        readonly NetDataReader data;

        /// <summary>
        /// LNL接続Requestを生成し、利用に必要な初期状態を設定します。
        /// </summary>
        internal LNLConnectionRequest(LiteNetLib.ConnectionRequest request)
        {
            this.request = request;
            data = new NetDataReader(request.Data);
        }

        /// <summary>
        /// Dataを保持します。型は NetDataReader で、関連処理から共有される値です。
        /// </summary>
        public NetDataReader Data => data;

        /// <summary>
        /// RemoteEndPointを保持します。型は IPEndPoint で、関連処理から共有される値です。
        /// </summary>
        public IPEndPoint RemoteEndPoint => request.RemoteEndPoint;

        NetPeer ConnectionRequest.Accept()
        {
            return new LNLNetPeer(request.Accept());
        }

        void ConnectionRequest.Reject(NetDataWriter w)
        {
            request.Reject(w.Data, 0, w.Length, false);
        }
    }

    /// <summary>
    /// LNLNetピアの責務をまとめるクラスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public class LNLNetPeer : NetPeer
    {
        private readonly LiteNetLib.NetPeer peer;

        /// <summary>
        /// LNLNetピアを生成し、利用に必要な初期状態を設定します。
        /// </summary>
        internal LNLNetPeer(LiteNetLib.NetPeer lnlPeer)
        {
            peer = lnlPeer;
        }

        int NetPeer.Id => peer.Id;

        IPAddress NetPeer.Address => peer.Address;

        int NetPeer.RemoteId => peer.RemoteId;

        int NetPeer.RoundTripTime => peer.RoundTripTime;

        float NetPeer.TimeSinceLastPacket => peer.TimeSinceLastPacket;

        long NetPeer.RemoteTimeDelta => peer.RemoteTimeDelta;

        int NetPeer.Mtu => peer.Mtu;

        object NetPeer.Tag
        {
            get => peer.Tag;
            set => peer.Tag = value;
        }

        void NetPeer.Disconnect()
        {
            peer.Disconnect();
        }

        void NetPeer.Disconnect(byte[] b)
        {
            peer.Disconnect(b);
        }

        void NetPeer.DisconnectForce()
        {
            peer.NetManager.DisconnectPeerForce(peer);
        }

        int NetPeer.GetPacketsCountInQueue(byte channel, DeliveryMethod deliveryMethod)
        {
            return peer.GetPacketsCountInQueue(channel, (LiteNetLib.DeliveryMethod)(byte)deliveryMethod);
        }

        void NetPeer.Send(byte[] data, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            peer.Send(data, channelNumber, (LiteNetLib.DeliveryMethod)(byte)deliveryMethod);
        }

        void NetPeer.Send(NetDataWriter data, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            peer.Send(data.AsReadOnlySpan(), channelNumber, (LiteNetLib.DeliveryMethod)(byte)deliveryMethod);
        }

        void NetPeer.SendUnreliableRawMerge(byte[] data, int offset, int length, byte channelNumber, int patchOffset, byte patchValue)
        {
            peer.SendUnreliableRawMerge(data, offset, length, channelNumber, patchOffset, patchValue);
        }

        /// <summary>
        /// Equalsを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is LNLNetPeer))
            {
                return false;
            }
            else
            {
                return peer.Equals(((LNLNetPeer)obj).peer);
            }
        }

        /// <summary>
        /// GetHashCodeを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public override int GetHashCode()
        {
            return peer.GetHashCode();
        }
    }

    /// <summary>
    /// LNLNet管理の責務をまとめるクラスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public class LNLNetManager : NetManager
    {
        /// <summary>
        /// managerを保持します。型は LiteNetLib.NetManager で、関連処理から共有される値です。
        /// </summary>
        public LiteNetLib.NetManager manager;

        /// <summary>
        /// LNLNet管理を生成し、利用に必要な初期状態を設定します。
        /// </summary>
        public LNLNetManager(EventBasedNetListener listener, Configuration configuration)
        {
            LNLTransportConfig lnl = BasisTransportConfigStore.Get<LNLTransportConfig>(BasisNetworkStackRegistry.LiteNetLibId);
            manager = new LiteNetLib.NetManager(listener, null)
            {
                AutoRecycle = false,
                UnconnectedMessagesEnabled = true,
                NatPunchEnabled = lnl.NatPunchEnabled,
                AllowPeerAddressChange = lnl.AllowPeerAddressChange,
                BroadcastReceiveEnabled = false,
                UseNativeSockets = lnl.UseNativeSockets,
                ChannelsCount = BasisNetworkCommons.TotalChannels,
                EnableStatistics = configuration.EnableStatistics,
                IPv6Enabled = lnl.IPv6Enabled,
                UpdateTime = BasisNetworkCommons.NetworkIntervalPoll,
                PingInterval = lnl.PingInterval,
                DisconnectTimeout = lnl.DisconnectTimeout,
                UnsyncedEvents = true,
                ReceivePollingTime = BasisNetworkCommons.ReceivePollingTime,
                PacketPoolSize = BasisNetworkCommons.PacketPoolSize,
                SimulateLatency = lnl.SimulateLatency,
                SimulatePacketLoss = lnl.SimulatePacketLoss,
                SimulationMaxLatency = lnl.SimulationMaxLatency,
                SimulationMinLatency = lnl.SimulationMinLatency,
                SimulationPacketLossChance = lnl.SimulationPacketLossChance,
                MtuDiscovery = lnl.MtuDiscovery,
                MtuOverride = lnl.MtuOverride,
                MultiSocketCount = lnl.MultiSocketCount
            };
        }
        /// <summary>
        /// Startを開始します。依存する状態を準備して実行ループや待ち受けを有効化します。
        /// </summary>
        public void Start(IPAddress IPv4Address, IPAddress IPv6Address, int SetPort)
        {
            manager.Start(IPv4Address, IPv6Address, SetPort);
        }

        /// <summary>
        /// StartManualを開始します。依存する状態を準備して実行ループや待ち受けを有効化します。
        /// </summary>
        public void StartManual(IPAddress IPv4Address, IPAddress IPv6Address, int SetPort)
        {
            manager.StartInManualMode(IPv4Address, IPv6Address, SetPort);
        }

        /// <summary>
        /// Pollイベントを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public void PollEvents()
        {
            manager.PollEvents();
        }

        /// <summary>
        /// ManualUpdateを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public void ManualUpdate(float elapsedMilliseconds)
        {
            manager.ManualUpdate(elapsedMilliseconds);
        }

        /// <summary>
        /// Stopを停止します。保持している状態を片付け、次回起動に影響が残らないようにします。
        /// </summary>
        public void Stop()
        {
            manager.Stop();
        }

        /// <summary>
        /// Connectを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public Basis.Network.Core.NetPeer Connect(string sIP, int port, NetDataWriter Writer)
        {

            LiteNetLib.NetPeer peer = manager.Connect(LiteNetLib.NetUtils.MakeEndPoint(sIP, port), Writer.AsReadOnlySpan());
            return new LNLNetPeer(peer);
        }

        /// <summary>
        /// SendUnconnectedメッセージを送信します。対象ピア、チャンネル、配送方式に合わせてパケット化します。
        /// </summary>
        public bool SendUnconnectedMessage(NetDataWriter writer, IPEndPoint remoteEndPoint)
        {
            return manager.SendUnconnectedMessage(writer.AsReadOnlySpan(), remoteEndPoint);
        }

        /// <summary>
        /// ConnectedPeersCountを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public int ConnectedPeersCount => manager.ConnectedPeersCount;

        /// <summary>
        /// 統計を保持します。型は NetStatistics で、関連処理から共有される値です。
        /// </summary>
        public NetStatistics Statistics => new NetStatistics(manager.Statistics);
    }
}
