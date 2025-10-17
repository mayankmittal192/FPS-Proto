using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class NameInputHandler : MonoBehaviour
{
    public int maxNameLength;
    public TMP_InputField inputField;
    public Image enterGameBtnImg;
    public Button enterGameBtn;

    public void HandleNameInput()
    {
        if (inputField.text.Length == 0)
        {
            Color c = enterGameBtnImg.color;
            c.a = 0.05f;
            enterGameBtnImg.color = c;
            enterGameBtn.interactable = false;
        }
        else
        {
            Color c = enterGameBtnImg.color;
            c.a = 1f;
            enterGameBtnImg.color = c;
            enterGameBtn.interactable = true;
        }

        if (inputField.text.Length > maxNameLength)
            inputField.text = inputField.text.Substring(0, maxNameLength);
    }

    public void SetPlayerName()
    {
        // Set the player name
        var world = World.DefaultGameObjectInjectionWorld;
        var e = world.EntityManager.CreateEntity();
        world.EntityManager.AddComponentData(e, new PlayerName { Value = new(inputField.text) });
    }
}
