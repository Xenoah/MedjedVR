using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace BasisNetworking.InitialData
{
    [Serializable]
    /// <summary>
    /// BasisDefaultライブラリ設定の責務をまとめるクラスです。
    /// InitalData領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public class BasisDefaultLibraryConfiguration
    {
        // client 側の BundledContentHolder.Mode と対応する: 0=Avatar, 1=World, 2=Prop。
        public byte Mode = 0;
        /// <summary>
        /// Urlを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string Url = "";
        /// <summary>
        /// Passwordを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string Password = "";

        /// <summary>
        /// 読み込みAllFromFolderを初期化します。設定、永続化ファイル、実行時キャッシュを起動時の状態へ整えます。
        /// </summary>
        public static BasisDefaultLibraryConfiguration[] LoadAllFromFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"The folder '{folderPath}' does not exist.");
            }

            List<BasisDefaultLibraryConfiguration> configurations = new List<BasisDefaultLibraryConfiguration>();

            string[] xmlFiles = Directory.GetFiles(folderPath, "*.xml");
            var serializer = new XmlSerializer(typeof(BasisDefaultLibraryConfiguration));
            foreach (var file in xmlFiles)
            {
                using var reader = new StreamReader(file);
                configurations.Add((BasisDefaultLibraryConfiguration)serializer.Deserialize(reader));
                reader.Close();
            }

            return configurations.ToArray();
        }
    }
}
