using System.Collections.Concurrent;

namespace BasisNetworkCore.Pooling
{
    /// <summary>
    /// ThreadSafeメッセージPoolの責務をまとめるクラスです。
    /// Pooling領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static class ThreadSafeMessagePool<T> where T : new()
    {
        private static readonly ConcurrentQueue<T> pool = new();
        private const int MaxPoolSize = 500;

        /// <summary>
        /// Rentを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static T Rent()
        {
            return pool.TryDequeue(out T obj) ? obj : new T();
        }

        /// <summary>
        /// Returnを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void Return(T obj)
        {
            if (pool.Count < MaxPoolSize)
            {
                pool.Enqueue(obj);
            }
        }
    }
}
