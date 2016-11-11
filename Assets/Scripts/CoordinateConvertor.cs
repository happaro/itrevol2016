using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class CoordinateConvertor
{
    public static Point IsoToSimple(Vector3 input)
    {
        int i = Mathf.RoundToInt(2 * input.y - input.x);
        int j = Mathf.RoundToInt(2 * input.x + i);
        return new Point(i, j);
    }
    public static Vector3 SimpleToIso(Point input)
    {
        int i = input.x;
        int j = input.y;
        int xPos = (int)((j - i) - (j * 1 + i * 0.5));
        int yPos = (int)((j + i) + (j * 1 - i * 0.5));
        return new Vector3((j - i) * 0.5f, (j + i) * 0.25f, Mathf.Sqrt(xPos * xPos + yPos * yPos));
    }
}

public struct Point
{
    public int x, y;
    public Point(int px, int py)
    {
        x = px;
        y = py;
    }
}

