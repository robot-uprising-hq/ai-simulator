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

    public int GetAction(float[] lowerObservations, float[] upperObservations) {
        BrainActionRequest obs = new BrainActionRequest();
        obs.LowerObservations.AddRange(lowerObservations);
        obs.UpperObservations.AddRange(upperObservations);

        var action = _client.GetAction(obs);

        return action.Action;
    }

    private void OnDisable() {
        _channel.ShutdownAsync().Wait();
    }
}