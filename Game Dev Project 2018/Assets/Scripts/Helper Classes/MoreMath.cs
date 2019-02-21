using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoreMath {

    /// <summary>
    /// Rotates a point counterclockwise a number of degrees around the origin.
    /// Use a negative angle for clockwise rotations.
    /// </summary>
    /// <param name="point">The vector to rotate</param>
    /// <param name="degrees">The rotation angle</param>
    /// <returns>New vector after rotation.</returns>
	public static Vector2 RotateVector2(Vector2 point, float degrees) {
        float rad = degrees * Mathf.Deg2Rad;
        float s = Mathf.Sin(rad);
        float c = Mathf.Cos(rad);
        return new Vector2(
            point.x * c - point.y * s,
            point.x * s + point.y * c
        );
    }

    /// <summary>
    /// Rotates a point counterclockwise a number of degrees around a pivot point.
    /// Use a negative angle for clockwise rotations.
    /// </summary>
    /// <param name="point">The vector to rotate</param>
    /// <param name="pivot">The point around which to perform the rotation</param>
    /// <param name="degrees">The rotation angle</param>
    /// <returns>New vector after rotation.</returns>
    public static Vector2 RotateVector2(Vector2 point, Vector2 pivot, float degrees) {
        point -= pivot;
        float rad = degrees * Mathf.Deg2Rad;
        float s = Mathf.Sin(rad);
        float c = Mathf.Cos(rad);
        return pivot + new Vector2(     // Linear algebra is why school shootings happen
            point.x * c - point.y * s,
            point.x * s + point.y * c
        );
    }
}
