using Basis.Network.Core;

/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    /// <summary>
    /// server -> clients: player が photo を撮った。position は不要。
    /// receiver はすでに tracking している PIP camera transform を lookup する。
    /// </summary>
    public struct CameraShutterSoundMessage
    {
        /// <summary>
        /// プレイヤーIDを保持します。型は ushort で、関連処理から共有される値です。
        /// </summary>
        public ushort PlayerID;

        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerID);
        }

        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            PlayerID = reader.GetUShort();
        }
    }

    /// <summary>
    /// server -> clients: player が countdown timer を開始した。
    /// receiver は同じ tick/shutter timing を local で replay する。
    /// </summary>
    public struct CameraCountdownMessage
    {
        /// <summary>
        /// プレイヤーIDを保持します。型は ushort で、関連処理から共有される値です。
        /// </summary>
        public ushort PlayerID;
        /// <summary>
        /// Secondsを保持します。型は byte で、関連処理から共有される値です。
        /// </summary>
        public byte Seconds;

        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PlayerID);
            writer.Put(Seconds);
        }

        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            PlayerID = reader.GetUShort();
            Seconds = reader.GetByte();
        }
    }

    /// <summary>
    /// client -> server: local player が countdown timer を開始した。
    /// </summary>
    public struct ClientCameraCountdownMessage
    {
        /// <summary>
        /// Secondsを保持します。型は byte で、関連処理から共有される値です。
        /// </summary>
        public byte Seconds;

        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Seconds);
        }

        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            Seconds = reader.GetByte();
        }
    }
}
