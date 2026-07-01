using Basis.Network.Core;
#if UNITY_2017_1_OR_NEWER
using UnityEngine;
#endif

namespace BasisNetworkClient
{
    /// <summary>
    /// BasisDID認証識別情報Providerの責務をまとめるクラスです。
    /// Client領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public sealed class BasisDIDAuthIdentityProvider : IPlayerIdentityProvider
    {
        /// <summary>
        /// Idを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public const string Id = "did";

        /// <summary>
        /// ProviderIdを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string ProviderId => Id;

        /// <summary>
        /// GetOrCreateを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public PlayerIdentity GetOrCreate()
        {
            string uuid = BasisDIDAuthIdentityClient.GetOrSaveDID();
            return new PlayerIdentity
            {
                Uuid = uuid,
                Provider = Id,
            };
        }

#if UNITY_2017_1_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        /// <summary>
        /// AutoRegisterを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private static void AutoRegister()
        {
            BasisPlayerIdentityRegistry.Register(new BasisDIDAuthIdentityProvider());
            BasisPlayerIdentityRegistry.ActiveProviderId = Id;
        }
#endif
    }
}
