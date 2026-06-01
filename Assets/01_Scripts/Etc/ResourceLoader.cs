using UnityEngine;

public static class ResourceLoader
{
    public static T Load<T>(string path) where T : Object
    {
        return Resources.Load<T>(path);
    }
}
