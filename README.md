# MOBO Designer Unity Integration Package

## Configuring Web Interface

### Setup

1. Navigate in browser to: https://www.jjdudley.com/mobo/
2. Click 'New ID' to obtain a unique participant ID
3. Select 'Hybrid' in the 'Condition' dropdown
4. Select 'Custom' in the 'Application' dropdown
5. Click 'Enter'

### Configure Design Problem

*Note: currently the interface is limited to use of exactly 2 objectives and 1-5 design parameters*

1. Add the design parameters (specify name of each parameter as well as upper and lower bound)
2. Add the design objectives (specify name of each objective as well as upper and lower bound)
3. Click 'Finish'

### Specify Client Address and Port

*Note: this configuration assumes that the Web Interface and Unity application will run on the same computer*

1. Specify IP Address: 127.0.0.1 (if on same computer)
2. Specify port: 4444 (default used in Unity package but can be modified)

## Configuring Unity Project

### Importing Package into a Unity project

*Note: tested with Unity 2019.4.29f1*

1. Open Unity project in which you want to use MOBO Designer
2. Import MOBO Designer package:
    1. Open Unity's Package Manager: Window > Package Manager
    2. Click '+' at top left of the Package Manager and select "Add package from git URL"
    3. Paste in: https://github.com/Jojadud/MOBODesignerPackage.git
    4. Click 'Add'
3. [Optional] With the new package selected in the Package Manager, you can import the 'ToyDesignPrefab' sample

### Integrating with a Design Task

Integration with a custom design task in Unity involves:
1. Listening to one action that is called when new design parameters are received from the Web Interface: `MOBODesigner.MDWebInterface.OnDesignParametersUpdated(List<float> paramValues)`
2. Calling one method when the design evaluation is complete: `MOBODesigner.MDWebInterface.EvaluationComplete(List<float> objValues, bool formal = true)`

Integration requires adding the `MDWebInterface.cs` script to the same GameObject that has the main interaction behavior script (i.e. the script governing the behavior of the interaction that is to be designed) attached. Let us call this main interaction behavior script `MyInteractionDesign.cs` and the main GameObject it is attached to in the scene `MyInteractionGameObject`.

Follow the following steps to integrate with a custom design task

1. Attach the `MDWebInterface` script to `MyInteractionGameObject`: within the inspector, Add Component > Scripts > MOBODesigner > MDWebInterface
2. Set the **Participant Id** value on the `MDWebInterface` inspector to match the unique participant ID obtained under Configure Web Interface > Setup (above).
3. Leave **Client Address** and **Port** unchanged assuming Web Interface and Unity application are running on the same computer.
4. Within `MyInteractionDesign.cs` add the following:
    1. Create a method that takes a `List<float>` where the list size is equal to the number of design parameters. This method should update the interface/interaction being designed based on the new parameters supplied. Let us call this new method `UpdateDesign(List<float> paramVals)`.
    2. Assign this new method to `MDWebInterface`'s `OnDesignParametersUpdated` Action within the `Start()` method of `MyInteractionDesign.cs`
        ```
        void Start() 
        {
            transform.GetComponent<MOBODesigner.MDWebInterface>().OnDesignParametersUpdated = UpdateDesign;
        }
        ```
    3. Add a to call `MDWebInterface`'s `EvaluationComplete(List<float> objValues, bool formal = true)` method when the evaluation of the current design parameters is complete. Note that if the evaluation is an incomplete test as opposed to a full evaluation, set `formal` equal to `false`.
        ```
        List<float> objVals = new List<float> { resultObj1, resultObj2 };
        bool formal = true;
        transform.GetComponent<MOBODesigner.MDWebInterface>().EvaluationComplete(objVals,formal);
        ```
    

## Running a Design Exercise

1. Press 'Play' in the Unity Editor
2. Adjust the design parameters in the Web Interface and then click the 'Send Design' button
3. Confirm that the design is updated in Unity and complete an evaluation to measure the performance objectives
4. Upon completion of the evaluation, the resultant objective values will be sent to the Web Interface (assuming the `EvaluationComplete(List<float> objValues, bool formal = true)` method is being called correctly)
5. Review results in the Web Interface and repeat Steps 2-4 until desired number of Pareto optimal designs have been obtained
