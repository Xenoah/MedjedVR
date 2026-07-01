using System;
using System.Net;
using System.Net.Sockets;

namespace Basis.Network.Core
{
    /// <summary>
    /// DisconnectReasonの責務をまとめる列挙型です。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public enum DisconnectReason
    {
        ConnectionFailed,
        Timeout,
        HostUnreachable,
        NetworkUnreachable,
        RemoteConnectionClose,
        DisconnectPeerCalled,
        ConnectionRejected,
        InvalidProtocol,
        UnknownHost,
        Reconnect,
        PeerToPeerConnection,
        PeerNotFound
    }

    /// <summary>
    /// DisconnectInfoの責務をまとめる構造体です。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public partial struct DisconnectInfo
    {
        /// <summary>
        /// Reasonを保持します。型は DisconnectReason で、関連処理から共有される値です。
        /// </summary>
        public DisconnectReason Reason;
        /// <summary>
        /// SocketエラーCodeを保持します。型は System.Net.Sockets.SocketError で、関連処理から共有される値です。
        /// </summary>
        public System.Net.Sockets.SocketError SocketErrorCode;
        /// <summary>
        /// AdditionalDataを保持します。型は NetPacketReader で、関連処理から共有される値です。
        /// </summary>
        public NetPacketReader AdditionalData;
    }


