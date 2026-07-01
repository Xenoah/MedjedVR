using Basis.Network.Core;

using static Basis.Network.Core.Serializable.SerializableBasis;
using static SerializableBasis;
/// <summary>
/// ネットワーククライアントの責務をまとめるクラスです。
/// Client領域で使われる状態、通信処理、またはデータ表現を一か所に集約します。
/// </summary>
public class NetworkClient
{
    /// <summary>
    /// clientを保持します。型は NetManager で、関連処理から共有される値です。
    /// </summary>
    public  NetManager client;
    /// <summary>
    /// listenerを保持します。型は EventBasedNetListener で、関連処理から共有される値です。
    /// </summary>
    public EventBasedNetListener listener;
    private NetPeer peer;
    private bool IsInUse;
    /// <summary>
    /// 初期 data は通常、接続時に server へ渡す ready payload。
    /// </summary> 
    /// <param name="IP"></param>
    /// <param name="port"></param>
    /// <param name="ReadyMessage"></param>
    public NetPeer StartClient(string IP, int port, ReadyMessage ReadyMessage, byte[] AuthenticationMessage, Configuration Configuration, bool manualMode = false)
    {
        if (IsInUse == false)
        {
            listener = new EventBasedNetListener();
            client = BasisNetworkStackRegistry.Create(Configuration.NetworkStackId, listener, Configuration);
            if (manualMode)
                client.StartManual();
            else
                client.Start();
            NetDataWriter Writer = new NetDataWriter(true,12);
            // key を入れないのはこの時だけ。
            Writer.Put(BasisNetworkVersion.ServerVersion);
            BytesMessage AuthBytes = new BytesMessage();
            AuthBytes.Serialize(Writer, AuthenticationMessage);
            ReadyMessage.Serialize(Writer);
            peer = client.Connect(IP, port, Writer);
            IsInUse = true;
            return peer;
        }
        else
        {
            BNL.LogError("Call Shutdown First!");
            return null;
        }
    }
    /// <summary>
    /// Pollを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
    /// </summary>
    public void Poll()
    {
        client?.PollEvents();
    }
    /// <summary>
    /// Updateを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
    /// </summary>
    public void Update(float elapsedMilliseconds)
    {
        client?.ManualUpdate(elapsedMilliseconds);
    }
    /// <summary>
    /// Disconnectを実行します。呼び出し元から渡された情報を基に、この型が担当する処理を進めます。
    /// </summary>
    public void Disconnect()
    {
        IsInUse = false;
        BNL.Log("Client Called Disconnect from server");
        peer?.Disconnect();
        client?.Stop();

        BNL.Log("Worker thread stopped.");
    }
}
