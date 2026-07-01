using Basis.Network.Core;
using System.Net;

namespace BasisNetworkServer
{
    /// <summary>
    /// LNLピアIntroducerの責務をまとめるクラスです。
    /// Server領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public sealed class LNLPeerIntroducer : IPeerIntroducer
    {
        /// <summary>
        /// Initializeを初期化します。設定、永続化ファイル、実行時キャッシュを起動時の状態へ整えます。
        /// </summary>
        public bool Initialize(NetManager activeManager)
        {
            BasisServerP2PBroker.Initialize();
            return true;
        }

        public void Introduce(IPEndPoint aInternal, IPEndPoint aExternal,
                              IPEndPoint bInternal, IPEndPoint bExternal,
                              string token)
        {
            LiteNetLib.NetManager lnl = (NetworkServer.Server as LNLNetManager)?.manager;
            if (lnl == null) return;
            lnl.NatPunchModule.NatIntroduce(aInternal, aExternal, bInternal, bExternal, token);
        }

        /// <summary>
        /// IsPairOffloadedを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public bool IsPairOffloaded(int peerIdA, int peerIdB)
            => BasisServerP2PBroker.IsP2POffloaded(peerIdA, peerIdB);

        public void Shutdown() { }
    }
}
