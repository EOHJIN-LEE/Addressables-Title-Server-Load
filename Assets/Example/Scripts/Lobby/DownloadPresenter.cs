using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class DownloadPresenter : MonoBehaviour
{
    [Header("UI")] 
    public Button downloadButton;
    public GameObject waitMessage;
    public GameObject downloadMessage;
    public Slider downSlider;
    public Text sizeInfoText;
    public Text downloadValueText;

    [Header("Label")] 
    public AssetLabelReference defaultLabel;

    private long _patchSize;
    private readonly Dictionary<string, long> _patchMap = new();

    private void Awake()
    {
        waitMessage.SetActive(true);
        downloadMessage.SetActive(false);
        downloadButton.onClick.AddListener(() => PatchFilesAsync().Forget());
    }

    // private void Start() => InitAsync().Forget();

    public async UniTaskVoid InitAsync()
    {
        await Addressables.InitializeAsync().Task;
        
        var checkResult = await CheckUpdateFilesAsync(GetLabelKeys());
        if (checkResult.needToUpdate)
        {
            waitMessage.SetActive(false);
            downloadMessage.SetActive(true);
            sizeInfoText.text = FormatFileSize(checkResult.patchSize);
        }
        else
        {
            SetProgressUI(1f);
            await UniTask.Delay(TimeSpan.FromSeconds(2));
            LoadNextScene();
        }
    }

    private IEnumerable<string> GetLabelKeys() => new[] { defaultLabel.labelString };

    private async UniTask<(bool needToUpdate, long patchSize)> CheckUpdateFilesAsync(IEnumerable<string> labels)
    {
        var patchSize = await CalculateTotalDownloadSizeAsync(labels);
        return _patchSize > 0 ? (true, patchSize) : (false, patchSize);
    }

    private async UniTask<long> CalculateTotalDownloadSizeAsync(IEnumerable<string> labels)
    {
        var sizes = await UniTask.WhenAll(labels.Select(async label =>
        {
            var handle = Addressables.GetDownloadSizeAsync(label);
            await handle.Task;
            return handle.Result;
        }));
        return sizes.Sum();
    }

    private async UniTaskVoid PatchFilesAsync()
    {
        var labels = GetLabelKeys();
        var downloadTasks = labels.Select(DownloadAsync).ToList();

        await MonitorDownloadProgress(labels);
        await UniTask.WhenAll(downloadTasks);

        LoadNextScene();
    }

    private async UniTask DownloadAsync(string label)
    {
        _patchMap[label] = 0;
        var handle = Addressables.DownloadDependenciesAsync(label);

        while (!handle.IsDone)
        {
            _patchMap[label] = handle.GetDownloadStatus().DownloadedBytes;
            await UniTask.WaitForEndOfFrame();
        }

        _patchMap[label] = handle.GetDownloadStatus().TotalBytes;
        Addressables.Release(handle);
    }

    private async UniTask MonitorDownloadProgress(IEnumerable<string> labels)
    {
        downloadValueText.text = "0 %";

        while (GetTotalDownloadedBytes(labels) < _patchSize)
        {
            var progress = GetDownloadProgress(labels);
            SetProgressUI(progress);
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
        }
    }

    private float GetDownloadProgress(IEnumerable<string> labels)
    {
        var total = GetTotalDownloadedBytes(labels);
        return _patchSize > 0 ? (float)total / _patchSize : 1f;
    }

    private long GetTotalDownloadedBytes(IEnumerable<string> labels)
    {
        return labels.Sum(label => _patchMap.TryGetValue(label, out var value) ? value : 0);
    }

    private void SetProgressUI(float sliderValue)
    {
        downSlider.value = sliderValue;
        downloadValueText.text = $"{(int)(sliderValue * 100)} %";
    }

    private void LoadNextScene()
    {
        LoadingPresenter.LoadScene("Example/Scenes/Playground");
    }

    private static readonly (long Threshold, string Suffix)[] SizeUnits =
    {
        (1L << 30, "GB"),
        (1L << 20, "MB"),
        (1L << 10, "KB"),
    };

    private static string FormatFileSize(long byteCount)
    {
        foreach (var (threshold, suffix) in SizeUnits)
        {
            if (byteCount >= threshold)
                return $"{(double)byteCount / threshold:0.##} {suffix}";
        }
        return byteCount > 0 ? $"{byteCount} Bytes" : "0 Bytes";
    }
}