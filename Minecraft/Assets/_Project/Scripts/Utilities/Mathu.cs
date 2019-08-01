using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Math utilities
/// </summary>
public static class Mathu
{
    public static int mod(int x, int m)
    {
        int r = x % m;
        return r < 0 ? r + m : r;
    }
}
