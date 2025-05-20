using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class AmmoEvent : UnityEngine.Events.UnityEvent<int, int> { } // 탄약 이벤트
[System.Serializable]
public class MagazineEvent : UnityEngine.Events.UnityEvent<int> { } // 탄창 이벤트

public class WeaponAssaultRifle : MonoBehaviour
{
    [HideInInspector]
    public AmmoEvent onAmmoEvent = new AmmoEvent(); // 탄약 이벤트

    [HideInInspector]
    public MagazineEvent onMagazineEvent = new MagazineEvent(); // 탄창 이벤트

    [Header("Fire Effects")]
    [SerializeField]
    private GameObject muzzleFlashEffect; // 총구 화염 효과

    [Header("Spawn Point")]
    [SerializeField]
    private Transform casingSpawnPoint; // 탄피 생성 위치
    [SerializeField]
    private Transform bulletSpawnPoint; // 총알 생성 위치

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipTakeOutWeapon; // 무기 장착 사운드
    [SerializeField]
    private AudioClip audioClipFire; // 총 발사 사운드
    [SerializeField]
    private AudioClip audioClipReload;

    [Header("Weapon Setting")]
    [SerializeField]
    private WeaponSetting weaponSetting; // 무기 설정

    [Header("Aim UI")]
    [SerializeField]
    private Image imageAim; //default/aim mode에 따라 Aim이미지 활성/비활성화

    private float lastAttackTime = 0; // 마지막 공격 시간
    private bool isReload = false; // 재장전 여부
    private bool isAttack = false;         // 공격 여부 체크용
    private bool isModeChange = false;     // 모드 전환 여부 체크용
    private float defaultModeFOV = 60;     // 기본모드에서의 카메라 FOV
    private float aimModeFOV = 30;         // AIM모드에서의 카메라 FOV

    private AudioSource audioSource; // 사운드 재생 컴포넌트
    private PlayerAnimatorController animator; // 애니메이션 재생 제어
    private CasingMemoryPool casingMemoryPool; //탄피 생성 후 활성/비활성 관리
    private ImpactMemoryPool impactMemoryPool; // 공격 효과 생성 후 활성/비활성 관리
    private Camera mainCamera; //광선 발사

    //외부에서 필요한 정보를 열람하기 위해 정의한 Get Property's
    public WeaponName weaponName => weaponSetting.weaponName; // 무기 이름
    public int CurrentAmmo => weaponSetting.currentMagazine;
    public int MaxAmmo => weaponSetting.maxMagazine;

    public int WeaponName { get; internal set; }
    public int MaxMagazine { get; internal set; }
    public int CurrentMagazine { get; internal set; }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<PlayerAnimatorController>();
        casingMemoryPool = GetComponent<CasingMemoryPool>();
        impactMemoryPool = GetComponent<ImpactMemoryPool>();
        mainCamera = Camera.main;

