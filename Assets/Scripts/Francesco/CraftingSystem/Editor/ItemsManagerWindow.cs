using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using AD = UnityEditor.AssetDatabase;
using ADU = AssetDatabaseUtils;

public class ItemsManagerWindow : EditorWindow
{
    private readonly Vector2 WINDOW_MIN_SIZE = new(1000, 600);

    [MenuItem("Tools/Items Manager")]
    public static void ShowWindow()
    {
        var window = GetWindow<ItemsManagerWindow>();
        window.minSize = window.WINDOW_MIN_SIZE;
    }

    private ItemRecipeCollectionSO _currentItemRecipeCollectionSO;
    private SearchField _itemRecipeSOListSearchField;
    private string _itemRecipeSOListSearchString = string.Empty;

    private int _selectedToolIndex = 0;

    private readonly string[] _tools = new string[]
    {
        "Item Editor",
        "Crafting Editor",
        "Placeholder",
        "Placeholder 1"
    };

    private Vector2 _itemRecipeSOsScrollPos;
    private bool _showUniqueItemRecipeSOs = true;

    private void OnEnable()
    {
        _itemRecipeSOListSearchField = new SearchField();
    }

    private void OnGUI()
    {
        // +MAIN VERTICAL
        EditorGUILayout.BeginVertical();

        // +TOP HORIZONTAL
        EditorGUILayout.BeginHorizontal(GUI.skin.window, GUILayout.MaxHeight(50));

        _currentItemRecipeCollectionSO = (ItemRecipeCollectionSO)EditorGUILayout.ObjectField("Item Collection SO:", _currentItemRecipeCollectionSO, typeof(ItemRecipeCollectionSO), false, GUILayout.Width(300));
        EditorGUILayout.EndHorizontal();
        // -TOP HORIZONTAL

        if (!_currentItemRecipeCollectionSO)
        {
            EditorGUILayout.HelpBox("Please assign an ItemRecipeCollectionSO to start managing your items and recipes.", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }

        // +CONTENT HORIZONTAL
        EditorGUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandHeight(true));
        #region SIDEBAR
        // +SIDEBAR VERTICAL
        EditorGUILayout.BeginVertical(GUI.skin.window, GUILayout.ExpandHeight(true), GUILayout.MinWidth(200), GUILayout.MaxWidth(300));

        // +SEARCH AREA
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Height(50));

        _itemRecipeSOListSearchString = _itemRecipeSOListSearchField.OnGUI(_itemRecipeSOListSearchString);
        // Show ItemRecipeSOs with the same ItemSO as a single Item in the scroll, this changes the UI for it
        _showUniqueItemRecipeSOs = EditorGUILayout.ToggleLeft(new GUIContent("Show unique ItemRecipeSOs", "Show ItemRecipeSOs with the same ItemSO as a single Item in the scroll, this changes the UI for it by displaying a different set of data"), _showUniqueItemRecipeSOs, GUILayout.Width(300));

        EditorGUILayout.EndVertical();
        // -SEARCH AREA

        // +ITEMS LIST SCROLL
        // shows all of the recipes in the current collection, if _showUniqueItemRecipeSOs is true, it shows only one recipe for each unique ItemSO, otherwise it shows all the recipes
        _itemRecipeSOsScrollPos = EditorGUILayout.BeginScrollView(_itemRecipeSOsScrollPos, GUILayout.ExpandHeight(true));

        var dictRecipes = _currentItemRecipeCollectionSO.GetDictionaryOfRecipes(out List<ItemRecipeSO> recipesWithNullResults);

