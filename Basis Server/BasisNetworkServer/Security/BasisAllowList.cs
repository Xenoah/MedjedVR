using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BasisNetworkServer.Security
{
    /// <summary>
    /// BasisAllowListの責務をまとめるクラスです。
    /// Security領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public class BasisAllowList
    {
        private readonly ConcurrentDictionary<string, byte> allowlistedPlayers = new ConcurrentDictionary<string, byte>();
        private readonly string filePath;

        /// <summary>
        /// BasisAllowListを生成し、利用に必要な初期状態を設定します。
        /// </summary>
        public BasisAllowList(string path = "BasisAllowList.txt")
        {
            filePath = path;
            _ = LoadAllowlistAsync(); // Fire and forget
        }

        /// <summary>
        /// 読み込みAllowlistAsyncを初期化します。設定、永続化ファイル、実行時キャッシュを起動時の状態へ整えます。
        /// </summary>
        private async Task LoadAllowlistAsync()
        {
            allowlistedPlayers.Clear();
            if (File.Exists(filePath))
            {
                string[] lines = await File.ReadAllLinesAsync(filePath);
                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (!string.IsNullOrEmpty(trimmedLine))
                    {
                        allowlistedPlayers.TryAdd(trimmedLine, 0);
                    }
                }
            }
        }

        public bool IsAllowed(string playerId) => allowlistedPlayers.ContainsKey(playerId);

        /// <summary>
        /// ReloadAllowlistAsyncを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task ReloadAllowlistAsync()
        {
            await LoadAllowlistAsync();
            Console.WriteLine("Allowlist reloaded.");
        }

        /// <summary>
        /// AddToAllowlistAsyncを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task AddToAllowlistAsync(string playerId)
        {
            if (!allowlistedPlayers.ContainsKey(playerId))
            {
                allowlistedPlayers.TryAdd(playerId, 0);
                await File.AppendAllTextAsync(filePath, playerId + Environment.NewLine);
                Console.WriteLine($"{playerId} added to allowlist.");
            }
        }

        /// <summary>
        /// RemoveFromAllowlistAsyncを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task RemoveFromAllowlistAsync(string playerId)
        {
            if (allowlistedPlayers.TryRemove(playerId, out _))
            {
                await SaveAllowlistAsync();
                Console.WriteLine($"{playerId} removed from allowlist.");
            }
        }

        /// <summary>
        /// SaveAllowlistAsyncを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private async Task SaveAllowlistAsync()
        {
            await File.WriteAllLinesAsync(filePath, allowlistedPlayers.Keys);
        }
    }
}
