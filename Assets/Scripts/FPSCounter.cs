using System;
using UnityEngine;
using UnityEngine.UI;

namespace MLAgents
{
    /// <summary>
    /// Calculates the frequency in which new decisions are taken from the brain.
    /// </summary>
    public class FPSCounter : MonoBehaviour
    {
        public DecisionRequester decisionRequester;

        public Text decisionFpsText;
        public Text gameFpsText;
        private float lastDecisionTime;
        private int count = 0;

        int m_Offset;
        public void Awake()
        {
            m_Offset = decisionRequester.offsetStep ? gameObject.GetInstanceID() : 0;
        }

        private void FixedUpdate()
        {
            float gameFps = 1f / Time.unscaledDeltaTime;
            gameFpsText.text = "FPS: " + String.Format("{0:0.00}", gameFps);

            if ((count + m_Offset) % decisionRequester.DecisionPeriod == 0)
            {
                float decisionFps = 1f / lastDecisionTime;
                decisionFpsText.text = "FPS: " + String.Format("{0:0.00}", decisionFps);
                lastDecisionTime = 0.0f;
            }
            else
            {
                lastDecisionTime += Time.unscaledDeltaTime;
            }
            count++;
        }
    }
}
