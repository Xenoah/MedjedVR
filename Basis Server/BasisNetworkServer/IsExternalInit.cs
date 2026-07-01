#if NETSTANDARD2_0 || NETSTANDARD2_1
using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    /// <summary>
    /// IsExternalInitの責務をまとめるクラスです。
    /// Server領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    internal static class IsExternalInit { }
}
#endif
