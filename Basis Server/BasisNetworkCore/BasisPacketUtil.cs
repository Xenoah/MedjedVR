using System;
using System.Collections.Generic;
using System.Text;

namespace BasisNetworkCore
{
    /// <summary>
    /// BasisパケットUtilの責務をまとめるクラスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static class BasisPacketUtil
    {
        /// <summary>
        /// Validateパケットを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static bool ValidatePacket(byte New, byte Old)
        {
            if (IsNewer(New, Old) && New != Old)
            {
                return true;
            }
            return false;
        }
        // seq1 が seq2 より新しい場合 true を返す。
        public static bool IsNewer(byte seq1, byte seq2)
        {
            return (byte)(seq1 - seq2) < 128;
        }
    }
}
