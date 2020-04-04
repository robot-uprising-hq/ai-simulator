// using UnityEngine;
// using System.Threading;
// using System.Threading.Tasks;
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using Grpc.Core;
// using Robotbackendcommunication;

// namespace MLAgents.Sensor
// {
//     public class ImageStreamServer : MonoBehaviour
//     {
//         public ScreenStreamer screenStreamer;

//         [Space(10)]
//         public int Port = 50051;

//         private Server server;

//         void Start()
//         {
//             StartServer();
//         }

//         private void StartServer()
//         {
//             RobotBackendCommunicatorImpl robotBackendCommunicatorImpl = new RobotBackendCommunicatorImpl(
//                 screenStreamers);
//             robotBackendCommunicatorImpl.screenStreamer = screenStreamer;

//             server = new Server
//                 {
//                     Services = { RobotBackendCommunicator.BindService(robotBackendCommunicatorImpl) },
//                     Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
//                 };
//             server.Start();
//             Debug.Log("Server started");
//         }

//         private void StopServer()
//         {
//             server.ShutdownAsync().Wait();
//         }

//         class RobotBackendCommunicatorImpl : RobotBackendCommunicator.RobotBackendCommunicatorBase
//         {
//             public RemoteAgent remoteAgent;

//             private float[] lowerObservations;
//             private float[] upperObservations;

//             public RobotBackendCommunicatorImpl(ScreenStreamer screenStreamer) : base()
//             {
//                 this.screenStreamer = screenStreamer;
//             }

//             public override Task<AgentAction> GetAction(Observations observations, ServerCallContext context)
//             {
//                 var lowerObsList = new List<float>();
//                 lowerObsList.AddRange(observations.LowerObservations);
//                 var upperObsList = new List<float>();
//                 upperObsList.AddRange(observations.UpperObservations);
                
//                 // Set observations to remote agent.
//                 remoteAgent.SetObservations(lowerObsList.ToArray(), upperObsList.ToArray());
//                 remoteAgent.RequestDecision();

//                 int action = remoteAgent.GetDecidedAction();

//                 // Send remote agents action back.
//                 return Task.FromResult(new AgentAction { Action = action });
//             }
//         }
//     }
// }