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
        public List<RemoteAIRobotAgent> m_AgentList = new List<RemoteAIRobotAgent>();

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

        Dictionary<int, RemoteAIRobotAgent> ListToDict(List<RemoteAIRobotAgent> agentList)
        {
            Dictionary<int, RemoteAIRobotAgent> agentDict = new Dictionary<int, RemoteAIRobotAgent>(); 

            foreach (var agent in agentList)
            {
                agentDict.Add(agent.m_ArucoMarkerID, agent);
            }
            return agentDict;
        }

        private void StartServer()
        {
            SimulationServerImpl simulationServerImpl = new SimulationServerImpl();
            simulationServerImpl.agentDict = ListToDict(m_AgentList);
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
            public Dictionary<int, RemoteAIRobotAgent> agentDict;
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
                var actions = req.Actions;

                foreach (var action in actions)
                {
                    agentDict[action.ArucoMarkerID].agentAction = action.Action;
                }

                return Task.FromResult(new SimulationActionResponse { Status = StatusType.Ok });
            }
        }
    }
}