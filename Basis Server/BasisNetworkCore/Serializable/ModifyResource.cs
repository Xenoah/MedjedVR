using Basis.Network.Core;
/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    /// <summary>
    /// すでに spawn 済みの resource 上の flag を変える client→server request であり、
    /// 全 client に変更を適用する server→client broadcast でもある。static-lock state を運ぶ。
    /// <see cref="Static"/> は item を全員に対して固定し、<see cref="StaticAdminLocked"/> は
    /// admin tier を示す。creator ではなく moderator のみが変更または解除できる。
    /// </summary>
    public struct ModifyResource
    {
        /// <summary>変更対象となる spawned resource の unique network id。</summary>
        public string LoadedNetID;
        /// <summary>0 = GameObject、1 = Scene。<see cref="LocalLoadResource.Mode"/> と一致する。</summary>
        public byte Mode;
        /// <summary>希望する frozen state。prop は pickup disabled + frozen、vehicle は locked out。</summary>
        public bool Static;
        /// <summary>希望する admin tier。true の場合は moderator のみ変更可能で、Static を含意する。</summary>
        public bool StaticAdminLocked;

        /// <summary>
        /// Serializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(LoadedNetID);
            writer.Put(Mode);
            writer.Put(Static);
            writer.Put(StaticAdminLocked);
        }
        /// <summary>
        /// Deserializeを行います。ネットワーク上の wire format とメモリ上の構造体を相互変換します。
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            LoadedNetID = reader.GetString();
            Mode = reader.GetByte();
            Static = reader.GetBool();
            StaticAdminLocked = reader.GetBool();
        }
    }
}
