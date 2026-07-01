namespace Basis.Scripts.Networking.Compression
{
    /// <summary>
    /// MathExtensionsの責務をまとめるクラスです。
    /// Mathematics領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static class MathExtensions
    {
        /// <summary>
        /// Clampを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// Clampを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// Clampを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
    /// <summary>
    /// Vector3の責務をまとめる構造体です。
    /// Mathematics領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public struct Vector3
    {
        /// <summary>
        /// xを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float x;
        /// <summary>
        /// yを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float y;
        /// <summary>
        /// zを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float z;
        /// <summary>
        /// Vector3を生成し、利用に必要な初期状態を設定します。
        /// </summary>
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        // 利便性のための減算 operator
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        // 二乗 magnitude (vector の長さの二乗)
        public float SquaredMagnitude()
        {
            return x * x + y * y + z * z;
        }

    }
    /// <summary>
    /// Vector4の責務をまとめる構造体です。
    /// Mathematics領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public struct Vector4
    {
        /// <summary>
        /// xを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float x;
        /// <summary>
        /// yを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float y;
        /// <summary>
        /// zを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float z;
        /// <summary>
        /// wを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float w;
    }
    /// <summary>
    /// Quaternionの責務をまとめる構造体です。
    /// Mathematics領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public struct Quaternion
    {
        /// <summary>
        /// valueを保持します。型は Vector4 で、関連処理から共有される値です。
        /// </summary>
        public Vector4 value;
        /// <summary>
        /// Quaternionを生成し、利用に必要な初期状態を設定します。
        /// </summary>
        public Quaternion(float x, float y, float z, float w) : this()
        {
            value.x = x;
            value.y = y;
            value.z = z;
            value.w = w;
        }
    }
    /// <summary>
    /// float3の責務をまとめる構造体です。
    /// Mathematics領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public struct float3
    {
        /// <summary>
        /// xを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float x;
        /// <summary>
        /// yを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float y;
        /// <summary>
        /// zを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float z;
    }
}
