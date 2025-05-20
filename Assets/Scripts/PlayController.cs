using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Input KeyCodes")]
    [SerializeField]
    private KeyCode keyCodeRun = KeyCode.LeftShift; //달리기 키
    [SerializeField]
    private KeyCode keyCodeJump = KeyCode.Space; //점프 키
    [SerializeField]
    private KeyCode keyCodeReload = KeyCode.R; // 재장전 키

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipWalk; //걷기 소리
    [SerializeField]
    private AudioClip audioClipRun; //달리기 소리

    private RotateToMouse rotateToMouse; // 마우스 이동으로 카메라 회전
    private MovementCharacterController movement;    // 키보드 입력으로 플레이어 이동, 점프
    private Status status; // 이동속도 등의 플레이어 정보
    private PlayerAnimatorController animator; // 애니메이션 재생 제어
    private AudioSource audioSource; // 소리 재생 제어
    private WeaponAssaultRifle weapon; //무기를 이용한 공격 제어
    private void Awake()
    {
        // 마우스 커서를 보이지 않게 설정하고, 현재 위치에 고정시킨다
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        rotateToMouse = GetComponent<RotateToMouse>();
        movement = GetComponent<MovementCharacterController>();
        status = GetComponent<Status>();
        animator = GetComponentInChildren<PlayerAnimatorController>();
        audioSource = GetComponent<AudioSource>();
        weapon = GetComponentInChildren<WeaponAssaultRifle>();
    }

    private void Update()
    {
        UpdateRotate();
        UpdateMove();
        UpdateJump();
        UpdateWeaponAction();
    }

    private void UpdateRotate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        rotateToMouse.UpdateRotate(mouseX, mouseY);
    }
    private void UpdateMove()
    {
        float x = Input.GetAxis("Horizontal"); // A, D키 입력
        float z = Input.GetAxis("Vertical");     // W, S키 입력

        //이동중 일때 (걷기 or 달리기)
        if (x != 0 || z != 0)
        {
            bool isRun = false;

            //옆이나 뒤로 이동할 때는 달릴 수 없다
            if (z > 0) isRun = Input.GetKey(keyCodeRun); // W키를 누르고 있을 때만 달리기 가능

            movement.MoveSpeed = isRun == true ? status.RunSpeed : status.WalkSpeed;
            animator.MoveSpeed = isRun == true ? 1 : 0.5f;
            audioSource.clip = isRun == true ? audioClipRun : audioClipWalk; // 걷기 or 달리기 소리

            if (audioSource.isPlaying == false)
            {
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        //제자리에 멈춰있을때
        else
        {
            movement.MoveSpeed = 0;
            animator.MoveSpeed = 0;

            //멈춰있을때 소리 정지
            if (audioSource.isPlaying == true)
            {
                audioSource.Stop();
            }
        }
        movement.MoveTo(new Vector3(x, 0, z)); // 이동 방향으로 이동
    }
    private void UpdateJump()
    {
        if (Input.GetKeyDown(keyCodeJump)) // 점프키를 눌렀다면
        {
            movement.Jump(); // 점프 실행
        }
    }
    private void UpdateWeaponAction()
    {
        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 버튼을 눌렀다면
        {
            weapon.StartWeponAction(weapon.GetWeaponSetting()); // 무기 공격 실행
        }
        else if (Input.GetMouseButtonUp(0)) // 마우스 왼쪽 버튼을 떼었다면
        {
            weapon.StopWeponAction(); // 무기 공격 정지
        }

        if (Input.GetMouseButtonDown(1))
        {
            weapon.StartWeponAction(1);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            weapon.StopWeponAction(1);
        }

        if (Input.GetKeyDown(keyCodeReload)) // 재장전 키를 눌렀다면
        {
            weapon.StartReload(); // 무기 재장전 실행
        }
    }

    private void TakeDamage(int damage)
    {
        bool isDead = status.DecreaseHP(damage);

        if (isDead == true)
        {
            Debug.Log("GameOver");
        }
    }
}
