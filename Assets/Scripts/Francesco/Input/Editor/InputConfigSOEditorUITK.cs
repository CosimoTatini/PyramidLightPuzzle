// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEditor;
// using UnityEditor.UIElements;
// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.UIElements;

// [CustomEditor(typeof(InputConfigSO))]
// public class InputConfigSOEditorUITK : Editor
// {
//     private VisualElement _root;
//     private ObjectField _loaderAsset;
//     private ObjectField _loaderAssetInstanceType;
//     private HelpBox _helpboxAssetIntanceType;
//     private Button _buttonAddMap;
//     private VisualElement _contentContainer;

//     public override VisualElement CreateInspectorGUI()
//     {
//         _root = new VisualElement();

//         _loaderAsset = new()
//         {
//             allowSceneObjects = false,
//             objectType = typeof(InputActionAsset),
//             bindingPath = "_lastUsedInputAsset"
//         };
//         _loaderAsset.RegisterValueChangedCallback(evt => UpdateUI(evt.newValue as InputActionAsset, _loaderAssetInstanceType.value as TypeVar));

//         _loaderAssetInstanceType = new()
//         {
//             allowSceneObjects = false,
//             objectType = typeof(TypeVar)
//         };
//         _loaderAssetInstanceType.RegisterValueChangedCallback(evt => UpdateUI(_loaderAsset.value as InputActionAsset, evt.newValue as TypeVar));

//         _helpboxAssetIntanceType = new();
//         _contentContainer = new();

//         // InputActionAsset inputAsset()
//         // {
//         //     return _loaderAsset?.value as InputActionAsset;
//         // }

//         // loaderAsset.value = inputConfigSO.LastUsedInputAsset;
//         InputConfigSO inputConfigSO = target as InputConfigSO;
//         _root.Add(_loaderAsset);
//         _root.Add(_loaderAssetInstanceType);
//         _root.Add(_helpboxAssetIntanceType);
//         _root.Add(_contentContainer);

//         // find all of the items that are linked to the loader asset
//         var inputAssetMaps = inputConfigSO.GetInputAssetMaps();

//         SerializedProperty assetMapListProp = serializedObject.FindProperty("_inputAssetMaps");

//         GenericDropdownMenu genericDropdownMenu = new();
//         genericDropdownMenu.AddItem("Pollo", false, () =>
//         {
//             Debug.Log("Pollo pressed");
//         });
//         genericDropdownMenu.AddItem("Gallina", false, () =>
//         {
//             Debug.Log("Gallina pressed");
//         });
//         _buttonAddMap = new();
//         _buttonAddMap.text = "Add map";
//         _buttonAddMap.clicked += () =>
//         {
//             genericDropdownMenu.DropDown(_buttonAddMap.worldBound, _buttonAddMap, DropdownMenuSizeMode.Auto);
//         };

//         _root.Add(_buttonAddMap);
//         // var assetMapGuids = inputAsset().actionMaps.Select(m => m.id.ToString()).ToHashSet();


//         // List<int> filteredMapsIndexes = new();
//         // foreach (var item in inputAssetMaps)
//         // {
//         // }

//         // // 1. Fetch top-level asset collection property

//         // ListView mapListView = new()
//         // {
//         //     bindingPath = assetMapListProp.propertyPath,
//         //     itemsSource = new List<AbstractGenericMenu>(),

//         //     showAddRemoveFooter = true,
//         //     reorderable = true,
//         //     headerTitle = "Input Asset Maps",
//         //     virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,

//         //     makeItem = () =>
//         //     {
//         //         VisualElement rowRoot = new();
//         //         PropertyField propertyField = new();
//         //         rowRoot.Add(propertyField);
//         //         return rowRoot;
//         //     },

//         //     bindItem = (VisualElement element, int index) =>
//         //     {
//         //         SerializedProperty itemProperty = assetMapListProp.GetArrayElementAtIndex(index);
//         //         PropertyField propertyField = element.Q<PropertyField>();
//         //         propertyField.BindProperty(itemProperty);
//         //         // element.Add(propertyField);
//         //     },


