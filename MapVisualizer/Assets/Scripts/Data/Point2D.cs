using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point2D : MonoBehaviour
{
    public int X { get; set; }

    public int Y { get; set; }

    public Point2D(int x, int y)
    {
        X = x;
        Y = y;
    }
}
