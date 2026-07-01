using System;
using System.Collections.Generic;
namespace Basis.Network.Core.Compression
{
    // 実行中の割り当てを避けるための byte 配列プール
    public class BasisObjectPool<T>
    {
        private readonly Func<T> createFunc;
        private readonly Stack<T> pool;
        private readonly object lockObj = new object(); // Lock object for thread safety

        /// <summary>
        /// BasisObjectPoolを生成し、利用に必要な初期状態を設定します。
        /// </summary>
        public BasisObjectPool(Func<T> createFunc)
        {
            this.createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            pool = new Stack<T>();
        }

        /// <summary>
        /// Getを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public T Get()
        {
            lock (lockObj)
            {
                return pool.Count > 0 ? pool.Pop() : createFunc();
            }
        }

        /// <summary>
        /// Returnを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public void Return(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            lock (lockObj)
            {
                pool.Push(item);
            }
        }
    }
}
