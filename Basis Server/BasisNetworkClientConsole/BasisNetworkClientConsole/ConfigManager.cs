using System.Xml.Linq;

namespace Basis.Config
{
    /// <summary>
    /// 設定管理の責務をまとめるクラスです。
    /// ClientConsole領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public static class ConfigManager
    {
        /// <summary>
        /// Passwordを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public static string Password = "default_password";
        /// <summary>
        /// Ipを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public static string Ip = "localhost";
        /// <summary>
        /// Portを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public static int Port = 4296;
        /// <summary>
        /// クライアントCountを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public static int ClientCount = 250;

        /// <summary>
        /// アバターPasswordを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public static string AvatarPassword = "default_avatar_password";
        /// <summary>
        /// アバターUrlを保持します。型は string で、関連処理から共有される値です。
        /// </summary>
        public static string AvatarUrl = "http://localhost/avatar";
        /// <summary>
        /// アバター読み込みModeを保持します。型は int で、関連処理から共有される値です。
        /// </summary>
        public static int AvatarLoadMode = 1;

        private static readonly object _lock = new();
        static XElement? Child(XElement parent, string name) =>
            parent.Elements().FirstOrDefault(e => e.Name.LocalName == name);

        /// <summary>
        /// ReadStringを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        static string ReadString(XElement root, string name, string fallback)
        {
            var el = Child(root, name);
            if (el == null)
            {
                BNL.Log($"Missing <{name}>, using fallback.");
                return fallback;
            }

            var value = el.Value.Trim();
            BNL.Log($"Loaded {name}: [{value}]");
            return value;
        }

        /// <summary>
        /// ReadIntを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        static int ReadInt(XElement root, string name, int fallback)
        {
            var el = Child(root, name);
            if (el == null)
            {
                BNL.Log($"Missing <{name}>, using fallback {fallback}.");
                return fallback;
            }

            if (!int.TryParse(el.Value, out var value))
            {
                BNL.Log($"Invalid <{name}> value '{el.Value}', using fallback {fallback}.");
                return fallback;
            }

            BNL.Log($"Loaded {name}: {value}");
            return value;
        }

        // ---------------- main entry ----------------

        public static void LoadOrCreateConfigXml(string filePath)
        {
            lock (_lock)
            {
                filePath = Path.GetFullPath(filePath);
                BNL.Log($"Config path: {filePath}");

                if (!File.Exists(filePath))
                {
                    BNL.Log("Config file not found. Creating default.");

                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                        var doc = new XDocument(
                            new XComment(" BasisNetworkClientConsole load-tester configuration。stress testing のため、server に接続する fake client を ClientCount 件 spawn する。 "),
                            new XElement("Configuration",
                                new XComment(" server connection password。server 側の <Password> と一致している必要がある。string。 "),
                                new XElement("Password", Password),
                                new XComment(" 接続先 server host。hostname または IP (例: localhost / 127.0.0.1)。string。 "),
                                new XElement("Ip", Ip),
                                new XComment(" server UDP port。server 側の <SetPort> と一致している必要がある。int、範囲は 1-65535。 "),
                                new XElement("Port", Port),
                                new XComment(" load testing 用に spawn する simulated client 数。int (>= 1)。大きい値ほど CPU、memory、socket を多く使う。 "),
                                new XElement("ClientCount", ClientCount),
                                new XComment(" avatar と一緒に送る unlock password/key。<AvatarUrl> の (encrypted .BEE) bundle を decrypt するために使う。string。 "),
                                new XElement("AvatarPassword", AvatarPassword),
                                new XComment(" 各 fake client が advertise する avatar source。AvatarLoadMode 0 では (encrypted .BEE) bundle の download URL。string。 "),
                                new XElement("AvatarUrl", AvatarUrl),
                                new XComment(" 受信側 client が avatar を load する方法。0 = AssetBundle (AvatarUrl から download)、1 = Addressables、2 = In-scene。許可値は 0、1、2。 "),
                                new XElement("AvatarLoadMode", AvatarLoadMode)
                            )
                        );

                        // atomic write。
                        var temp = filePath + ".tmp";
                        doc.Save(temp);
                        File.Move(temp, filePath);

                        BNL.Log("Default config created successfully.");
                    }
                    catch (Exception ex)
                    {
                        BNL.LogError("Failed to create config file." + ex.Message);
                    }

                    return;
                }

                XDocument docLoaded;
                try
                {
                    docLoaded = XDocument.Load(filePath, LoadOptions.PreserveWhitespace);
                }
                catch (Exception ex)
                {
                    BNL.LogError("Failed to load config XML (corrupt or in use)." + ex.Message);
                    return;
                }

                var root = docLoaded.Root;
                if (root == null)
                {
                    BNL.Log("Config XML has no root element.");
                    return;
                }

                BNL.Log($"Root element: {root.Name} | Namespace: '{root.Name.NamespaceName}'");

                try
                {
                    Password = ReadString(root, "Password", Password);
                    Ip = ReadString(root, "Ip", Ip);
                    Port = ReadInt(root, "Port", Port);
                    ClientCount = ReadInt(root, "ClientCount", ClientCount);

                    AvatarPassword = ReadString(root, "AvatarPassword", AvatarPassword);
                    AvatarUrl = ReadString(root, "AvatarUrl", AvatarUrl);
                    AvatarLoadMode = ReadInt(root, "AvatarLoadMode", AvatarLoadMode);
                }
                catch (Exception ex)
                {
                    BNL.LogError("Unexpected error while parsing config." + ex.Message);
                }
            }
        }
    }
}
