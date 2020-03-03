using Grpc.Core;
using Braincommunication;
using UnityEngine;

public class BrainClient {
    
    private readonly BrainCommunicator.BrainCommunicatorClient _client;
    private readonly Channel _channel;
    private readonly string _server = "localhost:50051";

    public BrainClient() {
        _channel = new Channel(_server, ChannelCredentials.Insecure);
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