using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace BasisNetworkServer.Security
{
    /// <summary>
    /// BasisBanListの責務をまとめるクラスです。
    /// Security領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public class BasisBanList
    {
        private readonly ConcurrentDictionary<string, byte> bannedPlayers = new();
        private readonly string filePath;

        /// <summary>
        /// BasisBanListを生成し、利用に必要な初期状態を設定します。
        /// </summary>
        public BasisBanList(string path = "BasisBanList.txt")
        {
            filePath = path;
            LoadBanList();
        }

        /// <summary>
        /// 読み込みBanListを初期化します。設定、永続化ファイル、実行時キャッシュを起動時の状態へ整えます。
        /// </summary>
        private void LoadBanList()
        {
            bannedPlayers.Clear();
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (!string.IsNullOrEmpty(trimmedLine))
                    {
                        bannedPlayers.TryAdd(trimmedLine, 0);
                    }
                }
            }
        }

        /// <summary>
        /// 読み込みBanListAsyncを初期化します。設定、永続化ファイル、実行時キャッシュを起動時の状態へ整えます。
        /// </summary>
        private async Task LoadBanListAsync()
        {
            bannedPlayers.Clear();
            if (File.Exists(filePath))
            {
                string[] lines = await File.ReadAllLinesAsync(filePath);
                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (!string.IsNullOrEmpty(trimmedLine))
                    {
                        bannedPlayers.TryAdd(trimmedLine, 0);
                    }
                }
            }
        }

        public bool IsBanned(string playerId) => bannedPlayers.ContainsKey(playerId);

        /// <summary>
        /// ReloadBanListAsyncを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task ReloadBanListAsync()
        {
            await LoadBanListAsync();
            Console.WriteLine("Ban list reloaded.");
        }

        /// <summary>
        /// AddToBanListAsyncを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task AddToBanListAsync(string playerId)
        {
            if (!bannedPlayers.ContainsKey(playerId))
            {
                bannedPlayers.TryAdd(playerId, 0);
                await File.AppendAllTextAsync(filePath, playerId + Environment.NewLine);
                Console.WriteLine($"{playerId} added to ban list.");
            }
        }

        /// <summary>
        /// RemoveFromBanListAsyncを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task RemoveFromBanListAsync(string playerId)
        {
            if (bannedPlayers.TryRemove(playerId, out _))
            {
                await SaveBanListAsync();
                Console.WriteLine($"{playerId} removed from ban list.");
            }
        }

        /// <summary>
        /// SaveBanListAsyncを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private async Task SaveBanListAsync()
        {
            await File.WriteAllLinesAsync(filePath, bannedPlayers.Keys);
        }
    }
}
