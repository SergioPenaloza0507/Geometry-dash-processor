using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnofficialEmguCVPackForUnity.Core.VideoCaptureGrabbers;

[RequireComponent(typeof(RawImage))]
public class RawImageSetter : MonoBehaviour
{
    [SerializeField] CaptureGrabberBase grabber;
    [SerializeField] bool adjustToCaptureResolution;
    [SerializeField] float sizeMultiplier;
    RawImage img;
    RectTransform rt;
    private void Awake()
    {
        img = GetComponent<RawImage>();
        rt = img.transform as RectTransform;
    }

    private void OnEnable()
    {
        if(grabber != null)
        {
            grabber.onConvertedFrame.AddListener(SetTexture);
        }
    }
    private void OnDisable()
    {
        if(grabber != null)
        {
            grabber.onConvertedFrame.RemoveListener(SetTexture);
        }
    }

    void SetTexture(Texture2D val)
    {
        img.texture = val;
        if (!adjustToCaptureResolution) return;
        rt.sizeDelta = new Vector2(val.width, val.height) * sizeMultiplier;
    }
}
