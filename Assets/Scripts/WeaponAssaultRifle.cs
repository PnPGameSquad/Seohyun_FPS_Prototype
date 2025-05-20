using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class AmmoEvent : UnityEngine.Events.UnityEvent<int, int> { } // ź�� �̺�Ʈ
[System.Serializable]
public class MagazineEvent : UnityEngine.Events.UnityEvent<int> { } // źâ �̺�Ʈ

public class WeaponAssaultRifle : MonoBehaviour
{
    [HideInInspector]
    public AmmoEvent onAmmoEvent = new AmmoEvent(); // ź�� �̺�Ʈ

    [HideInInspector]
    public MagazineEvent onMagazineEvent = new MagazineEvent(); // źâ �̺�Ʈ

    [Header("Fire Effects")]
    [SerializeField]
    private GameObject muzzleFlashEffect; // �ѱ� ȭ�� ȿ��

    [Header("Spawn Point")]
    [SerializeField]
    private Transform casingSpawnPoint; // ź�� ���� ��ġ
    [SerializeField]
    private Transform bulletSpawnPoint; // �Ѿ� ���� ��ġ

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipTakeOutWeapon; // ���� ���� ����
    [SerializeField]
    private AudioClip audioClipFire; // �� �߻� ����
    [SerializeField]
    private AudioClip audioClipReload;

    [Header("Weapon Setting")]
    [SerializeField]
    private WeaponSetting weaponSetting; // ���� ����

    [Header("Aim UI")]
    [SerializeField]
    private Image imageAim; //default/aim mode�� ���� Aim�̹��� Ȱ��/��Ȱ��ȭ

    private float lastAttackTime = 0; // ������ ���� �ð�
    private bool isReload = false; // ������ ����
    private bool isAttack = false;         // ���� ���� üũ��
    private bool isModeChange = false;     // ��� ��ȯ ���� üũ��
    private float defaultModeFOV = 60;     // �⺻��忡���� ī�޶� FOV
    private float aimModeFOV = 30;         // AIM��忡���� ī�޶� FOV

    private AudioSource audioSource; // ���� ��� ������Ʈ
    private PlayerAnimatorController animator; // �ִϸ��̼� ��� ����
    private CasingMemoryPool casingMemoryPool; //ź�� ���� �� Ȱ��/��Ȱ�� ����
    private ImpactMemoryPool impactMemoryPool; // ���� ȿ�� ���� �� Ȱ��/��Ȱ�� ����
    private Camera mainCamera; //���� �߻�

    //�ܺο��� �ʿ��� ������ �����ϱ� ���� ������ Get Property's
    public WeaponName weaponName => weaponSetting.weaponName; // ���� �̸�
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

