using Basis.Contrib.Auth.DecentralizedIds;
using Basis.Contrib.Auth.DecentralizedIds.Newtypes;
using Basis.Contrib.Crypto;
using Basis.Network.Core;
using Basis.Network.Server.Auth;
using BasisNetworkServer.Security;
using BasisServerHandle;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static Basis.Network.Core.Serializable.SerializableBasis;
using static BasisNetworkServer.Security.BasisPlayerModeration;
using static SerializableBasis;
using Challenge = Basis.Contrib.Auth.DecentralizedIds.Challenge;
using CryptoRng = System.Security.Cryptography.RandomNumberGenerator;

namespace BasisDidLink
{
    /// <summary>
    /// BasisDID認証識別情報の責務をまとめるクラスです。
    /// Security領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public class BasisDIDAuthIdentity : IAuthIdentity
    {
        /// <summary>
        /// Did認証を保持します。型は DidAuthentication で、関連処理から共有される値です。
        /// </summary>
        internal readonly DidAuthentication DidAuth;
        /// <summary>
        /// 認証識別情報を保持します。型は ConcurrentDictionary<int, OnAuth> で、関連処理から共有される値です。
        /// </summary>
        public ConcurrentDictionary<int, OnAuth> AuthIdentity = new ConcurrentDictionary<int, OnAuth>();
        private readonly ConcurrentDictionary<int, CancellationTokenSource> _timeouts = new ConcurrentDictionary<int, CancellationTokenSource>();
        /// <summary>
        /// Adminsを保持します。型は ConcurrentDictionary<string, byte> で、関連処理から共有される値です。
        /// </summary>
        public ConcurrentDictionary<string, byte> Admins = new ConcurrentDictionary<string, byte>();
        /// <summary>
        /// FilePathを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Configuration.ConfigFolderName, "admins.xml");
        /// <summary>
        /// BasisDID認証識別情報を生成し、利用に必要な初期状態を設定します。
        /// </summary>
        public BasisDIDAuthIdentity()
        {
            string[] LoadedAdmins = LoadAdmins(FilePath);
            if (LoadedAdmins != null)
            {
                foreach (var admin in LoadedAdmins)
                {
                    Admins.TryAdd(admin, 0);
                }
            }
            else
            {
                Admins = new ConcurrentDictionary<string, byte>();
            }
            string adminsList = string.Join(", ", Admins);
            BNL.Log($"Loaded Admins {Admins.Count} {adminsList}");
            CryptoRng rng = CryptoRng.Create();
            Config cfg = new Config { Rng = rng };
            DidAuth = new DidAuthentication(cfg);
            BasisServerHandleEvents.OnAuthReceived += OnAuthReceived;
            BNL.Log("DidAuthIdentity initialized.");
        }

        /// <summary>
        /// DeInitializeを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public void DeInitialize()
        {
            BasisServerHandleEvents.OnAuthReceived -= OnAuthReceived;
            BNL.Log("DidAuthIdentity deinitialized.");
        }

        /// <summary>
        /// UnpackStringを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static string UnpackString(byte[] compressedBytes)
        {
            return Encoding.UTF8.GetString(compressedBytes, 0, compressedBytes.Length);
        }

        /// <summary>
        /// On認証の責務をまとめる構造体です。
        /// Security領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
        /// </summary>
        public struct OnAuth
        {
            /// <summary>
            /// 準備完了メッセージを保持します。型は ReadyMessage で、関連処理から共有される値です。
            /// </summary>
            public ReadyMessage ReadyMessage;
            /// <summary>
            /// Challengeを保持します。型は Challenge で、関連処理から共有される値です。
            /// </summary>
            public Challenge Challenge;
            /// <summary>
            /// Didを保持します。型は Did で、関連処理から共有される値です。
            /// </summary>
            public Did Did;
        }
        /// <summary>
        /// CheckForDuplicatesを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public int CheckForDuplicates(Did Did)
        {
            return (from key in AuthIdentity.Values
                    where key.Did.V == Did.V
                    select key).Count();
        }
        /// <summary>
        /// Process接続を処理します。受信データを検証し、必要な状態更新や再配信を行います。
        /// </summary>
        public void ProcessConnection(Configuration Configuration, ConnectionRequest ConnectionRequest, NetPeer newPeer)
        {
            try
            {
                BNL.Log($"Processing connection from peer {newPeer.Id}.");
                ReadyMessage readyMessage = new ReadyMessage();
                readyMessage.Deserialize(ConnectionRequest.Data);

                if (readyMessage.WasDeserializedCorrectly())
                {
                    if (BasisServerHandleEvents.IsHeadlessDisallowed(readyMessage.playerMetaDataMessage, out string reason))
                    {
                        BasisServerHandleEvents.RejectWithReason(newPeer, reason);
                        return;
                    }

                    string UUID = readyMessage.playerMetaDataMessage.playerUUID;
                    Did playerDid = new Did(UUID);
                    if (BasisPlayerModeration.IsBanned(UUID))
                    {
                        if (BasisPlayerModeration.GetBannedReason(UUID, out string Reason))
                        {
                            BasisServerHandleEvents.RejectWithReason(newPeer, "Banned User!  Reason " + Reason);

                        }
                        else
                        {
                            BasisServerHandleEvents.RejectWithReason(newPeer, " Banned User!");
                        }
                        return;
                    }
                    if (Configuration.HowManyDuplicateAuthCanExist <= CheckForDuplicates(playerDid))
                    {
                        BasisServerHandleEvents.RejectWithReason(newPeer, "To Many Auths From this DID!");
                        return;
                    }

                    OnAuth OnAuth = new OnAuth
                    {
                        Did = playerDid,
                        Challenge = MakeChallenge(playerDid),
                        ReadyMessage = readyMessage
                    };

                    if (AuthIdentity.TryAdd(newPeer.Id, OnAuth))
                    {
                        readyMessage.playerMetaDataMessage.playerUUID = playerDid.V;
                        NetDataWriter Writer = NetworkServer.RentWriter();
                        BytesMessage NetworkMessage = new BytesMessage();
                        NetworkMessage.Serialize(Writer, OnAuth.Challenge.Nonce.V);
                        BNL.Log("Sending out Writer with size : " + Writer.Length);
                        NetworkServer.TrySend(newPeer, Writer, BasisNetworkCommons.AuthIdentityChannel, DeliveryMethod.ReliableOrdered);
                        NetworkServer.ReturnWriter(Writer);

                        CancellationTokenSource cts = new CancellationTokenSource();
                        _timeouts[newPeer.Id] = cts;
                        Task.Run(async () =>
                        {
                            await TimeOut(newPeer, UUID, cts);
                        });
                        //   BasisServerHandleEvents.OnNetworkAccepted(newPeer, readyMessage, playerDid.V);
                    }
                    else
                    {
                        BasisServerHandleEvents.RejectWithReason(newPeer, "Payload Provided was invalid! potential Duplication");
                    }
                }
                else
                {
                    BasisServerHandleEvents.RejectWithReason(newPeer, "Invalid ReadyMessage received.");
                }
            }
            catch (Exception e)
            {
                BNL.Log($"Error processing connection: {e.Message} {e.StackTrace}");
                BasisServerHandleEvents.RejectWithReason(newPeer, "Connection could not be processed.");
            }
        }
        /// <summary>
        /// TimeOutを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task TimeOut(NetPeer newPeer, string UUID, CancellationTokenSource cts)
        {
            try
            {
                await Task.Delay(NetworkServer.Configuration.AuthValidationTimeOutMiliseconds, cts.Token);
                if (!_timeouts.ContainsKey(newPeer.Id)) return;
                AuthIdentity.TryRemove(newPeer.Id, out _);
                _timeouts.TryRemove(newPeer.Id, out _);
                cts.Dispose();
                BNL.Log($"Authentication timeout for {UUID}.");
                BasisServerHandleEvents.RejectWithReason(newPeer, "Authentication timeout");
                newPeer.Disconnect();
            }
            catch (TaskCanceledException) { }
        }

        /// <summary>
        /// On認証Receivedイベントを受け取り、関連するサーバー状態や送信処理へ反映します。
        /// </summary>
        private async void OnAuthReceived(NetPacketReader reader, NetPeer newPeer)
        {
            try
            {
                //     BNL.Log($"Authentication response received from {newPeer.Id}.");
                if (_timeouts.TryRemove(newPeer.Id, out var cts))
                {
                    cts.Cancel();
                    cts.Dispose();
                }

                BytesMessage SignatureBytes = new BytesMessage();
                if (!SignatureBytes.Deserialize(reader, out byte[] SigBytes))
                {
                    BNL.LogError($"Malformed auth response from peer {newPeer.Id}: bad signature data");
                    BasisServerHandleEvents.RejectWithReason(newPeer, "Malformed auth response: bad signature data");
                    return;
                }
                BytesMessage FragmentBytes = new BytesMessage();
                if (!FragmentBytes.Deserialize(reader, out byte[] FragBytes))
                {
                    BNL.LogError($"Malformed auth response from peer {newPeer.Id}: bad fragment data");
                    BasisServerHandleEvents.RejectWithReason(newPeer, "Malformed auth response: bad fragment data");
                    return;
                }

                Signature Sig = new Signature(SigBytes);
                string FragmentAsString = UnpackString(FragBytes);
                if (FragmentAsString == "N/A")
                {
                    FragmentAsString = string.Empty;
                }
                DidUrlFragment Fragment = new DidUrlFragment(FragmentAsString);
                Response response = new Response(Sig, Fragment);

                if (AuthIdentity.TryGetValue(newPeer.Id, out OnAuth authIdentity))
                {
                    Challenge challenge = authIdentity.Challenge;
                    bool isAuthenticated = await RecvChallengeResponse(response, challenge);

                    if (isAuthenticated)
                    {
                        BasisServerHandleEvents.OnNetworkAccepted(newPeer, authIdentity.ReadyMessage, authIdentity.Did.V);
                    }
                    else
                    {
                        BNL.LogError($"Authentication failed for {authIdentity.Did.V}.");
                        BasisServerHandleEvents.RejectWithReason(newPeer, "was unable to authenticate!");
                    }
                }
            }
            catch (Exception e)
            {
                BNL.Log($"Error during authentication: {e.Message} {e.StackTrace}");
                BasisServerHandleEvents.RejectWithReason(newPeer, "Authentication failed.");
            }
        }
        /// <summary>
        /// MakeChallengeを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public Challenge MakeChallenge(Did ChallengingDID)
        {
            return DidAuth.MakeChallenge(ChallengingDID ?? throw new Exception("call RecvDid first"));
        }

        /// <summary>
        /// RecvChallengeResponseを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task<bool> RecvChallengeResponse(Response response, Challenge Challenge)
        {
            if (!response.DidUrlFragment.V.Equals(string.Empty))
            {
                throw new Exception("multiple pubkeys not yet supported");
            }
            var challenge = Challenge ?? throw new Exception("call SendChallenge first");
            var result = await DidAuth.VerifyResponse(response, challenge);
            return result.IsOk;
        }

        /// <summary>
        /// Remove接続を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public void RemoveConnection(int NetPeer)
        {
            AuthIdentity.TryRemove(NetPeer, out var authIdentity);
            if (_timeouts.TryRemove(NetPeer, out var cts))
            {
                try { cts.Cancel(); } catch { }
                cts.Dispose();
            }
        }
        /// <summary>
        /// IsNetピアAdminを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public bool IsNetPeerAdmin(string UUID)
        {
            if (Admins.ContainsKey(UUID))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// AddNetピアAsAdminを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public bool AddNetPeerAsAdmin(string UUID)
        {
            if (string.IsNullOrEmpty(UUID))
            {
                BNL.Log($"can't add was empty or null! {UUID}");
                return false;
            }
            else
            {
                BNL.Log($"AddNetPeerAsAdmin {UUID}");
                Admins.TryAdd(UUID, 0);
                SaveAdmins(Admins.Keys.ToArray(), FilePath);
                return true;
            }
        }
        /// <summary>
        /// SaveAdminsを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        static void SaveAdmins(string[] admins, string filePath)
        {
            if (!IAuthIdentity.HasFileSupport)
            {
                return;
            }
            admins ??= new string[0]; // null にならないようにする

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(string[]));
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    serializer.Serialize(writer, admins);
                }
            }
            catch (Exception ex)
            {
                BNL.LogError($"Error saving admins: {ex.Message} {ex.StackTrace}");
            }
        }

        /// <summary>
        /// 読み込みAdminsを初期化します。設定、永続化ファイル、実行時キャッシュを起動時の状態へ整えます。
        /// </summary>
        static string[] LoadAdmins(string filePath)
        {
            if (IAuthIdentity.HasFileSupport)
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(string[]));
                        using (StreamReader reader = new StreamReader(filePath))
                        {
                            return (string[])serializer.Deserialize(reader);
                        }
                    }
                    catch (Exception ex)
                    {
                        BNL.LogError($"Error loading admins (possibly corrupted file), deleting and recreating: {ex.Message}");
                        File.Delete(filePath);
                    }
                }

                // file がない、または壊れている場合は新しく作成する。
                BNL.Log("Creating a new admin list...");
                string[] newAdmins = new string[0];
                SaveAdmins(newAdmins, filePath);
                return newAdmins;
            }
            else
            {
                string[] newAdmins = new string[0];
                return newAdmins;
            }
        }


        /// <summary>
        /// NetIDToUUIDを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public bool NetIDToUUID(NetPeer Peer, out string UUID)
        {
            if (AuthIdentity.TryGetValue(Peer.Id, out OnAuth OnAuth))
            {
                UUID = OnAuth.Did.V;
                return true;
            }
            UUID = string.Empty;
            return false;
        }

        /// <summary>
        /// UUIDToNetIDを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public bool UUIDToNetID(string UUID, out int Peer)
        {
            foreach (KeyValuePair<int, OnAuth> Pair in AuthIdentity)
            {
                if (Pair.Value.Did.V == UUID)
                {
                    Peer = Pair.Key;
                    return true;
                }
            }
            Peer = 0;
            return false;
        }

        /// <summary>
        /// RemoveNetピアAsAdminを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public bool RemoveNetPeerAsAdmin(string UUID)
        {
            BNL.Log($"RemoveNetPeerAsAdmin {UUID}");
            if (Admins.TryRemove(UUID, out _))
            {
                SaveAdmins(Admins.Keys.ToArray(), FilePath);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
