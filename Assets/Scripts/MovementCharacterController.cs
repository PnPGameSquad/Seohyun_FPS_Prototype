using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementCharacterController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed;           // 이동속도
    private Vector3 moveForce;         // 이동 힘 (x, z와 y축을 별도로 계산해 실제 이동에 적용)

    [SerializeField]
    private float jumpForce; //점프 힘
    [SerializeField]
    private float gravity; // 중력
    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = Mathf.Max(0,value);
    }

    private CharacterController characterController;  // 플레이어 이동 제어를 위한 컴포넌트

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if ( !characterController.isGrounded ) // 땅에 닿아있지 않다면
        {
            moveForce.y += gravity * Time.deltaTime; // 중력 적용
        }

        // 1초당 moveForce 속력으로 이동
        characterController.Move(moveForce * Time.deltaTime);
    }

    public void MoveTo(Vector3 direction)
    {
        // 이동 방향 = 캐릭터의 회전 값 * 방향 값
        direction = transform.rotation * new Vector3(direction.x, 0, direction.z);

        // 이동 힘 = 이동방향 * 속도
        moveForce = new Vector3(direction.x * moveSpeed, moveForce.y, direction.z * moveSpeed);
    }

    public void Jump()
    {
        if (characterController.isGrounded) // 땅에 닿아있다면
        {
            moveForce.y = jumpForce; // 점프 힘 적용
        }
    }
}