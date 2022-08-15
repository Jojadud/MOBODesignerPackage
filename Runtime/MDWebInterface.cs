using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MOBODesigner
{
    public class MDWebInterface : MonoBehaviour
    {
        public int participantId = 0;
        public string clientAddress = "http://127.0.0.1";
        public string port = "4444";

        // Action called when new design parameters are received
        public Action<List<float>> OnDesignParametersUpdated;

        private List<float> curParamVals_;
        private MDClient mdClient_;

        // Start is called before the first frame update
        void Start()
        {
            curParamVals_ = new List<float> { 0f, 0f, 0f, 0f, 0f };

            // Initialize client
            mdClient_ = new MDClient(participantId);
            mdClient_.InitializeListener(clientAddress, port);

            mdClient_.OnNewDesignReceived = DesignParametersUpdated;
        }

        private void OnDestroy()
        {
            mdClient_.TerminateListener();
        }

        // Submit evaluation results
        public void EvaluationComplete(List<float> objVals, bool formal = true)
        {
            // Send performance results to client
            mdClient_.SendPerformanceResult(curParamVals_, objVals, formal);
        }

        private void DesignParametersUpdated(List<float> paramVals)
        {
            curParamVals_ = paramVals;

            OnDesignParametersUpdated?.Invoke(curParamVals_);
        }
    }
}
