using Basis.Scripts.Networking.Compression;

namespace BasisNetworkClientConsole
{
    /// <summary>
    /// Randomizerの責務をまとめるクラスです。
    /// ClientConsole領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public class Randomizer
    {
        /// <summary>
        /// GetRandomOffsetを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public static Vector3 GetRandomOffset()
        {
            return new Vector3(
                (float)(Random.Shared.NextDouble() * 2 - 1) / 4f,
                (float)(Random.Shared.NextDouble() * 2 - 1) / 4f,
                (float)(Random.Shared.NextDouble() * 2 - 1) / 4f
            );
        }
    }
}
