/*
 * This template is originally from
 * https://github.com/DanielWillett/SampleRocketPlugin
 */

namespace SampleRocketPlugin.Util;
internal static class LevelUtil
{
    public static LocationDevkitNode? GetClosestLocation(Vector3 point)
    {
        IReadOnlyList<LocationDevkitNode> list = LocationDevkitNodeSystem.Get().GetAllNodes();
        int index = -1;
        float smallest = -1f;
        for (int i = 0; i < list.Count; ++i)
        {
            float amt = (point - list[i].transform.position).sqrMagnitude;
            if (smallest < 0f || amt < smallest)
            {
                index = i;
                smallest = amt;
            }
        }

        if (index == -1)
            return null;
        return list[index];
    }

    /// <param name="minHeight">Minimum value for the y-coordinate.</param>
    /// <returns>The y value of the top of the level, accounting for objects.</returns>
    internal static float GetHeight(Vector3 point, float minHeight = 0f)
    {
        float height;
        if (Physics.Raycast(new Ray(new Vector3(point.x, Level.HEIGHT, point.z), Vector3.down), out RaycastHit hit, Level.HEIGHT, RayMasks.BLOCK_COLLISION))
        {
            height = hit.point.y;
            return !float.IsNaN(minHeight) ? Mathf.Max(height, minHeight) : height;
        }

        height = LevelGround.getHeight(point);
        return !float.IsNaN(minHeight) ? Mathf.Max(height, minHeight) : height;
    }
}
