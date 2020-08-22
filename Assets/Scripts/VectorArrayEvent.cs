using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class VectorArrayEvent : UnityEvent<Vector3[]>
{
    
}

[Serializable]
public class IntEvent : UnityEvent<int> { }
