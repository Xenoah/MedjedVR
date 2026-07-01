using System;
using System.Collections.Generic;

namespace Basis.Network.Core
{
    /// <summary>
    /// プレイヤー識別情報の責務をまとめるクラスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public sealed class PlayerIdentity
    {
        /// <summary>
        /// Uuidを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string Uuid;
        /// <summary>
        /// Providerを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string Provider;
        /// <summary>
        /// Propertiesを保持します。型は Dictionary<string, string> で、関連処理から共有される値です。
        /// </summary>
        public Dictionary<string, string> Properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Iプレイヤー識別情報Providerの責務をまとめるインターフェイスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public interface IPlayerIdentityProvider
    {
        string ProviderId { get; }
        PlayerIdentity GetOrCreate();
    }

    /// <summary>
    /// Basisプレイヤー識別情報Registryの責務をまとめるクラスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static class BasisPlayerIdentityRegistry
    {
        /// <summary>
        /// DefaultProviderIdを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public const string DefaultProviderId = "did";

        private static readonly Dictionary<string, IPlayerIdentityProvider> _providers
            = new Dictionary<string, IPlayerIdentityProvider>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _lock = new object();
        private static string _activeProviderId = DefaultProviderId;

        /// <summary>
        /// Registerを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void Register(IPlayerIdentityProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (string.IsNullOrEmpty(provider.ProviderId)) throw new ArgumentException("ProviderId is required", nameof(provider));
            lock (_lock) _providers[provider.ProviderId] = provider;
        }

        public static string ActiveProviderId
        {
            get { lock (_lock) return _activeProviderId; }
            set { lock (_lock) _activeProviderId = string.IsNullOrEmpty(value) ? DefaultProviderId : value; }
        }

        /// <summary>
        /// ResolveActiveを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static PlayerIdentity ResolveActive()
        {
            IPlayerIdentityProvider provider = null;
            lock (_lock)
            {
                if (!_providers.TryGetValue(_activeProviderId, out provider))
                    _providers.TryGetValue(DefaultProviderId, out provider);
            }
            return provider?.GetOrCreate();
        }

        /// <summary>
        /// Resolveを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static PlayerIdentity Resolve(string providerId)
        {
            IPlayerIdentityProvider provider = null;
            lock (_lock)
            {
                if (string.IsNullOrEmpty(providerId)) providerId = _activeProviderId;
                _providers.TryGetValue(providerId, out provider);
            }
            return provider?.GetOrCreate();
        }

        /// <summary>
        /// IsRegisteredを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static bool IsRegistered(string providerId)
        {
            if (string.IsNullOrEmpty(providerId)) return false;
            lock (_lock) return _providers.ContainsKey(providerId);
        }
    }
}
