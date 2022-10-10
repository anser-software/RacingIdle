using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using System.Linq;
using Newtonsoft.Json;
using PathCreation.Examples;
using System;
using Random = UnityEngine.Random;
public class PathManager : MonoBehaviour
{

    public VertexPath Path => pathCreator.path;

    public event Action OnPathChanged;

    public float PathLength => pathCreator.path.length;

    public int SegmentsAppendedCount => segmentsAppendedCount;

    [SerializeField]
    private PathCreator pathCreator;

    [SerializeField]
    private bool adjustToGrid;

    [SerializeField]
    private float gridScale;

    [SerializeField]
    private float pathSharpnessStep;

    [SerializeField]
    private PathSegment repeatedPathSegment;

    private PathSegment lastAppendedSegment;

    private Vector3 pathAveragePos;

    private float[] pathSharpness;

    private Dictionary<Car, float> progressPerCar = new Dictionary<Car, float>();

    private RoadMeshCreator roadMeshCreator;

    private int segmentsAppendedCount;

    private bool awaitNewSegments = false;

    private class SegmentCounter
    {
        public int segments;
    }

    private void Awake()
    {
        Global.Register<PathManager>(this);

        roadMeshCreator = pathCreator.GetComponent<RoadMeshCreator>();

        LoadRoadAsBase(2);

        var sc = SaveDataManager.LoadCustom<SegmentCounter>("Segments");
       
        int segments = sc == null ? 0 : sc.segments;

        Debug.Log("SEGMENTS = " + segments.ToString());

        for (int i = 0; i < segments; i++)
        {
            LoadRoadAsSegment(6);
        }

        awaitNewSegments = true;
    }

    private void Start()
    {
        if (gridScale <= 0F)
            adjustToGrid = false;

        var path = pathCreator.bezierPath;

        if (adjustToGrid)
        {
            for (int i = 0; i < path.NumPoints; i++)
            {
                var pos = path.GetPoint(i);           

                path.SetPoint(i, Utility.AlignWithGrid(pos, gridScale));
            }
        }

        pathCreator.path.localPoints.ToList().ForEach(p => pathAveragePos += p);
        pathAveragePos /= pathCreator.path.NumPoints;

        var sharpnessResolution = Mathf.FloorToInt(pathCreator.path.length / pathSharpnessStep);

        pathSharpness = new float[sharpnessResolution];

        for (int i = 0; i < sharpnessResolution; i++)
        {
            var dx = pathSharpnessStep;

            var d1 = pathCreator.path.GetDirectionAtDistance(i * dx, EndOfPathInstruction.Loop);

            var d2 = pathCreator.path.GetDirectionAtDistance((i + 1) * dx, EndOfPathInstruction.Loop);

            var dy = (d2 - d1).magnitude;

            pathSharpness[i] = dy / dx;
        }

        progressPerCar = new Dictionary<Car, float>();
    }

    private void LoadRoadAsBase(int index)
    {
        var roadBezierPathJson = Resources.Load<TextAsset>("Roads/Road_" + index.ToString());

        if (roadBezierPathJson == null)
        {
            Debug.LogError(string.Format("Road with index {0} not found", index.ToString()));
            return;
        }

        var bPath = JsonConvert.DeserializeObject<BezierPath>(roadBezierPathJson.text);

        var firstIndex = repeatedPathSegment.firstPointIndex;

        var count = repeatedPathSegment.count;

        repeatedPathSegment.points = bPath.points.GetRange(firstIndex, count);

        //bPath.isClosed = false;

        //bPath.points.RemoveRange(firstIndex, count);

        pathCreator.bezierPath = bPath;

        pathCreator.TriggerPathUpdate();

        roadMeshCreator.TriggerUpdate();

        OnPathChanged?.Invoke();
    }

    public void LoadRoadAsSegment(int index)
    {
        var roadBezierPathJson = Resources.Load<TextAsset>("Roads/Road_" + index.ToString());

        if (roadBezierPathJson == null)
        {
            Debug.LogError(string.Format("Road with index {0} not found", index.ToString()));
            return;
        }


        var bPath = JsonConvert.DeserializeObject<BezierPath>(roadBezierPathJson.text);

        var points = pathCreator.bezierPath.points;

        var firstIndex = repeatedPathSegment.firstPointIndex + 15 * segmentsAppendedCount;

        var count = repeatedPathSegment.count;

        points.RemoveRange(firstIndex, count);

        Vector3 offset = Vector3.forward * 32F * (segmentsAppendedCount + 1);

        var offsetbPath = bPath.points.Select(p => p + offset).ToList();

        offsetbPath.RemoveRange(offsetbPath.Count - repeatedPathSegment.trimEnd, repeatedPathSegment.trimEnd);

        points.InsertRange(firstIndex, offsetbPath);

        pathCreator.bezierPath.points = points;

        pathCreator.bezierPath.autoControlLength = 0.12F;

        pathCreator.bezierPath.AutoSetAllControlPoints();

        pathCreator.bezierPath.NotifyPathModified();

        segmentsAppendedCount++;

        if (awaitNewSegments)
            SaveDataManager.SaveCustom<SegmentCounter>(new SegmentCounter() { segments = segmentsAppendedCount }, "Segments");


        pathCreator.TriggerPathUpdate();

        roadMeshCreator.TriggerUpdate();

        OnPathChanged?.Invoke();
    }

