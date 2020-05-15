using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace Unity.MLAgents
{
    /// <summary>
    /// The DecisionRequester component automatically request decisions for an
    /// <see cref="Agent"/> instance at regular intervals.
    /// </summary>
    /// <remarks>
    /// Attach a DecisionRequester component to the same [GameObject] as the
    /// <see cref="Agent"/> component.
    ///
    /// The DecisionRequester component provides a convenient and flexible way to
    /// trigger the agent decision making process. Without a DecisionRequester,
    /// your <see cref="Agent"/> implmentation must manually call its
    /// <seealso cref="Agent.RequestDecision"/> function.
    /// </remarks>
    // [AddComponentMenu("ML Agents/Decision Requester", (int)MenuGroup.Default)]
    [RequireComponent(typeof(Agent))]
    public class DecisionRequesterWithFPS : MonoBehaviour
    {
        /// <summary>
        /// The frequency with which the agent requests a decision. A DecisionPeriod of 5 means
        /// that the Agent will request a decision every 5 Academy steps. /// </summary>
        [Range(1, 20)]
        [Tooltip("The frequency with which the agent requests a decision. A DecisionPeriod " +
                 "of 5 means that the Agent will request a decision every 5 Academy steps.")]
        public int DecisionPeriod = 5;

        [Space(10)]
        public bool m_ShowFPS;

        /// <summary>
        /// Indicates whether or not the agent will take an action during the Academy steps where
        /// it does not request a decision. Has no effect when DecisionPeriod is set to 1.
        /// </summary>
        [Tooltip("Indicates whether or not the agent will take an action during the Academy " +
                 "steps where it does not request a decision. Has no effect when DecisionPeriod " +
                 "is set to 1.")]
        [FormerlySerializedAs("RepeatAction")]
        public bool TakeActionsBetweenDecisions = true;

        [NonSerialized]
        Agent m_Agent;

        private Text m_FPSText;
        private float m_CurrentTime;
        private float m_LastDecisionTime = 0;
        private string m_FPSStr;

        internal void Awake()
        {
            m_Agent = gameObject.GetComponent<Agent>();
            Debug.Assert(m_Agent != null, "Agent component was not found on this gameObject and is required.");
            Academy.Instance.AgentPreStep += MakeRequests;
        }

        void OnDestroy()
        {
            if (Academy.IsInitialized)
            {
                Academy.Instance.AgentPreStep -= MakeRequests;
            }
        }

        /// <summary>
        /// Method that hooks into the Academy in order inform the Agent on whether or not it should request a
        /// decision, and whether or not it should take actions between decisions.
        /// </summary>
        /// <param name="academyStepCount">The current step count of the academy.</param>
        void MakeRequests(int academyStepCount)
        {
            m_CurrentTime = Time.time;
            m_FPSStr = "";

            if (academyStepCount % DecisionPeriod == 0)
            {
                m_Agent?.RequestDecision();
                m_FPSStr = m_FPSStr + "DecisionFPS: " + Mathf.FloorToInt(1 / (m_CurrentTime - m_LastDecisionTime));
                m_LastDecisionTime = m_CurrentTime;
                
            }
            if (TakeActionsBetweenDecisions)
            {
                m_Agent?.RequestAction();
            }

            if (m_ShowFPS && m_FPSStr != "")
            {
                if (m_FPSText == null)
                    m_FPSText = GameObject.FindWithTag("fps_text")?.GetComponent<Text>();
                m_FPSText.text = m_FPSStr;
            }
            else if (!m_ShowFPS && m_FPSText != null)
            {
                m_FPSText.text = "";
            }
        }
    }
}
