namespace Basis.Network.Core
{
    /// <summary>
    /// BasisネットワークVersionの責務をまとめるクラスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public class BasisNetworkVersion
    {
        /// <summary>
        /// サーバーVersionを保持します。型は ushort で、関連処理から共有される値です。
        /// </summary>
        public static ushort ServerVersion = 38;
    }
}
