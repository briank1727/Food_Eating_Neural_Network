using UnityEngine;
public static class Util
{
    public static float RayIntersectDist(Vector2 creaturePosition, Vector2 direction, float maxRayDist, Vector2 foodPosition, float foodRadius)
    {
        direction = direction.normalized; // Ensure direction is a unit vector
        Vector2 toCircle = foodPosition - creaturePosition;

        // Project toCircle onto direction
        float tClosest = Vector2.Dot(toCircle, direction);

        // Find the closest point on the ray to the circle center
        Vector2 closestPoint = creaturePosition + tClosest * direction;

        // Compute distance from closest point to circle center
        float distToCenterSq = (closestPoint - foodPosition).sqrMagnitude;
        float radiusSq = foodRadius * foodRadius;

        // If closest approach is outside the circle, no intersection
        if (distToCenterSq > radiusSq)
            return maxRayDist;

        // Compute intersection distance using Pythagoras' theorem
        float offsetDist = Mathf.Sqrt(radiusSq - distToCenterSq);
        float tIntersect = tClosest - offsetDist;

        // If intersection is behind the ray start or beyond max distance, return maxRayDist
        if (tIntersect < 0 || tIntersect > maxRayDist)
            return maxRayDist;

        return tIntersect;
    }


    public static float GetRayDistanceToCircleEdge(Vector2 creaturePosition, Vector2 direction, float radius, float maxRayDist)
    {
        float px = creaturePosition.x;
        float py = creaturePosition.y;
        float dx = direction.x;
        float dy = direction.y;
        // Quadratic equation coefficients
        float a = dx * dx + dy * dy;
        float b = 2 * (px * dx + py * dy);
        float c = px * px + py * py - radius * radius;

        // Compute the discriminant
        float discriminant = b * b - 4 * a * c;

        // If the discriminant is negative, no intersection occurs
        if (discriminant < 0)
            return maxRayDist; // No intersection, return maxRayDist

        // Compute the two possible intersection distances
        float sqrtD = (float)Mathf.Sqrt(discriminant);
        float t1 = (-b - sqrtD) / (2 * a);
        float t2 = (-b + sqrtD) / (2 * a);

        // We take the smallest positive t value
        float tFinal = float.MaxValue;

        if (t1 > 0) tFinal = t1;
        if (t2 > 0 && t2 < tFinal) tFinal = t2;

        // If no positive t value found, return maxRayDist
        if (tFinal == float.MaxValue)
        {
            return maxRayDist;
        }

        // Clamp to maxRayDist
        return Mathf.Min(tFinal, maxRayDist);
    }

    public static Vector2 RotateVector(Vector2 v, float degrees)
    {
        // Convert degrees to radians
        float radians = degrees * Mathf.Deg2Rad;

        // Compute cosine and sine
        float cosTheta = Mathf.Cos(radians);
        float sinTheta = Mathf.Sin(radians);

        // Apply the 2D rotation matrix
        float newX = v.x * cosTheta - v.y * sinTheta;
        float newY = v.x * sinTheta + v.y * cosTheta;

        return new Vector2(newX, newY);
    }
}