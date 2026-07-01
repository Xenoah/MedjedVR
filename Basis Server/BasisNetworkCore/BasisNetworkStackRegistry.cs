using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Basis.Network.Core
{
    /// <summary>
    /// サーバーProbeResultの責務をまとめるクラスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public sealed class ServerProbeResult
    {
        /// <summary>
        /// Reachableを保持します。型は bool で、関連処理から共有される値です。
        /// </summary>
        public bool Reachable;
        /// <summary>
        /// エラーを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string Error;
        /// <summary>
        /// TimedOutを保持します。型は bool で、関連処理から共有される値です。
        /// </summary>
        public bool TimedOut;
        /// <summary>
        /// Onlineを保持します。型は ushort で、関連処理から共有される値です。
        /// </summary>
        public ushort Online;
        /// <summary>
        /// Maxを保持します。型は ushort で、関連処理から共有される値です。
        /// </summary>
        public ushort Max;
        /// <summary>
        /// ProtocolVersionを保持します。型は ushort で、関連処理から共有される値です。
        /// </summary>
        public ushort ProtocolVersion;
        /// <summary>
        /// Nameを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string Name;
        /// <summary>
        /// Motdを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string Motd;
        /// <summary>
        /// RoundTripMsを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public int RoundTripMs;
        /// <summary>
        /// Extrasを保持します。型は Dictionary<string, string> で、関連処理から共有される値です。
        /// </summary>
        public Dictionary<string, string> Extras = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public delegate Task<ServerProbeResult> StackProbeDelegate(ConnectionTarget target, int timeoutMs, CancellationToken ct);
    public delegate IPeerIntroducer PeerIntroducerFactory(NetManager activeManager);

    /// <summary>
    /// BasisネットワークStackRegistryの責務をまとめるクラスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static class BasisNetworkStackRegistry
    {
        /// <summary>
        /// LiteNetLibIdを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public const string LiteNetLibId = "litenetlib";
        /// <summary>
        /// DefaultIdを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public const string DefaultId = LiteNetLibId;

        /// <summary>
        /// StackInfoの責務をまとめる構造体です。
        /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
        /// </summary>
        public readonly struct StackInfo
        {
            /// <summary>
            /// Idを保持します。型は string で、関連処理から共有される値です。
            /// </summary>
            public readonly string Id;
            /// <summary>
            /// DisplayNameを保持します。型は string で、関連処理から共有される値です。
            /// </summary>
            public readonly string DisplayName;
            /// <summary>
            /// StackInfoを生成し、利用に必要な初期状態を設定します。
            /// </summary>
            public StackInfo(string id, string displayName)
            {
                Id = id;
                DisplayName = displayName;
            }
        }

        public delegate NetManager NetManagerFactory(EventBasedNetListener listener, Configuration configuration);

        /// <summary>
        /// Slotの責務をまとめるクラスです。
        /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
        /// </summary>
        private sealed class Slot
        {
            /// <summary>
            /// Idを保持します。型は string で、関連処理から共有される値です。
            /// </summary>
            public string Id;
            /// <summary>
            /// DisplayNameを保持します。型は string で、関連処理から共有される値です。
            /// </summary>
            public string DisplayName;
            /// <summary>
            /// Factoryを保持します。型は NetManagerFactory で、関連処理から共有される値です。
            /// </summary>
            public NetManagerFactory Factory;
            /// <summary>
            /// Parserを保持します。型は IConnectionTargetParser で、関連処理から共有される値です。
            /// </summary>
            public IConnectionTargetParser Parser;
            /// <summary>
            /// Probeを保持します。型は StackProbeDelegate で、関連処理から共有される値です。
            /// </summary>
            public StackProbeDelegate Probe;
            /// <summary>
            /// Tickを保持します。型は Action で、関連処理から共有される値です。
            /// </summary>
            public Action Tick;
            /// <summary>
            /// IntroducerFactoryを保持します。型は PeerIntroducerFactory で、関連処理から共有される値です。
            /// </summary>
            public PeerIntroducerFactory IntroducerFactory;
        }

        private static readonly Dictionary<string, Slot> _slots
            = new Dictionary<string, Slot>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<StackInfo> _stacks = new List<StackInfo>();
        private static readonly object _lock = new object();
        private static string _activeStackId = string.Empty;

        /// <summary>
        /// ActiveStackChangedを保持します。型は event Action<string> で、関連処理から共有される値です。
        /// </summary>
        public static event Action<string> ActiveStackChanged;

        /// <summary>
        /// BasisネットワークStackRegistryを生成し、利用に必要な初期状態を設定します。
        /// </summary>
        static BasisNetworkStackRegistry()
        {
            Register(LiteNetLibId, "LiteNetLib", (listener, config) => new LNLNetManager(listener, config));
            RegisterParser(LiteNetLibId, new LNLConnectionTargetParser());
            BasisTransportConfigStore.RegisterType(LiteNetLibId, typeof(LNLTransportConfig));
        }

        /// <summary>
        /// Registerを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void Register(string id, string displayName, NetManagerFactory factory)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Stack id is required", nameof(id));
            if (factory == null) throw new ArgumentNullException(nameof(factory));

            lock (_lock)
            {
                if (_slots.ContainsKey(id)) return;
                _slots[id] = new Slot { Id = id, DisplayName = string.IsNullOrEmpty(displayName) ? id : displayName, Factory = factory };
                _stacks.Add(new StackInfo(id, _slots[id].DisplayName));
            }
        }

        /// <summary>
        /// RegisterParserを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void RegisterParser(string stackId, IConnectionTargetParser parser)
        {
            if (string.IsNullOrEmpty(stackId)) throw new ArgumentException("Stack id is required", nameof(stackId));
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            lock (_lock)
            {
                if (!_slots.TryGetValue(stackId, out Slot slot))
                {
                    BNL.LogWarning($"Cannot register parser for unknown stack '{stackId}'");
                    return;
                }
                slot.Parser = parser;
            }
        }

        /// <summary>
        /// RegisterProbeを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void RegisterProbe(string stackId, StackProbeDelegate probe)
        {
            if (string.IsNullOrEmpty(stackId)) throw new ArgumentException("Stack id is required", nameof(stackId));
            if (probe == null) throw new ArgumentNullException(nameof(probe));
            lock (_lock)
            {
                if (!_slots.TryGetValue(stackId, out Slot slot))
                {
                    BNL.LogWarning($"Cannot register probe for unknown stack '{stackId}'");
                    return;
                }
                slot.Probe = probe;
            }
        }

        /// <summary>
        /// RegisterTickを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void RegisterTick(string stackId, Action tick)
        {
            if (string.IsNullOrEmpty(stackId)) throw new ArgumentException("Stack id is required", nameof(stackId));
            if (tick == null) throw new ArgumentNullException(nameof(tick));
            lock (_lock)
            {
                if (!_slots.TryGetValue(stackId, out Slot slot))
                {
                    BNL.LogWarning($"Cannot register tick for unknown stack '{stackId}'");
                    return;
                }
                slot.Tick = tick;
            }
        }

        /// <summary>
        /// RegisterIntroducerFactoryを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void RegisterIntroducerFactory(string stackId, PeerIntroducerFactory factory)
        {
            if (string.IsNullOrEmpty(stackId)) throw new ArgumentException("Stack id is required", nameof(stackId));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            lock (_lock)
            {
                if (!_slots.TryGetValue(stackId, out Slot slot))
                {
                    BNL.LogWarning($"Cannot register peer introducer for unknown stack '{stackId}'");
                    return;
                }
                slot.IntroducerFactory = factory;
            }
        }

        /// <summary>
        /// Createを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static NetManager Create(string id, EventBasedNetListener listener, Configuration configuration)
        {
            string effective = string.IsNullOrEmpty(id) ? DefaultId : id;
            Slot slot;
            lock (_lock)
            {
                if (!_slots.TryGetValue(effective, out slot))
                {
                    BNL.LogWarning($"Network stack '{effective}' is not registered, falling back to '{DefaultId}'");
                    slot = _slots[DefaultId];
                    effective = DefaultId;
                }
            }
            NetManager mgr = slot.Factory(listener, configuration);
            SetActiveStackId(effective);
            return mgr;
        }

        public static string ActiveStackId
        {
            get { lock (_lock) return _activeStackId; }
        }

        /// <summary>
        /// SetActiveStackIdを設定します。以後のネットワーク処理で参照される状態を更新します。
        /// </summary>
        public static void SetActiveStackId(string id)
        {
            string normalized = string.IsNullOrEmpty(id) ? string.Empty : id;
            bool changed = false;
            lock (_lock)
            {
                if (!string.Equals(_activeStackId, normalized, StringComparison.OrdinalIgnoreCase))
                {
                    _activeStackId = normalized;
                    changed = true;
                }
            }
            if (changed)
            {
                try { ActiveStackChanged?.Invoke(normalized); }
                catch (Exception ex) { BNL.LogError($"ActiveStackChanged handler threw: {ex.Message}"); }
            }
        }

        /// <summary>
        /// GetParserを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public static IConnectionTargetParser GetParser(string stackId)
        {
            string effective = string.IsNullOrEmpty(stackId) ? DefaultId : stackId;
            lock (_lock)
            {
                if (_slots.TryGetValue(effective, out Slot slot) && slot.Parser != null) return slot.Parser;
                if (!string.Equals(effective, DefaultId, StringComparison.OrdinalIgnoreCase))
                {
                    BNL.LogWarning($"No connection-target parser registered for stack '{effective}', falling back to '{DefaultId}'");
                    if (_slots.TryGetValue(DefaultId, out Slot fallback)) return fallback.Parser;
                }
            }
            return null;
        }

        /// <summary>
        /// ProbeAsyncを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static Task<ServerProbeResult> ProbeAsync(ConnectionTarget target, int timeoutMs, CancellationToken ct)
        {
            if (target == null) return Task.FromResult(new ServerProbeResult { Error = "Target is null" });
            string stackId = string.IsNullOrEmpty(target.StackId) ? DefaultId : target.StackId;
            StackProbeDelegate probe;
            lock (_lock)
            {
                if (!_slots.TryGetValue(stackId, out Slot slot) || slot.Probe == null)
                {
                    if (!string.Equals(stackId, DefaultId, StringComparison.OrdinalIgnoreCase))
                    {
                        BNL.LogWarning($"No probe registered for stack '{stackId}', falling back to '{DefaultId}'");
                    }
                    if (!_slots.TryGetValue(DefaultId, out Slot fallback) || fallback.Probe == null)
                    {
                        return Task.FromResult(new ServerProbeResult { Error = $"No probe registered for stack '{stackId}' (no fallback available)" });
                    }
                    probe = fallback.Probe;
                }
                else
                {
                    probe = slot.Probe;
                }
            }
            return probe(target, timeoutMs, ct);
        }

        /// <summary>
        /// TickActiveを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void TickActive()
        {
            Action tick = null;
            lock (_lock)
            {
                if (!string.IsNullOrEmpty(_activeStackId)
                    && _slots.TryGetValue(_activeStackId, out Slot slot))
                {
                    tick = slot.Tick;
                }
            }
            if (tick == null) return;
            try { tick(); }
            catch (Exception ex) { BNL.LogError($"Stack tick threw: {ex.Message}"); }
        }

        /// <summary>
        /// CreateIntroducerを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static IPeerIntroducer CreateIntroducer(string stackId, NetManager activeManager)
        {
            string effective = string.IsNullOrEmpty(stackId) ? DefaultId : stackId;
            PeerIntroducerFactory factory;
            lock (_lock)
            {
                if (!_slots.TryGetValue(effective, out Slot slot) || slot.IntroducerFactory == null)
                {
                    if (!string.Equals(effective, DefaultId, StringComparison.OrdinalIgnoreCase))
                    {
                        BNL.LogWarning($"No peer introducer registered for stack '{effective}', falling back to '{DefaultId}'");
                    }
                    if (!_slots.TryGetValue(DefaultId, out Slot fallback) || fallback.IntroducerFactory == null) return null;
                    factory = fallback.IntroducerFactory;
                }
                else
                {
                    factory = slot.IntroducerFactory;
                }
            }
            return factory(activeManager);
        }

        /// <summary>
        /// IsRegisteredを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static bool IsRegistered(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            lock (_lock) return _slots.ContainsKey(id);
        }

        public static IReadOnlyList<StackInfo> Stacks
        {
            get { lock (_lock) return _stacks.ToArray(); }
        }

        /// <summary>
        /// GetDisplayNameを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public static string GetDisplayName(string id)
        {
            if (string.IsNullOrEmpty(id)) id = DefaultId;
            lock (_lock)
            {
                if (_slots.TryGetValue(id, out Slot slot)) return slot.DisplayName;
            }
            return id;
        }
    }
}
