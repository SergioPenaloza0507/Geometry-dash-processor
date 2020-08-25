
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;
using UnofficialEmguCVPackForUnity.Utils;
using UnofficialEmguCVPackForUnity.Utils.Delegates;

public class Processor : MonoBehaviour
{
    public static Processor Instance { get; private set; }

    [Header("Calibration")]
    [Range(2, 50)]public int bufferSize = 50;
    [Range(0, 255)]public int valueThreshold = 30;
    public int interestingDefectSize = 1500;

    [Header("Visualization")]
    public bool OutputContour;
    public bool OutputConvexHull;

    [HideInInspector]
    public Texture2DEvent onOutputImage;

    [HideInInspector]
    public IntEvent onDetectedDefects;

    int bufferCount = 0;
    Image<Gray, byte> backgroundRemover;
    Image<Gray, byte> backgroundTemp;

    Matrix<byte> dilateMorphKernel;
    Matrix<byte> erodeMorphKernel;
    VectorOfPoint largest = null;
    VectorOfInt hull = null;


    private int lastDefectCount = 0;

    Texture2D outputtex;

    Image<Bgr, byte> outputImg;
    int defectCounter = 0;
    private void Awake()
    {
        Instance = this;

        CvInvoke.UseOpenCL = true;
        dilateMorphKernel = new Matrix<byte>(new Byte[3, 3] { { 0, 255, 0 }, { 255, 255, 255 }, { 0, 255, 0 } });
        erodeMorphKernel = new Matrix<byte>(new Byte[3, 3] { { 255, 0, 255 }, { 0, 0, 0 }, { 255, 0, 255 } });
    }

    public void ProcessImage(Image<Bgr, byte> input)
    {
        if (input == null)
            return;
        if (bufferCount < bufferSize)
        {
            if (bufferCount > 0)
            {
                var gray = input.Convert<Gray, byte>();
                backgroundRemover = backgroundTemp.AddWeighted(gray, 0.5, 0.5, 0);
                Destroy(outputtex);
                outputtex = backgroundRemover.ToBitmap().ToTexture2D();
                onOutputImage?.Invoke(outputtex);
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
            outputImg = dilated.Convert<Bgr, byte>();

            // Contour Detection
            Image<Gray, byte> smallerImage = dilated.Resize((int)((float)dilated.Width * 0.2), (int)((float)dilated.Height * 0.2), Emgu.CV.CvEnum.Inter.Linear);
            
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            double largestArea = 0;
            int indexLargest = 0;
            IOutputArray hierarchy = null;
            CvInvoke.FindContours(smallerImage, contours, hierarchy, Emgu.CV.CvEnum.RetrType.Tree, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
            
            //Separate largest contour
            for (int i = 0; i < contours.Size; i++)
            {
                double a = CvInvoke.ContourArea(contours[i], false);

                if (a > largestArea) indexLargest = i;

                if (i >= contours.Size - 1)
                {
                    largest = new VectorOfPoint(contours[indexLargest].ToArray());
                }
            }

            //Update Largest contour visualization if toggled
            if (OutputContour)
            {
                CvInvoke.DrawContours(outputImg, contours, indexLargest,new MCvScalar(0, 0, 255),2);
            }

            try
            {
                //Calculate convex Hull from largest contour
                hull = new VectorOfInt();
                CvInvoke.ConvexHull(largest, hull, false, false);

                //Update Convex Hull Visualization if toggled
                if (OutputConvexHull)
                {
                    DrawConvexHull();
                }

                //Get Convexity Defects
                UMat defects = new UMat();
                try
                {
                    CvInvoke.ConvexityDefects(largest, hull, defects);
                    int width = defects.Rows;
                    int height = defects.Row(0).Rows;
                    List<int> defectDistances = new List<int>();
                    //Get Defect distances
                    for (int i = 0; i < width; i++)
                    {
                        var row = defects.Row(i);
                        for (int j = 0; j < height; j++)
                        {
                            var info = row.Row(j);
                            int converted = BitConverter.ToInt32(info.Bytes,12);
                            defectDistances.Add(converted);
                        }
                    }

                    //Count distances of interest
                    defectCounter = 0;
                    for (int i = 0; i < defectDistances.Count; i++)
                    {
                        if (defectDistances[i] > interestingDefectSize)
                        {
                            defectCounter++;
                        }
                    }

                    //Fire count event (for UI)
                    onDetectedDefects?.Invoke(defectCounter);

                    //Handle Inputs
                    HandleInput();
                }
                catch (Exception e)
                {
                }
            }
            catch (Exception e)
            {
            }

            Destroy(outputtex);
            outputtex = outputImg.ToBitmap().ToTexture2D();
            onOutputImage?.Invoke(outputtex);
        }
    }

    private void HandleInput()
    {
        if (defectCounter < 2 && lastDefectCount >= 5)
        {
            InputSim.PressLeftClick();
            lastDefectCount = defectCounter;
            Debug.Log("PRESS");
        }
        if (defectCounter >= 5 && lastDefectCount < 3)
        {
            InputSim.ReleaseLeftClick();
            lastDefectCount = defectCounter;
            Debug.Log("RELEASE");
        }
    }

    public void ResetCalibration()
    {
        bufferCount = 0;
    }

    public void SetDefectThreshold(float val)
    {
        interestingDefectSize = (int)val;
    }

    void DrawConvexHull()
    {
        try
        {
            List<Point> vects = new List<Point>();
            for (int i = 0; i < hull.Size; i++)
            {
                vects.Add(largest[hull[i]]);
                
            }
            outputImg.DrawPolyline(vects.ToArray(), true, new Bgr(255, 255, 0), 2);
        }
        catch (Exception e)
        {

        }
    }
}
