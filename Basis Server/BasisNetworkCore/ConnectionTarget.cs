using System;
using System.Collections.Generic;

namespace Basis.Network.Core
{
    /// <summary>
    /// 接続Targetの責務をまとめるクラスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public sealed class ConnectionTarget
    {
        /// <summary>
        /// StackIdを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string StackId;
        /// <summary>
        /// Rawを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string Raw;
        /// <summary>
        /// Propertiesを保持します。型は Dictionary<string, string> で、関連処理から共有される値です。
        /// </summary>
        public Dictionary<string, string> Properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public ConnectionTarget() { }

        /// <summary>
        /// 接続Targetを生成し、利用に必要な初期状態を設定します。
        /// </summary>
        public ConnectionTarget(string stackId, string raw)
        {
            StackId = stackId ?? string.Empty;
            Raw = raw ?? string.Empty;
        }

        /// <summary>
        /// Getを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public string Get(string key, string fallback = null)
        {
            if (string.IsNullOrEmpty(key) || Properties == null) return fallback;
            return Properties.TryGetValue(key, out string v) ? v : fallback;
        }

        /// <summary>
        /// TryGetを試行し、失敗時に呼び出し元が分岐できる結果を返します。
        /// </summary>
        public bool TryGet(string key, out string value)
        {
            value = null;
            if (string.IsNullOrEmpty(key) || Properties == null) return false;
            return Properties.TryGetValue(key, out value);
        }

        /// <summary>
        /// Setを設定します。以後のネットワーク処理で参照される状態を更新します。
        /// </summary>
        public void Set(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) return;
            Properties ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Properties[key] = value ?? string.Empty;
        }

        /// <summary>
        /// Keysの責務をまとめるクラスです。
        /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
        /// </summary>
        public static class Keys
        {
            /// <summary>
            /// Addressを保持します。型は string で、関連処理から共有される値です。
            /// </summary>
            public const string Address = "address";
            /// <summary>
            /// Portを保持します。型は string で、関連処理から共有される値です。
            /// </summary>
            public const string Port = "port";
            /// <summary>
            /// Passwordを保持します。型は string で、関連処理から共有される値です。
            /// </summary>
            public const string Password = "password";
            /// <summary>
            /// LobbyIdを保持します。型は string で、関連処理から共有される値です。
            /// </summary>
            public const string LobbyId = "lobbyId";
        }
    }

    /// <summary>
    /// I接続TargetParserの責務をまとめるインターフェイスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public interface IConnectionTargetParser
    {
        void Parse(ConnectionTarget target);
        string Format(ConnectionTarget target);
    }
}
