using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;
using Assembly = UnityEditor.Compilation.Assembly;
using SystemAssembly = System.Reflection.Assembly;

[CustomEditor(typeof(TypeVar))]
public class TypeVarCustomEditor : Editor
{
    private readonly ReadOnlyCollection<string> ALLOWED_ASSEMBLIES = new List<string> { "Assembly-CSharp", "Assembly-CSharp-Editor" }.AsReadOnly();
    private Type[] _matchingScripts;
    private int _selectedTypeIndex = 0;

    private string _typeString = "";
    private string _temporaryTypeString = "";
    private string _temporaryAssemblyName = "";

    private bool _isChangingType = false;
    private bool _isInitialized = false;
    private bool _saveChanges = false;

    private bool _showBaseInspector = false;

    private bool _includeUnityAssemblies = false;
    private Assembly[] _allAssemblies;
    private string[] _allAssembliesNames;
    private int _chosenAssemblyIndex;

    private SerializedProperty _propType;
    private SerializedProperty _propSearchFilter;
    private SerializedProperty _propIncludeUnityAssemblies;
    private SerializedProperty _propAssemblyPath;

    private MonoScript _monoscript;

    private readonly ReadOnlyCollection<string> TYPE_PICKER_TOOL = new List<string> { "Simple", "Complex" }.AsReadOnly();
    private int _typePickerToolIndex = 0;
    private bool _onEnabled = false;

