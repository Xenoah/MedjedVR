using Basis.Network.Core;
using Basis.Network.Core.Compression;
using System;
using static Basis.Network.Core.Compression.BasisAvatarBitPacking;

/// <summary>
/// SerializableBasisの責務をまとめるクラスです。
/// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public static partial class SerializableBasis
{
    /// <summary>
    /// Localアバター同期メッセージの責務をまとめる構造体です。
    /// Serializable領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public struct LocalAvatarSyncMessage
    {
        // on-wire contract:
        // client->server (channel 2):  [DataQualityLevel:1][PayloadBytes:FixedByQuality][AdditionalSize:1][LinkedAvatarIndex?][Additional...]
        // server->client (even ch):    [PayloadBytes:FixedByQuality]
        // server->client (odd ch):     [PayloadBytes:FixedByQuality][AdditionalSize:1][LinkedAvatarIndex:1][Additional...]
        //   quality と additional-data の有無は channel number から derive する。
        //
        // payload layout (current order):
        // position (12) -> bone rotations (bitstream、quality により変化) -> Posit16 scale (2) -> rotation (7) -> hips tail

        public byte DataQualityLevel; // 0=Low, 1=Medium, 2=High
        /// <summary>
        /// arrayを保持します。型は byte[] で、関連処理から共有される値です。
        /// </summary>
        public byte[] array;          // payload bytes (length must match ConvertToSize(quality))

        /// <summary>
        /// AdditionalアバターDatasを保持します。型は AdditionalAvatarData[] で、関連処理から共有される値です。
        /// </summary>
        public AdditionalAvatarData[] AdditionalAvatarDatas;
        /// <summary>
        /// AdditionalアバターDataSizeを保持します。型は byte で、関連処理から共有される値です。
        /// </summary>
        public byte AdditionalAvatarDataSize;
        /// <summary>
        /// LinkedアバターIndexを保持します。型は byte で、関連処理から共有される値です。
        /// </summary>
        public byte LinkedAvatarIndex;

        /// <summary>
        /// Localアバター同期メッセージを生成し、利用に必要な初期状態を設定します。
        /// </summary>
        public LocalAvatarSyncMessage(byte[] array) : this()
        {
            this.array = array;
        }

        /// <summary>
        /// TryGetExpectedPayloadLengthを試行し、失敗時に呼び出し元が分岐できる結果を返します。
        /// </summary>
        private static bool TryGetExpectedPayloadLength(byte dataQualityLevel, out ushort expected)
        {
            expected = 0;

            var q = (BitQuality)dataQualityLevel;
            if (!BasisAvatarBitPacking.IsValidQuality(q))
                return false;

            expected = (ushort)BasisAvatarBitPacking.ConvertToSize(q);
            return expected != 0;
        }

        /// <summary>
        /// DataQualityLevel が payload 内にある場合に deserialize する (client->server path)。
        /// </summary>
        public void Deserialize(NetDataReader reader)
        {
            if (!reader.TryGetByte(out DataQualityLevel))
            {
                BNL.LogError("Missing DataQualityLevel!");
                return;
            }

            DeserializePayload(reader);
        }

        /// <summary>
        /// quality と additional-data の有無が channel から derive される場合に deserialize する (server->client path)。
        /// even channel は additional data section をまったく持たない。odd channel は additional data を持つ。
        /// </summary>
        public void Deserialize(NetDataReader reader, byte channelDerivedQuality, bool hasAdditionalData)
        {
            DataQualityLevel = channelDerivedQuality;

            if (!TryGetExpectedPayloadLength(DataQualityLevel, out ushort expected))
            {
                BNL.LogError($"Invalid DataQualityLevel={DataQualityLevel}");
                return;
            }

            if (reader.AvailableBytes < expected)
            {
                BNL.LogError($"Unable to read avatar payload. Need {expected}, have {reader.AvailableBytes}.");
                return;
            }

            if (array == null || array.Length != expected)
            {
                array = new byte[expected];
            }

            reader.GetBytes(array, expected);

            if (!hasAdditionalData)
            {
                AdditionalAvatarDataSize = 0;
                AdditionalAvatarDatas = null;
                return;
            }

            DeserializeAdditionalData(reader);
        }

        /// <summary>
        /// DeserializePayloadを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private void DeserializePayload(NetDataReader reader)
        {
            if (!TryGetExpectedPayloadLength(DataQualityLevel, out ushort expected))
            {
                BNL.LogError($"Invalid DataQualityLevel={DataQualityLevel}");
                return;
            }

            if (reader.AvailableBytes < expected)
            {
                BNL.LogError($"Unable to read avatar payload. Need {expected}, have {reader.AvailableBytes}.");
                return;
            }

            if (array == null || array.Length != expected)
            {
                array = new byte[expected];
            }

            reader.GetBytes(array, expected);

            if (!reader.TryGetByte(out AdditionalAvatarDataSize))
            {
                BNL.LogError("Missing AdditionalAvatarDataSize!");
                return;
            }

            if (AdditionalAvatarDataSize == 0)
            {
                AdditionalAvatarDatas = null;
                return;
            }

            DeserializeAdditionalData(reader);
        }

        /// <summary>
        /// DeserializeAdditionalDataを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private void DeserializeAdditionalData(NetDataReader reader)
        {
            if (!reader.TryGetByte(out AdditionalAvatarDataSize))
            {
                BNL.LogError("Missing AdditionalAvatarDataSize!");
                return;
            }

            if (!reader.TryGetByte(out LinkedAvatarIndex))
            {
                BNL.LogError("Missing LinkedAvatarIndex!");
                return;
            }

            if (AdditionalAvatarDatas == null || AdditionalAvatarDatas.Length != AdditionalAvatarDataSize)
            {
                AdditionalAvatarDatas = new AdditionalAvatarData[AdditionalAvatarDataSize];
            }
            for (int i = 0; i < AdditionalAvatarDataSize; i++)
            {
                AdditionalAvatarDatas[i] = new AdditionalAvatarData();
                AdditionalAvatarDatas[i].Deserialize(reader);
            }
        }

        /// <summary>
        /// DataQualityLevel を payload 内に含めて serialize する (initial player creation、non-quality channel)。
        /// </summary>
        public void Serialize(NetDataWriter writer, BitQuality Quality)
        {
            DataQualityLevel = (byte)Quality;
            if (!TryGetExpectedPayloadLength(DataQualityLevel, out ushort expected))
            {
                BNL.LogError($"Serialize invalid quality={Quality} (DataQualityLevel={DataQualityLevel})");
                writer.Put(DataQualityLevel);
                writer.Put((byte)0);
                return;
            }

            writer.Put(DataQualityLevel);

            if (array == null)
            {
                BNL.LogError("array was null!!");
                writer.Put((byte)0);
                return;
            }

            if (array.Length != expected)
            {
                array = new byte[expected];
            }

            writer.Put(array, 0, expected);

            if (AdditionalAvatarDatas == null || AdditionalAvatarDatas.Length == 0 || AdditionalAvatarDatas.Length > 255)
            {
                writer.Put((byte)0);
                return;
            }

            AdditionalAvatarDataSize = (byte)AdditionalAvatarDatas.Length;
            writer.Put(AdditionalAvatarDataSize);
            writer.Put(LinkedAvatarIndex);

            for (int i = 0; i < AdditionalAvatarDataSize; i++)
            {
                AdditionalAvatarDatas[i].Serialize(writer);
            }
        }

        /// <summary>
        /// channel-based path (quality channel) 用に serialize する。
        /// quality と additional-data の有無は channel に encode され、payload には書かれない。
        /// </summary>
        public void SerializeForChannel(NetDataWriter writer, BitQuality Quality)
        {
            DataQualityLevel = (byte)Quality;
            if (!TryGetExpectedPayloadLength(DataQualityLevel, out ushort expected))
            {
                BNL.LogError($"SerializeForChannel invalid quality={Quality}");
                return;
            }

            if (array == null)
            {
                BNL.LogError("array was null!!");
                return;
            }

            if (array.Length != expected)
            {
                array = new byte[expected];
            }

            writer.Put(array, 0, expected);

            // additional data は存在する場合だけ書く。receiver には channel が知らせる。
            if (AdditionalAvatarDatas != null && AdditionalAvatarDatas.Length > 0 && AdditionalAvatarDatas.Length <= 255)
            {
                AdditionalAvatarDataSize = (byte)AdditionalAvatarDatas.Length;
                writer.Put(AdditionalAvatarDataSize);
                writer.Put(LinkedAvatarIndex);

                for (int i = 0; i < AdditionalAvatarDataSize; i++)
                {
                    AdditionalAvatarDatas[i].Serialize(writer);
                }
            }
        }
    }
}
