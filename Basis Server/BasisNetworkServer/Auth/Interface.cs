using Basis.Network.Core;
namespace Basis.Network.Server.Auth
{
    /// <summary>
    /// 認証できるかどうかを判定するための interface。
    /// (password が正しいかどうか)
    /// </summary>
    public interface IAuth
    {
        public bool IsAuthenticated(byte[] BytesMsg);
    }
    /// <summary>
    /// I認証識別情報の責務をまとめるインターフェイスです。
    /// Auth領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public interface IAuthIdentity
    {
        /// <summary>
        /// user の identity を取得するために使う interface。
        /// player の UUID はここで扱う identity になる。
        /// </summary>
        public void ProcessConnection(Configuration Configuration, ConnectionRequest ConnectionRequest, NetPeer NetPeer);
        public void DeInitialize();
        public void RemoveConnection(int NetPeer);
        public bool NetIDToUUID(NetPeer Peer, out string UUID);
        public bool UUIDToNetID(string UUID, out int Peer);

        /// <summary>
        /// HasFileSupportを保持します。型は bool で、関連処理から共有される値です。
        /// </summary>
        public static bool HasFileSupport = false;
    }
}
