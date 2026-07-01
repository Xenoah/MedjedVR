using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace Basis.Network.Core
{
    /// <summary>
    /// LNL接続TargetParserの責務をまとめるクラスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public sealed class LNLConnectionTargetParser : IConnectionTargetParser
    {
        /// <summary>
        /// DefaultPortを保持します。型は ushort で、関連処理から共有される値です。
        /// </summary>
        public const ushort DefaultPort = 4296;

        /// <summary>
        /// Parseを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public void Parse(ConnectionTarget target)
        {
            if (target == null) return;
            if (TryParseConnectionString(target.Raw, out string address, out ushort port, out _, out string password))
            {
                target.Set(ConnectionTarget.Keys.Address, address);
                target.Set(ConnectionTarget.Keys.Port, port.ToString(CultureInfo.InvariantCulture));
                target.Set(ConnectionTarget.Keys.Password, password ?? string.Empty);
            }
        }

        /// <summary>
        /// Formatを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public string Format(ConnectionTarget target)
        {
            if (target == null) return string.Empty;
            string address = target.Get(ConnectionTarget.Keys.Address, string.Empty);
            string portString = target.Get(ConnectionTarget.Keys.Port, DefaultPort.ToString(CultureInfo.InvariantCulture));
            string password = target.Get(ConnectionTarget.Keys.Password, string.Empty);

            bool isIPv6 = IPAddress.TryParse(address, out IPAddress ip)
                && ip.AddressFamily == AddressFamily.InterNetworkV6;
            string s = isIPv6 ? $"[{address}]:{portString}" : $"{address}:{portString}";
            if (!string.IsNullOrEmpty(password)) s += "#" + password;
            return s;
        }

        public static bool TryParseConnectionString(
            string raw, out string address, out ushort port, out bool portProvided, out string password)
        {
            address = string.Empty;
            port = DefaultPort;
            portProvided = false;
            password = string.Empty;
            if (string.IsNullOrEmpty(raw)) return false;

            string left = raw;
            int hashIdx = raw.IndexOf('#');
            if (hashIdx >= 0)
            {
                password = raw.Substring(hashIdx + 1);
                left = raw.Substring(0, hashIdx);
            }

            if (left.Length > 0 && left[0] == '[')
            {
                // bracketed IPv6 literal: [addr]:port または [addr]。
                int closeBracket = left.IndexOf(']');
                if (closeBracket > 0)
                {
                    address = left.Substring(1, closeBracket - 1).Trim();
                    string afterBracket = left.Substring(closeBracket + 1);
                    if (afterBracket.Length > 1 && afterBracket[0] == ':')
                    {
                        if (ushort.TryParse(afterBracket.Substring(1), out ushort parsedPort) && parsedPort > 0)
                        {
                            port = parsedPort;
                            portProvided = true;
                        }
                    }
                }
                else
                {
                    address = left.Trim(); // malformed bracket — treat whole string as address
                }
            }
            else
            {
                int colonIdx = left.LastIndexOf(':');
                if (colonIdx > 0
                    && colonIdx < left.Length - 1
                    && ushort.TryParse(left.Substring(colonIdx + 1), out ushort parsedPort)
                    && parsedPort > 0)
                {
                    string candidateAddress = left.Substring(0, colonIdx).Trim();
                    // candidate address にまだ colon が含まれる場合、bare IPv6 literal
                    // (例: "::1", "2001:db8::1") が host:port と誤読されている。
                    // split せずに残す。明示的な port 付き IPv6 address には
                    // [addr]:port notation を使う必要がある。
                    if (candidateAddress.IndexOf(':') < 0)
                    {
                        address = candidateAddress;
                        port = parsedPort;
                        portProvided = true;
                    }
                    else
                    {
                        address = left.Trim();
                    }
                }
                else
                {
                    address = left.Trim();
                }
            }

            return !string.IsNullOrEmpty(address);
        }
    }
}
