using System.Collections;
using UnityEngine;

public class ItemDropMotion : MonoBehaviour
{
    private Coroutine dropCoroutine;

    public void Play(Vector3 endPos, float jumpHeight, float duration)
    {
        if (dropCoroutine != null)
        {
            StopCoroutine(dropCoroutine);
            dropCoroutine = null;
        }

        if (duration <= 0f)
        {
            transform.position = endPos;
            return;
        }

        dropCoroutine = StartCoroutine(DropRoutine(endPos, jumpHeight, duration));
    }

    private IEnumerator DropRoutine(Vector3 endPos, float jumpHeight, float duration)
    {
        Vector3 startPos = transform.position;

        // 흐른 시간
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 진행도 0~1로 변환
            float t = Mathf.Clamp01(elapsed / duration);

            // t로 보정한 값
            Vector3 position = Vector3.Lerp(startPos, endPos, t);

            // 최고점 0.25이기 때문에 편하게 사용하기 위해 4 곱해줌
            // t * (1f * t) => 작음 -> 커짐 -> 작아짐
            // 진행도가 0 -> 1로 갈 동안 높이를 0 -> 1 -> 0으로 만들어주는 포물선 공식
            float arc = 4f * t * (1f - t);

            // 중간에 jumpHeight만큼 위로 올라가게 Y값 추가
            position.y += arc * jumpHeight;

            transform.position = position;

            yield return null;
        }

        // 마지막 위치 맞춰주기
        transform.position = endPos;
        dropCoroutine = null;
    }
}