    public void LoadRoad(int index)
    {
        var roadBezierPathJson = Resources.Load<TextAsset>("Roads/Road_" + index.ToString());

        if (roadBezierPathJson == null)
        {
            Debug.LogError(string.Format("Road with index {0} not found", index.ToString()));
            return;
        }

        var bPath = JsonConvert.DeserializeObject<BezierPath>(roadBezierPathJson.text);

        pathCreator.bezierPath = bPath;

        pathCreator.TriggerPathUpdate();

        roadMeshCreator.TriggerUpdate();

        OnPathChanged?.Invoke();
    }

    public void RegisterProgress(Car car, float progress)
    {
        progressPerCar[car] = progress;
    }

    public void UnregisterCar(Car car)
    {
        progressPerCar.Remove(car);
    }

    public float GetFreeProgressOnPath()
    {
        if(progressPerCar.Count == 0)
        {
            return Random.Range(0F, 1F);
        }

        var takenProgresses = progressPerCar.Values.ToList();

        return GetFreeProgressOnPath(takenProgresses);
    }

    public float GetFreeProgressOnPath(List<float> takenProgresses)
    {
        takenProgresses = takenProgresses.OrderBy(p => p).ToList();

        if (takenProgresses.Count == 0)
            return Random.Range(0F, 1F);
        else if (takenProgresses.Count == 1)
            return Utility.ModuloOne(takenProgresses[0] + 0.5F);

        float[] intervalLengths = new float[takenProgresses.Count];

        for (int i = 0; i < takenProgresses.Count; i++)
        {
            var i2 = (i == takenProgresses.Count - 1) ? 0 : (i + 1);

            var d1 = Utility.ModuloOne(takenProgresses[i] - takenProgresses[i2]);
            var d2 = 1F - d1;
            var intervalLength = Mathf.Min(d1, d2);

            intervalLengths[i] = intervalLength;
        }

        var longestIntervalIndex = intervalLengths.ToList().IndexOf(intervalLengths.Max());

        var startProgress = takenProgresses[longestIntervalIndex];

        var endProgress = takenProgresses[longestIntervalIndex] + intervalLengths[longestIntervalIndex];

        return Utility.ModuloOne(takenProgresses[longestIntervalIndex] + intervalLengths[longestIntervalIndex] * 0.5F);
    }

    public Vector3 GetPathAverageWorldPos(bool recalculate = false)
    {
        if(pathAveragePos == default(Vector3) || recalculate)
        {
            pathCreator.path.localPoints.ToList().ForEach(p => pathAveragePos += p);
            pathAveragePos /= pathCreator.path.NumPoints;
        }

        return pathAveragePos + pathCreator.transform.position;
    }

    public Vector3 GetPathPosition(float progress) 
        => pathCreator.path.GetPointAtTime(Utility.ModuloOne(progress), EndOfPathInstruction.Loop);

    public Quaternion GetPathRotation(float progress)
        => pathCreator.path.GetRotation(Utility.ModuloOne(progress), EndOfPathInstruction.Loop);

    public float GetPathSharpness(float progress)
    => pathSharpness[Mathf.FloorToInt((pathSharpness.Length - 1F) * Utility.ModuloOne(progress))];

    public float DistanceToProgress(float distance) => Utility.ModuloOne(distance / pathCreator.path.length);

    public float ProgressToDistance(float progress) => progress * pathCreator.path.length;

    public Vector3 GetPathDirection(float progress)
    => pathCreator.path.GetDirection(Utility.ModuloOne(progress), EndOfPathInstruction.Loop);

}

[Serializable]
public struct PathSegment
{
    public int firstPointIndex, lastPointIndex, trimEnd;

    public int count => lastPointIndex - firstPointIndex + 1;

    [HideInInspector]
    public List<Vector3> points;
}