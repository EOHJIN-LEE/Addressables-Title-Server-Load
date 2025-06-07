using UnityEngine;
using UnityEngine.UI;

public class TitlePresenter : MonoBehaviour
{
    [SerializeField] private Button startButton;

    public void SetOnStartButtonClick(System.Action onClick)
    {
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(() => onClick?.Invoke());
    }
}

public class LobbyModel
{
    public long PatchSize;
}
