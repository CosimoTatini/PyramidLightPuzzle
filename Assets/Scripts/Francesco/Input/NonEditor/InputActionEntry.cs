using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

[Serializable]
public class InputActionEntry
{
    public static readonly string BUTTON_PLACEHOLDER = "@BUTTON";
    public string Guid;
    public string Name;
    public int Priority;
    public bool Enabled;
    public string NameOverride;
    public List<InputPromptScheme> PromptSchemes;

    public static string GetActionTextWithSprites(InputAction inputAction, InputUser inputUser, InputControlScheme? inputControlScheme, SpriteAssetsInputList spriteAssetsInputList, out bool success, InputActionEntry overrideEntry = null)
    {
        success = false;
        string bindingPrompt = string.Empty;

        if (inputAction == null)
        {
            bindingPrompt = "InputAction doesn't exist";
            return bindingPrompt;
        }

        if (inputUser == null || !inputUser.valid)
        {
            bindingPrompt = $"{inputAction.name}: InputUser is null or invalid";
            return bindingPrompt;
        }

        InputActionEntry inputActionEntry = overrideEntry ?? InputConfigManager.GetInputActionEntry(inputUser, inputAction.id.ToString());
        if (inputActionEntry == null)
        {
            bindingPrompt = $"{inputAction.name}: Missing InputActionEntry";
            return bindingPrompt;
        }

        if (!inputControlScheme.HasValue)
        {
            bindingPrompt = $"{inputAction.name}: Control Scheme is null";
            return bindingPrompt;
        }

        if (spriteAssetsInputList == null)
        {
            bindingPrompt = $"{inputAction.name}: SpriteAssetsInputList is null";
            return bindingPrompt;
        }

        //TODO: a binding can have multiple schemes, need to count each scheme as a new entry in the list of promptschemes
        // so each scheme is getting added to the list a bindingPromptData for the same binding
        int inputPromptSchemeIndex = inputActionEntry.PromptSchemes.FindIndex(k => k.Scheme == inputControlScheme.Value.name);
        if (inputPromptSchemeIndex == -1)
        {
            bindingPrompt = $"{inputAction.name}: Missing PromptScheme => {inputControlScheme.Value.name}";
            return bindingPrompt;
        }

        InputPromptScheme inputPromptScheme = inputActionEntry.PromptSchemes[inputPromptSchemeIndex];
        BindingPromptData bindingPromptData = inputPromptScheme.Prompts.FirstOrDefault();
        if (bindingPromptData.Equals(default))
        {
            bindingPrompt = $"{inputAction.name}: Missing BindingPromptData => {inputPromptScheme.Scheme}";
            return bindingPrompt;
        }

        var binding = inputAction.bindings.FirstOrDefault(binding => binding.id.ToString() == bindingPromptData.Guid);
        if (binding.Equals(default))
        {
            bindingPrompt = $"{inputAction.name}: Missing InputBinding => {bindingPromptData.Name}";
            return bindingPrompt;
        }

        string spriteName = binding.effectivePath;
        spriteName = spriteName
            .Replace("<", string.Empty)
            .Replace(">", string.Empty)
            .Replace("/", "_")
            .ToLowerInvariant();

        SpriteAssetInputScheme spriteAssetInputScheme = spriteAssetsInputList.SpriteAssetInputSchemes.FirstOrDefault(k => k.SchemeName == inputControlScheme.Value.name);
        if (spriteAssetInputScheme == null)
        {
            bindingPrompt = $"{inputAction.name}: Missing SpriteAssetInputScheme => {inputControlScheme.Value.name}";
            return bindingPrompt;
        }

        string spriteAssetName = spriteAssetInputScheme.SpriteAsset.name;

        bindingPrompt = bindingPromptData.Prompt;

        // standalone binding
        if (!binding.isComposite && !binding.isPartOfComposite)
        {
            int spriteIndex = spriteAssetInputScheme.SpriteAsset.GetSpriteIndexFromName(spriteName);
            bindingPrompt = bindingPrompt.Replace(BUTTON_PLACEHOLDER, $"<sprite=\"{spriteAssetName}\" index={spriteIndex}>");
        }
        // composite, cycle through its subindings
        else if (binding.isComposite)
        {
            var allbindings = inputAction.bindings;
            // get current binding index
            int currentBindingIndex = Array.IndexOf(allbindings.Select(b => b.id.ToString()).ToArray(), binding.id.ToString());
            bool isModifier = binding.path.Contains("Modifier", StringComparison.OrdinalIgnoreCase);
            List<string> bindingPromptParts = new();

            string temporaryBindingPrompt = bindingPrompt;
            while (currentBindingIndex + 1 < allbindings.Count && allbindings[currentBindingIndex + 1].isPartOfComposite)
            {
                currentBindingIndex++;
                if (!allbindings[currentBindingIndex].groups.Contains(inputControlScheme.Value.bindingGroup))
                {
                    continue;
                }
                binding = allbindings[currentBindingIndex];

                int index = temporaryBindingPrompt.IndexOf(BUTTON_PLACEHOLDER, 0, StringComparison.OrdinalIgnoreCase);
                if (index != -1)
                {
                    // placeholder found
                    // first add the part without it
                    bindingPromptParts.Add(temporaryBindingPrompt.Substring(0, index));
                    // then replaced placeholder
                    spriteName = binding.effectivePath
                        .Replace("<", string.Empty)
                        .Replace(">", string.Empty)
                        .Replace("/", "_")
                        .ToLowerInvariant();
                    int spriteIndex = spriteAssetInputScheme.SpriteAsset.GetSpriteIndexFromName(spriteName);
                    bindingPromptParts.Add($"<sprite=\"{spriteAssetName}\" index={spriteIndex}>");
                    temporaryBindingPrompt = temporaryBindingPrompt.Substring(index + BUTTON_PLACEHOLDER.Length);
                }
                else
                {
                    // no more placeholders available
                    // add the remaining text
                    bindingPromptParts.Add(temporaryBindingPrompt);
                    temporaryBindingPrompt = string.Empty;
                    break;
                }
            }

            if (temporaryBindingPrompt != string.Empty)
            {
                bindingPromptParts.Add(temporaryBindingPrompt);
            }
            bindingPrompt = string.Join(string.Empty, bindingPromptParts);
        }
        // part of a composite, there was something wrong
        else
        {
            bindingPrompt = $"{inputAction.name}: binding is part of a composite, this is not supported => {bindingPromptData.Name}";
            return bindingPrompt;
        }

        if (bindingPrompt == string.Empty)
        {
            bindingPrompt = "Something Happened";
            return bindingPrompt;
        }

        success = true;
        return bindingPrompt;
    }
}