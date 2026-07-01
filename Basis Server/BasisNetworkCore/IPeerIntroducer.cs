using System.Net;

namespace Basis.Network.Core
{
    /// <summary>
    /// IピアIntroducerの責務をまとめるインターフェイスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public interface IPeerIntroducer
    {
        bool Initialize(NetManager activeManager);
        void Introduce(IPEndPoint aInternal, IPEndPoint aExternal,
                       IPEndPoint bInternal, IPEndPoint bExternal,
                       string token);
        bool IsPairOffloaded(int peerIdA, int peerIdB);
        void Shutdown();
    }
}
