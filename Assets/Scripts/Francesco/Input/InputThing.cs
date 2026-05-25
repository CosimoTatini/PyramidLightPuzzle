using UnityEngine;
using UnityEngine.InputSystem;

public class InputThing : MonoBehaviour
{
    // Button to add an input map
    // an Enum to select the input map is besides the add button, so enum on the <,> button
    // check if the input map is already added, if so disable the add button
    // remove option is on the right of each input map
    // the input map is displayed as a Header with the name of the input map, and under
    // there is a foldout with the list of actions
    // for each action there is the option to enable/disable it
    // 

    InputAction action;
    InputActionMap actionMap;
    InputActionAsset actionAsset;
    public InputActionProperty inputActionProperty;

    public void Ac()
    {
        InputSystem_Actions inputSystem_Actions = new InputSystem_Actions();
        actionAsset = inputSystem_Actions.asset;
        actionAsset.actionMaps[0].Enable();
        inputSystem_Actions.Disable();
        inputSystem_Actions.FindAction().enabled;
        actionAsset.find
        actionMap.id;
        action.id;
    }

    /*
     * critically consider my idea, no sugar-coating, for an Editor that lets the user make SO that are configurations for the actions available:
-  PopUp or ObjectField for InputActionAsset, this is not saved, just used in the editor for displaying the maps and  thus their actions
- under that, after choosing one InputActionAsset, I display another PopUp to choose the map, besides this an Add button, so the currently chosen map is saved in the SO as a Class => { InputActionMap, List<struct{InputAction, bool active}>} and the Class will be a List<Class> so for each map, I have a List of all actions together with their enabled status, next to each map area there is also a remove button or something
- There will be a script that uses these config SO, so when triggered the config can be loaded, so the manager sets the maps/actions to enabled/disabled, there could also be a priority value for each config, so if 2 actions conflict I can pick the one with the higher value, same goes for entire maps, I don't have to track all of the enabled/disabled actions, I just check when loading a config, so since after all it's just a List<InputAction> i can simply do         inputSystem_Actions.FindAction().enabled;
and check if I have to change status then I do a priority check
     */
}
