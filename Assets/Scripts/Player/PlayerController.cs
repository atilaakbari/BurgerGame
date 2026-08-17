using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidbody;
    [SerializeField] private FloatingJoystick joystick;
    [SerializeField] private Animator animator;
    [SerializeField] PlayerPickup playerPickup;

    [Header("Movement")]
    [SerializeField] public float playerSpeed = 5f;
    [SerializeField] public float playerCarrySpeed = 3f;

    private bool isCarrying;


    void FixedUpdate()
    {
        float joystickAmount = new Vector2(
            joystick.Horizontal,
            joystick.Vertical
        ).magnitude;


        if (joystickAmount > 0.15f)
        {
            Vector3 direction = new Vector3(
                joystick.Horizontal,
                0,
                joystick.Vertical
            ).normalized;

            float currentSpeed = isCarrying ? playerCarrySpeed : playerSpeed;

            rigidbody.linearVelocity = new Vector3(
                direction.x * currentSpeed * joystickAmount,
                rigidbody.linearVelocity.y,
                direction.z * currentSpeed * joystickAmount
            );

            transform.rotation = Quaternion.LookRotation(direction);

            animator.SetBool("IsWalking", true);

            if (joystickAmount >= 0.9f)
            {
                animator.SetBool("IsRunning", true);
            }
            else
            {
                animator.SetBool("IsRunning", false);
            }

            // ???? ??????? ?? ??? ??? ?????
            animator.speed = Mathf.Lerp(0.3f, 1f, joystickAmount);
        }
        else
        {
            rigidbody.linearVelocity = new Vector3(
                0,
                rigidbody.linearVelocity.y,
                0
            );

            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);

            animator.speed = 1f;
        }
    }



    public void SetCarryState(bool carrying)
    {
        isCarrying = carrying;

        animator.SetBool("IsCarry", carrying);
    }

    public void RefreshAnimationState()
    {
        float joystickAmount = new Vector2(
            joystick.Horizontal,
            joystick.Vertical
        ).magnitude;

        bool isWalking = joystickAmount > 0.15f;

        animator.SetBool("IsWalking", isWalking);
        animator.SetBool("IsCarry", playerPickup.IsCarrying);

        if (!isWalking)
        {
            animator.SetBool("IsRunning", false);
        }
    }
}