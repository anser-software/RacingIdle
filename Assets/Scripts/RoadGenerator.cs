using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using PathCreation.Examples;
using System.Linq;
using Newtonsoft.Json;
public class RoadGenerator : MonoBehaviour
{

    [SerializeField]
    private PathCreator pathCreator;

    [SerializeField]
    private RoadMeshCreator roadMeshCreator;

    [SerializeField]
    private int points;

    [SerializeField]
    private float minSegmentLength, maxSegmentLength, minDistanceBetweenPoints;

    [SerializeField, Range(0F, 180F)]
    private float minAngle, maxAngle;

    [SerializeField]
    private float intersectionHeightDif;

    private void Awake()
    {
        GeneratePoints();
    }

    private void GeneratePoints()
    {
        var positions = new List<Vector2>();

        var a = Random.Range(minAngle, maxAngle) * Mathf.Deg2Rad;

        var direction = new Vector2(Mathf.Cos(a), Mathf.Sin(a)); ;

        var lastPosition = Vector2.zero;

        positions.Add(lastPosition);

        var displacement = Vector2.zero;

        for (int i = 0; i < points; i++)
        {
            var currentAngle = Mathf.Atan2(direction.y, direction.x);

            for (int t = 0; t < 20; t++)
            {
                var angle = Mathf.Sign(Random.Range(-1F, 1F)) *  Random.Range(minAngle, maxAngle) * Mathf.Deg2Rad;

                direction = new Vector2(Mathf.Cos(currentAngle + angle), Mathf.Sin(currentAngle + angle));

                bool invalidPosition = false;

                displacement = direction * Random.Range(minSegmentLength, maxSegmentLength);

                var AB1 = new LineSegment(lastPosition, lastPosition + displacement);

                for (int j = 0; j < i; j++)
                {
                    var AB2 = new LineSegment(positions[j], positions[j + 1]);

                    if (Utility.DoLineSegmentsIntersect(AB1, AB2))
                    {
                        invalidPosition = true;
                        Debug.Log(string.Format("Intersection roughly at index {0}, position {1}", j, lastPosition + displacement * 0.5F));

                        Debug.DrawLine(AB1.A.ToVector3(PathSpace.xz), AB1.B.ToVector3(PathSpace.xz), Color.green, 10000F);

                        Debug.DrawLine(AB2.A.ToVector3(PathSpace.xz), AB2.B.ToVector3(PathSpace.xz), Color.blue, 10000F);
                    }
                    else if (Utility.ArePointsTooClose(positions[j], lastPosition + displacement, minDistanceBetweenPoints))
                    {
                        invalidPosition = true;
                        Debug.Log(string.Format("Points too close at index {0}, position ~{1}", j, lastPosition + displacement * 0.5F));
                    }
                }

                if (invalidPosition == false)
                    break;

                if(t == 19)
                {
                    Debug.Log("WTF");
                }
            }            

            lastPosition += displacement;

            positions.Add(lastPosition);
        }

        var pathPoints = new Vector3[positions.Count];

        var scaledHeights = new Dictionary<int, bool>();

        for (int i = 0; i < positions.Count; i++)
        {
            pathPoints[i] = new Vector3(positions[i].x, 0F, positions[i].y);

            var targetI = (i == positions.Count - 1) ? 0 : i + 1;

            var AB1 = new LineSegment(positions[i], positions[targetI]);

            for (int j = i+1; j < positions.Count; j++)
            {
                var targetJ = (j == positions.Count - 1) ? 0 : j + 1;

                var AB2 = new LineSegment(positions[j], positions[targetJ]);

                if (!scaledHeights.ContainsKey(j) && !scaledHeights.ContainsKey(i) && Utility.DoLineSegmentsIntersect(AB1, AB2))
                {
                    pathPoints[i].y += intersectionHeightDif;
                    scaledHeights[i] = true;
                    scaledHeights[j] = true;
                    break;
                }
            }
        }

        var xVariance = Mathf.Sqrt(Utility.Variance(pathPoints.Select(v => v.x).ToArray()));
        var zVariance = Mathf.Sqrt(Utility.Variance(pathPoints.Select(v => v.z).ToArray()));

        var mean = Utility.GetMean(pathPoints);

        var avgVariance = (xVariance + zVariance) / 2F;

        var xVarFactor = avgVariance / xVariance;
        var zVarFactor =  avgVariance / zVariance;

        for (int i = 0; i < pathPoints.Length; i++)
        {
            pathPoints[i] -= mean;
            pathPoints[i].x *= xVarFactor;
            pathPoints[i].z *= zVarFactor;
            pathPoints[i] += mean;
        }

        Debug.Log(string.Format("xVariance = {0}; zVariance = {1}", xVariance, zVariance));

        var bPath = new BezierPath(pathPoints, true);

        bPath.ControlPointMode = BezierPath.ControlMode.Automatic;

        bPath.AutoControlLength = 0.35F;

        bPath.ResetNormalAngles();

        pathCreator.bezierPath = bPath;

        pathCreator.TriggerPathUpdate();

        roadMeshCreator.TriggerUpdate();
    }



    private VertexPath GeneratePath(Vector2[] points, bool closedPath)
    {
        BezierPath bezierPath = new BezierPath(points, closedPath, PathSpace.xz);

        return new VertexPath(bezierPath, transform);
    }
}
