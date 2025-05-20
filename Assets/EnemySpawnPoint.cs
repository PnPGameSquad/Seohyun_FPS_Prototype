using System.Collections;
using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    [SerializeField]
    private float fadeSpeed = 4;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnEnable()
    {
        StartCoroutine("OnFadeEffect");
    }

    private void OnDisable()
    {
        StopCoroutine("OnFadeEffect");
    }

    private IEnumerator OnFadeEffect()
    {
        while (true)
        {
            Color color = meshRenderer.material.color;
            // float f = Mathf.PingPong(float t, float length);
            // t 값에 따라 0~length 사이의 값이 반환됨
            // t 값이 증가할 때 length까지는 t값 반환,
            // t가 length보다 커졌을 때는 0까지 -, length까지 + 반복
            // ex) length = 2  =>    0 -> 1 -> 2 -> 1 -> 0 -> 1 -> ...
            color.a = Mathf.Lerp(1, 0, Mathf.PingPong(Time.time * fadeSpeed, 1));
            meshRenderer.material.color = color;

            yield return null;
        }
    }
}
