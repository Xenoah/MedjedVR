using Basis.Network.Core;
using System.Threading;
using static BasisNetworkCore.Serializable.SerializableBasis;

namespace BasisNetworkServer.Security
{
    /// <summary>
    /// headless audio clip playback 用の runtime-only server toggle。
    /// </summary>
    public static class BasisHeadlessAudioStateManager
    {
        private static int _headlessAudioOff;

        /// <summary>
        /// HeadlessAudioOffを保持します。型は bool で、関連処理から共有される値です。
        /// </summary>
        public static bool HeadlessAudioOff => Interlocked.CompareExchange(ref _headlessAudioOff, 0, 0) == 1;

        /// <summary>
        /// SetHeadlessAudioを設定します。以後のネットワーク処理で参照される状態を更新します。
        /// </summary>
        public static bool SetHeadlessAudio(bool headlessAudioOff)
        {
            int requestedValue = headlessAudioOff ? 1 : 0;
            int previousValue = Interlocked.Exchange(ref _headlessAudioOff, requestedValue);
            return previousValue != requestedValue;
        }

        /// <summary>
        /// Send状態Toピアを送信します。対象ピア、チャンネル、配送方式に合わせてパケット化します。
        /// </summary>
        public static void SendStateToPeer(NetPeer peer)
        {
            NetDataWriter writer = NetworkServer.RentWriter();
            try
            {
                new AdminRequest().Serialize(writer, AdminRequestMode.GlobalGetHeadlessAudioState);
                writer.Put(HeadlessAudioOff);
                NetworkServer.TrySend(peer, writer, BasisNetworkCommons.AdminChannel, DeliveryMethod.ReliableOrdered);
            }
            finally
            {
                NetworkServer.ReturnWriter(writer);
            }
        }

        /// <summary>
        /// Broadcast状態を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void BroadcastState()
        {
            NetDataWriter writer = NetworkServer.RentWriter();
            try
            {
                new AdminRequest().Serialize(writer, AdminRequestMode.GlobalGetHeadlessAudioState);
                writer.Put(HeadlessAudioOff);
                NetworkServer.BroadcastMessageToClients(
                    writer,
                    BasisNetworkCommons.AdminChannel,
                    NetworkServer.PeerSnapshot,
                    DeliveryMethod.ReliableOrdered);
            }
            finally
            {
                NetworkServer.ReturnWriter(writer);
            }
        }
    }
}
