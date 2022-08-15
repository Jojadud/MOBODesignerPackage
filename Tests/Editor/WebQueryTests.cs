using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MOBODesigner.Tests
{
    public class WebQueryTests
    {

        [UnityTest]
        public IEnumerator TestInitializeListener()
        {
            MDClient mdClient = new MDClient(1);

            mdClient.InitializeListener("http://127.0.0.1","4444");

            mdClient.TerminateListener();

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestSendData()
        {
            MDClient mdClient = new MDClient(1);

            List<float> paramVals = new List<float> { 0.75f, .0f, -0.75f, 0.5f, -1.0f };
            List<float> objVals = new List<float> { 0.4f, 0.8f };

            mdClient.SendPerformanceResult(paramVals, objVals, true);

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
