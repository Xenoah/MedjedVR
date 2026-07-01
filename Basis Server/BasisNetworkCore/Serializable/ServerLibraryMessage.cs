using Basis.Network.Core;
/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    /// <summary>
    /// client 接続時に server が push する単一 library entry。
    /// Mode は client 側の BundledContentHolder.Mode に従う。0=Avatar、1=World、2=Prop。
    /// </summary>
    public struct ServerLibraryItem
    {
        /// <summary>
        /// Modeを保持します。型は byte で、関連処理から共有される値です。
        /// </summary>
        public byte Mode;
        /// <summary>
        /// Urlを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string Url;
        /// <summary>
        /// Passwordを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string Password;

        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Mode);
            writer.Put(Url ?? string.Empty);
            writer.Put(Password ?? string.Empty);
        }

        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            Mode = reader.GetByte();
            Url = reader.GetString();
            Password = reader.GetString();
        }
    }

    /// <summary>
    /// default-library list 全体を包む。client 接続時に BasisNetworkCommons.ServerLibraryChannel 上で
    /// client ごとに一度送られる。empty array は有効で、default がないことを意味する。
    /// </summary>
    public struct ServerLibraryMessage
    {
        /// <summary>
        /// Itemsを保持します。型は ServerLibraryItem[] で、関連処理から共有される値です。
        /// </summary>
        public ServerLibraryItem[] Items;

        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            int count = Items?.Length ?? 0;
            writer.Put((ushort)count);
            for (int i = 0; i < count; i++)
            {
                Items[i].Serialize(writer);
            }
        }

        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            ushort count = reader.GetUShort();
            Items = new ServerLibraryItem[count];
            for (int i = 0; i < count; i++)
            {
                Items[i].Deserialize(reader);
            }
        }
    }
}
