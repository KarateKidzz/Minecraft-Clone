using UnityEngine;

public enum BlockFace
{
    Top,
    Bottom,
    Left,
    Right,
    Forward,
    Back
}

/// <summary>
/// Rounds the <see cref="Vector3"/> to the nearest integer position
/// </summary>
public static class RoundedVector
{
    /// <summary>
    /// Rounds the <see cref="Vector3"/> to the nearest integer position
    /// </summary>
    /// <returns>The round.</returns>
    /// <param name="original">Original.</param>
    public static Vector3 Round (this Vector3 original)
    {
        return new Vector3(Mathf.Round(original.x), Mathf.Round(original.y), Mathf.Round(original.z));
    }
}
