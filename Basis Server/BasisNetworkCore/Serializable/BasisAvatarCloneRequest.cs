using Basis.Network.Core;

/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    /// <summary>
    /// BasisアバターCloneRequestの責務をまとめる構造体です。
    /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public struct BasisAvatarCloneRequest
    {
        /// <summary>
        /// requestingUserを保持します。型は ushort で、関連処理から共有される値です。
        /// </summary>
        public ushort requestingUser;
        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader NetDataReader)
        {
            requestingUser = NetDataReader.GetUShort();
        }
        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter NetDataWriter)
        {
            NetDataWriter.Put(requestingUser);
        }
    }
    /// <summary>
    /// BasisアバターCloneResponseの責務をまとめる構造体です。
    /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public struct BasisAvatarCloneResponse
    {
        /// <summary>
        /// requestingUserを保持します。型は ushort で、関連処理から共有される値です。
        /// </summary>
        public ushort requestingUser;
        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader NetDataReader)
        {
            requestingUser = NetDataReader.GetUShort();
        }
        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter NetDataWriter)
        {
            NetDataWriter.Put(requestingUser);
        }
    }
}
