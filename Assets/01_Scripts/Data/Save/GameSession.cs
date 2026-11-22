using UnityEngine;

public class GameSession : MonoBehaviour
{
    private static GameSession instance;

    public static GameSession Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject sessionObject = new("GameSession");
                instance = sessionObject.AddComponent<GameSession>();
            }

            return instance;
        }
    }

    public GameSaveData CurrentData { get; private set; }
    public bool HasData => CurrentData != null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic()
    {
        instance = null;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public void SetData(GameSaveData data)
    {
        CurrentData = data;
    }

    public void ClearData()
    {
        CurrentData = null;
    }
}