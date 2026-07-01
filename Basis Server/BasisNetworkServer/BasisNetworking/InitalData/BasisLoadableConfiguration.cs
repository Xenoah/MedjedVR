using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace BasisNetworking.InitialData
{
    [Serializable]
    /// <summary>
    /// BasisLoadable設定の責務をまとめるクラスです。
    /// InitalData領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public class BasisLoadableConfiguration
    {
        /// <summary>
        /// Modeを保持します。型は byte で、関連処理から共有される値です。
        /// </summary>
        public byte Mode = 0;
        /// <summary>
        /// LoadedNetIDを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string LoadedNetID = "";
        /// <summary>
        /// UnlockPasswordを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string UnlockPassword = "";
        /// <summary>
        /// CombinedURLを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public string CombinedURL = "";

        /// <summary>
        /// PositionXを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float PositionX = 0f;
        /// <summary>
        /// PositionYを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float PositionY = 0f;
        /// <summary>
        /// PositionZを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float PositionZ = 0f;

        /// <summary>
        /// QuaternionXを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float QuaternionX = 0f;
        /// <summary>
        /// QuaternionYを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float QuaternionY = 0f;
        /// <summary>
        /// QuaternionZを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float QuaternionZ = 0f;
        /// <summary>
        /// QuaternionWを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float QuaternionW = 1f;

        /// <summary>
        /// ScaleXを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float ScaleX = 1f;
        /// <summary>
        /// ScaleYを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float ScaleY = 1f;
        /// <summary>
        /// ScaleZを保持します。型は float で、関連処理から共有される値です。
        /// </summary>
        public float ScaleZ = 1f;

        /// <summary>
        /// Persistを保持します。型は bool で、関連処理から共有される値です。
        /// </summary>
        public bool Persist = false;
        /// <summary>
        /// ModifyScaleを保持します。型は bool で、関連処理から共有される値です。
        /// </summary>
        public bool ModifyScale;
        /// <summary>
        /// 読み込みAllFromFolderを初期化します。設定、永続化ファイル、実行時キャッシュを起動時の状態へ整えます。
        /// </summary>
        public static BasisLoadableConfiguration[] LoadAllFromFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"The folder '{folderPath}' does not exist.");
            }

            List<BasisLoadableConfiguration> configurations = new List<BasisLoadableConfiguration>();

            string[] xmlFiles = Directory.GetFiles(folderPath, "*.xml");
            var serializer = new XmlSerializer(typeof(BasisLoadableConfiguration));
            foreach (var file in xmlFiles)
            {
                using var reader = new StreamReader(file);
                configurations.Add((BasisLoadableConfiguration)serializer.Deserialize(reader));
                reader.Close();
            }

            return configurations.ToArray();
        }
    }
}
