using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(ColoredDustEditorData))]
public class ColoredDustEditorDataCustomEditor : Editor
{
    private int _previewSize = 100;
    private Color _previewBackgroundColor = Color.gray;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        SerializedProperty baseColorDustProp = serializedObject.FindProperty("_baseDustColor");
        SerializedProperty spriteProp = serializedObject.FindProperty("_baseSprite");
        SerializedProperty relativeFolderPathProp = serializedObject.FindProperty("_assetPathToCreateNewData");

        // don't allow to edit the path manually
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = false;
        relativeFolderPathProp.stringValue = EditorGUILayout.TextField("Folder Path To Create New Data", relativeFolderPathProp.stringValue);
        GUI.enabled = true;
        if (GUILayout.Button("Select Folder"))
        {
            string savePath = EditorUtility.SaveFolderPanel("Select folder where ColoredDustEditorData are going to be created", Application.dataPath, "");
            if (!string.IsNullOrEmpty(savePath))
            {
                if (!savePath.Contains(Application.dataPath))
                {
                    Debug.LogWarning("Selected folder must be inside the Assets folder");
                }
                else
                {
                    savePath = savePath.Replace(Application.dataPath, "Assets");
                    relativeFolderPathProp.stringValue = savePath;
                }
            }
        }

        EditorGUILayout.EndHorizontal();
        baseColorDustProp.colorValue = EditorGUILayout.ColorField(new GUIContent("Base Dust Color","The default value set to newly created ColoredDusts"), baseColorDustProp.colorValue);
        spriteProp.objectReferenceValue = EditorGUILayout.ObjectField(new GUIContent("Base Sprite", "The defualt color set to newly created ColoredDusts"), spriteProp.objectReferenceValue, typeof(Sprite), false);

        // if there's a sprite, show a preview of it with the base color applied as tint
        if (spriteProp.objectReferenceValue)
        {
            EditorGUILayout.LabelField("Sprite Preview");
            _previewBackgroundColor = EditorGUILayout.ColorField("Preview Background Color", _previewBackgroundColor);
            _previewSize = EditorGUILayout.IntSlider("Preview Size", _previewSize, 50, 500);
            Texture2D spriteTexture = AssetPreview.GetAssetPreview(spriteProp.objectReferenceValue);

            // draw a gray rect as background for the sprite preview
            Rect rect = GUILayoutUtility.GetRect(_previewSize, _previewSize, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));

            EditorGUI.DrawRect(rect, _previewBackgroundColor);
            GUI.DrawTexture(rect, spriteTexture, ScaleMode.ScaleToFit, true, 0, baseColorDustProp.colorValue, 0, 0);
        }


        serializedObject.ApplyModifiedProperties();
    }

}
