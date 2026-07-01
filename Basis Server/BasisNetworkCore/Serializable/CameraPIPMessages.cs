using Basis.Network.Core;

/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    /// <summary>
    /// player の PIP camera が作成/破棄されたとき reliable に送る。
    /// server はこれを保存し、late joiner へ replay する。
    /// </summary>
    public struct CameraPIPStateMessage
    {
        /// <summary>
        /// プレイヤーIDを保持します。型は ushort で、関連処理から共有される値です。
        /// </summary>
        public ushort PlayerID;
        /// <summary>
        /// IsActiveを保持します。型は bool で、関連処理から共有される値です。
        /// </summary>
        public bool IsActive;
        // position と rotation は IsActive == true (initial spawn) の場合だけ送る。
        public float PositionX;
        /// <summary>
        /// PositionYを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float PositionY;
        /// <summary>
        /// PositionZを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float PositionZ;
        /// <summary>
        /// RotationXを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float RotationX;
        /// <summary>
        /// RotationYを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float RotationY;
        /// <summary>
        /// RotationZを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float RotationZ;
        /// <summary>
        /// RotationWを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float RotationW;

        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerID);
            writer.Put(IsActive);
            if (IsActive)
            {
                writer.Put(PositionX);
                writer.Put(PositionY);
                writer.Put(PositionZ);
                writer.Put(RotationX);
                writer.Put(RotationY);
                writer.Put(RotationZ);
                writer.Put(RotationW);
            }
        }

        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            PlayerID = reader.GetUShort();
            IsActive = reader.GetBool();
            if (IsActive)
            {
                PositionX = reader.GetFloat();
                PositionY = reader.GetFloat();
                PositionZ = reader.GetFloat();
                RotationX = reader.GetFloat();
                RotationY = reader.GetFloat();
                RotationZ = reader.GetFloat();
                RotationW = reader.GetFloat();
            }
        }
    }

    /// <summary>
    /// PIP camera 用の position / rotation update。
    /// </summary>
    public struct CameraPIPPositionMessage
    {
        /// <summary>
        /// プレイヤーIDを保持します。型は ushort で、関連処理から共有される値です。
        /// </summary>
        public ushort PlayerID;
        /// <summary>
        /// PositionXを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float PositionX;
        /// <summary>
        /// PositionYを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float PositionY;
        /// <summary>
        /// PositionZを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float PositionZ;
        /// <summary>
        /// RotationXを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float RotationX;
        /// <summary>
        /// RotationYを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float RotationY;
        /// <summary>
        /// RotationZを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float RotationZ;
        /// <summary>
        /// RotationWを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float RotationW;

        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerID);
            writer.Put(PositionX);
            writer.Put(PositionY);
            writer.Put(PositionZ);
            writer.Put(RotationX);
            writer.Put(RotationY);
            writer.Put(RotationZ);
            writer.Put(RotationW);
        }

        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            PlayerID = reader.GetUShort();
            PositionX = reader.GetFloat();
            PositionY = reader.GetFloat();
            PositionZ = reader.GetFloat();
            RotationX = reader.GetFloat();
            RotationY = reader.GetFloat();
            RotationZ = reader.GetFloat();
            RotationW = reader.GetFloat();
        }
    }

    /// <summary>
    /// client -> server: camera state change (PlayerID なし。server が peer から埋める)。
    /// </summary>
    public struct ClientCameraPIPStateMessage
    {
        /// <summary>
        /// IsActiveを保持します。型は bool で、関連処理から共有される値です。
        /// </summary>
        public bool IsActive;
        /// <summary>
        /// PositionXを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float PositionX;
        /// <summary>
        /// PositionYを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float PositionY;
        /// <summary>
        /// PositionZを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float PositionZ;
        /// <summary>
        /// RotationXを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float RotationX;
        /// <summary>
        /// RotationYを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float RotationY;
        /// <summary>
        /// RotationZを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float RotationZ;
        /// <summary>
        /// RotationWを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float RotationW;

        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(IsActive);
            if (IsActive)
            {
                writer.Put(PositionX);
                writer.Put(PositionY);
                writer.Put(PositionZ);
                writer.Put(RotationX);
                writer.Put(RotationY);
                writer.Put(RotationZ);
                writer.Put(RotationW);
            }
        }

        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            IsActive = reader.GetBool();
            if (IsActive)
            {
                PositionX = reader.GetFloat();
                PositionY = reader.GetFloat();
                PositionZ = reader.GetFloat();
                RotationX = reader.GetFloat();
                RotationY = reader.GetFloat();
                RotationZ = reader.GetFloat();
                RotationW = reader.GetFloat();
            }
        }
    }

    /// <summary>
    /// client -> server: position / rotation update (PlayerID なし。server が peer から埋める)。
    /// </summary>
    public struct ClientCameraPIPPositionMessage
    {
        /// <summary>
        /// PositionXを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float PositionX;
        /// <summary>
        /// PositionYを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float PositionY;
        /// <summary>
        /// PositionZを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float PositionZ;
        /// <summary>
        /// RotationXを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float RotationX;
        /// <summary>
        /// RotationYを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float RotationY;
        /// <summary>
        /// RotationZを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float RotationZ;
        /// <summary>
        /// RotationWを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float RotationW;

        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PositionX);
            writer.Put(PositionY);
            writer.Put(PositionZ);
            writer.Put(RotationX);
            writer.Put(RotationY);
            writer.Put(RotationZ);
            writer.Put(RotationW);
        }

        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            PositionX = reader.GetFloat();
            PositionY = reader.GetFloat();
            PositionZ = reader.GetFloat();
            RotationX = reader.GetFloat();
            RotationY = reader.GetFloat();
            RotationZ = reader.GetFloat();
            RotationW = reader.GetFloat();
        }
    }
}
