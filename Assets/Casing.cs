using System.Collections;
using UnityEngine;

public class Casing : MonoBehaviour
{
    [SerializeField]
    private float deactivateTime = 5.0f; // 탄피 등장 후 비활성화 되는 시간
    [SerializeField]
    private float casingSpin = 1.0f; // 탄피가 회전하는 속력 계수
    [SerializeField]
    private AudioClip[] audioClips; // 탄피가 충돌했을 때 재생되는 사운드

    private Rigidbody rigidbody3D;
    private AudioSource audioSource;
    private MemoryPool memoryPool;

    public void Setup(MemoryPool pool, Vector3 direction)
    {
        rigidbody3D = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        memoryPool = pool;

        // 탄피의 이동 속력와 회전 속력 설정
        rigidbody3D.velocity = new Vector3(direction.x, 1.0f, direction.z);
        rigidbody3D.angularVelocity = new Vector3(Random.Range(-casingSpin, casingSpin),
            Random.Range(-casingSpin, casingSpin),
            Random.Range(-casingSpin, casingSpin));

        // 탄피 자동 비활성화를 위한 코루틴 실행
        StartCoroutine("DeactivateAfterTime");
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 여러 개의 탄피 사운드 중 임의의 사운드 선택
        int index = Random.Range(0, audioClips.Length);
        audioSource.clip = audioClips[index];
        audioSource.Play();
    }

    private IEnumerator DeactivateAterTime()
    {
        yield return new WaitForSeconds(deactivateTime);

        memoryPool.DeactivatePoolItem(this.gameObject);
    }
}
