using Grpc.Core;
using Robotsystemcommunication;
using UnityEngine;

public class UnityBrainServerClient
{
    private readonly BrainServer.BrainServerClient _client;
    private readonly Channel _channel;

    public UnityBrainServerClient(string ip, string port) {
        _channel = new Channel(ip + ":" + port, ChannelCredentials.Insecure);
        _client = new BrainServer.BrainServerClient(_channel);
    }

    public BrainActionResponse GetAction(BrainActionRequest actionReqs) {
        var actions = _client.GetAction(actionReqs);
        return actions;
    }

    private void OnDisable() {
        _channel.ShutdownAsync().Wait();
    }
}
