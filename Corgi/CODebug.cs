using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CODebug : MonoBehaviour
{

    public static Collider2D OverlapBox(Vector2 point, Vector2 size, float angle, LayerMask layerMask, Color color, bool drawGizmo=false, float duration=1f) {

        if(drawGizmo)
            DebugDrawBox(point, size, angle, color, duration);
        
        return Physics2D.OverlapBox(point, size, angle, layerMask);

    }
    
    public static void DebugDrawBox( Vector2 point, Vector2 size, float angle, Color color, float duration) {

        var orientation = Quaternion.Euler(0, 0, angle);

        // Basis vectors, half the size in each direction from the center.
        Vector2 right = orientation * Vector2.right * size.x/2f;
        Vector2 up = orientation * Vector2.up * size.y/2f;

        // Four box corners.
        var topLeft = point + up - right;
        var topRight = point + up + right;
        var bottomRight = point - up + right;
        var bottomLeft = point - up - right;

        // Now we've reduced the problem to drawing lines.
        Debug.DrawLine(topLeft, topRight, color, duration);
        Debug.DrawLine(topRight, bottomRight, color, duration);
        Debug.DrawLine(bottomRight, bottomLeft, color, duration);
        Debug.DrawLine(bottomLeft, topLeft, color, duration);
    }
    
}
