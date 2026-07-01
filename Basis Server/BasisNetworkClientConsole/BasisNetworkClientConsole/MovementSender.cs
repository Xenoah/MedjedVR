using System.Diagnostics;
using Basis.Network.Core;
using Basis.Network.Core.Compression;
using Basis.Scripts.Networking.Compression;
using BasisNetworkClientConsole;
using static Basis.Network.Core.Compression.BasisAvatarBitPacking;
using static SerializableBasis;

namespace Basis.Network
{
    /// <summary>
    /// MovementSenderの責務をまとめるクラスです。
    /// ClientConsole領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static class MovementSender
    {
        /// <summary>
        /// Rotationを保持します。型は Quaternion で、関連処理から共有される値です。
        /// </summary>
        public static Quaternion Rotation = new Quaternion(0, 0, 0, 1);

        /// <summary>
        /// PlayersCurrentPositionを保持します。型は Vector3[] で、関連処理から共有される値です。
        /// </summary>
        public static Vector3[] PlayersCurrentPosition;
        /// <summary>
        /// ActiveプレイヤーDataを保持します。型は PlayerData[] で、関連処理から共有される値です。
        /// </summary>
        public static PlayerData[] ActivePlayerData;

        // animation timer は全 player で共有し、player ごとの phase offset で揺らぎを付ける。
        private static readonly Stopwatch AnimTimer = Stopwatch.StartNew();

        // High quality packet 内の byte offset を事前計算する。
        private static readonly int RotationRegionOffset = BasisAvatarBitPacking.WritePosition; // 12
        private static readonly int ScaleOffset = BasisAvatarBitPacking.WritePosition
            + BasisBoneRotationCompression.RotationBytes(BitQuality.High);
        // flip 後は、ここが HIPS WORLD rotation slot になる。
        // 以前の "body rotation" (= root world rotation)。7-byte smallest-three quaternion。
        private static readonly int HipsRotationOffset = ScaleOffset + BasisAvatarBitPacking.WriteScale;
        // 6 bytes。±1m の signed short が 3 つ。signed encoding のおかげで default の zero byte は
        // すでに zero delta に decode されるため、fake client 用に人工的な値を書かなくてよい。
        private static readonly int HipsLocalDeltaOffset = HipsRotationOffset + BasisAvatarBitPacking.WriteRotation;
        // hips local-rotation delta 用の 7-byte smallest-three quaternion。
        // default の zero byte は identity に decode されない
        // (encoding では saturated-low drop-X quat として扱われる) ため、
        // test client は init 時に explicit identity を一度書く。
        private static readonly int HipsLocalRotationOffset = HipsLocalDeltaOffset + BasisAvatarBitPacking.WriteHipsDelta;

        /// <summary>
        /// プレイヤーDataの責務をまとめる構造体です。
        /// ClientConsole領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
        /// </summary>
        public struct PlayerData
        {
            /// <summary>
            /// Writerを保持します。型は NetDataWriter で、関連処理から共有される値です。
            /// </summary>
            public NetDataWriter Writer;
            /// <summary>
            /// メッセージを保持します。型は LocalAvatarSyncMessage で、関連処理から共有される値です。
            /// </summary>
            public LocalAvatarSyncMessage Message;
            /// <summary>
            /// SequenceByteを保持します。型は byte で、関連処理から共有される値です。
            /// </summary>
            public byte SequenceByte;
            /// <summary>
            /// PhaseOffsetを保持します。型は float で、関連処理から共有される値です。
            /// </summary>
            public float PhaseOffset;
        }

        // compressed scale は一度だけ事前計算し、すべての message で再利用する。
        private static readonly ushort CompressedScale = CompressScaleOnce(1f);

        /// <summary>
        /// Initializeを初期化します。設定、永続化ファイル、実行時キャッシュを起動時の状態へ整えます。
        /// </summary>
        public static void Initialize(int clientCount)
        {
            PlayersCurrentPosition = new Vector3[clientCount];
            ActivePlayerData = new PlayerData[clientCount];

            for (int i = 0; i < clientCount; i++)
            {
                PlayersCurrentPosition[i] = Randomizer.GetRandomOffset();
                ActivePlayerData[i] = Generate();
            }
        }
        /// <summary>
        /// Generateを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static PlayerData Generate()
        {
            var message = new LocalAvatarSyncMessage
            {
                DataQualityLevel = (byte)BitQuality.High,
                AdditionalAvatarDatas = null,
                AdditionalAvatarDataSize = 0,
                LinkedAvatarIndex = 0,
                array = new byte[ClientManager.Size],
            };

            // idle animation が同期しないよう、player ごとの random phase offset を使う。
            float phase = (float)(Random.Shared.NextDouble() * MathF.PI * 2f);

            // full initial payload (position、bone rotations、scale、hips rotation) を組み立てる。
            WriteInitialPayload(ref message, phase);

            return new PlayerData
            {
                Writer = new NetDataWriter(),
                Message = message,
                PhaseOffset = phase,
            };
        }

        /// <summary>
        /// WriteInitialPayloadを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private static void WriteInitialPayload(ref LocalAvatarSyncMessage message, float phase)
        {
            // buffer が High 用の正しい size であることを保証する。
            int size = BasisAvatarBitPacking.ConvertToSize(BitQuality.High);
            if (message.array == null || message.array.Length != size)
                message.array = new byte[size];

            double time = AnimTimer.Elapsed.TotalSeconds;

            // 1) position (最近の flip 後は HIPS WORLD position)
            int offset = 0;
            WritePosition(Randomizer.GetRandomOffset(), ref message.array, ref offset);

            // 2) bone rotations: idle animation 付きの natural standing pose。
            FakePoseGenerator.WriteBoneRotations(message.array, RotationRegionOffset, BitQuality.High, time, phase);

            // 3) scale。
            WriteScaleUShort(CompressedScale, message.array, ScaleOffset);

            // 4) hips world rotation: わずかな body orientation。
            FakePoseGenerator.WriteCompressedHipsRotation(message.array, HipsRotationOffset, time, phase);

            // 5) hips local-position delta は zero byte のままにする。
            //    receiver 側の signed-short decode では zero delta として扱われるため、
            //    fake client 用の人工的な書き込みは不要。

            // 6) hips local-rotation delta は explicit identity にする必要がある。
            //    all-zero byte に対する smallest-three は identity に decode されない。
            //    test client はこの channel を animate しないため、ここで一度だけ設定する。
            WriteIdentityQuaternion(message.array, HipsLocalRotationOffset);
        }

        /// <summary>
        /// identity quaternion (0,0,0,1) を 7-byte smallest-three slot へ書き込む。
        /// identity では w が最大 component (= 1) なので:
        ///   index byte = 3 (w を drop)
        ///   3 つの small component = 0 -> quantized = midpoint = 32768
        /// </summary>
        private static void WriteIdentityQuaternion(byte[] dst, int offset)
        {
            // QuantizeSmall(0f) = midpoint = 32768 = 0x8000 -> lo 0x00, hi 0x80。
            dst[offset] = 3;
            dst[offset + 1] = 0x00;
            dst[offset + 2] = 0x80;
            dst[offset + 3] = 0x00;
            dst[offset + 4] = 0x80;
            dst[offset + 5] = 0x00;
            dst[offset + 6] = 0x80;
        }
        /// <summary>
        /// WriteScaleUShortを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private static void WriteScaleUShort(ushort value, byte[] buffer, int byteOffset)
        {
            buffer[byteOffset + 0] = (byte)value;
            buffer[byteOffset + 1] = (byte)(value >> 8);
        }
        /// <summary>
        /// ProcessSingleを処理します。受信データを検証し、必要な状態更新や再配信を行います。
        /// </summary>
        public static void ProcessSingle(NetPeer peer, int index)
        {
            if (peer == null) return;

            double time = AnimTimer.Elapsed.TotalSeconds;
            float phase = ActivePlayerData[index].PhaseOffset;

            // position を更新する。
            PlayersCurrentPosition[index] += Randomizer.GetRandomOffset();

            var msg = ActivePlayerData[index].Message;

            // 1) position (先頭 12 bytes)
            int offset = 0;
            WritePosition(PlayersCurrentPosition[index], ref msg.array, ref offset);

            // 2) animated bone rotations (natural pose + idle animation)
            FakePoseGenerator.WriteBoneRotations(msg.array, RotationRegionOffset, BitQuality.High, time, phase);

            // 3) scale は変更しない。

            // 4) animated hips rotation。
            FakePoseGenerator.WriteCompressedHipsRotation(msg.array, HipsRotationOffset, time, phase);

            // serialize して送信する。channel は quality (High) と additional data なしを encode する。
            var writer = ActivePlayerData[index].Writer;
            writer.Reset();
            writer.Put(ActivePlayerData[index].SequenceByte);
            unchecked { ActivePlayerData[index].SequenceByte++; }
            msg.SerializeForChannel(writer, BitQuality.High);

            byte channel = BasisNetworkCommons.GetPlayerAvatarChannelForQuality((int)BitQuality.High, false);
            peer.Send(writer, channel, DeliveryMethod.Unreliable);

            ActivePlayerData[index].Message = msg;
        }

        /// <summary>
        /// WritePositionを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void WritePosition(Scripts.Networking.Compression.Vector3 position, ref byte[] buffer, ref int offset)
        {
            unsafe
            {
                fixed (byte* dst = &buffer[offset])
                {
                    float* f = (float*)dst;
                    f[0] = position.x;
                    f[1] = position.y;
                    f[2] = position.z;
                }
            }
            offset += 12;
        }

        /// <summary>
        /// WriteQuaternionToBytesを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public unsafe static void WriteQuaternionToBytes(Quaternion q, ref byte[] bytes, ref int offset)
        {
            fixed (byte* ptr = &bytes[offset])
            {
                *((float*)ptr) = float.IsNaN(q.value.x) ? 0f : q.value.x;
                *((float*)(ptr + 4)) = float.IsNaN(q.value.y) ? 0f : q.value.y;
                *((float*)(ptr + 8)) = float.IsNaN(q.value.z) ? 0f : q.value.z;
                *((float*)(ptr + 12)) = float.IsNaN(q.value.w) ? 1f : q.value.w;
            }

            offset += 16;
        }

        /// <summary>
        /// CompressScaleOnceを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private static ushort CompressScaleOnce(float scale)
        {
            if (scale != 1f)
            {
                throw new System.ArgumentOutOfRangeException(nameof(scale), scale, "MovementSender only supports precomputed scale 1.0.");
            }

            return 0x4000;
        }

        /// <summary>
        /// WriteUShortを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void WriteUShort(ushort value, ref byte[] bytes, ref int offset)
        {
            bytes[offset++] = (byte)value;
            bytes[offset++] = (byte)(value >> 8);
        }
    }
}