        //처음 탄창 수는 최대로 설정
        weaponSetting.currentMagazine = weaponSetting.maxMagazine;
        // 처음 탄 수는 최대로 설정
        weaponSetting.currentAmmo = weaponSetting.maxAmmo;
    }

    private void OnEnable()
    {
        // 무기 장착 사운드 재생
        PlaySound(audioClipTakeOutWeapon);
        // 총구 화염 효과 비활성
        muzzleFlashEffect.SetActive(false);

        //무기가 활성화될 때 탄창 수 정보를 갱신한다
        onMagazineEvent.Invoke(weaponSetting.currentMagazine);

        //무기가 활성화될 때 해당 무기의 탄 수 정보를 갱신한다
        onAmmoEvent.Invoke(weaponSetting.currentAmmo, weaponSetting.maxAmmo);

        ResetVariables();
    }
    public void StartWeponAction(int type = 0)
    {
        if (isReload == true || weaponSetting.currentMagazine <= 0) // 재장전 중이면 공격 안 함
        {
            return;
        }
        // 모드 전환중이면 무기 액션을 할 수 없다
        if (isModeChange == true)
        {
            return;
        }
        if (type == 0)
        {
            // float이기 때문에 명시적 비교 사용 (0이 아니면 true)
            if (weaponSetting.isAutomaticAttack != 0f) // 연속 공격일 경우
            {
                isAttack = true;
                StartCoroutine("OnAttackLoop"); // 공격 코루틴 시작
            }
            else
            {
                OnAttack(GetCasingMemoryPool()); // 단일 공격
            }
        }
        // 마우스 오른쪽 클릭 (모드 전환)
        else
        {
            //공격 중일때 모드 전환을 할 수 없다
            if (isAttack == true)
            {
                return;
            }

            StartCoroutine("OnModeChange");
        }
    }

    public void StopWeponAction(int type = 0)
    {
        if (type == 0) // 연속 공격일 경우
        {
            isAttack = false;
            StopCoroutine("OnAttackLoop");
        }
    }

    public void StartReload()
    {
        if (isReload == true) // 재장전 중이면 재장전 안 함
        {
            return;
        }
        // 현재 재장전 중이면 재장전 불가능
        StopWeponAction();

        StartCoroutine("OnReload");
    }

    private IEnumerator OnAttackLoop()
    {
        while (true)
        {
            OnAttack(GetCasingMemoryPool());
            yield return null;
        }
    }

    private CasingMemoryPool GetCasingMemoryPool()
    {
        return casingMemoryPool;
    }

    private void OnAttack(CasingMemoryPool casingMemoryPool)
    {
        if (Time.time - lastAttackTime > weaponSetting.attackRate) // 공격 속도에 따라 공격
        {
            if (animator.MoveSpeed > 0.5f) // 이동 속도가 0.5보다 크면 공격 안 함
            {
                return;
            }

            lastAttackTime = Time.time; // 마지막 공격 시간 갱신
            if (weaponSetting.currentAmmo <= 0) // 탄약이 없으면 공격 안 함
            {
                return;
            }
            //공격시 currentAmmo 1 감소
            weaponSetting.currentAmmo --;
            onAmmoEvent.Invoke(weaponSetting.currentAmmo, weaponSetting.maxAmmo);

            //무기 애니메이션 재생 (모드에 따라 AimFire or Fire 애니메이션 재생)
            //animator.Play("Fire", -1, 0);
            string animation = animator.AimModeIs == true ? "AimFire" : "Fire";
            animator.Play(animation, -1, 0);
            //총구 이펙트 재생(default mode일때만 재생)
            if (animator.CurrentStateIs(animation) == false)
            {
                StartCoroutine("OnMuzzleFlashEffect");
            }

            animator.Play("Fire", -1, 0); // 공격 애니메이션 재생
            StartCoroutine("OnMuzzleFlashEffect"); // 총구 화염 효과 재생
            PlaySound(audioClipFire); // 총 발사 사운드 재생
            casingMemoryPool.SpawnCasing(casingSpawnPoint.position, transform.right); // 탄피 생성

            TwoStepRaycast(); //광선을 발사해 원하는 위치 공격 (+Impact Effect)
        }
    }

    private IEnumerator OnMuzzleFlashEffect()
    {
        muzzleFlashEffect.SetActive(true); // 총구 화염 효과 활성화
        yield return new WaitForSeconds(weaponSetting.attackRate * 0.3f); // 0.1초 대기
        muzzleFlashEffect.SetActive(false); // 총구 화염 효과 비활성화
    }

    private IEnumerator OnReload()
    {
        isReload = true;

        // 재장전 애니메이션, 사운드 재생
        animator.OnReload();
        PlaySound(audioClipReload);

        while (true)
        {
            // 사운드가 재생중이 아니고, 현재 애니메이션이 Movement이면
            // 재장전 애니메이션(사운드) 재생이 종료되었다는 뜻
            if (audioSource.isPlaying == false && animator.CurrentAnimationIs("Movement"))
            {
                isReload = false;

                //현재 탄창 수를 1 감소시키고, 바뀐 탄창 정보를 Text UI에 업데이트
                weaponSetting.currentMagazine--;
                onMagazineEvent.Invoke(weaponSetting.currentMagazine);

                // 현재 탄 수를 최대로 설정하고, 바뀐 탄 수 정보를 Text UI에 업데이트
                weaponSetting.currentAmmo = weaponSetting.maxAmmo;
                onAmmoEvent.Invoke(weaponSetting.currentAmmo, weaponSetting.maxAmmo);

                yield break;
            }

            yield return null;
        }
    }
    private void PlaySound(AudioClip clip)
    {
        audioSource.Stop();       // 기존에 재생중인 사운드를 정지하고,
        audioSource.clip = clip;  // 새로운 사운드 clip으로 교체 후
        audioSource.Play();       // 사운드 재생
    }

    internal int GetWeaponSetting()
    {
        throw new NotImplementedException();
    }

    private void TwoStepRaycast()
    {
        Ray ray;
        RaycastHit hit;
        Vector3 targetPoint = Vector3.zero;

        // 화면의 중앙 좌표 (Aim 기준으로 Raycast 연산)
        ray = mainCamera.ViewportPointToRay(Vector2.one * 0.5f);

        // 공격 사거리(attackDistance) 안에 부딪히는 오브젝트가 있으면 targetPoint는 광선에 부딪힌 위치
        if (Physics.Raycast(ray, out hit, weaponSetting.attackDistance))
        {
            targetPoint = hit.point;
        }
        // 공격 사거리 안에 부딪히는 오브젝트가 없으면 targetPoint는 최대 사거리 위치
        else
        {
            targetPoint = ray.origin + ray.direction * weaponSetting.attackDistance;
        }

        Debug.DrawRay(ray.origin, ray.direction * weaponSetting.attackDistance, Color.red);

        // 첫번째 Raycast 연산으로 얻어진 targetPoint를 목표지점으로 설정하고,
        // 총구를 시작지점으로 하여 Raycast 연산
        Vector3 attackDirection = (targetPoint - bulletSpawnPoint.position).normalized;
        if (Physics.Raycast(bulletSpawnPoint.position, attackDirection, out hit, weaponSetting.attackDistance))
        {
            impactMemoryPool.SpawnImpact(hit);

            if (hit.transform.CompareTag("ImpactEnemy"))
            {
                hit.transform.GetComponent<EnemyFSM>().TakeDamage(weaponSetting.damage);
            }
        }

        Debug.DrawRay(bulletSpawnPoint.position, attackDirection * weaponSetting.attackDistance, Color.blue);
    }

    private IEnumerator OnModeChange()
    {
        float current = 0;
        float percent = 0;
        float time = 0.35f;

        animator.AimModeIs = !animator.AimModeIs;
        imageAim.enabled = !imageAim.enabled;

        float start = mainCamera.fieldOfView;
        float end = animator.AimModeIs == true ? aimModeFOV : defaultModeFOV;

        isModeChange = true;

        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / time;

            // mode에 따라 카메라의 시야각을 변경
            mainCamera.fieldOfView = Mathf.Lerp(start, end, percent);

            yield return null;
        }

        isModeChange = false;
    }

    private void ResetVariables()
    {
        isReload = false;
        isAttack = false;
        isModeChange = false;
    }

    
}