using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EnemyPath : MonoBehaviour
{
    public List<Transform> waypoints = new List<Transform>(); // List of waypoints
    [SerializeField] private bool alwaysDrawPath = true; // Always draw the path in the editor
    [SerializeField] private bool drawAsLoop = false; // Draw the path as a loop
    [SerializeField] private bool drawNumbers = true; // Draw waypoint numbers
    public Color debugColour = Color.white; // Colour of the path and labels

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (alwaysDrawPath)
        {
            DrawPath();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!alwaysDrawPath)
        {
            DrawPath();
        }
    }

    private void DrawPath()
    {
        if (waypoints == null || waypoints.Count == 0)
            return;
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null)
                continue;
            if (drawNumbers)
            {
                GUIStyle labelStyle = new GUIStyle();
                labelStyle.fontSize = 15;
                labelStyle.normal.textColor = debugColour;
                Handles.Label(waypoints[i].position + Vector3.up * 0.5f, i.ToString(), labelStyle);
            }
            if (i >= 1 && waypoints[i - 1] != null)
            {
                Gizmos.color = debugColour;
                Gizmos.DrawLine(waypoints[i - 1].position, waypoints[i].position);
                if (drawAsLoop && i == waypoints.Count - 1)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
                }
            }
        }
    }
#endif
}