        //ó�� źâ ���� �ִ�� ����
        weaponSetting.currentMagazine = weaponSetting.maxMagazine;
        // ó�� ź ���� �ִ�� ����
        weaponSetting.currentAmmo = weaponSetting.maxAmmo;
    }

    private void OnEnable()
    {
        // ���� ���� ���� ���
        PlaySound(audioClipTakeOutWeapon);
        // �ѱ� ȭ�� ȿ�� ��Ȱ��
        muzzleFlashEffect.SetActive(false);

        //���Ⱑ Ȱ��ȭ�� �� źâ �� ������ �����Ѵ�
        onMagazineEvent.Invoke(weaponSetting.currentMagazine);

        //���Ⱑ Ȱ��ȭ�� �� �ش� ������ ź �� ������ �����Ѵ�
        onAmmoEvent.Invoke(weaponSetting.currentAmmo, weaponSetting.maxAmmo);

        ResetVariables();
    }
    public void StartWeponAction(int type = 0)
    {
        if (isReload == true || weaponSetting.currentMagazine <= 0) // ������ ���̸� ���� �� ��
        {
            return;
        }
        // ��� ��ȯ���̸� ���� �׼��� �� �� ����
        if (isModeChange == true)
        {
            return;
        }
        if (type == 0)
        {
            // float�̱� ������ ����� �� ��� (0�� �ƴϸ� true)
            if (weaponSetting.isAutomaticAttack != 0f) // ���� ������ ���
            {
                isAttack = true;
                StartCoroutine("OnAttackLoop"); // ���� �ڷ�ƾ ����
            }
            else
            {
                OnAttack(GetCasingMemoryPool()); // ���� ����
            }
        }
        // ���콺 ������ Ŭ�� (��� ��ȯ)
        else
        {
            //���� ���϶� ��� ��ȯ�� �� �� ����
            if (isAttack == true)
            {
                return;
            }

            StartCoroutine("OnModeChange");
        }
    }

    public void StopWeponAction(int type = 0)
    {
        if (type == 0) // ���� ������ ���
        {
            isAttack = false;
            StopCoroutine("OnAttackLoop");
        }
    }

    public void StartReload()
    {
        if (isReload == true) // ������ ���̸� ������ �� ��
        {
            return;
        }
        // ���� ������ ���̸� ������ �Ұ���
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
        if (Time.time - lastAttackTime > weaponSetting.attackRate) // ���� �ӵ��� ���� ����
        {
            if (animator.MoveSpeed > 0.5f) // �̵� �ӵ��� 0.5���� ũ�� ���� �� ��
            {
                return;
            }

            lastAttackTime = Time.time; // ������ ���� �ð� ����
            if (weaponSetting.currentAmmo <= 0) // ź���� ������ ���� �� ��
            {
                return;
            }
            //���ݽ� currentAmmo 1 ����
            weaponSetting.currentAmmo --;
            onAmmoEvent.Invoke(weaponSetting.currentAmmo, weaponSetting.maxAmmo);

            //���� �ִϸ��̼� ��� (��忡 ���� AimFire or Fire �ִϸ��̼� ���)
            //animator.Play("Fire", -1, 0);
            string animation = animator.AimModeIs == true ? "AimFire" : "Fire";
            animator.Play(animation, -1, 0);
            //�ѱ� ����Ʈ ���(default mode�϶��� ���)
            if (animator.CurrentStateIs(animation) == false)
            {
                StartCoroutine("OnMuzzleFlashEffect");
            }

            animator.Play("Fire", -1, 0); // ���� �ִϸ��̼� ���
            StartCoroutine("OnMuzzleFlashEffect"); // �ѱ� ȭ�� ȿ�� ���
            PlaySound(audioClipFire); // �� �߻� ���� ���
            casingMemoryPool.SpawnCasing(casingSpawnPoint.position, transform.right); // ź�� ����

            TwoStepRaycast(); //������ �߻��� ���ϴ� ��ġ ���� (+Impact Effect)
        }
    }

    private IEnumerator OnMuzzleFlashEffect()
    {
        muzzleFlashEffect.SetActive(true); // �ѱ� ȭ�� ȿ�� Ȱ��ȭ
        yield return new WaitForSeconds(weaponSetting.attackRate * 0.3f); // 0.1�� ���
        muzzleFlashEffect.SetActive(false); // �ѱ� ȭ�� ȿ�� ��Ȱ��ȭ
    }

    private IEnumerator OnReload()
    {
        isReload = true;

        // ������ �ִϸ��̼�, ���� ���
        animator.OnReload();
        PlaySound(audioClipReload);

        while (true)
        {
            // ���尡 ������� �ƴϰ�, ���� �ִϸ��̼��� Movement�̸�
            // ������ �ִϸ��̼�(����) ����� ����Ǿ��ٴ� ��
            if (audioSource.isPlaying == false && animator.CurrentAnimationIs("Movement"))
            {
                isReload = false;

                //���� źâ ���� 1 ���ҽ�Ű��, �ٲ� źâ ������ Text UI�� ������Ʈ
                weaponSetting.currentMagazine--;
                onMagazineEvent.Invoke(weaponSetting.currentMagazine);

                // ���� ź ���� �ִ�� �����ϰ�, �ٲ� ź �� ������ Text UI�� ������Ʈ
                weaponSetting.currentAmmo = weaponSetting.maxAmmo;
                onAmmoEvent.Invoke(weaponSetting.currentAmmo, weaponSetting.maxAmmo);

                yield break;
            }

            yield return null;
        }
    }
    private void PlaySound(AudioClip clip)
    {
        audioSource.Stop();       // ������ ������� ���带 �����ϰ�,
        audioSource.clip = clip;  // ���ο� ���� clip���� ��ü ��
        audioSource.Play();       // ���� ���
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

        // ȭ���� �߾� ��ǥ (Aim �������� Raycast ����)
        ray = mainCamera.ViewportPointToRay(Vector2.one * 0.5f);

        // ���� ��Ÿ�(attackDistance) �ȿ� �ε����� ������Ʈ�� ������ targetPoint�� ������ �ε��� ��ġ
        if (Physics.Raycast(ray, out hit, weaponSetting.attackDistance))
        {
            targetPoint = hit.point;
        }
        // ���� ��Ÿ� �ȿ� �ε����� ������Ʈ�� ������ targetPoint�� �ִ� ��Ÿ� ��ġ
        else
        {
            targetPoint = ray.origin + ray.direction * weaponSetting.attackDistance;
        }

        Debug.DrawRay(ray.origin, ray.direction * weaponSetting.attackDistance, Color.red);

        // ù��° Raycast �������� ����� targetPoint�� ��ǥ�������� �����ϰ�,
        // �ѱ��� ������������ �Ͽ� Raycast ����
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

            // mode�� ���� ī�޶��� �þ߰��� ����
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