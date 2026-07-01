using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BasisNetworkServer.BasisNetworking
{
    [DataContract]
    /// <summary>
    /// BasisDataの責務をまとめるクラスです。
    /// ing領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public class BasisData
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public ConcurrentDictionary<string, object> JsonPayload { get; set; } = new();

        /// <summary>
        /// BasisDataを生成し、利用に必要な初期状態を設定します。
        /// </summary>
        public BasisData(string name, ConcurrentDictionary<string, object> jsonPayload)
        {
            Name = name;
            JsonPayload = jsonPayload ?? new ConcurrentDictionary<string, object>();
        }
    }

    /// <summary>
    /// BasisPersistentデータベースの責務をまとめるクラスです。
    /// ing領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static class BasisPersistentDatabase
    {
        private static readonly ConcurrentDictionary<string, BasisData> _dataByName = new();
        private static readonly object _fileLock = new();

        private static string _filePath = "basis_data.json";
        private static volatile bool _isDirty = false;
        private static readonly CancellationTokenSource _cts = new();
        private static readonly TimeSpan _saveInterval = TimeSpan.FromSeconds(5);
        private static readonly AutoResetEvent _saveTrigger = new(false);

        /// <summary>
        /// BasisPersistentデータベースを生成し、利用に必要な初期状態を設定します。
        /// </summary>
        static BasisPersistentDatabase()
        {
            Load();
            StartAutoSaveLoop();
        }

        /// <summary>
        /// SetFilePathを設定します。以後のネットワーク処理で参照される状態を更新します。
        /// </summary>
        public static void SetFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            lock (_fileLock)
            {
                _filePath = path;
                Load();
            }
        }

        /// <summary>
        /// AddOrUpdateを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static bool AddOrUpdate(BasisData item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                BNL.LogError("Name must not be null or whitespace. (basis database)");
                return false;
            }
            if (item.Name.Length > BasisNetworkServer.Security.BasisResourceLimitManager.MaxDatabaseNameLength)
            {
                BNL.LogError("Name exceeds maximum length. (basis database)");
                return false;
            }
            if (item.JsonPayload != null && item.JsonPayload.Count > BasisNetworkServer.Security.BasisResourceLimitManager.MaxDatabasePayloadEntries)
            {
                BNL.LogError("Payload exceeds maximum entry count. (basis database)");
                return false;
            }
            if (!_dataByName.ContainsKey(item.Name) && _dataByName.Count >= BasisNetworkServer.Security.BasisResourceLimitManager.MaxDatabaseEntries)
            {
                BNL.LogError("Database entry limit reached; rejecting new entry. (basis database)");
                return false;
            }
            _dataByName.AddOrUpdate(item.Name,
                addValueFactory: _ => item,
                updateValueFactory: (_, existing) =>
                {
                    existing.JsonPayload = new ConcurrentDictionary<string, object>(item.JsonPayload);
                    return existing;
                });

            MarkDirty();
            return true;
        }

        public static IEnumerable<BasisData> GetAll() => _dataByName.Values.ToArray();

        /// <summary>
        /// GetByNameを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public static bool GetByName(string name, out BasisData BasisData)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                BasisData = null;
                return false;
            }
            return _dataByName.TryGetValue(name, out BasisData);
        }
        /// <summary>
        /// Removeを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static bool Remove(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            var result = _dataByName.TryRemove(name, out _);
            if (result) MarkDirty();
            return result;
        }

        /// <summary>
        /// MarkDirtyを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private static void MarkDirty()
        {
            _isDirty = true;
            _saveTrigger.Set();
        }

        /// <summary>
        /// Saveを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void Save()
        {
            lock (_fileLock)
            {
                try
                {
                    var list = GetAll().ToList();

                    using var stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                    var serializer = new DataContractJsonSerializer(typeof(List<BasisData>), new DataContractJsonSerializerSettings
                    {
                        UseSimpleDictionaryFormat = true
                    });

                    serializer.WriteObject(stream, list);
                    _isDirty = false;
                }
                catch (IOException ex)
                {
                    BNL.LogError($"Failed to save data: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 読み込みを初期化します。設定、永続化ファイル、実行時キャッシュを起動時の状態へ整えます。
        /// </summary>
        public static void Load()
        {
            lock (_fileLock)
            {
                if (!File.Exists(_filePath))
                    return;

                try
                {
                    using var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    var serializer = new DataContractJsonSerializer(typeof(List<BasisData>), new DataContractJsonSerializerSettings
                    {
                        UseSimpleDictionaryFormat = true
                    });

                    if (serializer.ReadObject(stream) is List<BasisData> loadedData)
                    {
                        _dataByName.Clear();
                        foreach (var item in loadedData)
                        {
                            if (!string.IsNullOrWhiteSpace(item.Name))
                                _dataByName[item.Name] = item;
                        }
                    }
                }
                catch (SerializationException ex)
                {
                    BNL.LogError($"Deserialization error: {ex.Message}");
                }
                catch (IOException ex)
                {
                    BNL.LogError($"Failed to load data: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// StartAutoSaveLoopを開始します。依存する状態を準備して実行ループや待ち受けを有効化します。
        /// </summary>
        private static void StartAutoSaveLoop()
        {
            Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    _saveTrigger.WaitOne(_saveInterval);

                    if (_isDirty)
                    {
                        Save();
                    }
                }
            }, _cts.Token);
        }

        /// <summary>
        /// Shutdownを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static void Shutdown()
        {
            _cts.Cancel();
            _saveTrigger.Set(); // Wake the loop to exit
            Save(); // Final save on shutdown
        }
    }
}
