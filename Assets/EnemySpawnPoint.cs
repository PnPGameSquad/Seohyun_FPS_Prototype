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
            // t ���� ���� 0~length ������ ���� ��ȯ��
            // t ���� ������ �� length������ t�� ��ȯ,
            // t�� length���� Ŀ���� ���� 0���� -, length���� + �ݺ�
            // ex) length = 2  =>    0 -> 1 -> 2 -> 1 -> 0 -> 1 -> ...
            color.a = Mathf.Lerp(1, 0, Mathf.PingPong(Time.time * fadeSpeed, 1));
            meshRenderer.material.color = color;

            yield return null;
        }
    }
}
