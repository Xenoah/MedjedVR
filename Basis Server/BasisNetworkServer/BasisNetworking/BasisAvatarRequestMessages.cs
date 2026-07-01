using Basis.Network.Core;

namespace BasisNetworkServer.BasisNetworking
{
    /// <summary>
    /// BasisアバターRequestMessagesの責務をまとめるクラスです。
    /// ing領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static class BasisAvatarRequestMessages
    {
        /// <summary>
        /// アバターCloneRequestメッセージを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void AvatarCloneRequestMessage(NetPacketReader Reader, NetPeer Peer)
        {
         ushort RemotePlayerID = Reader.GetUShort();
        }
        /// <summary>
        /// アバターCloneResponseメッセージを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void AvatarCloneResponseMessage(NetPacketReader Reader, NetPeer Peer)
        {
          ushort EndUser =  Reader.GetUShort();
            string ApprivalID = Reader.GetString();
        }
    }
}
