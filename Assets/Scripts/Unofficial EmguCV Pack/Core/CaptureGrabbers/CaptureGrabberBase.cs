using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnofficialEmguCVPackForUnity.Utils.Delegates;

namespace UnofficialEmguCVPackForUnity.Core.VideoCaptureGrabbers
{
    public abstract class CaptureGrabberBase : MonoBehaviour
    {
        public Texture2DEvent onConvertedFrame;
    }
}
