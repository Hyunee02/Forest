using UnityEngine;

public static class Helper
{
    public static Transform FindChildByName(this Transform transform, string name)
    {
        Transform[] transforms = transform.GetComponentsInChildren<Transform>();

        foreach (Transform t in transforms)
        {
            if (t.name.Equals(name))
                return t;
        }

        return null;
    }
}
