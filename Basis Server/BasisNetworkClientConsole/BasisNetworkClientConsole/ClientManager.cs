using Basis.Config;
using Basis.Contrib.Auth.DecentralizedIds.Newtypes;
using Basis.Contrib.Crypto;
using Basis.Network.Core;
using Basis.Network.Core.Compression;
using Basis.Scripts.BasisSdk.Players;
using Basis.Utilities;
using BasisNetworkClient;
using System.Text;
using System.Threading;
using static Basis.Network.Core.Compression.BasisAvatarBitPacking;
using static Basis.Network.Core.Serializable.SerializableBasis;
using static SerializableBasis;

namespace Basis.Network
{
    /// <summary>
    /// クライアント管理の責務をまとめるクラスです。
    /// ClientConsole領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public class ClientManager
    {
        /// <summary>
        /// クライアントCountを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public int ClientCount => ConfigManager.ClientCount;
        private readonly CancellationTokenSource cts = new();
        /// <summary>
        /// FinalPeersを保持します。型は NetPeer[] で、関連処理から共有される値です。
        /// </summary>
        public NetPeer[] FinalPeers;
        /// <summary>
        /// FinalClientsを保持します。型は NetworkClient[] で、関連処理から共有される値です。
        /// </summary>
        public NetworkClient[] FinalClients;
        /// <summary>
        /// Sizeを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public static int Size;

        // runtime 中に config は変わらないため、一度だけ cache する。
        private byte[] _cachedPasswordBytes;
        private byte[] _cachedAvatarBytes;

        /// <summary>
        /// Prepareを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public void Prepare()
        {
            Size = BasisAvatarBitPacking.ConvertToSize(BitQuality.High);
            BNL.Log($"Payload Size for muscles is now {Size}");

            _cachedPasswordBytes = Encoding.UTF8.GetBytes(ConfigManager.Password);
            var avatarInfo = new BasisAvatarNetworkLoad
            {
                URL = ConfigManager.AvatarUrl,
                UnlockPassword = ConfigManager.AvatarPassword
            };
            _cachedAvatarBytes = avatarInfo.EncodeToBytes();

            FinalPeers = new NetPeer[ClientCount];
            FinalClients = new NetworkClient[ClientCount];
        }

        /// <summary>
        /// StartClientsAsyncを開始します。依存する状態を準備して実行ループや待ち受けを有効化します。
        /// </summary>
        public async Task StartClientsAsync()
        {
            for (int Index = 0; Index < ClientCount; Index++)
            {
                var name = NameGenerator.GenerateRandomPlayerName();
                var identity = new ConsoleClientIdentity();

                var readyMessage = new ReadyMessage
                {
                    playerMetaDataMessage = new ClientMetaDataMessage
                    {
                        playerDisplayName = name,
                        playerUUID = identity.Did,
                        playerPlatform = "Headless",
                    },
                    clientAvatarChangeMessage = new ClientAvatarChangeMessage
                    {
                        byteArray = _cachedAvatarBytes,
                        loadMode = (byte)ConfigManager.AvatarLoadMode,
                        LocalAvatarIndex = 0,
                    },
                    localAvatarSyncMessage = new LocalAvatarSyncMessage
                    {
                        array = MovementSender.Generate().Message.array,
                        AdditionalAvatarDataSize = 0,
                        LinkedAvatarIndex = 0,
                        DataQualityLevel = (byte)BitQuality.High,
                        AdditionalAvatarDatas = null,

                    }
                };
                var netClient = new NetworkClient();
                var peer = netClient.StartClient(ConfigManager.Ip, ConfigManager.Port, readyMessage, _cachedPasswordBytes, CreateConfig(), manualMode: true);

                if (peer != null)
                {
                    peer.Tag = identity;
                    netClient.listener.NetworkReceiveEvent += (p, r, ch, m) => MessageHandler.OnReceive(identity, p, r, ch, m);
                    netClient.listener.PeerDisconnectedEvent += MessageHandler.OnDisconnect;

                    Volatile.Write(ref FinalClients[Index], netClient);
                    Volatile.Write(ref FinalPeers[Index], peer);

                    BNL.Log($"Connecting: {name} ({identity.Did})");
                }

                await Task.Delay(1, cts.Token);
            }
        }
        /// <summary>
        /// ReconnectクライアントAsyncを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task ReconnectClientAsync(int index)
        {
            if (index < 0 || index >= FinalClients.Length) return;

            var oldClient = Volatile.Read(ref FinalClients[index]);

            if (oldClient?.listener != null)
            {
                oldClient.listener.PeerDisconnectedEvent -= MessageHandler.OnDisconnect;
            }
            oldClient?.Disconnect();
            BNL.Log($"Disconnected client at index {index}");

            await Task.Delay(3000); // reconnect 前に待機する。

            var name = NameGenerator.GenerateRandomPlayerName();
            var identity = new ConsoleClientIdentity();

            var readyMessage = new ReadyMessage
            {
                playerMetaDataMessage = new ClientMetaDataMessage
                {
                    playerDisplayName = name,
                    playerUUID = identity.Did,
                    playerPlatform = "Headless",
                },
                clientAvatarChangeMessage = new ClientAvatarChangeMessage
                {
                    byteArray = _cachedAvatarBytes,
                    loadMode = (byte)ConfigManager.AvatarLoadMode,
                    LocalAvatarIndex = 1,

                },
                localAvatarSyncMessage = new LocalAvatarSyncMessage
                {
                    array = MovementSender.Generate().Message.array,
                    AdditionalAvatarDataSize = 0,
                    LinkedAvatarIndex = 0,
                }
            };

            var netClient = new NetworkClient();
            var peer = netClient.StartClient(ConfigManager.Ip, ConfigManager.Port, readyMessage, _cachedPasswordBytes, CreateConfig(), manualMode: true);

            if (peer != null)
            {
                peer.Tag = identity;
                netClient.listener.NetworkReceiveEvent += (p, r, ch, m) => MessageHandler.OnReceive(identity, p, r, ch, m);
                netClient.listener.PeerDisconnectedEvent += MessageHandler.OnDisconnect;

                Interlocked.Exchange(ref FinalClients[index], netClient);
                Interlocked.Exchange(ref FinalPeers[index], peer);

                BNL.Log($"Reconnected: {name} ({identity.Did}) at index {index}");
            }
        }
        /// <summary>
        /// StopClientsAsyncを停止します。保持している状態を片付け、次回起動に影響が残らないようにします。
        /// </summary>
        public Task StopClientsAsync()
        {
            if (FinalClients != null)
                foreach (var client in FinalClients) client?.Disconnect();
            return Task.CompletedTask;
        }
        /// <summary>
        /// Create設定を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public Configuration CreateConfig()
        {
            Configuration Configuration = new Configuration();
            Basis.Network.Core.BasisTransportConfigStore.Get<Basis.Network.Core.LNLTransportConfig>(
                Basis.Network.Core.BasisNetworkStackRegistry.LiteNetLibId).UseNativeSockets = true;
            Configuration.UseAuthIdentity = true;

            return Configuration;
        }
    }