        //TODO: use an altern color for each row, so it's clearer
        if (_showUniqueItemRecipeSOs)
        {
            foreach (var kvp in dictRecipes)
            {
                ItemSO itemSO = kvp.Key;
                ItemRecipeSO[] recipes = kvp.Value;

                using (new EditorGUILayout.HorizontalScope(GUI.skin.box, GUILayout.Height(120)))
                {
                    Sprite itemSprite = itemSO ? itemSO.Icon : null;
                    GUILayout.Label(itemSprite ? itemSprite.texture : EditorGUIUtility.IconContent("Sprite Icon").image, GUILayout.Width(50), GUILayout.Height(50));
                    using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                    {
                        EditorGUI.BeginChangeCheck();

                        itemSO = (ItemSO)EditorGUILayout.ObjectField(itemSO, typeof(ItemSO), false);

                        if (EditorGUI.EndChangeCheck())
                        {
                            EditorUtility.SetDirty(_currentItemRecipeCollectionSO);
                            ADU.SaveAndRefresh();
                        }
                    }
                }
            }

            // draw recipes that have no assigned result
            foreach (var recipe in recipesWithNullResults)
            {
                using (new EditorGUILayout.HorizontalScope(GUI.skin.box, GUILayout.Height(120)))
                {
                    GUILayout.Label(EditorGUIUtility.IconContent("Sprite Icon").image, GUILayout.Width(50), GUILayout.Height(50));
                    using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                    {
                        recipe.ResultingSO = (ItemSO)EditorGUILayout.ObjectField(recipe.ResultingSO, typeof(ItemSO), false);
                        if (GUILayout.Button("Create"))
                        {
                            ItemSO newItemSO = CreateInstance<ItemSO>();
                            string path = EditorUtility.SaveFilePanelInProject("Save new ItemSO", "New Item SO", "asset", "Select location to save the new ItemSO");
                            path = AD.GenerateUniqueAssetPath(path);

                            if (!string.IsNullOrEmpty(path))
                            {
                                AD.CreateAsset(newItemSO, path);
                                recipe.ResultingSO = newItemSO;
                                EditorUtility.SetDirty(recipe);
                                ADU.SaveAndRefresh();
                            }
                        }
                    }

                }
            }
        }
        else
        {

        }


        EditorGUILayout.EndScrollView();
        // -ITEMS LIST SCROLL

        // +SIDEBAR BOTTOM
        GUILayout.FlexibleSpace();

        Color previousColor = GUI.color;
        GUI.color = Color.gray3;
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.MinHeight(100));
        GUI.color = previousColor;

        // +ITEMS ACTIONS HORIZONTAL
        EditorGUILayout.BeginHorizontal();

        // creates a new ItemRecipeSO asset and adds it to the current item recipe collection, then selects it in the items list
        if (GUILayout.Button("New"))
        {
            ItemRecipeSO newItemRecipeSO = CreateInstance<ItemRecipeSO>();
            string path = EditorUtility.SaveFilePanelInProject("Save new ItemRecipeSO", "New Item Recipe SO", "asset", "Select location to save the new ItemRecipeSO");
            path = AD.GenerateUniqueAssetPath(path);

            if (!string.IsNullOrEmpty(path))
            {
                AD.CreateAsset(newItemRecipeSO, path);
                _currentItemRecipeCollectionSO.ItemRecipes.Add(newItemRecipeSO);
                EditorUtility.SetDirty(_currentItemRecipeCollectionSO);
                ADU.SaveAndRefresh();
                //TODO: refresh the recipes list

            }
        }

        if (GUILayout.Button("Delete"))
        {

        }

        if (GUILayout.Button("Refresh"))
        {

        }

        EditorGUILayout.EndHorizontal();
        // -ITEMS ACTIONS HORIZONTAL

        EditorGUILayout.EndVertical();
        // -SIDEBAR BOTTOM

        EditorGUILayout.EndVertical();
        // -SIDEBAR VERTICAL
        #endregion

        // +TOOL AREA VERTICAL
        EditorGUILayout.BeginVertical(GUI.skin.window, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

        #region TOOL_CHOICE
        // +TOOL CHOICE HORIZONTAL
        EditorGUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.MinHeight(50));

        GUILayout.FlexibleSpace();

        EditorGUILayout.BeginVertical();

        GUILayout.FlexibleSpace();
        _selectedToolIndex = GUILayout.Toolbar(_selectedToolIndex, _tools, GUILayout.Height(30));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();

        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();
        // -TOOL CHOICE HORIZONTAL 
        #endregion

        #region TOOL_CONTENT
        // +TOOL CONTENT HORIZONTAL
        EditorGUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

        // *******************************************
        // HERE GOES THE TOOL CONTENT, FOR EXAMPLE THE RECIPE EDITOR, OR THE COMBINATION EDITOR, OR THE ITEM CONFIGURATION EDITOR, ETC...
        // *******************************************

        EditorGUILayout.EndHorizontal();
        // -TOOL CONTENT HORIZONTAL 
        #endregion

        EditorGUILayout.EndHorizontal();
        // -TOOL AREA VERTICAL

        EditorGUILayout.EndHorizontal();
        // -CONTENT HORIZONTAL

        EditorGUILayout.EndVertical();
        // -MAIN VERTICAL
    }
}