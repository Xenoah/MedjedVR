using System;
using System.Collections.Generic;
using System.Text;

namespace BasisNetworkCore.Pooling
{
    /// <summary>
    /// BasisByteArrayPoolingの責務をまとめるクラスです。
    /// Pooling領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static class BasisByteArrayPooling
    {
        private static readonly Dictionary<int, Queue<byte[]>> _pool = new();
        private static readonly object _lock = new();

        /// <summary>
        /// Rentを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static byte[] Rent(int size)
        {
            lock (_lock)
            {
                if (_pool.TryGetValue(size, out Queue<byte[]> queue) && queue.Count > 0)
                {
                    return queue.Dequeue();
                }

                // 利用できなければ新しい配列を作成する
                return new byte[size];
            }
        }

        /// <summary>
        /// Returnを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void Return(byte[] array)
        {
            if (array == null) return;

            lock (_lock)
            {
                if (!_pool.TryGetValue(array.Length, out Queue<byte[]> queue))
                {
                    queue = new Queue<byte[]>();
                    _pool[array.Length] = queue;
                }

                queue.Enqueue(array);
            }
        }

        // 任意: pool 済み配列をすべて clear する
        public static void Clear()
        {
            lock (_lock)
            {
                _pool.Clear();
            }
        }
    }
}
