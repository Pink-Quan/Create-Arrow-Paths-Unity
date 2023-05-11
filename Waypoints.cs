using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Waypoints : MonoBehaviour
{
    public Vector3[] waypoints;

    public bool isDraw;
    public float lineThickness = 1;
    public Color color = Color.red;
}

[CustomEditor(typeof(Waypoints))]
public class WaypointsEditor : Editor
{
    private Waypoints wp;
    private void OnEnable()
    {
        wp = (Waypoints)target;
    }
    private void OnSceneGUI()
    {
        if (!wp.isDraw) return;

        Handles.color = wp.color;
        for (int i = 0; i < wp.waypoints.Length ; i++)
        {
            Handles.Label(wp.waypoints[i] + Vector3.up * 0.5f, i.ToString());

            if(i!=wp.waypoints.Length - 1) Handles.DrawLine(wp.waypoints[i], wp.waypoints[i + 1], wp.lineThickness);

            wp.waypoints[i] = Handles.FreeMoveHandle(wp.waypoints[i], Quaternion.identity, 0.2f, Vector3.one * 0.1f, Handles.DotHandleCap);
        }
    }
}
