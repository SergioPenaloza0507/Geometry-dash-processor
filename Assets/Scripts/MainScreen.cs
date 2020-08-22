using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnofficialEmguCVPackForUnity.Core.VideoCaptureGrabbers;

public class MainScreen : MonoBehaviour
{
    [SerializeField] BgrByteCaptureGrabber grabber;

    [Header("UI Elements")]
    [SerializeField] RawImage inputImg;
    [SerializeField] RawImage outputImg;
    [SerializeField] TextMeshProUGUI detectedTxt;


    private void Awake()
    {
        Application.OpenURL("steam://rungameid/322170");
    }
    private void OnEnable()
    {
        grabber.onProcessableframeCaptured.AddListener(Processor.Instance.ProcessImage);
        grabber.onConvertedFrame.AddListener((val) => inputImg.texture = val);
        Processor instance = Processor.Instance;
        instance.onOutputImage.AddListener((val) => outputImg.texture = val);
        instance.onDetectedDefects.AddListener((int val) => detectedTxt.text = val.ToString());
        
    }

    private void OnDisable()
    {
        grabber.onProcessableframeCaptured.RemoveAllListeners();
        grabber.onConvertedFrame.RemoveAllListeners();
        Processor instance = Processor.Instance;
        instance.onOutputImage.RemoveAllListeners();
        instance.onDetectedDefects.RemoveAllListeners();
        
    }
}
