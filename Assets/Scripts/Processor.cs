
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;
using UnofficialEmguCVPackForUnity.Core.VideoCaptureGrabbers;
using UnofficialEmguCVPackForUnity.Utils;

public class Processor : MonoBehaviour
{
    [SerializeField] private BgrByteCaptureGrabber grabber;
    private int state;
    [Header("Output")]
    [SerializeField] RawImage img;
    [SerializeField] LineRenderer rend;

    [Header("Calibration")]
    [Header("Stage One, Background Removal")]
    [SerializeField] [Range(2, 50)] int bufferSize;
    [SerializeField] [Range(0, 255)] int valueThreshold;
    [SerializeField] int interestingDefectSize = 1500;

    int bufferCount = 0;
    Image<Gray, byte> backgroundRemover;
    Image<Gray, byte> backgroundTemp;

    Matrix<byte> dilateMorphKernel;
    Matrix<byte> erodeMorphKernel;
    List<PointF> contourPoints = new List<PointF>();
    List<PointF> chPoints = new List<PointF>();
    VectorOfPoint largest = null;

    private int lastDefectCount = 0;

    private void Awake()
    {
        CvInvoke.UseOpenCL = true;
        dilateMorphKernel = new Matrix<byte>(new Byte[3, 3] { { 0, 255, 0 }, { 255, 255, 255 }, { 0, 255, 0 } });
        erodeMorphKernel = new Matrix<byte>(new Byte[3, 3] { { 255, 0, 255 }, { 0, 0, 0 }, { 255, 0, 255 } });

    }
    private void OnEnable()
    {
        print(grabber);
        grabber.onProcessableframeCaptured.AddListener(ProcessImage);
    }

    private void OnDisable()
    {
        grabber.onProcessableframeCaptured.RemoveListener(ProcessImage);
    }

    private void ProcessImage(Image<Bgr, byte> input)
    {
        if (input == null)
            return;
        if (bufferCount < bufferSize)
        {
            if (bufferCount > 0)
            {
                var gray = input.Convert<Gray, byte>();
                backgroundRemover = backgroundTemp.AddWeighted(gray, 0.5, 0.5, 0);
                //img.texture = backgroundRemover.AsBitmap().ToTexture2D();
            }
            else
            {
                backgroundTemp = input.Convert<Gray, byte>();
            }
            bufferCount++;

        }
        else
        {

            // Segmentation
            var mask = backgroundRemover.Sub(input.Convert<Gray, byte>()).ThresholdBinary(new Gray(valueThreshold), new Gray(255));
            var eroded = mask.MorphologyEx(Emgu.CV.CvEnum.MorphOp.Erode, erodeMorphKernel, new System.Drawing.Point(1, 1), 5, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar());
            var dilated = eroded.MorphologyEx(Emgu.CV.CvEnum.MorphOp.Dilate, dilateMorphKernel, new System.Drawing.Point(1, 1), 7, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar());


            // Contour Detection
            Image<Gray, byte> smallerImage = dilated.Resize((int)((float)dilated.Width * 0.2), (int)((float)dilated.Height * 0.2), Emgu.CV.CvEnum.Inter.Linear);
            //img.texture = smallerImage.AsBitmap().ToTexture2D();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            double largestArea = 0;
            int indexLargest = 0;
            IOutputArray hierarchy = null;
            CvInvoke.FindContours(smallerImage, contours, hierarchy, Emgu.CV.CvEnum.RetrType.Tree, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
            for (int i = 0; i < contours.Size; i++)
            {
                double a = CvInvoke.ContourArea(contours[i], false);

                if (a > largestArea) indexLargest = i;

                if (i >= contours.Size - 1)
                {
                    largest = new VectorOfPoint(contours[indexLargest].ToArray());
                }
            }

            try
            {
                var arr = largest.ToArray();

                for (int i = 0; i < arr.Length; i++)
                {
                    contourPoints.Add(arr[i]);
                }
                VectorOfInt hull = new VectorOfInt();
                CvInvoke.ConvexHull(largest, hull, false, false);
                UMat defects = new UMat();
                try
                {
                    CvInvoke.ConvexityDefects(largest, hull, defects);

                    int devest = 0;
                    int avg = 0;

                    int width = defects.Rows;
                    int height = defects.Row(0).Rows;

                    List<int> defectDistances = new List<int>();
                    //Discard non interesting defects
                    for (int i = 0; i < width; i++)
                    {
                        var row = defects.Row(i);
                        for (int j = 0; j < height; j++)
                        {
                            var info = row.Row(j);
                            int converted = BitConverter.ToInt32(info.Bytes,12);
                            //Debug.Log($"Test: {converted}");
                            avg += converted / width * height;
                            defectDistances.Add(converted);
                        }
                    }

                    //Get Standard Deviation
                    for (int i = 0; i < defectDistances.Count; i++)
                    {
                        devest = defectDistances[i] - avg;
                    }
                    devest = (int)(Math.Sqrt(Math.Pow(devest, 2) / defectDistances.Count));
                    //Debug.Log($"Standard deviation: {devest}");
                    //Debug.Log($"average minus two standard deviations: {avg - devest * 2}");
                    int defectCounter = 0;
                    for (int i = 0; i < defectDistances.Count; i++)
                    {
                        if (defectDistances[i] > interestingDefectSize)
                        {
                            defectCounter++;
                        }
                    }

                    if(defectCounter < 3 && lastDefectCount >= 5 )
                    {
                        InputSim.PressLeftClick();
                        lastDefectCount = defectCounter;
                        Debug.Log("PRESS");
                    }
                    if(defectCounter >= 5 && lastDefectCount < 3)
                    {
                        InputSim.ReleaseLeftClick();
                        lastDefectCount = defectCounter;
                        Debug.Log("RELEASE");
                    }

                    //defects.Dispose();
                    //hull.Dispose();
                    //contours.Dispose();
                    //largest.Dispose();

                   //Debug.Log($"defect size {defectCounter}");
                }
                catch (Exception e)
                {
                    //Debug.LogError($"Error calculating Defects{e}");
                }
            }
            catch (Exception e)
            {
                //if (rend != null) rend.positionCount = 0;
                //Debug.Log(e);
            }


            chPoints.Clear();
            contourPoints.Clear();
        }
    }
}
