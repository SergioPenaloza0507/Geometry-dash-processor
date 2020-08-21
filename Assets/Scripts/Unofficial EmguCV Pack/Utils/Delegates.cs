using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UnofficialEmguCVPackForUnity.Utils.Delegates
{
    public delegate Image<TColor, TDepth> CVOperationAction<TColor, TDepth, ITColor , ITDepth>() where TColor : struct, IColor where TDepth : new() where ITColor : struct, IColor where ITDepth : new();

    [Serializable]
    public class EmguImageEvent<TColor,TDepth> : UnityEvent<Image<TColor, TDepth>> where TColor : struct, IColor where TDepth : new() { }
    
    [Serializable]
    public class Texture2DEvent : UnityEvent<Texture2D>{}
}
