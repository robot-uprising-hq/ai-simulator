using Grpc.Core;
using Braincommunication;
using UnityEngine;

public class BrainServerClient
{
    
    private readonly BrainCommunicator.BrainCommunicatorClient _client;
    private readonly Channel _channel;

    public BrainServerClient(string ip, string port) {
        _channel = new Channel(ip + ":" + port, ChannelCredentials.Insecure);
        _client = new BrainCommunicator.BrainCommunicatorClient(_channel);
    }

    public int GetAction(float[] lowerObservations, float[] upperObservations) {
        Observations obs = new Observations();
        obs.LowerObservations.AddRange(lowerObservations);
        obs.UpperObservations.AddRange(upperObservations);

        var action = _client.GetAction(obs);

        return action.Action;
    }

    private void OnDisable() {
        _channel.ShutdownAsync().Wait();
    }
}