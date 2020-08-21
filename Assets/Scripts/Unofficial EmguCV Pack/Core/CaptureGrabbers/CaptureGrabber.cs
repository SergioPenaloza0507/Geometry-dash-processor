using UnityEngine;
using Emgu.CV;
using UnofficialEmguCVPackForUnity.Utils.Delegates;
using UnofficialEmguCVPackForUnity.Utils;

namespace UnofficialEmguCVPackForUnity.Core.VideoCaptureGrabbers
{
    public abstract class CaptureGrabber <TColor,TDepth> : CaptureGrabberBase where TColor : struct, IColor where TDepth : new()
    {
        [HideInInspector]public EmguImageEvent<TColor,TDepth> onProcessableframeCaptured = new EmguImageEvent<TColor, TDepth>();
        [SerializeField] float updateDelay;
        VideoCapture cap;

        Image<TColor, TDepth> processableImage;
        Texture2D processedResult;

        private float timer;
        void OnEnable()
        {
            cap = new VideoCapture();
        }

        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= updateDelay)
            {
                Grab();
                timer = 0;
            }
        }

        protected virtual void Grab()
        {
            Mat frame = cap.QueryFrame();
            if (frame == null)
            {
                Debug.LogWarning("Frame was not captured, please check if cameras are available");
                return;
            }
            Destroy(processedResult);
            processedResult = frame.ToBitmap().ToTexture2D();
            onConvertedFrame?.Invoke(processedResult);
            FireProcessableImageEvent(frame);
        }

        protected virtual void FireProcessableImageEvent(Mat frame)
        {
            onProcessableframeCaptured?.Invoke(frame.ToImage<TColor, TDepth>());
        }

        private void OnDisable()
        {
            cap.Dispose();
        }
    }
}
