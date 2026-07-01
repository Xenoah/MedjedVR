using Basis.Network.Core;

/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    /// <summary>
    /// content sphere 経由で share できる content type。
    /// </summary>
    public enum ContentShareType : byte
    {
        Avatar = 0,
        Prop = 1,
        World = 2,
        /// <summary>
        /// saved-server entry。ContentURL は connection string
        /// (address[:port][#password]) を運ぶ。この type では UnlockPassword は未使用。
        /// receiver は in-world orb を spawn せず、server を saved list に追加するか確認する dialog を受け取る。
        /// </summary>
        Server = 3,
    }

    /// <summary>
    /// client が content share sphere を world に drop するために送る。
    /// 他 player が content を load するために必要なものをすべて含む。
    /// </summary>
    public struct ContentShareMessage
    {
        /// <summary>
        /// この sphere instance の unique ID (GUID string)。
        /// </summary>
        public string SphereNetID;

        /// <summary>
        /// content bundle への URL。
        /// </summary>
        public string ContentURL;

        /// <summary>
        /// content bundle を unlock/decrypt するための password。
        /// </summary>
        public string UnlockPassword;

        /// <summary>
        /// この sphere が表す content の種類。
        /// </summary>
        public ContentShareType ContentType;

        /// <summary>
        /// sphere が drop された world position。
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
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            SphereNetID = reader.GetString();
            ContentURL = reader.GetString();
            UnlockPassword = reader.GetString();
            ContentType = (ContentShareType)reader.GetByte();
            PositionX = reader.GetFloat();
            PositionY = reader.GetFloat();
            PositionZ = reader.GetFloat();
        }

        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(SphereNetID);
            writer.Put(ContentURL);
            writer.Put(UnlockPassword);
            writer.Put((byte)ContentType);
            writer.Put(PositionX);
            writer.Put(PositionY);
            writer.Put(PositionZ);
        }
    }

    /// <summary>
    /// server は client の ContentShareMessage を sender の player ID と、
    /// saved state から解決した authoritative identity (UUID + display name) で包む。
    /// </summary>
    public struct ServerContentShareMessage
    {
        /// <summary>
        /// playerIdメッセージを保持します。型は PlayerIdMessage で、関連処理から共有される値です。
        /// </summary>
        public PlayerIdMessage playerIdMessage;
        /// <summary>
        /// SharerUUIDを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string SharerUUID;
        /// <summary>
        /// SharerDisplayNameを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string SharerDisplayName;
        /// <summary>
        /// contentShareメッセージを保持します。型は ContentShareMessage で、関連処理から共有される値です。
        /// </summary>
        public ContentShareMessage contentShareMessage;

        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            playerIdMessage.Deserialize(reader);
            SharerUUID = reader.GetString();
            SharerDisplayName = reader.GetString();
            contentShareMessage.Deserialize(reader);
        }

        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            playerIdMessage.Serialize(writer);
            writer.Put(SharerUUID ?? string.Empty);
            writer.Put(SharerDisplayName ?? string.Empty);
            contentShareMessage.Serialize(writer);
        }
    }

    /// <summary>
    /// content share sphere を world から削除するために送る。
    /// </summary>
    public struct ContentShareCleanupMessage
    {
        /// <summary>
        /// SphereNetIDを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string SphereNetID;

        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            SphereNetID = reader.GetString();
        }

        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(SphereNetID);
        }
    }

    /// <summary>
    /// server は cleanup を sender player ID で包む。
    /// </summary>
    public struct ServerContentShareCleanupMessage
    {
        /// <summary>
        /// playerIdメッセージを保持します。型は PlayerIdMessage で、関連処理から共有される値です。
        /// </summary>
        public PlayerIdMessage playerIdMessage;
        /// <summary>
        /// contentShareCleanupメッセージを保持します。型は ContentShareCleanupMessage で、関連処理から共有される値です。
        /// </summary>
        public ContentShareCleanupMessage contentShareCleanupMessage;

        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            playerIdMessage.Deserialize(reader);
            contentShareCleanupMessage.Deserialize(reader);
        }

        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            playerIdMessage.Serialize(writer);
            contentShareCleanupMessage.Serialize(writer);
        }
    }
}
