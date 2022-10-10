using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public static class Utility
{

    public static float FitImageToScreen(float imageHeight, float imageWidth, bool matchWithBorder)
    {
        var t = 0.5F;

        if (matchWithBorder)
            t = Screen.height > Screen.width ? 0F : 1F;

        var divident = Screen.height * t + Screen.width * (1F - t);

        var divisor = imageHeight * t + imageWidth * (1F - t);

        return divident / divisor;

    }

    public static float ModuloOne(float x)
    {
        return x - Mathf.Floor(x);
    }

    public static int Modulo(int a, int _base)
    {
        if (_base == 0)
            return 0;

        var q = a / (float)_base;

        return Mathf.RoundToInt((q - Mathf.Floor(q)) * _base);
    }

    public static Color ToColor(this Vector3 v)
    {
        return new Color(v.x, v.y, v.z);
    }

    public static Vector3 GetMean(this Vector3[] source)
    {
        return source.Aggregate(new Vector3(0, 0, 0), (s, v) => s + v) / (float)source.Length;
    }

    public static Color GetMean(this Color[] source)
    {
        return source.Aggregate(new Color(0, 0, 0), (s, v) => s + v) / (float)source.Length;
    }

    public static float Variance(this Vector3[] source)
    {
        var mean = GetMean(source);

        var variance = source.Sum(v => (mean - v).sqrMagnitude) / (float)source.Length;

        return variance;
    }

    public static float Variance(this Color[] source)
    {
        var mean = GetMean(source);

        var variance = source.Sum(v => SqrDifference(mean, v)) / (float)source.Length;

        return variance;
    }

    public static float Variance(this float[] source)
    {
        var mean = source.Sum() / (float)source.Length;

        var variance = source.Sum(v => Mathf.Pow(v - mean, 2F)) / (float)source.Length;

        return variance;
    }

    public static Vector3 AlignWithGrid(Vector3 vector, float gridScale)
    {
        if (gridScale == 0F)
            return vector;

        var x = Mathf.Round(vector.x / gridScale);
        var y = Mathf.Round(vector.y / gridScale);
        var z = Mathf.Round(vector.z / gridScale);

        return new Vector3(x, y, z) * gridScale;
    }

    public static float SqrDifference(Color a, Color b)
    {
        return Mathf.Pow(b.r - a.r, 2F) + Mathf.Pow(b.g - a.g, 2F) + Mathf.Pow(b.b - a.b, 2F);
    }

    public static float Sigmoid(float x, float a, float xCenter = 0F, float yCenter = 0F)
    {
        return 1F / (1F + Mathf.Exp(-a * (x - xCenter))) + yCenter;
    }

    public static float HardSigmoid(float x, float absRange = 1F, float xCenter = 0F, float yCenter = 0F)
    {
        return Mathf.Max(0F, Mathf.Min(1F, (absRange * (x - xCenter) + 1F) / 2F)) + yCenter;
    }

    public static float SigmoidCustom(float x, float smooth, float steep, float xCenter = 0F, float yCenter = 0F)
    {
        smooth = Mathf.Clamp01(smooth);

        steep = Mathf.Clamp(steep, 1F, 10F);

        var a = steep * 2F * (smooth + 1F);

        var smoothSigmoid = Sigmoid(x, a, xCenter, yCenter);

        var hardSigmoid = HardSigmoid(x, a, xCenter, yCenter);

        return smooth * smoothSigmoid + (1F - smooth) * hardSigmoid;
    }

    public static Texture2D Rescale(this Texture2D texture2D, int targetX, int targetY)
    {
        RenderTexture rt = new RenderTexture(targetX, targetY, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        Texture2D result = new Texture2D(targetX, targetY);
        result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        result.Apply();
        return result;
    }

    // Given three collinear points p, q, r, the function checks if
    // point q lies on line segment 'pr'
    public static bool OnSegment(Vector2 p, Vector2 q, Vector2 r)
    {
        if (q.x <= Mathf.Max(p.x, r.x) && q.x >= Mathf.Min(p.x, r.x) &&
            q.y <= Mathf.Max(p.y, r.y) && q.y >= Mathf.Min(p.y, r.y))
            return true;

        return false;
    }

    // To find orientation of ordered triplet (p, q, r).
    // The function returns following values
    // 0 --> p, q and r are collinear
    // 1 --> Clockwise
    // 2 --> Counterclockwise
    public static int Orientation(Vector2 p, Vector2 q, Vector2 r)
    {
        // See https://www.geeksforgeeks.org/orientation-3-ordered-points/
        // for details of below formula.
        float val = (q.y - p.y) * (r.x - q.x) -
                (q.x - p.x) * (r.y - q.y);

        if (val == 0) return 0; // collinear

        return (val > 0) ? 1 : 2; // clock or counterclock wise
    }

    // The main function that returns true if line segment 'p1q1'
    // and 'p2q2' intersect.
    public static bool DoIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
    {
        // Find the four orientations needed for general and
        // special cases
        int o1 = Orientation(p1, q1, p2);
        int o2 = Orientation(p1, q1, q2);
        int o3 = Orientation(p2, q2, p1);
        int o4 = Orientation(p2, q2, q1);

        // General case
        if (o1 != o2 && o3 != o4)
            return true;
        /*
        // Special Cases
        // p1, q1 and p2 are collinear and p2 lies on segment p1q1
        if (o1 == 0 && OnSegment(p1, p2, q1)) return true;

        // p1, q1 and q2 are collinear and q2 lies on segment p1q1
        if (o2 == 0 && OnSegment(p1, q2, q1)) return true;

        // p2, q2 and p1 are collinear and p1 lies on segment p2q2
        if (o3 == 0 && OnSegment(p2, p1, q2)) return true;

        // p2, q2 and q1 are collinear and q1 lies on segment p2q2
        if (o4 == 0 && OnSegment(p2, q1, q2)) return true;*/

        return false; // Doesn't fall in any of the above cases
    }

    public static bool DoLineSegmentsIntersect(LineSegment AB1, LineSegment AB2)
    {
        var m1 = AB1.slope;
        var m2 = AB2.slope;

        if (m1 == m2)
            return false;

        var numer = AB1.b - AB2.b;

        var denom = m2 - m1;

        var intersectionX = numer / denom;

        var intersect = PointWithinBounds(intersectionX, AB1, AB2);

        return intersect;
    }

    public static bool ArePointsTooClose(Vector2 p1, Vector2 p2, float minAllowedDistance)
    {
        return (p2 - p1).sqrMagnitude < minAllowedDistance * minAllowedDistance;
    }

    public static bool PointWithinBounds(float x, LineSegment AB1, LineSegment AB2)
    {
        return Mathf.Min(AB1.A.x, AB1.B.x) < x
            && Mathf.Min(AB2.A.x, AB2.B.x) < x
            && Mathf.Max(AB1.A.x, AB1.B.x) > x
            && Mathf.Max(AB2.A.x, AB2.B.x) > x;
    }

    public static Vector3 ToVector3(this Point p, PathCreation.PathSpace pathSpace)
    {
        if (pathSpace == PathCreation.PathSpace.xz)
        {
            return new Vector3(p.x, 0F, p.y);
        }
        else
        {
            return new Vector3(p.x, p.y);
        }
    }

    public static Vector3 ToVector3(this Vector2 p, PathCreation.PathSpace pathSpace)
    {
        if (pathSpace == PathCreation.PathSpace.xz)
        {
            return new Vector3(p.x, 0F, p.y);
        }
        else
        {
            return new Vector3(p.x, p.y);
        }
    }
}

public struct Point
{
    public int x;
    public int y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Point(float x, float y)
    {
        this.x = Mathf.RoundToInt(x);
        this.y = Mathf.RoundToInt(y);
    }

    public static implicit operator Point(Vector2 v)
    {
        return new Point(v.x, v.y);
    }

    public static implicit operator Vector2(Point p)
    {
        return new Vector2(p.x, p.y);
    }


};

public class LineSegment
{
    public Vector2 A, B;

    public float slope => ((B.x - A.x) == 0) ? 0 : ((B.y - A.y) / (B.x - A.x));

    public float magnitude => Mathf.Sqrt(Mathf.Pow(B.x - A.x, 2F) + Mathf.Pow(B.y - A.y, 2F));

    public float b => A.y - slope * A.x;

    public LineSegment(Vector2 A, Vector2 B)
    {
        this.A = A;
        this.B = B;

        Debug.Log(A.ToString() + "; " + B.ToString());
        Debug.Log(string.Format("Slope: {0}; Magnitude: {1}; b: {2}", slope, magnitude, b));
    }

}