using Basis.Network.Core;
/// <summary>
/// BasisサーバーLoggerの責務をまとめるクラスです。
/// Server領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public class BasisServerLogger : INetLogger
{
    /// <summary>
    /// WriteNetを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
    /// </summary>
    public void WriteNet(NetLogLevel level, string str, params object[] args)
    {
        switch (level)
        {
            case NetLogLevel.Warning:
                BNL.LogWarning(str);
                break;
            case NetLogLevel.Error:
                BNL.LogError(str);
                break;
                // case NetLogLevel.Trace:
                //  BNL.Log(str);
                //break;
                //  case NetLogLevel.Info:
                //   BNL.Log(str);
                //break;
        }
    }
}
