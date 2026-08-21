using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidbody;
    [SerializeField] private FloatingJoystick joystick;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerPickup playerPickup;

    [Header("Movement")]
    [SerializeField] public float playerSpeed = 5f;
    [SerializeField] public float playerCarrySpeed = 3f;

    private bool isCarrying;
    private bool animWalking;
    private bool animRunning;
    private bool animCarry;
    private static readonly int WalkHash = Animator.StringToHash("IsWalking");
    private static readonly int RunHash = Animator.StringToHash("IsRunning");
    private static readonly int CarryHash = Animator.StringToHash("IsCarry");

    private void Awake()
    {
        if (rigidbody == null)
            rigidbody = GetComponent<Rigidbody>();

        if (rigidbody != null)
        {
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    private void FixedUpdate()
    {
        if (joystick == null || rigidbody == null)
            return;

        float x = joystick.Horizontal;
        float z = joystick.Vertical;
        float joystickAmount = Mathf.Sqrt(x * x + z * z);

        if (joystickAmount > 0.15f)
        {
            Vector3 direction = new Vector3(x, 0f, z);
            float inverse = 1f / joystickAmount;
            direction.x *= inverse;
            direction.z *= inverse;

            float currentSpeed = isCarrying ? playerCarrySpeed : playerSpeed;
            float yVelocity = rigidbody.linearVelocity.y;

            rigidbody.linearVelocity = new Vector3(
                direction.x * currentSpeed * joystickAmount,
                yVelocity,
                direction.z * currentSpeed * joystickAmount
            );

            transform.rotation = Quaternion.LookRotation(direction);
            SetAnimBool(WalkHash, ref animWalking, true);
            SetAnimBool(RunHash, ref animRunning, joystickAmount >= 0.9f);

            if (animator != null)
                animator.speed = Mathf.Lerp(0.3f, 1f, joystickAmount);
        }
        else
        {
            Vector3 velocity = rigidbody.linearVelocity;
            rigidbody.linearVelocity = new Vector3(0f, velocity.y, 0f);
            SetAnimBool(WalkHash, ref animWalking, false);
            SetAnimBool(RunHash, ref animRunning, false);

            if (animator != null)
                animator.speed = 1f;
        }
    }

    public void SetCarryState(bool carrying)
    {
        isCarrying = carrying;
        SetAnimBool(CarryHash, ref animCarry, carrying);
    }

    public void RefreshAnimationState()
    {
        if (joystick == null)
            return;

        float joystickAmount = new Vector2(joystick.Horizontal, joystick.Vertical).magnitude;
        bool walking = joystickAmount > 0.15f;

        SetAnimBool(WalkHash, ref animWalking, walking);
        SetAnimBool(CarryHash, ref animCarry, playerPickup != null && playerPickup.IsCarrying);

        if (!walking)
            SetAnimBool(RunHash, ref animRunning, false);
    }

    private void SetAnimBool(int hash, ref bool cached, bool value)
    {
        if (cached == value || animator == null)
            return;

        cached = value;
        animator.SetBool(hash, value);
    }
}
