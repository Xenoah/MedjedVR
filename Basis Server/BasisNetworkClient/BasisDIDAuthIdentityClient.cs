using System;
using System.Diagnostics;
using System.Text;
using Basis.Contrib.Auth.DecentralizedIds;
using Basis.Contrib.Auth.DecentralizedIds.Newtypes;
using Basis.Contrib.Crypto;
using Basis.Network.Core;
#if UNITY_2017_1_OR_NEWER
using UnityEngine;
#endif
using static Basis.Network.Core.Serializable.SerializableBasis;
using CryptoRng = System.Security.Cryptography.RandomNumberGenerator;

namespace BasisNetworkClient
{
    /// <summary>
    /// BasisDID認証識別情報クライアントの責務をまとめるクラスです。
    /// Client領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static class BasisDIDAuthIdentityClient
    {
        private static (PubKey, PrivKey) Key;
        private static Did DID;
        private static DidUrlFragment DidUrlFragment;
        private const string PrivateKeyDID = "PrivateKeyDID";
        private const string PublicKeyDID = "PublicKeyDID";
        private const string DIDID = "DIDID";
        /// <summary>
        /// GetOrSaveDIDを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public static string GetOrSaveDID()
        {
#if UNITY_2017_1_OR_NEWER
            DidUrlFragment = new DidUrlFragment(string.Empty);

            string privateKeyBase64 = PlayerPrefs.GetString(PrivateKeyDID, string.Empty);
            string publicKeyBase = PlayerPrefs.GetString(PublicKeyDID, string.Empty);
            string didId = PlayerPrefs.GetString(DIDID, string.Empty);

            if (string.IsNullOrEmpty(privateKeyBase64) || string.IsNullOrEmpty(publicKeyBase) || string.IsNullOrEmpty(didId))
            {
                ClientKeyCreation(out Key, out DID);
                PlayerPrefs.SetString(PrivateKeyDID, Convert.ToBase64String(Key.Item2.V));
                PlayerPrefs.SetString(PublicKeyDID, Convert.ToBase64String(Key.Item1.V));
                PlayerPrefs.SetString(DIDID, DID.V);
                PlayerPrefs.Save();
            }
            else
            {
                DID = new Did(didId);
                byte[] publicKeyBytes = Convert.FromBase64String(publicKeyBase);
                byte[] privateKeyBytes = Convert.FromBase64String(privateKeyBase64);

                PubKey pubKey = new PubKey(publicKeyBytes);
                PrivKey privKey = new PrivKey(privateKeyBytes);

                Key = (pubKey, privKey);
            }
            return DID.V;
#else
            return string.Empty;
#endif
        }

        /// <summary>
        /// 識別情報メッセージを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static bool IdentityMessage(NetPeer peer, NetPacketReader Reader, out NetDataWriter Writer)
        {
            Writer = new NetDataWriter();
            BytesMessage ChallengeBytes = new BytesMessage();

            ChallengeBytes.Deserialize(Reader, out byte[] PayloadBytes);
            // client 側で challenge payload に署名する。
            Payload payloadToSign = new Payload(PayloadBytes);
            if (Ed25519.Sign(Key.Item2, payloadToSign, out Signature sig) == false)
            {
                BNL.LogError("Unable to sign Key");
                return false;
            }
            if (Ed25519.Verify(Key.Item1, sig, payloadToSign) == false)
            {
                BNL.LogError("Unable to Very Key");
                return false;
            }
            // client は pubkey を 1 つだけ持つため、単純化のため空 fragment を使う。
            Response response = new Response(sig, DidUrlFragment);
            BytesMessage SignatureBytes = new BytesMessage();
            BytesMessage FragmentBytes = new BytesMessage();
            SignatureBytes.Serialize(Writer, response.Signature.V);
            string Fragment = response.DidUrlFragment.V;
            if (string.IsNullOrEmpty(Fragment))
            {
                Fragment = "N/A";
            }
            FragmentBytes.Serialize(Writer, Encoding.UTF8.GetBytes(Fragment));
            return true;
        }
        public static (PubKey, PrivKey) RandomKeyPair(CryptoRng rng)
        {
            var privKeyBytes = new byte[Ed25519.PrivkeySize];
            rng.GetBytes(privKeyBytes);
            var privKey = new PrivKey(privKeyBytes);
            var pubKey = Ed25519.ConvertPrivkeyToPubkey(privKey) ?? throw new Exception("privkey was invalid");
            return (pubKey, privKey);
        }
        /// <summary>
        /// クライアントKeyCreationを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void ClientKeyCreation(out (PubKey, PrivKey) Keys, out Did Did)
        {
            // client 用 key pair と DID を生成する。
            CryptoRng rng = CryptoRng.Create();
            Keys = RandomKeyPair(rng);
            Did = DidKeyResolver.EncodePubkeyAsDid(Keys.Item1);
        }
    }
}
