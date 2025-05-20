using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Input KeyCodes")]
    [SerializeField]
    private KeyCode keyCodeRun = KeyCode.LeftShift; //�޸��� Ű
    [SerializeField]
    private KeyCode keyCodeJump = KeyCode.Space; //���� Ű
    [SerializeField]
    private KeyCode keyCodeReload = KeyCode.R; // ������ Ű

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipWalk; //�ȱ� �Ҹ�
    [SerializeField]
    private AudioClip audioClipRun; //�޸��� �Ҹ�

    private RotateToMouse rotateToMouse; // ���콺 �̵����� ī�޶� ȸ��
    private MovementCharacterController movement;    // Ű���� �Է����� �÷��̾� �̵�, ����
    private Status status; // �̵��ӵ� ���� �÷��̾� ����
    private PlayerAnimatorController animator; // �ִϸ��̼� ��� ����
    private AudioSource audioSource; // �Ҹ� ��� ����
    private WeaponAssaultRifle weapon; //���⸦ �̿��� ���� ����
    private void Awake()
    {
        // ���콺 Ŀ���� ������ �ʰ� �����ϰ�, ���� ��ġ�� ������Ų��
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
        float x = Input.GetAxis("Horizontal"); // A, DŰ �Է�
        float z = Input.GetAxis("Vertical");     // W, SŰ �Է�

        //�̵��� �϶� (�ȱ� or �޸���)
        if (x != 0 || z != 0)
        {
            bool isRun = false;

            //���̳� �ڷ� �̵��� ���� �޸� �� ����
            if (z > 0) isRun = Input.GetKey(keyCodeRun); // WŰ�� ������ ���� ���� �޸��� ����

            movement.MoveSpeed = isRun == true ? status.RunSpeed : status.WalkSpeed;
            animator.MoveSpeed = isRun == true ? 1 : 0.5f;
            audioSource.clip = isRun == true ? audioClipRun : audioClipWalk; // �ȱ� or �޸��� �Ҹ�

            if (audioSource.isPlaying == false)
            {
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        //���ڸ��� ����������
        else
        {
            movement.MoveSpeed = 0;
            animator.MoveSpeed = 0;

            //���������� �Ҹ� ����
            if (audioSource.isPlaying == true)
            {
                audioSource.Stop();
            }
        }
        movement.MoveTo(new Vector3(x, 0, z)); // �̵� �������� �̵�
    }
    private void UpdateJump()
    {
        if (Input.GetKeyDown(keyCodeJump)) // ����Ű�� �����ٸ�
        {
            movement.Jump(); // ���� ����
        }
    }
    private void UpdateWeaponAction()
    {
        if (Input.GetMouseButtonDown(0)) // ���콺 ���� ��ư�� �����ٸ�
        {
            weapon.StartWeponAction(weapon.GetWeaponSetting()); // ���� ���� ����
        }
        else if (Input.GetMouseButtonUp(0)) // ���콺 ���� ��ư�� �����ٸ�
        {
            weapon.StopWeponAction(); // ���� ���� ����
        }

        if (Input.GetMouseButtonDown(1))
        {
            weapon.StartWeponAction(1);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            weapon.StopWeponAction(1);
        }

        if (Input.GetKeyDown(keyCodeReload)) // ������ Ű�� �����ٸ�
        {
            weapon.StartReload(); // ���� ������ ����
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
