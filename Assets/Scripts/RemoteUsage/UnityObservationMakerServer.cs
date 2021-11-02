using System;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Grpc.Core;
using Google.Protobuf;
using Robotsystemcommunication;
using Unity.MLAgents;

namespace MLAgents.Sensor
{
    public class UnityObservationMakerServer : MonoBehaviour
    {
        public List<MainThreadUpdater> m_FriendlyAgents = new List<MainThreadUpdater>();
        public List<MainThreadUpdater> m_EnemyAgents = new List<MainThreadUpdater>();
        public List<MainThreadUpdater> m_NegativeECores = new List<MainThreadUpdater>();
        public List<MainThreadUpdater> m_PositiveECores = new List<MainThreadUpdater>();

        [Space(10)]
        public int port = 50055;

        [Space(10)]
        public int m_OpenCvXCoordinateMax = 1232;
        public int m_OpenCvYCoordinateMax = 1232;
        public float m_UnityArenaXCoordinateMax = 1.7f;
        public float m_UnityArenaYCoordinateMax = 1.7f;

        private Server server;
        private ScreenStreamer screenStreamer;

        void Start()
        {
            Academy.Instance.Dispose();
            screenStreamer = FindObjectOfType<ScreenStreamer>();
            StartServer();
        }

        void OnDisable()
        {
            StopServer();
        }

