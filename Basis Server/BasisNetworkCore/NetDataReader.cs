// SPDX-License-Identifier: MIT
// Copyright (c) 2020 Ruslan Pyrch
// This code has been copied from LiteNetLib:
//  - <https://github.com/RevenantX/LiteNetLib/blob/6a9e5e39d15642a07482b1c883220cffe5823ce6/LiteNetLib/Utils/NetDataReader.cs>

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Basis.Network.Core {
    /// <summary>
    /// NetDataReaderの責務をまとめるクラスです。
    /// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public class NetDataReader
    {
        /// <summary>
        /// dataを保持します。型は byte[] で、関連処理から共有される値です。
        /// </summary>
        protected byte[] _data;
        /// <summary>
        /// positionを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        protected int _position;
        /// <summary>
        /// dataSizeを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        protected int _dataSize;
        private int _offset;

        public byte[] RawData
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data;
        }
        public int RawDataSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dataSize;
        }
        public int UserDataOffset
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _offset;
        }
        public int UserDataSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dataSize - _offset;
        }
        public bool IsNull
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data == null;
        }
        public int Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _position;
        }
        public bool EndOfData
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _position == _dataSize;
        }
        public int AvailableBytes
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dataSize - _position;
        }

        /// <summary>
        /// SkipBytesを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public void SkipBytes(int count)
        {
            _position += count;
        }

        /// <summary>
        /// SetPositionを設定します。以後のネットワーク処理で参照される状態を更新します。
        /// </summary>
        public void SetPosition(int position)
        {
            _position = position;
        }

        // public void SetSource(NetDataWriter dataWriter)
        // {
        //     _data = dataWriter.Data;
        //     _position = 0;
        //     _offset = 0;
        //     _dataSize = dataWriter.Length;
        // }

        public void SetSource(byte[] source)
        {
            _data = source;
            _position = 0;
            _offset = 0;
            _dataSize = source.Length;
        }

        /// <summary>
        /// SetSourceを設定します。以後のネットワーク処理で参照される状態を更新します。
        /// </summary>
        public void SetSource(byte[] source, int offset, int maxSize)
        {
            _data = source;
            _position = offset;
            _offset = offset;
            _dataSize = maxSize;
        }

        internal void SetSource(LiteNetLib.Utils.NetDataReader reader) {
            SetSource(reader.RawData, reader.UserDataOffset, reader.RawDataSize);
            _position = reader.Position;
        }

        /// <summary>
        /// NetDataReaderを生成し、利用に必要な初期状態を設定します。
        /// </summary>
        public NetDataReader()
        {

        }

        // public NetDataReader(NetDataWriter writer)
        // {
        //     SetSource(writer);
        // }

        public NetDataReader(byte[] source)
        {
            SetSource(source);
        }

        /// <summary>
        /// NetDataReaderを生成し、利用に必要な初期状態を設定します。
        /// </summary>
        public NetDataReader(byte[] source, int offset, int maxSize)
        {
            SetSource(source, offset, maxSize);
        }

        internal NetDataReader(LiteNetLib.Utils.NetDataReader reader) {
            SetSource(reader);
        }

        #region GetMethods

        // public void Get<T>(out T result) where T : struct, INetSerializable
        // {
        //     result = default(T);
        //     result.Deserialize(this);
        // }

        // public void Get<T>(out T result, Func<T> constructor) where T : class, INetSerializable
        // {
        //     result = constructor();
        //     result.Deserialize(this);
        // }

        // public void Get(out IPEndPoint result)
        // {
        //     result = GetNetEndPoint();
        // }

        public void Get(out byte result)
        {
            result = GetByte();
        }

        /// <summary>
        /// Getを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public void Get(out sbyte result)
        {
            result = (sbyte)GetByte();
        }

        /// <summary>
        /// Getを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public void Get(out bool result)
        {
            result = GetBool();
        }

        /// <summary>
        /// Getを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public void Get(out char result)
        {
            result = GetChar();
        }

        /// <summary>
        /// Getを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public void Get(out ushort result)
        {
            result = GetUShort();
        }

        /// <summary>
        /// Getを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public void Get(out short result)
        {
            result = GetShort();
        }

        /// <summary>
        /// Getを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public void Get(out ulong result)
        {
            result = GetULong();
        }

        /// <summary>
        /// Getを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public void Get(out long result)
        {
            result = GetLong();
        }

        /// <summary>
        /// Getを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public void Get(out uint result)
        {
            result = GetUInt();
        }

        /// <summary>
        /// Getを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public void Get(out int result)
        {
            result = GetInt();
        }

        /// <summary>
        /// Getを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public void Get(out double result)
        {
            result = GetDouble();
        }

        /// <summary>
        /// Getを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public void Get(out float result)
        {
            result = GetFloat();
        }

        /// <summary>
        /// Getを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public void Get(out string result)
        {
            result = GetString();
        }

        /// <summary>
        /// Getを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public void Get(out string result, int maxLength)
        {
            result = GetString(maxLength);
        }
        
        /// <summary>
        /// Getを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public void Get(out Guid result)
        {
            result = GetGuid();
        }

        // public IPEndPoint GetNetEndPoint()
        // {
        //     string host = GetString(1000);
        //     int port = GetInt();
        //     return NetUtils.MakeEndPoint(host, port);
        // }

        public byte GetByte()
        {
            byte res = _data[_position];
            _position++;
            return res;
        }

        /// <summary>
        /// GetSByteを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public sbyte GetSByte()
        {
            return (sbyte)GetByte();
        }

        public T[] GetArray<T>(ushort size)
        {
            ushort length = BinaryPrimitives.ReadUInt16LittleEndian(_data.AsSpan(_position));
            _position += 2;
            int byteCount = length * size;
            if (byteCount > _dataSize - _position)
                throw new ArgumentException($"Array length {byteCount} exceeds available data ({_dataSize - _position} bytes).");
            T[] result = new T[length];
            Buffer.BlockCopy(_data, _position, result, 0, byteCount);
            _position += byteCount;
            return result;
        }

        // public T[] GetArray<T>() where T : INetSerializable, new()
        // {
        //     ushort length = BinaryPrimitives.ReadUInt16LittleEndian(_data.AsSpan(_position));
        //     _position += 2;
        //     T[] result = new T[length];
        //     for (int i = 0; i < length; i++)
        //     {
        //         var item = new T();
        //         item.Deserialize(this);
        //         result[i] = item;
        //     }
        //     return result;
        // }
        
        // public T[] GetArray<T>(Func<T> constructor) where T : class, INetSerializable
        // {
        //     ushort length = BinaryPrimitives.ReadUInt16LittleEndian(_data.AsSpan(_position));
        //     _position += 2;
        //     T[] result = new T[length];
        //     for (int i = 0; i < length; i++)
        //         Get(out result[i], constructor);
        //     return result;
        // }
        
        public bool[] GetBoolArray()
        {
            return GetArray<bool>(1);
        }

        /// <summary>
        /// GetUShortArrayを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public ushort[] GetUShortArray()
        {
            return GetArray<ushort>(2);
        }

        /// <summary>
        /// GetShortArrayを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public short[] GetShortArray()
        {
            return GetArray<short>(2);
        }

        /// <summary>
        /// GetIntArrayを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public int[] GetIntArray()
        {
            return GetArray<int>(4);
        }

        /// <summary>
        /// GetUIntArrayを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public uint[] GetUIntArray()
        {
            return GetArray<uint>(4);
        }

        /// <summary>
        /// GetFloatArrayを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public float[] GetFloatArray()
        {
            return GetArray<float>(4);
        }

        /// <summary>
        /// GetDoubleArrayを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public double[] GetDoubleArray()
        {
            return GetArray<double>(8);
        }

        /// <summary>
        /// GetLongArrayを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public long[] GetLongArray()
        {
            return GetArray<long>(8);
        }

        /// <summary>
        /// GetULongArrayを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public ulong[] GetULongArray()
        {
            return GetArray<ulong>(8);
        }

        /// <summary>
        /// GetStringArrayを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public string[] GetStringArray()
        {
            ushort length = GetUShort();
            string[] arr = new string[length];
            for (int i = 0; i < length; i++)
            {
                arr[i] = GetString();
            }
            return arr;
        }

        /// <summary>
        /// Note that "maxStringLength" only limits the number of characters in a string, not its size in bytes.
        /// Strings that exceed this parameter are returned as empty
        /// </summary>
        public string[] GetStringArray(int maxStringLength)
        {
            ushort length = GetUShort();
            string[] arr = new string[length];
            for (int i = 0; i < length; i++)
            {
                arr[i] = GetString(maxStringLength);
            }
            return arr;
        }

        /// <summary>
        /// GetBoolを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public bool GetBool()
        {
            return GetByte() == 1;
        }

        /// <summary>
        /// GetCharを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public char GetChar()
        {
            return (char)GetUShort();
        }

        /// <summary>
        /// GetUShortを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public ushort GetUShort()
        {
            ushort result = BinaryPrimitives.ReadUInt16LittleEndian(_data.AsSpan(_position));
            _position += 2;
            return result;
        }

        /// <summary>
        /// GetShortを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public short GetShort()
        {
            short result = BinaryPrimitives.ReadInt16LittleEndian(_data.AsSpan(_position));
            _position += 2;
            return result;
        }

        /// <summary>
        /// GetLongを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public long GetLong()
        {
            long result = BinaryPrimitives.ReadInt64LittleEndian(_data.AsSpan(_position));
            _position += 8;
            return result;
        }

        /// <summary>
        /// GetULongを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public ulong GetULong()
        {
            ulong result = BinaryPrimitives.ReadUInt64LittleEndian(_data.AsSpan(_position));
            _position += 8;
            return result;
        }

        /// <summary>
        /// GetIntを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public int GetInt()
        {
            int result = BinaryPrimitives.ReadInt32LittleEndian(_data.AsSpan(_position));
            _position += 4;
            return result;
        }

        /// <summary>
        /// GetUIntを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public uint GetUInt()
        {
            uint result = BinaryPrimitives.ReadUInt32LittleEndian(_data.AsSpan(_position));
            _position += 4;
            return result;
        }

        /// <summary>
        /// GetFloatを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public float GetFloat()
        {
            int bits = BinaryPrimitives.ReadInt32LittleEndian(_data.AsSpan(_position));
            _position += 4;
            return BitConverter.Int32BitsToSingle(bits);
        }

        /// <summary>
        /// GetDoubleを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public double GetDouble()
        {
            long bits = BinaryPrimitives.ReadInt64LittleEndian(_data.AsSpan(_position));
            _position += 8;
            return BitConverter.Int64BitsToDouble(bits);
        }

        /// <summary>
        /// Note that "maxLength" only limits the number of characters in a string, not its size in bytes.
        /// </summary>
        /// <returns>"string.Empty" if value > "maxLength"</returns>
        public string GetString(int maxLength)
        {
            ushort size = GetUShort();
            if (size == 0)
                return string.Empty;

            int actualSize = size - 1;
            if (actualSize > _dataSize - _position)
                throw new ArgumentException($"String length {actualSize} exceeds available data ({_dataSize - _position} bytes).");
            string result = maxLength > 0 && NetDataWriter.uTF8Encoding.Value.GetCharCount(_data, _position, actualSize) > maxLength ?
                string.Empty :
                NetDataWriter.uTF8Encoding.Value.GetString(_data, _position, actualSize);
            _position += actualSize;
            return result;
        }

        /// <summary>
        /// GetStringを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public string GetString()
        {
            ushort size = GetUShort();
            if (size == 0)
                return string.Empty;

            int actualSize = size - 1;
            if (actualSize > _dataSize - _position)
                throw new ArgumentException($"String length {actualSize} exceeds available data ({_dataSize - _position} bytes).");
            string result = NetDataWriter.uTF8Encoding.Value.GetString(_data, _position, actualSize);
            _position += actualSize;
            return result;
        }

        /// <summary>
        /// GetLargeStringを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public string GetLargeString()
        {
            int size = GetInt();
            if (size <= 0)
                return string.Empty;
            if (size > _dataSize - _position)
                throw new ArgumentException($"String length {size} exceeds available data ({_dataSize - _position} bytes).");
            string result = NetDataWriter.uTF8Encoding.Value.GetString(_data, _position, size);
            _position += size;
            return result;
        }
        
        /// <summary>
        /// GetGuidを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public Guid GetGuid()
        {
            if (16 > _dataSize - _position)
                throw new ArgumentException($"Guid read exceeds available data ({_dataSize - _position} bytes).");
            var result = new Guid(_data.AsSpan(_position, 16));
            _position += 16;
            return result;
        }

        /// <summary>
        /// GetBytesSegmentを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public ArraySegment<byte> GetBytesSegment(int count)
        {
            if (count < 0 || count > _dataSize - _position)
                throw new ArgumentException($"Segment length {count} exceeds available data ({_dataSize - _position} bytes).");
            ArraySegment<byte> segment = new ArraySegment<byte>(_data, _position, count);
            _position += count;
            return segment;
        }

        /// <summary>
        /// GetRemainingBytesSegmentを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public ArraySegment<byte> GetRemainingBytesSegment()
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_data, _position, AvailableBytes);
            _position = _data.Length;
            return segment;
        }

        // public T Get<T>() where T : struct, INetSerializable
        // {
        //     var obj = default(T);
        //     obj.Deserialize(this);
        //     return obj;
        // }

        // public T Get<T>(Func<T> constructor) where T : class, INetSerializable
        // {
        //     var obj = constructor();
        //     obj.Deserialize(this);
        //     return obj;
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        /// GetRemainingBytesSpanを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public ReadOnlySpan<byte> GetRemainingBytesSpan()
        {
            return new ReadOnlySpan<byte>(_data, _position, _dataSize - _position);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        /// <summary>
        /// GetRemainingBytesMemoryを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public ReadOnlyMemory<byte> GetRemainingBytesMemory()
        {
            return new ReadOnlyMemory<byte>(_data, _position, _dataSize - _position);
        }
        /// <summary>
        /// GetRemainingBytesを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public byte[] GetRemainingBytes()
        {
            byte[] outgoingData = new byte[AvailableBytes];
            Buffer.BlockCopy(_data, _position, outgoingData, 0, AvailableBytes);
            _position = _data.Length;
            return outgoingData;
        }

        /// <summary>
        /// GetBytesを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public void GetBytes(byte[] destination, int start, int count)
        {
            if (count < 0 || count > _dataSize - _position)
                throw new ArgumentException($"Byte read {count} exceeds available data ({_dataSize - _position} bytes).");
            Buffer.BlockCopy(_data, _position, destination, start, count);
            _position += count;
        }

        /// <summary>
        /// GetBytesを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public void GetBytes(byte[] destination, int count)
        {
            if (count < 0 || count > _dataSize - _position)
                throw new ArgumentException($"Byte read {count} exceeds available data ({_dataSize - _position} bytes).");
            Buffer.BlockCopy(_data, _position, destination, 0, count);
            _position += count;
        }

        /// <summary>
        /// GetSBytesWithLengthを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public sbyte[] GetSBytesWithLength()
        {
            return GetArray<sbyte>(1);
        }

        /// <summary>
        /// GetBytesWithLengthを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public byte[] GetBytesWithLength()
        {
            return GetArray<byte>(1);
        }
        #endregion

        #region PeekMethods

        /// <summary>
        /// PeekByteを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public byte PeekByte()
        {
            return _data[_position];
        }

        /// <summary>
        /// PeekSByteを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public sbyte PeekSByte()
        {
            return (sbyte)_data[_position];
        }

        /// <summary>
        /// PeekBoolを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public bool PeekBool()
        {
            return _data[_position] == 1;
        }

        /// <summary>
        /// PeekCharを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public char PeekChar()
        {
            return (char)PeekUShort();
        }

        /// <summary>
        /// PeekUShortを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public ushort PeekUShort()
        {
            return BinaryPrimitives.ReadUInt16LittleEndian(_data.AsSpan(_position));
        }

        /// <summary>
        /// PeekShortを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public short PeekShort()
        {
            return BinaryPrimitives.ReadInt16LittleEndian(_data.AsSpan(_position));
        }

        /// <summary>
        /// PeekLongを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public long PeekLong()
        {
            return BinaryPrimitives.ReadInt64LittleEndian(_data.AsSpan(_position));
        }

        /// <summary>
        /// PeekULongを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public ulong PeekULong()
        {
            return BinaryPrimitives.ReadUInt64LittleEndian(_data.AsSpan(_position));
        }

        /// <summary>
        /// PeekIntを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public int PeekInt()
        {
            return BinaryPrimitives.ReadInt32LittleEndian(_data.AsSpan(_position));
        }

        /// <summary>
        /// PeekUIntを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public uint PeekUInt()
        {
            return BinaryPrimitives.ReadUInt32LittleEndian(_data.AsSpan(_position));
        }

        /// <summary>
        /// PeekFloatを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public float PeekFloat()
        {
            return BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(_data.AsSpan(_position)));
        }

        /// <summary>
        /// PeekDoubleを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public double PeekDouble()
        {
            return BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64LittleEndian(_data.AsSpan(_position)));
        }

        /// <summary>
        /// Note that "maxLength" only limits the number of characters in a string, not its size in bytes.
        /// </summary>
        public string PeekString(int maxLength)
        {
            ushort size = PeekUShort();
            if (size == 0)
                return string.Empty;
            
            int actualSize = size - 1;
            return (maxLength > 0 && NetDataWriter.uTF8Encoding.Value.GetCharCount(_data, _position + 2, actualSize) > maxLength) ?
                string.Empty :
                NetDataWriter.uTF8Encoding.Value.GetString(_data, _position + 2, actualSize);
        }

        /// <summary>
        /// PeekStringを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public string PeekString()
        {
            // Defensive: callers (e.g., HandleDisconnectionReason) sometimes
            // hand us a buffer that doesn't actually contain a length-prefixed
            // string — a version-mismatch reject or any other malformed
            // additional-data payload would otherwise tip GetString into an
            // ArgumentOutOfRangeException. Validate before reading.
            if (AvailableBytes < 2) return string.Empty;
            ushort size = PeekUShort();
            if (size == 0)
                return string.Empty;

            int actualSize = size - 1;
            if (actualSize < 0 || _position + 2 + actualSize > _data.Length)
                return string.Empty;
            return NetDataWriter.uTF8Encoding.Value.GetString(_data, _position + 2, actualSize);
        }
        #endregion

        #region TryGetMethods
        /// <summary>
        /// TryGetByteを試行し、失敗時に呼び出し元が分岐できる結果を返します。
        /// </summary>
        public bool TryGetByte(out byte result)
        {
            if (AvailableBytes >= 1)
            {
                result = GetByte();
                return true;
            }
            result = 0;
            return false;
        }

        /// <summary>
        /// TryGetSByteを試行し、失敗時に呼び出し元が分岐できる結果を返します。
        /// </summary>
        public bool TryGetSByte(out sbyte result)
        {
            if (AvailableBytes >= 1)
            {
                result = GetSByte();
                return true;
            }
            result = 0;
            return false;
        }

        /// <summary>
        /// TryGetBoolを試行し、失敗時に呼び出し元が分岐できる結果を返します。
        /// </summary>
        public bool TryGetBool(out bool result)
        {
            if (AvailableBytes >= 1)
            {
                result = GetBool();
                return true;
            }
            result = false;
            return false;
        }

        /// <summary>
        /// TryGetCharを試行し、失敗時に呼び出し元が分岐できる結果を返します。
        /// </summary>
        public bool TryGetChar(out char result)
        {
            if (!TryGetUShort(out ushort uShortValue))
            {
                result = '\0';
                return false;
            }
            result = (char)uShortValue;
            return true;
        }

        /// <summary>
        /// TryGetShortを試行し、失敗時に呼び出し元が分岐できる結果を返します。
        /// </summary>
        public bool TryGetShort(out short result)
        {
            if (AvailableBytes >= 2)
            {
                result = GetShort();
                return true;
            }
            result = 0;
            return false;
        }

        /// <summary>
        /// TryGetUShortを試行し、失敗時に呼び出し元が分岐できる結果を返します。
        /// </summary>
        public bool TryGetUShort(out ushort result)
        {
            if (AvailableBytes >= 2)
            {
                result = GetUShort();
                return true;
            }
            result = 0;
            return false;
        }

        /// <summary>
        /// TryGetIntを試行し、失敗時に呼び出し元が分岐できる結果を返します。
        /// </summary>
        public bool TryGetInt(out int result)
        {
            if (AvailableBytes >= 4)
            {
                result = GetInt();
                return true;
            }
            result = 0;
            return false;
        }

        /// <summary>
        /// TryGetUIntを試行し、失敗時に呼び出し元が分岐できる結果を返します。
        /// </summary>
        public bool TryGetUInt(out uint result)
        {
            if (AvailableBytes >= 4)
            {
                result = GetUInt();
                return true;
            }
            result = 0;
            return false;
        }

        /// <summary>
        /// TryGetLongを試行し、失敗時に呼び出し元が分岐できる結果を返します。
        /// </summary>
        public bool TryGetLong(out long result)
        {
            if (AvailableBytes >= 8)
            {
                result = GetLong();
                return true;
            }
            result = 0;
            return false;
        }

        /// <summary>
        /// TryGetULongを試行し、失敗時に呼び出し元が分岐できる結果を返します。
        /// </summary>
        public bool TryGetULong(out ulong result)
        {
            if (AvailableBytes >= 8)
            {
                result = GetULong();
                return true;
            }
            result = 0;
            return false;
        }

        /// <summary>
        /// TryGetFloatを試行し、失敗時に呼び出し元が分岐できる結果を返します。
        /// </summary>
        public bool TryGetFloat(out float result)
        {
            if (AvailableBytes >= 4)
            {
                result = GetFloat();
                return true;
            }
            result = 0;
            return false;
        }

        /// <summary>
        /// TryGetDoubleを試行し、失敗時に呼び出し元が分岐できる結果を返します。
        /// </summary>
        public bool TryGetDouble(out double result)
        {
            if (AvailableBytes >= 8)
            {
                result = GetDouble();
                return true;
            }
            result = 0;
            return false;
        }

        /// <summary>
        /// TryGetStringを試行し、失敗時に呼び出し元が分岐できる結果を返します。
        /// </summary>
        public bool TryGetString(out string result)
        {
            if (AvailableBytes >= 2)
            {
                ushort strSize = PeekUShort();
                if (AvailableBytes >= strSize + 1)
                {
                    result = GetString();
                    return true;
                }
            }
            result = null;
            return false;
        }

        /// <summary>
        /// TryGetStringArrayを試行し、失敗時に呼び出し元が分岐できる結果を返します。
        /// </summary>
        public bool TryGetStringArray(out string[] result)
        {
            if (!TryGetUShort(out ushort strArrayLength)) {
                result = null;
                return false;
            }

            result = new string[strArrayLength];
            for (int i = 0; i < strArrayLength; i++)
            {
                if (!TryGetString(out result[i]))
                {
                    result = null;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// TryGetBytesWithLengthを試行し、失敗時に呼び出し元が分岐できる結果を返します。
        /// </summary>
        public bool TryGetBytesWithLength(out byte[] result)
        {
            if (AvailableBytes >= 2)
            {
                ushort length = PeekUShort();
                if (length >= 0 && AvailableBytes >= 2 + length)
                {
                    result = GetBytesWithLength();
                    return true;
                }
            }
            result = null;
            return false;
        }
        #endregion

        /// <summary>
        /// Clearを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public void Clear()
        {
            _position = 0;
            _dataSize = 0;
            _data = null;
        }
    }
}
