using System.Linq;
using UnityEditor;
using UnityEngine;
using AD = UnityEditor.AssetDatabase;
using ADU = AssetDatabaseUtils;

public class ColoredDustEditorWindow : EditorWindow
{
    private const int WINDOW_MIN_WIDTH = 300;
    private const int WINDOW_MIN_HEIGHT = 400;

    [MenuItem("Tools/Colored Dust Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<ColoredDustEditorWindow>("Colored Dust Editor");
        window.minSize = new Vector2(WINDOW_MIN_WIDTH, WINDOW_MIN_HEIGHT);
    }

    private void OnEnable()
    {
        FindEditorDataAndNames();
    }

    private ColoredDustEditorData[] _editorDataFound;
    private string[] _editorDataNames;
    private ColoredDustEditorData _currentEditorData;
    private int _currentEditorDataIndex = 0;
    private int _previousEditorDataIndex = -1;
    private Editor _currentColoredDustDataEditor;

    private Color _baseDustColor => _currentEditorData.BaseDustColor;
    private Sprite _baseSprite => _currentEditorData.BaseSprite;
    private string _assetPathToCreateNewData => _currentEditorData.AssetPathToCreateNewData;

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("New", GUILayout.Width(70f)))
        {
            ColoredDustEditorData newData = CreateInstance<ColoredDustEditorData>();
            string path = EditorUtility.SaveFilePanelInProject("Save new Colored Dust Editor Data", "New Colored Dust Editor Data", "asset", "Select location to save the new Colored Dust Editor Data");
            path = AD.GenerateUniqueAssetPath(path);

            if (!string.IsNullOrEmpty(path))
            {
                AD.CreateAsset(newData, path);
                ADU.SaveAndRefresh();
                RefreshEditorData();
            }
        }

        if (_editorDataFound == null || _editorDataFound.Length == 0 || _currentEditorData == null)
            GUI.enabled = false;

        if (GUILayout.Button("Delete", GUILayout.Width(70f)))
        {
            bool proceedWithDeletion = EditorUtility.DisplayDialog("Delete Colored Dust Editor Data", $"Are you sure you want to delete the selected Colored Dust Editor Data: {_currentEditorData.name}?", "Yes", "No");

            if (proceedWithDeletion)
            {
                AD.DeleteAsset(AD.GetAssetPath(_currentEditorData));
                ADU.SaveAndRefresh();
                RefreshEditorData();
            }
        }
        GUI.enabled = true;

        if (GUILayout.Button("Refresh", GUILayout.Width(70f)))
        {
            RefreshEditorData();
        }

        EditorGUILayout.EndHorizontal();

        if (_editorDataFound == null || _editorDataFound.Length == 0)
        {
            EditorGUILayout.HelpBox("No Colored Dust Editor Data found. Create one to start editing.", MessageType.Info);
            return;
        }

        _currentEditorDataIndex = EditorGUILayout.Popup("Editor Data", _currentEditorDataIndex, _editorDataNames);
        if (_currentEditorDataIndex != _previousEditorDataIndex)
        {
            _currentEditorData = _editorDataFound[_currentEditorDataIndex];
            _previousEditorDataIndex = _currentEditorDataIndex;
            _currentColoredDustDataEditor = null;
        }

        if (!_currentColoredDustDataEditor)
        {
            _currentColoredDustDataEditor = Editor.CreateEditor(_currentEditorData);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Selected Colored Dust Editor Data Editor:", EditorStyles.boldLabel);
        _currentColoredDustDataEditor.OnInspectorGUI();


    }

    private void RefreshEditorData()
    {
        FindEditorDataAndNames();
        ResetIndexes();
    }
    private void FindEditorDataAndNames()
    {
        _editorDataFound = AssetDatabaseUtils.GetAssetsByType<ColoredDustEditorData>();
        _editorDataNames = _editorDataFound?.Select(data => data.name).ToArray();
    }

    private void ResetIndexes()
    {
        _currentEditorDataIndex = 0;
        _previousEditorDataIndex = -1;
    }
}
