using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] TextMeshProUGUI convexityDefectsTxt;
    [SerializeField] Slider convexityDefectsSld;
    [SerializeField] TextMeshProUGUI segmentationTxt;
    [SerializeField] Slider segmentationSld;
    [SerializeField] Button resetBgButton;
    [Header("Overlays")]
    [SerializeField] Toggle contourToggle;
    [SerializeField] Toggle convexHullToggle;

    private void OnEnable()
    {
        convexityDefectsSld.onValueChanged.AddListener((val) =>
        {
            convexityDefectsTxt.text = val.ToString();
            Processor.Instance.interestingDefectSize = Mathf.RoundToInt(val);
        });

        segmentationSld.onValueChanged.AddListener((val) => 
        {
            segmentationTxt.text = val.ToString();
            Processor.Instance.valueThreshold = Mathf.RoundToInt(val);
        });

        contourToggle.onValueChanged.AddListener((val) =>
        {
            Processor.Instance.OutputContour = val;
        });
        convexHullToggle.onValueChanged.AddListener((val) =>
        {
            Processor.Instance.OutputConvexHull = val;
        });
        resetBgButton.onClick.AddListener(Processor.Instance.ResetCalibration);
    }

    private void OnDisable()
    {
        convexityDefectsSld.onValueChanged.RemoveAllListeners();
        segmentationSld.onValueChanged.RemoveAllListeners();
        contourToggle.onValueChanged.RemoveAllListeners();
        convexHullToggle.onValueChanged.RemoveAllListeners();
        resetBgButton.onClick.RemoveAllListeners();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        LeanTween.scale(gameObject,Vector3.one, 0.2f);
    }

    public void Hide()
    {
        LeanTween.scale(gameObject, Vector3.zero, 0.2f).setOnComplete(() => gameObject.SetActive(false));
    }

    public void OpenClose()
    {
        if (gameObject.activeSelf)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }
}