    /// <summary>
    /// イベントBasedNetListenerの責務をまとめるクラスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public partial class EventBasedNetListener
    {
        public delegate void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo);
        public delegate void OnNetworkError(IPEndPoint endPoint, SocketError socketError);
        public delegate void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod);
        public delegate void OnConnectionRequest(ConnectionRequest request);
        public delegate void OnPeerConnected(NetPeer peer);
        public delegate void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader);

        /// <summary>
        /// 接続Requestイベントを保持します。型は event OnConnectionRequest で、関連処理から共有される値です。
        /// </summary>
        public event OnConnectionRequest ConnectionRequestEvent;
        /// <summary>
        /// ピアDisconnectedイベントを保持します。型は event OnPeerDisconnected で、関連処理から共有される値です。
        /// </summary>
        public event OnPeerDisconnected PeerDisconnectedEvent;
        /// <summary>
        /// ネットワークReceiveイベントを保持します。型は event OnNetworkReceive で、関連処理から共有される値です。
        /// </summary>
        public event OnNetworkReceive NetworkReceiveEvent;
        /// <summary>
        /// ネットワークエラーイベントを保持します。型は event OnNetworkError で、関連処理から共有される値です。
        /// </summary>
        public event OnNetworkError NetworkErrorEvent;
        /// <summary>
        /// ピアConnectedイベントを保持します。型は event OnPeerConnected で、関連処理から共有される値です。
        /// </summary>
        public event OnPeerConnected PeerConnectedEvent;
        /// <summary>
        /// ネットワークReceiveUnconnectedイベントを保持します。型は event OnNetworkReceiveUnconnected で、関連処理から共有される値です。
        /// </summary>
        public event OnNetworkReceiveUnconnected NetworkReceiveUnconnectedEvent;

        /// <summary>
        /// Raise接続Requestを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public void RaiseConnectionRequest(ConnectionRequest request)
        {
            ConnectionRequestEvent?.Invoke(request);
        }

        /// <summary>
        /// RaiseピアDisconnectedを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public void RaisePeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            PeerDisconnectedEvent?.Invoke(peer, disconnectInfo);
        }

        /// <summary>
        /// RaiseネットワークReceiveを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public void RaiseNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            NetworkReceiveEvent?.Invoke(peer, reader, channel, deliveryMethod);
        }

        /// <summary>
        /// RaiseピアConnectedを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public void RaisePeerConnected(NetPeer peer)
        {
            PeerConnectedEvent?.Invoke(peer);
        }

        /// <summary>
        /// RaiseネットワークReceiveUnconnectedを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public void RaiseNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader)
        {
            NetworkReceiveUnconnectedEvent?.Invoke(remoteEndPoint, reader);
        }
    }

    /// <summary>
    /// 接続Requestの責務をまとめるインターフェイスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public interface ConnectionRequest
    {
        public void Reject(NetDataWriter w);
        public NetPeer Accept();
        public NetDataReader Data { get; }
        public IPEndPoint RemoteEndPoint { get; }
    }

    /// <summary>
    /// Netピアの責務をまとめるインターフェイスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public interface NetPeer
    {
        public void Disconnect();
        public void Disconnect(byte[] b);
        public void DisconnectForce();
        public void Send(byte[] data, byte channelNumber, DeliveryMethod deliveryMethod);
        public void Send(NetDataWriter data, byte channelNumber, DeliveryMethod deliveryMethod);
        public void SendUnreliableRawMerge(byte[] data, int offset, int length, byte channelNumber, int patchOffset = -1, byte patchValue = 0);
        public int GetPacketsCountInQueue(byte channel, DeliveryMethod deliveryMethod);
        public int Id { get; }
        public IPAddress Address { get; }
        public int RemoteId { get; }
        public int RoundTripTime { get; }
        /// <summary>
        /// Pingを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public int Ping => RoundTripTime / 2;
        public float TimeSinceLastPacket { get; }
        public long RemoteTimeDelta { get; }
        /// <summary>
        /// RemoteUtcTimeを保持します。型は DateTime で、関連処理から共有される値です。
        /// </summary>
        public DateTime RemoteUtcTime => new DateTime(DateTime.UtcNow.Ticks + RemoteTimeDelta);
        // この peer と negotiate 済みの最大 UDP payload (fragmentation なし)。
        // avatar bundle compressor が、compressed payload を 1 datagram に収めるために使う。
        public int Mtu { get; }

        public object Tag { get; set; }

        // public readonly NetStatistics Statistics;
    }

    /// <summary>
    /// Net管理の責務をまとめるインターフェイスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public interface NetManager
    {
        /// <summary>
        /// Startを開始します。依存する状態を準備して実行ループや待ち受けを有効化します。
        /// </summary>
        public void Start()
        {
            Start(0);
        }
        /// <summary>
        /// Startを開始します。依存する状態を準備して実行ループや待ち受けを有効化します。
        /// </summary>
        public void Start(int SetPort)
        {
            Start(IPAddress.Any, IPAddress.IPv6Any, SetPort);
        }
        public void Start(IPAddress IPv4Address, IPAddress IPv6Address, int SetPort);
        /// <summary>
        /// StartManualを開始します。依存する状態を準備して実行ループや待ち受けを有効化します。
        /// </summary>
        public void StartManual()
        {
            StartManual(0);
        }
        /// <summary>
        /// StartManualを開始します。依存する状態を準備して実行ループや待ち受けを有効化します。
        /// </summary>
        public void StartManual(int SetPort)
        {
            StartManual(IPAddress.Any, IPAddress.IPv6Any, SetPort);
        }
        /// <summary>
        /// StartManualを開始します。依存する状態を準備して実行ループや待ち受けを有効化します。
        /// </summary>
        public void StartManual(IPAddress IPv4Address, IPAddress IPv6Address, int SetPort)
            => throw new NotSupportedException("This transport does not support manual mode.");
        /// <summary>
        /// Pollイベントを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public void PollEvents()
            => throw new NotSupportedException("This transport does not support manual mode.");
        /// <summary>
        /// ManualUpdateを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public void ManualUpdate(float elapsedMilliseconds)
            => throw new NotSupportedException("This transport does not support manual mode.");
        public void Stop();
        public Basis.Network.Core.NetPeer Connect(string sIP, int port, NetDataWriter Writer);
        public bool SendUnconnectedMessage(NetDataWriter writer, IPEndPoint remoteEndPoint);

        public NetStatistics Statistics { get; }

        public int ConnectedPeersCount { get; }
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
        public NetStatistics()
        {
        }

        /// <summary>
        /// PacketsSentを保持します。型は long で、関連処理から共有される値です。
        /// </summary>
        public long PacketsSent;
        /// <summary>
        /// PacketsReceivedを保持します。型は long で、関連処理から共有される値です。
        /// </summary>
        public long PacketsReceived;
        /// <summary>
        /// BytesSentを保持します。型は long で、関連処理から共有される値です。
        /// </summary>
        public long BytesSent;
        /// <summary>
        /// BytesReceivedを保持します。型は long で、関連処理から共有される値です。
        /// </summary>
        public long BytesReceived;
        /// <summary>
        /// パケットLossを保持します。型は long で、関連処理から共有される値です。
        /// </summary>
        public long PacketLoss;
    }

    /// <summary>
    /// NetパケットReaderの責務をまとめるクラスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public partial class NetPacketReader : NetDataReader
    {
        Action RecycleInternal;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		/// <summary>
		/// channelを保持します。型は byte で、関連処理から共有される値です。
		/// </summary>
		internal byte channel;
		/// <summary>
		/// methodを保持します。型は DeliveryMethod で、関連処理から共有される値です。
		/// </summary>
		internal DeliveryMethod method;
#endif

        /// <summary>
        /// NetパケットReaderを生成し、利用に必要な初期状態を設定します。
        /// </summary>
        public NetPacketReader()
        {
        }

        /// <summary>
        /// NetパケットReaderを生成し、利用に必要な初期状態を設定します。
        /// </summary>
        public NetPacketReader(byte[] source, int offset, int maxSize, Action recycle) : base(source, offset, maxSize)
        {
            RecycleInternal = recycle;
        }

        /// <summary>
        /// Createを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static NetPacketReader Create(byte[] source, int offset, int maxSize, Action recycle)
        {
            return new NetPacketReader(source, offset, maxSize, recycle);
        }

        /// <summary>
        /// Recycleを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public void Recycle(bool IsOkTOHaveEmptyData = false)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (IsOkTOHaveEmptyData == false)
			{
				if (!EndOfData && AvailableBytes > 0)
				{
					BNL.LogWarning($"Message on channel {channel} with delivery method {method} had {AvailableBytes} bytes remaining when recycling! Is this a parsing bug?");
					// TODO: message の byte 表示を検討する。
				}
			}
#endif

            RecycleInternal?.Invoke();
        }
    }

    // litenetlib から直接持ってきたもの。


    public enum NetLogLevel
    {
        Warning,
        Error,
        Trace,
        Info
    }
    /// <summary>
    /// INetLoggerの責務をまとめるインターフェイスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public interface INetLogger
    {
        void WriteNet(NetLogLevel level, string str, params object[] args);
    }

    /// <summary>
    /// NetDebugの責務をまとめるクラスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public class NetDebug
    {
        /// <summary>
        /// Loggerを保持します。型は INetLogger で、関連処理から共有される値です。
        /// </summary>
        public static INetLogger Logger;
    }

    /// <summary>
    /// 送信方式の type。
    /// </summary>
    public enum DeliveryMethod : byte
    {
        /// <summary>
        /// unreliable。packet は drop / duplicate される可能性があり、順序どおりに届くとは限らない。
        /// </summary>
        Unreliable = 4,

        /// <summary>
        /// reliable。packet は drop / duplicate されないが、順序どおりに届くとは限らない。
        /// </summary>
        ReliableUnordered = 0,

        /// <summary>
        /// unreliable。packet は drop される可能性があるが duplicate されず、順序どおりに届く。
        /// </summary>
        Sequenced = 1,

        /// <summary>
        /// reliable かつ ordered。packet は drop / duplicate されず、順序どおりに届く。
        /// </summary>
        ReliableOrdered = 2,

        /// <summary>
        /// 最後の packet のみ reliable。最後以外の packet は drop される可能性があり、duplicate されず、順序どおりに届く。
        /// fragment できない。
        /// </summary>
        ReliableSequenced = 3
    }

}


