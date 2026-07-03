using UnityEngine;
using UnityEngine.InputSystem.Users;

public class InputConfigLoader : MonoBehaviour
{
    [SerializeField, Min(0)] private int _playerNumber = 1;

    public void Load(InputConfigSO inputConfigSO)
    {
        InputUser inputUser = default;
        if (InputUser.all.Count >= _playerNumber)
        {
            inputUser = InputUser.all[_playerNumber - 1];
        }

        InputConfigManager.RegisterConfig(inputConfigSO, inputUser);
    }

    public void Unload(InputConfigSO inputConfigSO)
    {
        InputUser inputUser = default;
        if (InputUser.all.Count >= _playerNumber)
        {
            inputUser = InputUser.all[_playerNumber - 1];
        }
        InputConfigManager.UnregisterConfig(inputConfigSO, inputUser);
    }
}
