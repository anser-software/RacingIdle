using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PlotGraph : MonoBehaviour
{

    [SerializeField]
    private LineRenderer lineRenderer;

    private void Awake()
    {
        Global.Register<PlotGraph>(this);
    }

    public void Plot(float[] X, Func<float, float> function, float xScale, float yScale)
    {
        var points = new Vector3[X.Length];

        for (int i = 0; i < X.Length; i++)
        {
            var x = X[i];

            var y = function(x);

            points[i] = new Vector3(x * xScale, y * yScale, 0);
        }

        lineRenderer.positionCount = X.Length;

        lineRenderer.SetPositions(points);
    }

}