    private void OnEnable()
    {
        _onEnabled = true;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (!_isInitialized) Initialize();

        _typePickerToolIndex = GUILayout.Toolbar(_typePickerToolIndex, TYPE_PICKER_TOOL.ToArray());

        switch (TYPE_PICKER_TOOL[_typePickerToolIndex])
        {
            case "Simple":
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledGroupScope(!_isChangingType))
                    {
                        if (_onEnabled && !_monoscript && !string.IsNullOrEmpty(_propType.stringValue))
                        {
                            Type typeToMatch = Type.GetType(_propType.stringValue);
                            _monoscript = AssetDatabaseUtils.GetAssetsByType<MonoScript>().Where(ms => ms.GetClass() == typeToMatch).FirstOrDefault();
                        }
                        _monoscript = (MonoScript)EditorGUILayout.ObjectField("Choose script:", _monoscript, typeof(MonoScript), false);
                    }
                    DrawConfirmAndChangeTypeButtons();
                }
                EditorGUILayout.HelpBox("To find Types defined in the same script as another use Complex mode, MonoScript only shows one Type of a script, giving priority to the primary class, if primary is not found then the first valid type", MessageType.Info);
                if (_saveChanges && _monoscript)
                {
                    Type type = _monoscript.GetClass();
                    try
                    {
                        _chosenAssemblyIndex = Array.IndexOf(_allAssemblies, _allAssemblies.Where(asm => type.Assembly.Location.EndsWith(asm.outputPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar))).Select(asm => asm).First());
                        Save(type.Name, type.AssemblyQualifiedName, _allAssemblies[_chosenAssemblyIndex].outputPath);
                        _temporaryAssemblyName = _allAssembliesNames[_chosenAssemblyIndex];
                        _temporaryTypeString = type.Name;
                        _saveChanges = false;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                        EditorGUILayout.HelpBox("Couldn't find this Type inside of the found assemblies, if not active try refreshing with Unity Assemblies in Complex mode", MessageType.Warning);
                    }
                }

                break;
            case "Complex":
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawTypeFilter();
                    DrawConfirmAndChangeTypeButtons();
                }

                using (new EditorGUI.DisabledGroupScope(!_isChangingType))
                {
                    DrawAssembliesPopup();
                    DrawTypesPopup();

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        _includeUnityAssemblies = EditorGUILayout.ToggleLeft("Include Unity's assemblies:", _includeUnityAssemblies);
                        if (GUILayout.Button("Refresh Assemblies"))
                        {
                            GetAllAssemblies(ALLOWED_ASSEMBLIES.ToArray(), _includeUnityAssemblies);
                            _allAssembliesNames = _allAssemblies.Select(asm => asm.name).ToArray();
                        }
                    }
                }

                break;
        }

        EditorGUILayout.Space();

        _showBaseInspector = EditorGUILayout.Foldout(_showBaseInspector, "Show Base Inspector", true);
        if (_showBaseInspector)
        {
            EditorGUI.indentLevel++;
            DrawDefaultDisabledGUI();
            EditorGUI.indentLevel--;
        }

        _onEnabled = false;
    }

    private void DrawDefaultDisabledGUI()
    {
        using (new EditorGUI.DisabledGroupScope(true))
        {
            base.OnInspectorGUI();
        }
    }

    private void DrawTypesPopup()
    {
        //if (!GUI.enabled) return;

        // if the property is not initialized, we use the first assembly in the list
        string assemblyRelativePath = string.IsNullOrEmpty(_propAssemblyPath.stringValue) ?
            _allAssemblies[0].outputPath :
            _propAssemblyPath.stringValue;

        // Load the assembly from the absolute path
        //SystemAssembly assembly = SystemAssembly.LoadFile(GetAbsoluteAssemblyPath(assemblyRelativePath));

        SystemAssembly assembly = SystemAssembly.LoadFile(GetAbsoluteAssemblyPath(_allAssemblies[_chosenAssemblyIndex].outputPath));

        // We can now access all of the script thanks to System's assembly GetTypes() method
        // filtering the results with _typeString
        // we also make the filter case insensitive

        Type[] types = assembly.GetTypes()
            .Where(t => t.Name.ToLower().Contains(_typeString.ToLower()))
            .ToArray();

        // Get only the names to display them in the Popup
        string[] typeNames = types
            .Select(t => t.Name)
            .ToArray();

        // Get the full qualified names for saving
        string[] qualifiedNames = types
            .Select(t => t.AssemblyQualifiedName)
            .ToArray();

        if (typeNames.Length == 0)
        {
            EditorGUILayout.Popup("Select Type", 0, new string[] { "No match" });

            if (_saveChanges)
            {
                EditorUtility.DisplayDialog("No match", "Can't save, try modifying your filter", "Okay");
                _saveChanges = false;
            }
        }
        else
        {
            /* still looking for the old typeProp, so in case there are no matches
             the index will be reset to 0, this happens since we don't save anymore the Type
             everytime, but only when the user clicks the Confirm button,
             we use a temporary string which is saved each cycle
             and save on click.
             The null check for _temporaryTypeString is meant for the first time
             as if the user chose something, then the SO reloads
             we should have a match 100%*/

            string comparationValue = string.IsNullOrEmpty(_temporaryTypeString) ? _propType.stringValue : _temporaryTypeString;
            _selectedTypeIndex = Array.FindIndex(qualifiedNames, s => s == comparationValue);

            // Reset to first index if no match found
            if (_selectedTypeIndex < 0) _selectedTypeIndex = 0;

            // Let the user select the type they want
            _selectedTypeIndex = EditorGUILayout.Popup("Select Type", _selectedTypeIndex, typeNames);

            // saving the chosen type
            _temporaryTypeString = qualifiedNames[_selectedTypeIndex];

            // if the user clicked the Confirm button, we save the changes
            if (_saveChanges)
            {
                Save(typeNames[_selectedTypeIndex], qualifiedNames[_selectedTypeIndex], _allAssemblies[_chosenAssemblyIndex].outputPath);
                // tries to update the object field
                if (!string.IsNullOrEmpty(_propType.stringValue))
                {
                    Type typeToMatch = Type.GetType(_propType.stringValue);
                    _monoscript = AssetDatabaseUtils.GetAssetsByType<MonoScript>().Where(ms => ms.GetClass() == typeToMatch).FirstOrDefault();
                }
                _saveChanges = false;
            }
        }
    }

    private void Save(string typeName, string qualifiedName, string relativePath)
    {
        _propType.stringValue = qualifiedName;
        _propSearchFilter.stringValue = typeName;
        // update typeString to the new value
        _typeString = typeName;

        // We save the relative path as an absolute path would not work on another device
        _propAssemblyPath.stringValue = relativePath;
        _propIncludeUnityAssemblies.boolValue = _includeUnityAssemblies;

        serializedObject.ApplyModifiedProperties();

        _isChangingType = false;
    }

    /// <summary>
    /// Turns the relative path of the assembly into an absolute path, used for getting a System Assembly
    /// </summary>
    /// <param name="relativePath">The relative path of the assembly: "Library/ScriptAssemblies/...dll"</param>
    /// <returns></returns>
    private string GetAbsoluteAssemblyPath(string relativePath)
    {
        // Returns the absolute path to the 'Assets' folder
        string projectPath = Application.dataPath;

        // Combines the project path with the assembly's relative path to get the full path,
        // we remove the last 6 characters from the projectPath to remove "Assets"
        // then we fuse the project path with the assembly relative path to get the absolute path
        return Path.Combine(projectPath[..^6], relativePath);
    }

    private void DrawAssembliesPopup()
    {
        string comparationValue = string.Empty;

        // if temporary isn't there, then we select the serialized value
        if (string.IsNullOrEmpty(_temporaryAssemblyName))
        {
            string absolutePath = string.Empty;
            // if no serialized value then just select the first assembly, else get the actual value
            absolutePath = string.IsNullOrEmpty(_propAssemblyPath.stringValue) ?
                GetAbsoluteAssemblyPath(_allAssemblies[0].outputPath) :
                GetAbsoluteAssemblyPath(_propAssemblyPath.stringValue);
            comparationValue = SystemAssembly.LoadFile(absolutePath).GetName().Name;
        }
        else
        {
            comparationValue = _temporaryAssemblyName;
        }

        // retrieve the assembly path from the serialized object
        _chosenAssemblyIndex = Array.FindIndex(_allAssemblies, a => a.name == comparationValue);

        if (_chosenAssemblyIndex < 0)
        {
            _chosenAssemblyIndex = 0; // Reset to first index if not found
        }

        EditorGUI.BeginChangeCheck();
        _chosenAssemblyIndex = EditorGUILayout.Popup("Select Assembly", _chosenAssemblyIndex, _allAssembliesNames);

        if (EditorGUI.EndChangeCheck())
        {
            // Save the temporary assembly path to use it later
            _temporaryAssemblyName = _allAssembliesNames[_chosenAssemblyIndex];
        }
    }

    private void DrawConfirmAndChangeTypeButtons()
    {
        if (_isChangingType)
        {
            if (GUILayout.Button("Confirm type"))
            {
                _saveChanges = true;
                // Make textField for _typeString lose focus to update its content
                GUIUtility.keyboardControl = 0;
                GUIUtility.hotControl = 0;
            }
        }
        else
        if (GUILayout.Button("Change type"))
        {
            _isChangingType = true;
        }
    }

    private void DrawTypeFilter()
    {
        GUI.enabled = _isChangingType;

        if (_isChangingType)
        {
            _typeString = EditorGUILayout.TextField("Type to find:", _typeString);
        }
        else
        {
            EditorGUILayout.TextField("Type to find:", _typeString);
        }

        if (!GUI.enabled) GUI.enabled = true;
    }

    private void Initialize()
    {
        _propType = serializedObject.FindProperty("_typeName");
        _propSearchFilter = serializedObject.FindProperty("_filter");
        _propIncludeUnityAssemblies = serializedObject.FindProperty("_includeUnityAssemblies");
        _propAssemblyPath = serializedObject.FindProperty("_assemblyPath");

        _includeUnityAssemblies = _propIncludeUnityAssemblies.boolValue;
        GetAllAssemblies(ALLOWED_ASSEMBLIES.ToArray(), _includeUnityAssemblies);
        _allAssembliesNames = _allAssemblies.Select(asm => asm.name).ToArray();

        // if the user previously chose a type, we recover the filter
        if (!string.IsNullOrEmpty(_propSearchFilter.stringValue))
            _typeString = _propSearchFilter.stringValue;


        _isInitialized = true;
    }

    /// <summary>
    /// Gets all assemblies compiled by Unity, this means some assemblies can't be found this way, it's not an issue for most cases, but if needed then we need to use AppDomain.CurrentDomain.GetAssemblies() instead.
    /// </summary>
    /// <param name="allowedAssemblies">(NOT WORKING) Only allows for the passed assemblies</param>
    /// <param name="includeUnityAssemblies">Whether to include or not Unity's assemblies</param>
    private void GetAllAssemblies(string[] allowedAssemblies, bool includeUnityAssemblies)
    {
        var userAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.PlayerWithoutTestAssemblies);
        var editorAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Editor);

        var assemblies = userAssemblies.Concat(editorAssemblies).
            Where(asm => (allowedAssemblies.Contains(asm.name) || asm.defines != null));
        if (!includeUnityAssemblies)
        {
            assemblies = assemblies.Where(asm => !asm.name.StartsWith("Unity"));
        }
        _allAssemblies = assemblies.ToArray();
    }
}