    /// <summary>
    /// Consoleクライアント識別情報の責務をまとめるクラスです。
    /// ClientConsole領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public sealed class ConsoleClientIdentity
    {
        private readonly PrivKey _privateKey;

        /// <summary>
        /// Authenticatedを保持します。型は volatile bool で、関連処理から共有される値です。
        /// </summary>
        public volatile bool Authenticated;

        public string Did { get; }

        /// <summary>
        /// Consoleクライアント識別情報を生成し、利用に必要な初期状態を設定します。
        /// </summary>
        public ConsoleClientIdentity()
        {
            BasisDIDAuthIdentityClient.ClientKeyCreation(out (PubKey, PrivKey) keys, out Did did);
            _privateKey = keys.Item2;
            Did = did.V;
        }

        /// <summary>
        /// TryRespondToChallengeを試行し、失敗時に呼び出し元が分岐できる結果を返します。
        /// </summary>
        public bool TryRespondToChallenge(NetPacketReader reader, out NetDataWriter writer)
        {
            writer = new NetDataWriter();

            BytesMessage challenge = new BytesMessage();
            if (!challenge.Deserialize(reader, out byte[] nonce))
            {
                BNL.LogError("Malformed auth challenge from server");
                return false;
            }

            if (!Ed25519.Sign(_privateKey, new Payload(nonce), out Signature? signature) || signature == null)
            {
                BNL.LogError("Unable to sign auth challenge");
                return false;
            }

            BytesMessage signatureBytes = new BytesMessage();
            BytesMessage fragmentBytes = new BytesMessage();
            signatureBytes.Serialize(writer, signature.V);
            fragmentBytes.Serialize(writer, Encoding.UTF8.GetBytes("N/A"));
            return true;
        }
    }
}
