using Basis.Scripts.Networking.Compression;

using System;
/// <summary>Functions to Compress Quaternions and Floats</summary>
public static class BasisNetworkPrimitiveCompression
{
    [Serializable]
    /// <summary>
    /// BasisRangedUshortFloatDataの責務をまとめるクラスです。
    /// Compression領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public class BasisRangedUshortFloatData
    {
        /// <summary>
        /// Precisionを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float Precision;
        /// <summary>
        /// InversePrecisionを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float InversePrecision;
        /// <summary>
        /// MinValueを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float MinValue;
        /// <summary>
        /// MaxValueを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float MaxValue;
        /// <summary>
        /// RequiredBitsを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public int RequiredBits;
        /// <summary>
        /// Maskを保持します。型は ushort で、関連処理から共有される値です。
        /// </summary>
        public ushort Mask;
        /// <summary>
        /// BasisRangedUshortFloatDataを生成し、利用に必要な初期状態を設定します。
        /// </summary>
        public BasisRangedUshortFloatData(float minValue, float maxValue, float precision)
        {
            MinValue = minValue;
            MaxValue = maxValue;
            Precision = precision;
            InversePrecision = 1.0f / precision;
            RequiredBits = CalculateRequiredBits();
            Mask = (ushort)((1 << RequiredBits) - 1);
        }
        /// <summary>
        /// Compressを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public ushort Compress(float value)
        {
            value = MathExtensions.Clamp(value, MinValue, MaxValue);
            float normalizedValue = (value - MinValue) * InversePrecision;
            return (ushort)((ushort)(normalizedValue + 0.5f) & Mask);
        }
        /// <summary>
        /// Decompressを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public float Decompress(ushort compressedValue)
        {
            float decompressedValue = ((float)compressedValue * Precision) + MinValue;
            return MathExtensions.Clamp(decompressedValue, MinValue, MaxValue);
        }
        /// <summary>
        /// CalculateRequiredBitsを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private int CalculateRequiredBits()
        {
            float range = MaxValue - MinValue;
            float maxValueInRange = range * InversePrecision;
            return FastLog2((uint)(maxValueInRange + 0.5f)) + 1;
        }
        /// <summary>
        /// FastLog2を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static int FastLog2(uint value)
        {
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return deBruijnLookup[(value * 0x07C4ACDDU) >> 27];
        }

        private static readonly int[] deBruijnLookup = new int[32]
        {
            0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30,
            8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31
        };
    }
}
