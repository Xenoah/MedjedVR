using System;

namespace BasisNetworkCore.Security
{
    [Serializable]
    /// <summary>
    /// BasisUserRestrictionModeの責務をまとめる列挙型です。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public enum BasisUserRestrictionMode : byte
    {
        Normal,
        BanList,
        AllowList,
        RejoinOnly,
    }
}
