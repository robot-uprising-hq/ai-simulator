using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Grpc.Core;
using Google.Protobuf;
using Robotsystemcommunication;

namespace MLAgents.Sensor
{
    public class UnitySimulationServer : MonoBehaviour
    {
        public RemoteAgent remoteAgent;

        [Space(10)]
        public int port = 50051;

        private Server server;
        private ScreenStreamer screenStreamer;

        void Start()
        {
            screenStreamer = FindObjectOfType<ScreenStreamer>();
            StartServer();
        }

        void OnDisable()
        {
            StopServer();
        }

        private void StartServer()
        {
            SimulationServerImpl simulationServerImpl = new SimulationServerImpl();
            simulationServerImpl.remoteAgent = remoteAgent;
            simulationServerImpl.screenStreamer = screenStreamer;

            server = new Server
                {
                    Services = { SimulationServer.BindService(simulationServerImpl) },
                    Ports = { new ServerPort("localhost", port, ServerCredentials.Insecure) }
                };
            server.Start();
            Debug.Log("Server started");
        }

        private void StopServer()
        {
            Debug.Log("Stopping server");
            server.ShutdownAsync().Wait();
        }

        class SimulationServerImpl : SimulationServer.SimulationServerBase
        {
            public RemoteAgent remoteAgent;
            public ScreenStreamer screenStreamer;
            // public Camera camera;

            private Rect rect;
            private RenderTexture renderTexture;
            private Texture2D screenShot;
            public SimulationServerImpl() : base() {}

            public override Task<SimulationScreenCaptureResponse> GetScreenCapture(SimulationScreenCaptureRequest req, ServerCallContext context)
            {
                screenStreamer.captureWidth = req.Widht;
                screenStreamer.captureHeight = req.Height;
                screenStreamer.imageType = req.ImageType;
                screenStreamer.jpegQuality = req.JpgQuality;

                byte[] image = screenStreamer.latestScreenCapture;
                return Task.FromResult(new SimulationScreenCaptureResponse { Image = ByteString.CopyFrom(image) });
            }

            public override Task<SimulationActionResponse> MakeAction(SimulationActionRequest req, ServerCallContext context)
            {
                int action = req.Action;

                remoteAgent.OnActionReceived(new float[] {action});
                //remoteAgent.agentAction = action;

                return Task.FromResult(new SimulationActionResponse { Status = StatusType.Ok });
            }
        }
    }
}