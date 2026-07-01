using Basis.Network.Core;
using BasisNetworkCore.Security;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

[Serializable]
/// <summary>
/// 設定の責務をまとめるクラスです。
/// Core領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public class Configuration
{
    /// <summary>
    /// 設定FolderNameを保持します。型は string で、関連処理から共有される値です。
    /// </summary>
    public const string ConfigFolderName = "config";
    /// <summary>
    /// LogsFolderNameを保持します。型は string で、関連処理から共有される値です。
    /// </summary>
    public const string LogsFolderName = "logs";
    /// <summary>
    /// InitialResourcesFolderNameを保持します。型は string で、関連処理から共有される値です。
    /// </summary>
    public const string InitialResourcesFolderName = "initialresources";
    /// <summary>
    /// DefaultライブラリFolderNameを保持します。型は string で、関連処理から共有される値です。
    /// </summary>
    public const string DefaultLibraryFolderName = "defaultlibrary";

    /// <summary>
    /// config change により既存 file を強制的に rewrite したい場合に上げる
    /// (例: doc comment の refresh)。新しく追加された setting はどちらにせよ自動で heal される。
    /// load 時に current field が欠けている config は、新しい setting を追加した状態で re-save される。
    /// </summary>
    public const int CurrentConfigVersion = 3;
    /// <summary>config.xml に stamp される schema version。0 = versioning 前の file で、load 時に upgrade される。</summary>
    public int ConfigVersion = 0;

    /// <summary>
    /// ピアLimitを保持します。型は int で、関連処理から共有される値です。
    /// </summary>
    public int PeerLimit = ushort.MaxValue;
    /// <summary>
    /// SetPortを保持します。型は ushort で、関連処理から共有される値です。
    /// </summary>
    public ushort SetPort = 4296;
    /// <summary>unconnected server-info query が返す display name。client server-list UI の row title に表示される。</summary>
    public string ServerName = "Basis Server";
    /// <summary>info query response で server name と一緒に返す短い MOTD。list UI では短い 2 行がきれいに表示される。</summary>
    public string ServerMotd = "";
    /// <summary>
    /// Enable統計を保持します。型は bool で、関連処理から共有される値です。
    /// </summary>
    public bool EnableStatistics = true;
    /// <summary>
    /// HasFileSupportを保持します。型は bool で、関連処理から共有される値です。
    /// </summary>
    public bool HasFileSupport = true;
    /// <summary>
    /// HealthCheckHostを保持します。型は string で、関連処理から共有される値です。
    /// </summary>
    public string HealthCheckHost = "localhost";
    /// <summary>
    /// HealthCheckPortを保持します。型は ushort で、関連処理から共有される値です。
    /// </summary>
    public ushort HealthCheckPort = 10666;
    /// <summary>
    /// HealthPathを保持します。型は string で、関連処理から共有される値です。
    /// </summary>
    public string HealthPath = "/health";
    /// <summary>
    /// BSRSMillisecondDefaultIntervalを保持します。型は int で、関連処理から共有される値です。
    /// </summary>
    public int BSRSMillisecondDefaultInterval = 50;
    /// <summary>
    /// BSRBaseMultiplierを保持します。型は int で、関連処理から共有される値です。
    /// </summary>
    public int BSRBaseMultiplier = 1;
    /// <summary>
    /// BSRSIncreaseRateを保持します。型は float で、関連処理から共有される値です。
    /// </summary>
    public float BSRSIncreaseRate = 0.005f;
    /// <summary>
    /// BSRSlowestSendRateを保持します。型は float で、関連処理から共有される値です。
    /// </summary>
    public float BSRSlowestSendRate = 2.55f;
    /// <summary>
    /// HighQualityDistanceを保持します。型は float で、関連処理から共有される値です。
    /// </summary>
    public float HighQualityDistance = 10f;
    /// <summary>
    /// MediumQualityDistanceを保持します。型は float で、関連処理から共有される値です。
    /// </summary>
    public float MediumQualityDistance = 20f;
    /// <summary>
    /// LowQualityDistanceを保持します。型は float で、関連処理から共有される値です。
    /// </summary>
    public float LowQualityDistance = 40f;
    /// <summary>
    /// OverrideAutoDiscoveryOfIpvを保持します。型は bool で、関連処理から共有される値です。
    /// </summary>
    public bool OverrideAutoDiscoveryOfIpv = false;
    /// <summary>
    /// IPv4Addressを保持します。型は string で、関連処理から共有される値です。
    /// </summary>
    public string IPv4Address = "0.0.0.0";
    /// <summary>
    /// IPv6Addressを保持します。型は string で、関連処理から共有される値です。
    /// </summary>
    public string IPv6Address = "::";
    /// <summary>
    /// Passwordを保持します。型は string で、関連処理から共有される値です。
    /// </summary>
    public string Password = "default_password";
    /// <summary>
    /// Use認証を保持します。型は bool で、関連処理から共有される値です。
    /// </summary>
    public bool UseAuth = true;
    /// <summary>
    /// Use認証識別情報を保持します。型は bool で、関連処理から共有される値です。
    /// </summary>
    public bool UseAuthIdentity = true;
    /// <summary>
    /// ネットワークStackIdを保持します。型は string で、関連処理から共有される値です。
    /// </summary>
    public string NetworkStackId = "";
    /// <summary>
    /// BasisUserRestrictionModeを保持します。型は BasisUserRestrictionMode で、関連処理から共有される値です。
    /// </summary>
    public BasisUserRestrictionMode BasisUserRestrictionMode;
    /// <summary>
    /// HowManyDuplicate認証CanExistを保持します。型は int で、関連処理から共有される値です。
    /// </summary>
    public int HowManyDuplicateAuthCanExist = 2;
    /// <summary>
    /// 認証ValidationTimeOutMilisecondsを保持します。型は int で、関連処理から共有される値です。
    /// </summary>
    public int AuthValidationTimeOutMiliseconds = 9000;
    /// <summary>
    /// EnableConsoleを保持します。型は bool で、関連処理から共有される値です。
    /// </summary>
    public bool EnableConsole = true;
    /// <summary>
    /// DisableWriteUnlessAdminPersistentFlagを保持します。型は bool で、関連処理から共有される値です。
    /// </summary>
    public bool DisableWriteUnlessAdminPersistentFlag = true;
    /// <summary>
    /// DisableReadUnlessAdminPersistentFlagを保持します。型は bool で、関連処理から共有される値です。
    /// </summary>
    public bool DisableReadUnlessAdminPersistentFlag = false;
    /// <summary>
    /// true の場合、avatar reduction system は receiver ごとの avatar message を bundle し、
    /// CompressedAvatarBundleChannel で deflated として emit する。
    /// receiver の queued message が少なすぎて compression の価値がない場合、
    /// または compressed result が peer MTU を超える場合は、message ごとの uncompressed send に fallback する。
    /// client は対応する decoder を実装している必要がある。
    /// </summary>
    public bool EnableAvatarBundleCompression = true;
    /// <summary>single receiver に対して bundle を試みる前に必要な queued avatar message の最小数。</summary>
    public int AvatarBundleMinMessages = 4;
    /// <summary>LZ4 compression を試みる前に必要な uncompressed bundle bytes の最小値。LZ4 は per-call setup がほぼ 0 のため、128 は LZ4 が redundancy を見つけられない極小 case を guard するだけ。</summary>
    public int AvatarBundleMinBytes = 128;
    /// <summary>
    /// EnableBSRProfilingを保持します。型は bool で、関連処理から共有される値です。
    /// </summary>
    public bool EnableBSRProfiling = false;
    /// <summary>
    /// DisallowHeadlessを保持します。型は bool で、関連処理から共有される値です。
    /// </summary>
    public bool DisallowHeadless = false;

    // server boot 時に適用する global lockout default。
    // lock 中に load するには、対応する basis.resource.lockbypass.{avatar,prop,world} permission が必要。
    public bool AvatarsLocked = false;
    /// <summary>
    /// PropsLockedを保持します。型は bool で、関連処理から共有される値です。
    /// </summary>
    public bool PropsLocked = false;
    /// <summary>
    /// WorldsLockedを保持します。型は bool で、関連処理から共有される値です。
    /// </summary>
    public bool WorldsLocked = true;
    /// <summary>
    /// true の場合、peer は content share system 経由で saved-server entry を share できない。
    /// admin panel から live toggle でき、他の content lockout と一緒に config.xml へ persist される。
    /// 既存 deployment が従来どおり動くよう、default は off。
    /// </summary>
    public bool ServersLocked = false;
    /// <summary>
    /// true の場合、server はすべての client に desktop third-person camera を hard-disable するよう伝える。
    /// admin panel から live toggle でき、他の content lockout と一緒に config.xml へ persist される。
    /// 既存 deployment が従来どおり動くよう、default は off。
    /// </summary>
    public bool ThirdPersonDisabled = false;
    /// <summary>
    /// true の場合、server は inbound avatar sync message を他 peer へ propagate する前に
    /// AdditionalAvatarDatas (blendshape、custom-behaviour param) を strip する。
    /// muscle/position/rotation は通常どおり sync し、additional-data payload だけが drop される。
    /// admin panel から live toggle でき、他の content lockout と一緒に persist される。default は off。
    /// </summary>
    public bool AdditionalAvatarDataLock = false;
    /// <summary>
    /// 全 client に対して disallow する camera photo-metadata embedding category の per-category bitmask。
    /// 0 = すべて許可 (default)。boot 時に BasisGlobalLockManager へ seed され、
    /// GlobalGetLockState で client へ broadcast される。
    /// </summary>
    public byte CameraMetadataDisallowMask = 0;
    /// <summary>
    /// CrashReportingEnabledを保持します。型は bool で、関連処理から共有される値です。
    /// </summary>
    public bool CrashReportingEnabled = true;
    /// <summary>
    /// MaxMicrophoneRangeMetersを保持します。型は float で、関連処理から共有される値です。
    /// </summary>
    public float MaxMicrophoneRangeMeters = 25f;
    /// <summary>
    /// MaxHearingRangeMetersを保持します。型は float で、関連処理から共有される値です。
    /// </summary>
    public float MaxHearingRangeMeters = 25f;
    /// <summary>
    /// MinアバターEyeHeightMetersを保持します。型は float で、関連処理から共有される値です。
    /// </summary>
    public float MinAvatarEyeHeightMeters = 0.1f;
    /// <summary>
    /// MaxアバターEyeHeightMetersを保持します。型は float で、関連処理から共有される値です。
    /// </summary>
    public float MaxAvatarEyeHeightMeters = 100f;
    /// <summary>
    /// MaxデータベースEntriesを保持します。型は int で、関連処理から共有される値です。
    /// </summary>
    public int MaxDatabaseEntries = 10000;
    /// <summary>
    /// MaxデータベースNameLengthを保持します。型は int で、関連処理から共有される値です。
    /// </summary>
    public int MaxDatabaseNameLength = 256;
    /// <summary>
    /// MaxデータベースPayloadEntriesを保持します。型は int で、関連処理から共有される値です。
    /// </summary>
    public int MaxDatabasePayloadEntries = 1000;
    /// <summary>
    /// MaxContentSpheresPerプレイヤーを保持します。型は int で、関連処理から共有される値です。
    /// </summary>
    public int MaxContentSpheresPerPlayer = 32;
    /// <summary>
    /// PlayspaceMoverLockedを保持します。型は bool で、関連処理から共有される値です。
    /// </summary>
    public bool PlayspaceMoverLocked = false;
    /// <summary>
    /// DirectConnectLockedを保持します。型は bool で、関連処理から共有される値です。
    /// </summary>
    public bool DirectConnectLocked = false;

    // ── REST API ──────────────────────────────────────────────────────────────
    /// <summary>REST management API を有効にするには true にする。</summary>
    public bool ApiEnabled = false;
    /// <summary>
    /// ApiHostを保持します。型は string で、関連処理から共有される値です。
    /// </summary>
    public string ApiHost = "localhost";
    /// <summary>
    /// ApiPortを保持します。型は ushort で、関連処理から共有される値です。
    /// </summary>
    public ushort ApiPort = 10667;
    /// <summary>すべての API request で必要な bearer token。空文字列なら ApiEnabled が true でも API は無効。</summary>
    public string ApiKey = "";
    /// <summary>
    /// file から config を読む。file が見つからない場合は filePath に default config file を作成する。
    /// <c>{configDir}/transports/{stackId}.xml</c> から transport ごとの config sidecar も load する。
    /// </summary>
    public static Configuration LoadFromXml(string filePath)
    {
        RuntimeHelpers.RunClassConstructor(typeof(BasisNetworkStackRegistry).TypeHandle);

        Configuration result;
        var serializer = new XmlSerializer(typeof(Configuration));
        if (File.Exists(filePath))
        {
            using (var fileReader = new StreamReader(filePath))
            {
                result = (Configuration)serializer.Deserialize(fileReader);
            }

            // 古い config を heal する。current schema version より古いか、現在書き出す setting が欠けている場合、
            // 既存値を崩さず、新しい setting (default と doc comment 付き) を追加するため re-save する。
            if (BasisConfigXmlDocs.NeedsUpgrade(filePath, typeof(Configuration), result))
            {
                BNL.Log($"{filePath} is from an older version; adding missing settings.");
                result.WriteXml(filePath);
            }
        }
        else
        {
            BNL.Log($"{filePath} not found, creating with default values");
            result = new Configuration();
            result.WriteXml(filePath);
        }

        string configDir = Path.GetDirectoryName(filePath);
        BasisTransportConfigStore.LoadAll(configDir);
        return result;
    }

    /// <summary>
    /// この configuration を <paramref name="filePath"/> へ persist する。
    /// admin panel による in-game change (server name、MOTD、allowlist mode) を restart 後も残すために使う。
    /// sibling temp file + atomic move で書き込み、write 中の crash で live config が壊れないようにする。
    /// </summary>
    public void SaveToXml(string filePath)
    {
        WriteXml(filePath);
        BasisTransportConfigStore.SaveAll(Path.GetDirectoryName(filePath));
    }

    /// <summary>
    /// この config.xml だけを atomic に write する (temp file + replace)。
    /// current schema version を stamp し、doc comment を inject する。transport sidecar には触れない。
    /// </summary>
    private void WriteXml(string filePath)
    {
        ConfigVersion = CurrentConfigVersion;
        var serializer = new XmlSerializer(typeof(Configuration));
        string dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        string tempPath = filePath + ".tmp";
        using (var writer = new StreamWriter(tempPath))
        {
            BasisConfigXmlDocs.Serialize(serializer, typeof(Configuration), this, writer);
        }
        if (File.Exists(filePath)) File.Replace(tempPath, filePath, null);
        else File.Move(tempPath, filePath);
    }

    /// <summary>
    /// <c>{BaseDirectory}/{ConfigFolderName}/config.xml</c> 配下の canonical config.xml path を解決する。
    /// startup 時に bootstrapper (BasisServerConsole.Program / Unity host runner) が読む path と同じ。
    /// </summary>
    public static string GetDefaultPath()
    {
        return Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, ConfigFolderName, "config.xml");
    }

    /// <summary>
    /// public config field と同じ名前の environment variable が見つかった場合、
    /// config.xml に書かれている値をこの code が override する。
    ///
    /// Windows では console で次のように test できる:
    ///    $env:PeerLimit = "256"
    ///   .\BasisNetworkConsole.exe
    /// ただし本来は、Linux admin が launch 時に default を override できるようにするためのもの。
    /// </summary>
    public void ProcessEnvironmentalOverrides()
    {
        ApplyEnvironmentalOverridesTo(this);
    }

    /// <summary>
    /// ApplyEnvironmentalOverridesToを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
    /// </summary>
    private static void ApplyEnvironmentalOverridesTo(object target)
    {
        if (target == null) return;
        Type type = target.GetType();
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (!field.FieldType.IsPrimitive && field.FieldType != typeof(string) && field.FieldType.IsClass)
            {
                object nested = field.GetValue(target);
                if (nested != null) ApplyEnvironmentalOverridesTo(nested);
                continue;
            }

            string value = Environment.GetEnvironmentVariable(field.Name);
            if (value == null) continue;

            BNL.Log($"Applying Environmental Override with Field:{field.Name} Value:{value}");

            if (field.FieldType == typeof(int))
            {
                if (int.TryParse(value, out int number)) field.SetValue(target, number);
                else BNL.LogWarning("Could not cast to int. Failed Override");
            }
            else if (field.FieldType == typeof(ushort))
            {
                if (ushort.TryParse(value, out ushort number)) field.SetValue(target, number);
                else BNL.LogWarning("Could not cast to ushort. Failed Override.");
            }
            else if (field.FieldType == typeof(float))
            {
                if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float number)) field.SetValue(target, number);
                else BNL.LogWarning("Could not cast to float. Failed Override.");
            }
            else if (field.FieldType == typeof(string))
            {
                field.SetValue(target, value);
            }
            else if (field.FieldType == typeof(bool))
            {
                if (bool.TryParse(value, out bool boolResult)) field.SetValue(target, boolResult);
                else BNL.LogWarning($"Could not parse '{value}' as bool for field {field.Name}. Failed Override");
            }
            else
            {
                BNL.LogWarning($"Environmental variable type could not be processed for Config Field:{field.Name} Value:{value}");
            }
        }
    }
}
