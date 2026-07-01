using BasisNetworking.InitialData;
using System;
using System.IO;
using static SerializableBasis;

namespace BasisNetworking.InitialData
{
    /// <summary>
    /// BasisLoadableローダーの責務をまとめるクラスです。
    /// InitalData領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static class BasisLoadableLoader
    {
        /// <summary>
        /// 読み込みXMLを初期化します。設定、永続化ファイル、実行時キャッシュを起動時の状態へ整えます。
        /// </summary>
        public static void LoadXML(string FolderName)
        {
            try
            {
                // 実行ファイルのディレクトリを取得する
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // 新しいフォルダー名を定義する
                string newFolderPath = Path.Combine(exeDirectory, FolderName);

                // フォルダーが存在するか確認し、なければ作成する
                if (!Directory.Exists(newFolderPath))
                {
                    Directory.CreateDirectory(newFolderPath);
                    BNL.Log("Folder created successfully: " + newFolderPath);
                    // ユーザーがコピーまたはコメント解除できる XML サンプルを用意する

                    string exampleFilePath = Path.Combine(newFolderPath, "ExampleConfigdisabled.xml[remove]");
                    File.WriteAllText(exampleFilePath, exampleXml);
                    BNL.Log("Example XML file created at: " + exampleFilePath);
                }

                BasisLoadableConfiguration[] configurations = BasisLoadableConfiguration.LoadAllFromFolder(FolderName);

                foreach (BasisLoadableConfiguration config in configurations)
                {
                    BNL.Log($"CombinedURL: {config.CombinedURL}, LoadAssetPassword: {config.UnlockPassword}");
                    LocalLoadResource LLR = FromBasisLoadableConfiguration(config);
                    if(string.IsNullOrEmpty(LLR.LoadedNetID))
                    {
                        LLR.LoadedNetID = GenerateUniqueID();
                        BNL.Log($"No Network Id Assigned Generated to be {LLR.LoadedNetID}");
                    }
                    BasisNetworkResourceManagement.LoadResource(LLR);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        /// <summary>
        /// GenerateUniqueIDを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static string GenerateUniqueID()
        {
            Guid newGuid = Guid.NewGuid();  // Generate a new GUID
            string utcDate = DateTime.UtcNow.ToString("yyyyMMdd");  // Get the current UTC date (YYYYMMDD)
            string guid = newGuid.ToString("N"); // Remove dashes from GUID

            return $"{guid}{utcDate}";
        }
        /// <summary>
        /// FromBasisLoadable設定を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public static LocalLoadResource FromBasisLoadableConfiguration(BasisLoadableConfiguration config)
        {
            return new LocalLoadResource
            {
                Mode = config.Mode,
                LoadedNetID = config.LoadedNetID,
                UnlockPassword = config.UnlockPassword,
                CombinedURL = config.CombinedURL,
                PositionX = config.PositionX,
                PositionY = config.PositionY,
                PositionZ = config.PositionZ,
                QuaternionX = config.QuaternionX,
                QuaternionY = config.QuaternionY,
                QuaternionZ = config.QuaternionZ,
                QuaternionW = config.QuaternionW,
                ScaleX = config.ScaleX,
                ScaleY = config.ScaleY,
                ScaleZ = config.ScaleZ,
                Persist = config.Persist,
                ModifyScale = config.ModifyScale,
                IsAdminLocked = true,
            };
        }
        /// <summary>
        /// exampleXmlを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public const string exampleXml = @"<BasisLoadableConfiguration>
    <!-- Mode of the configuration -->
    <Mode>0</Mode>
    <!-- Network ID -->
    <LoadedNetID></LoadedNetID>
    <!-- Unlock password -->
    <UnlockPassword></UnlockPassword>
    <!-- Combined URl -->
    <CombinedURL></CombinedURL>
    <!-- Local load flag -->
    <IsLocalLoad>false</IsLocalLoad>

    <!-- Position values -->
    <PositionX>0</PositionX>
    <PositionY>0</PositionY>
    <PositionZ>0</PositionZ>

    <!-- Quaternion 値 -->
    <QuaternionX>0</QuaternionX>
    <QuaternionY>0</QuaternionY>
    <QuaternionZ>0</QuaternionZ>
    <QuaternionW>1</QuaternionW>

    <!-- Scale 値 -->
    <ScaleX>1</ScaleX>
    <ScaleY>1</ScaleY>
    <ScaleZ>1</ScaleZ>

    <!-- 永続化 flag -->
    <Persist>false</Persist>
</BasisLoadableConfiguration>";
    }
}
