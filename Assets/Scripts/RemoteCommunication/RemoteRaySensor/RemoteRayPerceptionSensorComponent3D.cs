using System;
using UnityEngine;

namespace MLAgents.Sensor
{
    // [AddComponentMenu("ML Agents/Remote Ray Perception Sensor 3D", (int)MenuGroup.Sensors)]
    public class RemoteRayPerceptionSensorComponent3D : RemoteRayPerceptionSensorComponentBase
    {
        [Header("3D Properties", order = 100)]
        [Range(-10f, 10f)]
        [Tooltip("Ray start is offset up or down by this amount.")]
        public float startVerticalOffset;

        [Range(-10f, 10f)]
        [Tooltip("Ray end is offset up or down by this amount.")]
        public float endVerticalOffset;

        public override RemoteRayPerceptionSensor.CastType GetCastType()
        {
            return RemoteRayPerceptionSensor.CastType.Cast3D;
        }

        public override float GetStartVerticalOffset()
        {
            return startVerticalOffset;
        }

        public override float GetEndVerticalOffset()
        {
            return endVerticalOffset;
        }
    }
}