//         //     //     }
//         // };

//         _root.Bind(serializedObject);

//         UpdateUI(_loaderAsset.value as InputActionAsset, _loaderAssetInstanceType.value as TypeVar);

//         return _root;
//     }

//     private void UpdateUI(InputActionAsset currentAsset, TypeVar typeVar)
//     {
//         if (currentAsset == null)
//         {
//             HideVE(_loaderAssetInstanceType);
//             HideVE(_helpboxAssetIntanceType);
//             HideVE(_buttonAddMap);
//             HideVE(_contentContainer);
//             return;
//         }

//         ShowVE(_loaderAssetInstanceType);
//         if (typeVar == null || typeVar.Type == null)
//         {
//             ShowVE(_helpboxAssetIntanceType);
//             HideVE(_buttonAddMap);
//             HideVE(_contentContainer);
//             _helpboxAssetIntanceType.messageType = HelpBoxMessageType.Warning;
//             _helpboxAssetIntanceType.text = "Insert a valid TypeVar";
//             return;
//         }

//         if (!typeof(IInputActionCollection2).IsAssignableFrom(typeVar.Type))
//         {
//             ShowVE(_helpboxAssetIntanceType);
//             HideVE(_buttonAddMap);
//             HideVE(_contentContainer);
//             _helpboxAssetIntanceType.messageType = HelpBoxMessageType.Warning;
//             _helpboxAssetIntanceType.text = "Insert a TypeVar with Type being a C# generated version of an InputActionAsset";
//             return;
//         }

//         HideVE(_helpboxAssetIntanceType);
//         ShowVE(_buttonAddMap);
//         ShowVE(_contentContainer);
//     }

//     private void LoaderAssetInstanceTypeChanged(ChangeEvent<UnityEngine.Object> evt)
//     {
//         TypeVar newValue = (evt != null) ? evt.newValue as TypeVar : null;

//         if (newValue == null || newValue.Type == null)
//         {
//             ShowVE(_helpboxAssetIntanceType);
//             HideVE(_contentContainer);
//             _helpboxAssetIntanceType.messageType = HelpBoxMessageType.Warning;
//             _helpboxAssetIntanceType.text = "Insert a valid TypeVar";
//         }
//         else if (!typeof(IInputActionCollection2).IsAssignableFrom(newValue.Type))
//         {
//             ShowVE(_helpboxAssetIntanceType);
//             HideVE(_contentContainer);
//             _helpboxAssetIntanceType.messageType = HelpBoxMessageType.Warning;
//             _helpboxAssetIntanceType.text = "Insert a TypeVar with Type being a C# generated version of an InputActionAsset";
//         }
//         else
//         {
//             HideVE(_helpboxAssetIntanceType);
//             ShowVE(_contentContainer);
//         }
//     }

//     private void LoaderAssetChanged(ChangeEvent<UnityEngine.Object> evt)
//     {
//         if (evt == null || evt.newValue == null)
//         {
//             HideVE(_loaderAssetInstanceType);
//             HideVE(_helpboxAssetIntanceType);
//             HideVE(_contentContainer);
//         }
//         else
//         {
//             ShowVE(_loaderAssetInstanceType);
//             if (!VEVisible(_helpboxAssetIntanceType))
//             {
//                 ShowVE(_helpboxAssetIntanceType);
//             }
//             if (!VEVisible(_contentContainer))
//             {
//                 ShowVE(_contentContainer);
//             }
//         }
//     }

//     private void HideVE(VisualElement visualElement)
//     {
//         if (visualElement == null) return;
//         visualElement.style.display = DisplayStyle.None;
//     }

//     private void ShowVE(VisualElement visualElement)
//     {
//         if (visualElement == null) return;
//         visualElement.style.display = DisplayStyle.Flex;
//     }

//     private bool VEVisible(VisualElement visualElement)
//     {
//         if (visualElement == null) return false;
//         return (visualElement.style.display == DisplayStyle.Flex);
//     }
// }