using UnityEngine;

public class GameSession : MonoBehaviour
{
    private static GameSession instance;

    public static GameSession Instance
    {
        get
        {
            // instance 없으면 새로 생성
            // 기존 instance 있으면 새 객체 생성 X
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

    // RuntimeInitializeOnLoadMethod : 런타임 초기화 과정에서 특정 함수 자동 호출
    // RuntimeInitializeLoadType.SubsystemRegistration : 런타임 초기화 이른 단계에서 실행되도록 지정
    // Play Mode 시작 -> 정적 변수 초기화
    // Domain Reload 비활성화 하면 이전 실행 정적 값 남아있을 수 있기 때문에 새로운 플레이 세션에서 이전 정적 참조 제거
    // => 정적 변수 참조 초기화 위함
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic()
    {
        instance = null;
    }

    private void Awake()
    {
        // 중복 객체 방지
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // GameSession을 싱글톤 인스턴스로 등록
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
