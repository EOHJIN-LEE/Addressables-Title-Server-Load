using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class LobbyPresenter : MonoBehaviour
{
    [SerializeField] private TitlePresenter titlePresenter;
    [SerializeField] private LoadingPresenter loadingPresenter;
    [SerializeField] private DownloadPresenter downloadPresenter;
    
    private LobbyModel _lobbyModel;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        titlePresenter.SetOnStartButtonClick(OnStartButton);
        titlePresenter.gameObject.SetActive(true);
        loadingPresenter.gameObject.SetActive(false);
        downloadPresenter.gameObject.SetActive(false);
    }

    private void OnStartButton()
    {
        downloadPresenter.gameObject.SetActive(true);
        downloadPresenter.InitAsync();
        SceneManager.LoadScene("Example/Scenes/Download");
    }
}
