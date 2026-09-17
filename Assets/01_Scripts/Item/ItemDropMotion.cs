using System.Collections;
using UnityEngine;

public class ItemDropMotion : MonoBehaviour
{
    private Coroutine dropCoroutine;

    public void Play(Vector3 endPos, float jumpHeight, float duration)
    {
        if (dropCoroutine != null)
            StopCoroutine(dropCoroutine);

        dropCoroutine = StartCoroutine(DropRoutine(endPos, jumpHeight, duration));
    }

    private IEnumerator DropRoutine(Vector3 endPos, float jumpHeight, float duration)
    {
        Vector3 startPos = transform.position;

        duration = Mathf.Max(0.01f, duration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            // 출발 지점에서 도착 지점으로 이동
            Vector3 position = Vector3.Lerp(startPos, endPos, t);

            // t = 0 -> jumpHeight -> t = 1
            float height = 4f * jumpHeight * t * (1f - t);
            position.y += height;

            transform.position = position;

            yield return null;
        }

        transform.position = endPos;
        dropCoroutine = null;
    }
}