        private void StartServer()
        {
            ObservationMakerServermIpl obsMakerServerImpl = new ObservationMakerServermIpl(
                m_FriendlyAgents,
                m_EnemyAgents,
                m_PositiveECores,
                m_NegativeECores,
                screenStreamer,
                m_OpenCvXCoordinateMax,
                m_OpenCvYCoordinateMax,
                m_UnityArenaXCoordinateMax,
                m_UnityArenaYCoordinateMax
            );

            server = new Server
                {
                    Services = { ObservationMakerServer.BindService(obsMakerServerImpl) },
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

        class ObservationMakerServermIpl : ObservationMakerServer.ObservationMakerServerBase
        {
            List<MainThreadUpdater> m_FriendlyAgents;
            List<MainThreadUpdater> m_EnemyAgents;
            List<MainThreadUpdater> m_NegativeECores;
            List<MainThreadUpdater> m_PositiveECores;
            ScreenStreamer m_ScreenStreamer;
            int m_OpenCvXCoordinateMax;
            int m_OpenCvYCoordinateMax;
            float m_UnityArenaXCoordinateMax;
            float m_UnityArenaYCoordinateMax;

            public ObservationMakerServermIpl(
                List<MainThreadUpdater> friendlyAgents,
                List<MainThreadUpdater> enemyAgents,
                List<MainThreadUpdater> negativeECores,
                List<MainThreadUpdater> positiveECores,
                ScreenStreamer screenStreamer,
                int openCvXCoordinateMax,
                int openCvYCoordinateMax,
                float unityArenaXCoordinateMax,
                float unityArenaYCoordinateMax
            ) : base() {
                this.m_FriendlyAgents = friendlyAgents;
                this.m_EnemyAgents = enemyAgents;
                this.m_NegativeECores = negativeECores;
                this.m_PositiveECores = positiveECores;
                this.m_ScreenStreamer = screenStreamer;
                this.m_OpenCvXCoordinateMax = openCvXCoordinateMax;
                this.m_OpenCvYCoordinateMax = openCvYCoordinateMax;
                this.m_UnityArenaXCoordinateMax = unityArenaXCoordinateMax;
                this.m_UnityArenaYCoordinateMax = unityArenaYCoordinateMax;
            }

            class ModifiedTransform{
                public float xCoordinate;
                public float yCoordinate;
                public float Rotation;
            }

            public override Task<ObservationResponse> GetObservations(ObservationRequest req, ServerCallContext context)
            {
                try
                {
                    // Move enemy robots to their new positions and rotations
                    SetGameObjectTransforms(req.EnemyRobotTransforms, m_EnemyAgents);

                    // Move negative energy cores to their new positions
                    SetGameObjectTransforms(req.NegativeEnergyCoreTransforms, m_NegativeECores);

                    // Move negative energy cores to their new positions
                    SetGameObjectTransforms(req.PositiveEnergyCoreTransforms, m_PositiveECores);

                    // Move friendly robots to their new positions and rotations
                    var newFriendlyTrans = new Google.Protobuf.Collections.RepeatedField<Robotsystemcommunication.Transform>();
                    foreach (var obj in req.FriendlyRobotTransforms) {
                        newFriendlyTrans.Add(obj.Transform);
                    }
                    SetGameObjectTransforms(newFriendlyTrans, m_FriendlyAgents);

                    // Make observations
                    foreach (var agent in m_FriendlyAgents) {
                        if (agent.GetActiveState() == true)
                            agent.MakeObservations();
                    }
                    Thread.Sleep(100);

                    // Get observations
                    var activeFriendlyRobots = req.FriendlyRobotTransforms.Count;
                    var obsRes = new ObservationResponse();
                    for (var i = 0; i < activeFriendlyRobots; i++) {
                        var obs = new Observations(){};
                        obs.LowerObservations.AddRange(m_FriendlyAgents[i].GetLowerObservations());
                        obs.UpperObservations.AddRange(m_FriendlyAgents[i].GetUpperObservations());
                        obs.ArucoMarkerID = req.FriendlyRobotTransforms[i].ArucoMarkerID;
                        obsRes.Observations.Add(obs);
                    }
                    obsRes.GameImage = ByteString.CopyFrom(m_ScreenStreamer.latestScreenCapture);
                    return Task.FromResult(obsRes);
                }
                catch (Exception e)
                {
                    Debug.Log("Exception caught:\n" + e.ToString());
                    var obsRes = new ObservationResponse();
                    return Task.FromResult(obsRes);
                }
            }

            private void SetGameObjectTransforms(
                Google.Protobuf.Collections.RepeatedField<Robotsystemcommunication.Transform> newTransforms,
                List<MainThreadUpdater> gos
            ) {
                var activeGos = newTransforms.Count;
                for (var i = 0; i < gos.Count; i++) {
                    if (i < activeGos) {
                        var modTrans = ModifyTransform(newTransforms[i]);
                        
                        var newPos = new Vector3(
                            modTrans.xCoordinate,
                            gos[i].GetPosition().y,
                            modTrans.yCoordinate);

                        var oldRot = gos[i].GetRotation();
                        var newRot = new Vector3(
                            oldRot.x,
                            modTrans.Rotation,
                            oldRot.z
                        );
                        gos[i].SetTransformToBeUpdated(newPos, Quaternion.Euler(newRot));
                        gos[i].SetActiveState(true);
                    }
                    else {
                        var newPos = new Vector3(500f, 500f, 500f);
                        var newRot = new Quaternion();
                        gos[i].SetTransformToBeUpdated(newPos, newRot);
                        gos[i].SetActiveState(false);
                    }
                }
            }

            // Changes coordinates from OpenCV's pixel coordinates to Unity coordinates.
            // In OpenCV the upper left corner is (0,0) and lower right corner is (MAX_X, MAX_Y)
            // In Unity the upper left corner is Vector3(-MAX_X/2, 0, +MAX_X/2) and lower right corner is Vector3(+MAX_X/2, 0, -MAX_X/2)
            private ModifiedTransform ModifyTransform(
                Robotsystemcommunication.Transform newTransforms
            ) {
                float xUnity = (newTransforms.XPosition / m_OpenCvXCoordinateMax) * m_UnityArenaXCoordinateMax - m_UnityArenaXCoordinateMax / 2;
                float yUnity = m_UnityArenaYCoordinateMax / 2 - (newTransforms.YPosition / m_OpenCvYCoordinateMax) * m_UnityArenaYCoordinateMax;
                
                var mods = new ModifiedTransform();
                mods.xCoordinate = xUnity;
                mods.yCoordinate = yUnity;
                mods.Rotation = newTransforms.Rotation;
                
                return mods;
            }
        }
    }
}
