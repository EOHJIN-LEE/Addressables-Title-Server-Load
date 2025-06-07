using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingPresenter : MonoBehaviour
{
    [SerializeField] private Slider loadingBar;

    private static string nextScene;

    private void Awake()
    {
        StartLoadingSceneAsync().Forget();
    }

    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("Example/Scenes/Loading");
    }

    private async UniTaskVoid StartLoadingSceneAsync()
    {
        await UniTask.Yield(); // 한 프레임 기다리기

        var op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float timer = 0f;

        while (!op.isDone)
        {
            await UniTask.Yield(); // 프레임마다 대기
            timer += Time.deltaTime;

            if (op.progress < 0.9f)
            {
                loadingBar.value = Mathf.Lerp(loadingBar.value, op.progress, timer);

                if (loadingBar.value >= op.progress)
                {
                    timer = 0f;
                }
            }
            else
            {
                loadingBar.value = Mathf.Lerp(loadingBar.value, 1f, timer);

                if (Math.Abs(loadingBar.value - 1f) < float.Epsilon)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(2f));
                    op.allowSceneActivation = true;
                    break;
                }
            }
        }
    }
}