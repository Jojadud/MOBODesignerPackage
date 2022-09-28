using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToyDesignExample : MonoBehaviour
{
    // Prefabs used to instantiate basic test interface
    public GameObject targetPrefab;
    public GameObject buttonPrefab;
    public GameObject infoTextPrefab;

    // Basic target configuration
    private int nTargets_ = 12;
    private float amplitude_ = 1.0f;
    private float width_ = 0.1f;

    // Measured design outcomes are completion time and 'touch' errors
    private float startTime_ = 0.0f;
    private List<float> errors_;

    // Current target index
    private int iTarget_ = 0;

    // Flag indicating design has been updated
    private bool designUpdated_ = false;

    // Button controllers
    private ButtonController startButtonController_, abandonButtonController_;

    // Information text
    private TextMesh infoTexMesh_;

    // Target sequence defines order in which targets are displayed
    private List<int> targetSequence = new List<int> { 0, 6, 1, 7, 2, 8, 3, 9, 4, 10, 5, 11 };

    // Start is called before the first frame update
    void Start()
    {
        // Connect MOBO Designer WebInterface OnDesignParametersUpdated action to UpdateDesign
        transform.GetComponent<MOBODesigner.MDWebInterface>().OnDesignParametersUpdated = UpdateDesign;
        
        // Instantiate targets
        for (int i = 0; i < nTargets_; i++)
        {
            GameObject targetInstance = Instantiate(targetPrefab,transform);
            targetInstance.name = string.Format("Target-{0}", i);

            TargetController targetController = targetInstance.GetComponent<TargetController>();
            targetController.id = i;
            targetController.OnSelect = TargetSelected;
        }
        HideAllTargets();

        // Create Start button
        GameObject startButtonInstance = Instantiate(buttonPrefab, transform);
        startButtonController_ = startButtonInstance.GetComponent<ButtonController>();
        startButtonController_.transform.localPosition = new Vector3(-1.5f, -2.0f);
        startButtonController_.SetText("Start");
        startButtonController_.OnClick = StartTask;
        startButtonController_.SetDisabled(true);

        // Create Abandon button
        GameObject abandonButtonInstance = Instantiate(buttonPrefab, transform);
        abandonButtonController_ = abandonButtonInstance.GetComponent<ButtonController>();
        abandonButtonController_.transform.localPosition = new Vector3(1.5f, -2.0f);
        abandonButtonController_.SetText("Abandon");
        abandonButtonController_.OnClick = AbandonTask;
        abandonButtonController_.SetDisabled(true);

        // Create Information text
        GameObject infoTextInstance = Instantiate(infoTextPrefab, transform);
        infoTextInstance.transform.localPosition = new Vector3(0f, -3.0f);
        infoTexMesh_ = infoTextInstance.GetComponent<TextMesh>();
        infoTexMesh_.text = "Waiting for new design...";

        errors_ = new List<float>();
    }

    // Update is called once per frame
    void Update()
    {
        // If UpdateDesign has been triggered by the WebInterface
        if (designUpdated_)
        {
            // Set Information text
            infoTexMesh_.text = string.Format("New Design: {0:F2}, {1:F2}", amplitude_, width_);

            // Reset flag
            designUpdated_ = false;

            // Update targets based on new design
            for (int i = 0; i < nTargets_; i++)
            {
                string targetName = string.Format("Target-{0}", i);                
                Transform targetInstance = transform.Find(targetName);

                float theta = Mathf.PI / 2 - (i * (2 * Mathf.PI) / (float)nTargets_);
                float xOffset = amplitude_ * Mathf.Cos(theta);
                float yOffset = amplitude_ * Mathf.Sin(theta);

                targetInstance.transform.localPosition = new Vector3(xOffset, yOffset);
                targetInstance.transform.localScale = new Vector3(width_, 0.01f, width_);

                targetInstance.gameObject.SetActive(false);
            }

            HideAllTargets();

            // Enable start button
            startButtonController_.SetDisabled(false);
        }
    }

    public void UpdateDesign(List<float> paramVals)
    {
        // New design received from WebInterface
        Debug.Log("New Design: " + string.Join(",", paramVals));
        
        // Check that we have right number of parameters given current design problem
        if (paramVals.Count != 2)
        {
            Debug.LogError("This toy design problem assumes only 2 parameters");
            return;
        }

        // Set design paramters (used in Update to update the design)
        amplitude_ = paramVals[0];
        width_ = paramVals[1];

        // Note that we cannot update scene objects as this may be called outside main game loop
        // Could use a dispatcher but checking for flag in Update is a simple solution
        designUpdated_ = true;        
    }

    private void HideAllTargets()
    {
        for (int i = 0; i < nTargets_; i++)
        {
            string targetName = string.Format("Target-{0}", i);
            Transform targetInstance = transform.Find(targetName);
            targetInstance.gameObject.SetActive(false);
        }
    }

    private void TargetSelected(int targetId, float error)
    {
        // Increment target index
        iTarget_++;

        // Append 'touch' error to current list
        errors_.Add(error);

        // Get target name based in selected targetId
        string targetName = string.Format("Target-{0}", targetId);
        Debug.Log("Target selected: " + targetName);

        // Disable selected target
        transform.Find(targetName)?.gameObject.SetActive(false);

        // Show next target or end
        if (iTarget_ == nTargets_)
        {
            TaskComplete();
        }
        else
        {
            ShowTarget(targetSequence[iTarget_]);
        }        
    }

    private void StartTask()
    {
        // Set start time
        startTime_ = Time.time;

        // Reset errors list
        errors_ = new List<float>();

        // Reset target index
        iTarget_ = 0;
                
        HideAllTargets();

        // Disable Start button and enable and enable Abandon button
        startButtonController_.SetDisabled(true);
        abandonButtonController_.SetDisabled(false);

        // Show first target
        ShowTarget(targetSequence[iTarget_]);        
    }

    private void ShowTarget(int id)
    {
        // Retrieve target by id and show
        string targetName = string.Format("Target-{0}", id);
        transform.Find(targetName)?.gameObject.SetActive(true);
    }

    private void AbandonTask()
    {        
        HideAllTargets();

        // Get number of targets actually selected
        int nTargets = errors_.Count;

        // If abandoned before any selected then return
        if (nTargets == 0)
        {
            return;
        }

        // Compute total time and mean target selection time
        float completionTime = Time.time - startTime_;
        float meanSelectionTime = completionTime / nTargets;

        // Compute mean offset error (touch point from centre of target in screen coordinates)
        float errorSum = 0.0f;
        foreach (float errorVal in errors_)
        {
            errorSum += errorVal;
        }
        float meanError = errorSum / (float)nTargets;

        // Get target radius in screen coordinates
        float wScreen = Camera.main.WorldToScreenPoint(new Vector3(width_ / 2.0f, 0, 0)).x;
        float oScreen = Camera.main.WorldToScreenPoint(new Vector3(0, 0, 0)).x;
        wScreen = wScreen - oScreen;

        // Selection Rate: selections per second
        float obj1 = 1 / meanSelectionTime;

        // Selection Accuracy: normalized promiximity to target centre
        float obj2 = 1 - (meanError / wScreen);

        // Combine objectives into objectives list
        List<float> objVals = new List<float> { obj1, obj2 };

        // Second argument, [formal] set to false to indicate an abandoned evaluation
        transform.GetComponent<MOBODesigner.MDWebInterface>().EvaluationComplete(objVals,false);

        // Disable both buttons and update information text
        startButtonController_.SetDisabled(true);
        abandonButtonController_.SetDisabled(true);
        infoTexMesh_.text = "Waiting for new design...";
    }

    private void TaskComplete()
    {
        // Compute total time and mean target selection time
        float completionTime = Time.time - startTime_;
        float meanSelectionTime = completionTime / (float)nTargets_;

        // Compute mean offset error (touch point from centre of target in screen coordinates)
        float errorSum = 0.0f;
        foreach (float errorVal in errors_)
        {
            errorSum += errorVal;
        }
        float meanError = errorSum / (float)nTargets_;
        
        // Get target radius in screen coordinates
        float wScreen = Camera.main.WorldToScreenPoint(new Vector3(width_ / 2.0f, 0, 0)).x;
        float oScreen = Camera.main.WorldToScreenPoint(new Vector3(0, 0, 0)).x;
        wScreen = wScreen - oScreen;

        // Selection Rate: selections per second
        float obj1 = 1 / meanSelectionTime;

        // Selection Accuracy: normalized promiximity to target centre
        float obj2 = 1 - (meanError / wScreen);

        // Combine objectives into objectives list
        List<float> objVals = new List<float> { obj1, obj2 };
                
        // Send evaluation results back to the WebInterface
        transform.GetComponent<MOBODesigner.MDWebInterface>().EvaluationComplete(objVals);

        // Disable both buttons and update information text
        startButtonController_.SetDisabled(true);
        abandonButtonController_.SetDisabled(true);
        infoTexMesh_.text = "Waiting for new design...";
    }
}
