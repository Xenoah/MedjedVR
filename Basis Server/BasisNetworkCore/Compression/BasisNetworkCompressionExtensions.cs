using Basis.Scripts.Networking.Compression;
using System.Runtime.CompilerServices;

namespace Basis.Network.Core.Compression
{
    /// <summary>
    /// BasisネットワークCompressionExtensionsの責務をまとめるクラスです。
    /// Compression領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static class BasisNetworkCompressionExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        /// WritePositionを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void WritePosition(Vector3 position, ref byte[] buffer, ref int offset)
        {
            unsafe
            {
                fixed (byte* dst = &buffer[offset])
                {
                    float* fDst = (float*)dst;
                    fDst[0] = position.x;
                    fDst[1] = position.y;
                    fDst[2] = position.z;
                }
            }

            offset += 12;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        /// ReadPositionを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static Vector3 ReadPosition(ref byte[] buffer)
        {
            Vector3 result;
            unsafe
            {
                fixed (byte* src = &buffer[0])
                {
                    float* fSrc = (float*)src;
                    result.x = fSrc[0];
                    result.y = fSrc[1];
                    result.z = fSrc[2];
                }
            }
            return result;
        }
    }
}
