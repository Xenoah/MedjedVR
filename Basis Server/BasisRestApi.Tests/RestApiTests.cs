using Basis.Network.Server;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Xunit;
using static SerializableBasis;

namespace BasisRestApi.Tests
{
    [Collection("RestApi")]
    /// <summary>
    /// RestApiTestsの責務をまとめるクラスです。
    /// RestApi.Tests領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public class RestApiTests : IDisposable
    {
        private const string ApiKey = "test-secret-key";
        private readonly BasisRestApiHandler _handler;
        private readonly HttpClient _authed;
        private readonly HttpClient _anon;
        private readonly string _base;

        /// <summary>
        /// RestApiTestsを生成し、利用に必要な初期状態を設定します。
        /// </summary>
        public RestApiTests()
        {
            BasisNetworkResourceManagement.UshortNetworkDatabase.Clear();
            BasisNetworkPreloadResourceManagement.Reset();

            ushort port = FreePort();
            _base = $"http://localhost:{port}";

            _handler = new BasisRestApiHandler(new Configuration
            {
                ApiEnabled = true,
                ApiKey = ApiKey,
                ApiHost = "localhost",
                ApiPort = port,
            });

            _authed = new HttpClient { BaseAddress = new Uri(_base) };
            _authed.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");

            _anon = new HttpClient { BaseAddress = new Uri(_base) };
        }

        /// <summary>
        /// Disposeを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public void Dispose()
        {
            _handler.Dispose();
            _authed.Dispose();
            _anon.Dispose();
            BasisNetworkResourceManagement.UshortNetworkDatabase.Clear();
            BasisNetworkPreloadResourceManagement.Reset();
        }

        // ── 認証 ──────────────────────────────────────────────────────────────

        [Fact]
        /// <summary>
        /// No認証HeaderReturns401を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task NoAuthHeader_Returns401()
        {
            var res = await _anon.GetAsync("/api/worlds");
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        }

        [Fact]
        /// <summary>
        /// WrongTokenReturns401を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task WrongToken_Returns401()
        {
            using var client = new HttpClient { BaseAddress = new Uri(_base) };
            client.DefaultRequestHeaders.Add("Authorization", "Bearer wrong-token");
            var res = await client.GetAsync("/api/worlds");
            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        }

        [Fact]
        /// <summary>
        /// ValidTokenDoesNotReturn401を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task ValidToken_DoesNotReturn401()
        {
            var res = await _authed.GetAsync("/api/worlds");
            Assert.NotEqual(HttpStatusCode.Unauthorized, res.StatusCode);
        }

        // ── routing ───────────────────────────────────────────────────────────

        [Fact]
        /// <summary>
        /// UnknownPathReturns404を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task UnknownPath_Returns404()
        {
            var res = await _authed.GetAsync("/api/doesnotexist");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }

        [Fact]
        /// <summary>
        /// WrongMethodReturns405を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task WrongMethod_Returns405()
        {
            var res = await _authed.DeleteAsync("/api/announce");
            Assert.Equal(HttpStatusCode.MethodNotAllowed, res.StatusCode);
        }

        // ── GET /api/worlds ───────────────────────────────────────────────────

        [Fact]
        /// <summary>
        /// GetWorldsEmptyReturnsEmptyListを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public async Task GetWorlds_Empty_ReturnsEmptyList()
        {
            var res = await _authed.GetAsync("/api/worlds");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
            Assert.Equal(0, doc.RootElement.GetProperty("worlds").GetArrayLength());
        }

        [Fact]
        /// <summary>
        /// GetWorldsReturnsOnlyScenesを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public async Task GetWorlds_ReturnsOnlyScenes()
        {
            BasisNetworkResourceManagement.UshortNetworkDatabase.TryAdd("scene1", new LocalLoadResource
                { LoadedNetID = "scene1", Mode = 1, CombinedURL = "https://example.com/world.bee", Persist = false });
            BasisNetworkResourceManagement.UshortNetworkDatabase.TryAdd("prop1", new LocalLoadResource
                { LoadedNetID = "prop1", Mode = 0, CombinedURL = "https://example.com/prop.bee", Persist = false });

            var res = await _authed.GetAsync("/api/worlds");
            var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
            var worlds = doc.RootElement.GetProperty("worlds");

            Assert.Equal(1, worlds.GetArrayLength());
            Assert.Equal("scene1", worlds[0].GetProperty("netId").GetString());
        }

        [Fact]
        /// <summary>
        /// GetWorldsFieldsAreMappedCorrectlyを取得します。通信状態や設定値を読み取り専用で参照するための入口です。
        /// </summary>
        public async Task GetWorlds_FieldsAreMappedCorrectly()
        {
            BasisNetworkResourceManagement.UshortNetworkDatabase.TryAdd("w1", new LocalLoadResource
            {
                LoadedNetID = "w1", Mode = 1,
                CombinedURL = "https://example.com/world.bee",
                Persist = true, IsAdminLocked = true, LoadStrategy = 0,
            });

            var res = await _authed.GetAsync("/api/worlds");
            var world = JsonDocument.Parse(await res.Content.ReadAsStringAsync())
                .RootElement.GetProperty("worlds")[0];

            Assert.Equal("w1", world.GetProperty("netId").GetString());
            Assert.Equal("https://example.com/world.bee", world.GetProperty("url").GetString());
            Assert.True(world.GetProperty("persistent").GetBoolean());
            Assert.True(world.GetProperty("adminLocked").GetBoolean());
            Assert.Equal(0, world.GetProperty("strategy").GetInt32());
        }

        // ── POST /api/worlds ──────────────────────────────────────────────────

        [Fact]
        /// <summary>
        /// 読み込みWorldMissingUrlReturns400を初期化します。設定、永続化ファイル、実行時キャッシュを起動時の状態へ整えます。
        /// </summary>
        public async Task LoadWorld_MissingUrl_Returns400()
        {
            var res = await PostJson("/api/worlds", """{"password":"pass"}""");
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        /// <summary>
        /// 読み込みWorldMissingPasswordReturns400を初期化します。設定、永続化ファイル、実行時キャッシュを起動時の状態へ整えます。
        /// </summary>
        public async Task LoadWorld_MissingPassword_Returns400()
        {
            var res = await PostJson("/api/worlds", """{"url":"https://example.com/w.bee"}""");
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        /// <summary>
        /// 読み込みWorldImmediateReturns200AndAddsToデータベースを初期化します。設定、永続化ファイル、実行時キャッシュを起動時の状態へ整えます。
        /// </summary>
        public async Task LoadWorld_Immediate_Returns200AndAddsToDatabase()
        {
            var res = await PostJson("/api/worlds",
                """{"url":"https://example.com/world.bee","password":"pass","strategy":"immediate"}""");

            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
            Assert.True(doc.RootElement.GetProperty("ok").GetBoolean());

            string netId = doc.RootElement.GetProperty("netId").GetString()!;
            Assert.True(BasisNetworkResourceManagement.UshortNetworkDatabase.ContainsKey(netId));
        }

        [Fact]
        /// <summary>
        /// 読み込みWorldSynchronizedReturns200AndAddsToデータベースを初期化します。設定、永続化ファイル、実行時キャッシュを起動時の状態へ整えます。
        /// </summary>
        public async Task LoadWorld_Synchronized_Returns200AndAddsToDatabase()
        {
            var res = await PostJson("/api/worlds",
                """{"url":"https://example.com/world.bee","password":"pass","strategy":"synchronized"}""");

            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
            string netId = doc.RootElement.GetProperty("netId").GetString()!;
            Assert.True(BasisNetworkResourceManagement.UshortNetworkDatabase.ContainsKey(netId));
        }

        // ── DELETE /api/worlds/{netId} ────────────────────────────────────────

        [Fact]
        /// <summary>
        /// UnloadWorldNotFoundReturns404を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task UnloadWorld_NotFound_Returns404()
        {
            var res = await _authed.DeleteAsync("/api/worlds/nonexistent-id");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }

        [Fact]
        /// <summary>
        /// UnloadWorldFoundReturns200AndRemovesFromデータベースを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task UnloadWorld_Found_Returns200AndRemovesFromDatabase()
        {
            BasisNetworkResourceManagement.UshortNetworkDatabase.TryAdd("to-delete", new LocalLoadResource
                { LoadedNetID = "to-delete", Mode = 1, CombinedURL = "https://example.com/w.bee" });

            var res = await _authed.DeleteAsync("/api/worlds/to-delete");

            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            Assert.False(BasisNetworkResourceManagement.UshortNetworkDatabase.ContainsKey("to-delete"));
        }

        // ── POST /api/announce ────────────────────────────────────────────────

        [Fact]
        /// <summary>
        /// AnnounceAllMissingメッセージReturns400を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task AnnounceAll_MissingMessage_Returns400()
        {
            var res = await PostJson("/api/announce", """{}""");
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        /// <summary>
        /// AnnounceAllEmptyメッセージReturns400を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task AnnounceAll_EmptyMessage_Returns400()
        {
            var res = await PostJson("/api/announce", """{"message":""}""");
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        /// <summary>
        /// AnnounceAllメッセージTooLongReturns400を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task AnnounceAll_MessageTooLong_Returns400()
        {
            var body = JsonSerializer.Serialize(new { message = new string('a', 513) });
            var res = await PostJson("/api/announce", body);
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        /// <summary>
        /// AnnounceAllNonStringメッセージReturns400を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task AnnounceAll_NonStringMessage_Returns400()
        {
            var res = await PostJson("/api/announce", """{"message":42}""");
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        /// <summary>
        /// AnnounceAllValidメッセージReturns200を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task AnnounceAll_ValidMessage_Returns200()
        {
            var res = await PostJson("/api/announce", """{"message":"hello world"}""");
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        }

        // ── POST /api/announce/{uuid} ─────────────────────────────────────────

        [Fact]
        /// <summary>
        /// AnnounceプレイヤーUnknownUuidReturns404を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task AnnouncePlayer_UnknownUuid_Returns404()
        {
            var res = await PostJson("/api/announce/unknown-uuid-123", """{"message":"hi"}""");
            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }

        [Fact]
        /// <summary>
        /// 読み込みWorldPasswordEmbeddedInUrlReturns200AndStoresCleanUrlを初期化します。設定、永続化ファイル、実行時キャッシュを起動時の状態へ整えます。
        /// </summary>
        public async Task LoadWorld_PasswordEmbeddedInUrl_Returns200AndStoresCleanUrl()
        {
            const string cleanUrl = "https://example.com/world.bee";
            var res = await PostJson("/api/worlds", """{"url":"https://example.com/world.bee#secretpassword"}""");

            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            string netId = JsonDocument.Parse(await res.Content.ReadAsStringAsync())
                .RootElement.GetProperty("netId").GetString()!;

            Assert.True(BasisNetworkResourceManagement.UshortNetworkDatabase.TryGetValue(netId, out var r));
            Assert.Equal(cleanUrl, r!.CombinedURL);
            Assert.Equal("secretpassword", r.UnlockPassword);
        }

        [Fact]
        /// <summary>
        /// 読み込みWorldExplicitPasswordOverridesEmbeddedを初期化します。設定、永続化ファイル、実行時キャッシュを起動時の状態へ整えます。
        /// </summary>
        public async Task LoadWorld_ExplicitPasswordOverridesEmbedded()
        {
            var res = await PostJson("/api/worlds",
                """{"url":"https://example.com/world.bee#embedded","password":"explicit"}""");

            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            string netId = JsonDocument.Parse(await res.Content.ReadAsStringAsync())
                .RootElement.GetProperty("netId").GetString()!;

            Assert.True(BasisNetworkResourceManagement.UshortNetworkDatabase.TryGetValue(netId, out var r));
            Assert.Equal("explicit", r!.UnlockPassword);
        }

        // ── POST /api/worlds/switch ───────────────────────────────────────────

        [Fact]
        /// <summary>
        /// SwitchWorldMissingUrlReturns400を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task SwitchWorld_MissingUrl_Returns400()
        {
            var res = await PostJson("/api/worlds/switch", """{"password":"pass"}""");
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        /// <summary>
        /// SwitchWorldMissingPasswordNoFragmentReturns400を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task SwitchWorld_MissingPasswordNoFragment_Returns400()
        {
            var res = await PostJson("/api/worlds/switch", """{"url":"https://example.com/next.bee"}""");
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        /// <summary>
        /// SwitchWorldValidReturns200AndAddsToデータベースを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task SwitchWorld_Valid_Returns200AndAddsToDatabase()
        {
            var res = await PostJson("/api/worlds/switch",
                """{"url":"https://example.com/next.bee","password":"pass","announceMessage":"Switching!"}""");

            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
            Assert.True(doc.RootElement.GetProperty("ok").GetBoolean());

            string netId = doc.RootElement.GetProperty("netId").GetString()!;
            Assert.True(BasisNetworkResourceManagement.UshortNetworkDatabase.ContainsKey(netId));
        }

        [Fact]
        /// <summary>
        /// SwitchWorldPasswordEmbeddedInUrlReturns200AndStoresCleanUrlを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task SwitchWorld_PasswordEmbeddedInUrl_Returns200AndStoresCleanUrl()
        {
            const string cleanUrl = "https://beefile.io/7ec036b1a8fdd4e7f439339be9cbf54d";
            const string password = "MGFmZWU0Y2ZlMjExMzlkY2Y5MDJlMjQ3NTc1ZDhiODAwODk3ZjZiZWM4NWVmMzkyODA5YTk3NDRhMjE3NTQzZQ==";
            var res = await PostJson("/api/worlds/switch",
                $$"""{"url":"{{cleanUrl}}#{{password}}"}""");

            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            string netId = JsonDocument.Parse(await res.Content.ReadAsStringAsync())
                .RootElement.GetProperty("netId").GetString()!;

            Assert.True(BasisNetworkResourceManagement.UshortNetworkDatabase.TryGetValue(netId, out var r));
            Assert.Equal(cleanUrl, r!.CombinedURL);
            // fragment password は base64-encoded なので、server は保存前に decode する。
            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(password));
            Assert.Equal(decoded, r.UnlockPassword);
        }

        [Fact]
        /// <summary>
        /// SwitchWorldInvalidDelayReturns400を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task SwitchWorld_InvalidDelay_Returns400()
        {
            var res = await PostJson("/api/worlds/switch",
                """{"url":"https://example.com/next.bee","password":"pass","delay":-1}""");
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        /// <summary>
        /// SwitchWorldDelayTooLargeReturns400を実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task SwitchWorld_DelayTooLarge_Returns400()
        {
            var res = await PostJson("/api/worlds/switch",
                """{"url":"https://example.com/next.bee","password":"pass","delay":301}""");
            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        /// <summary>
        /// SwitchWorldWithDelayNetIdReturnedImmediately読み込みDeferredを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        public async Task SwitchWorld_WithDelay_NetIdReturnedImmediatelyLoadDeferred()
        {
            // delay > 0: announce を先に送信し、cross-channel ordering を保ってから delay 後に load を開始する。
            var res = await PostJson("/api/worlds/switch",
                """{"url":"https://example.com/next.bee","password":"pass","delay":1,"announceMessage":"Loading in 1s"}""");

            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            string netId = JsonDocument.Parse(await res.Content.ReadAsStringAsync())
                .RootElement.GetProperty("netId").GetString()!;

            Assert.False(BasisNetworkResourceManagement.UshortNetworkDatabase.ContainsKey(netId),
                "load should not be in DB until delay expires");

            await Task.Delay(TimeSpan.FromMilliseconds(1500));
            Assert.True(BasisNetworkResourceManagement.UshortNetworkDatabase.ContainsKey(netId),
                "load should be in DB after delay expires");
        }

        // ── helper ───────────────────────────────────────────────────────────

        private Task<HttpResponseMessage> PostJson(string path, string json) =>
            _authed.PostAsync(path, new StringContent(json, Encoding.UTF8, "application/json"));

        /// <summary>
        /// FreePortを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
        /// </summary>
        private static ushort FreePort()
        {
            using var tmp = new TcpListener(IPAddress.Loopback, 0);
            tmp.Start();
            ushort port = (ushort)((IPEndPoint)tmp.LocalEndpoint).Port;
            tmp.Stop();
            return port;
        }
    }

    [CollectionDefinition("RestApi", DisableParallelization = true)]
    /// <summary>
    /// RestApiCollectionの責務をまとめるクラスです。
    /// RestApi.Tests領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
    /// </summary>
    public class RestApiCollection { }
}
