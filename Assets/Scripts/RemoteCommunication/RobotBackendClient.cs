using Grpc.Core;
using Google.Protobuf;
using Robotbackendcommunication;
using UnityEngine;

public class RobotBackendClient
{
    private readonly RobotBackendCommunicator.RobotBackendCommunicatorClient _client;
    private readonly Channel _channel;

    public RobotBackendClient(string ip, string port) {
        _channel = new Channel(ip + ":" + port, ChannelCredentials.Insecure);
        _client = new RobotBackendCommunicator.RobotBackendCommunicatorClient(_channel);
    }

    public int GetAction(byte[] screencapture) {
        var action = _client.GetAction(
            new Screenshot() {Image = ByteString.CopyFrom(screencapture)});
        return action.Action;
    }

    private void OnDisable() {
        _channel.ShutdownAsync().Wait();
    }
}
