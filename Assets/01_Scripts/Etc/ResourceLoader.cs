using UnityEngine;

public static class ResourceLoader
{
    public static T Load<T>(string path) where T : Object
    {
        if (string.IsNullOrEmpty(path))
            return null;

        return Resources.Load<T>(path);
    }
}
