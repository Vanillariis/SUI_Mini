using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Donkey_Movement : MonoBehaviour
{
    [Header("Mounting")]
    public Transform xrRig;
    public Transform seatAnchor;

    [Header("Targets")]
    public Transform carrot;

    [Header("Right Hand (Carrot)")]
    [SerializeField] private InputActionReference selectAction;
    [SerializeField] private Transform rightHandController;
    [SerializeField] private GameObject carrotStick;

    [Header("Left Hand (Whip)")]
    [SerializeField] private InputActionReference leftSelectAction;
    [SerializeField] private Transform leftHandController;
    [SerializeField] private GameObject whip;

    [Header("Movement")]
    public float baseSpeed = 2f;
    public float currentSpeed = 2f;
    public float speedDecayRate = 2f;
    public float turnSpeed = 5f;
    public float stopDistance = 0.5f;

    [Header("Boost")]
    public float boostPerHit = 1f;
    public float maxBoostBonus = 4f;
    public float boostDecayRate = 1.5f;
    private float boostBonus = 0f;

    [Header("Jump")]
    public float jumpHeightThreshold = 0.3f;
    public float jumpForce = 5f;

    [Header("Ride Bobbing")]
    public float bobAmount = 0.03f;
    public float bobSpeed = 5f;
    public float minSpeedForBobbing = 0.01f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float walkThreshold = 0.1f;
    [SerializeField] private float trotThreshold = 2.5f;
    [SerializeField] private float runThreshold = 4.5f;

    [Header("Audio")]
    public AudioClip jumpSound;
    public AudioClip braySound;
    public AudioClip gallopLoop;

    [Header("Audio Sources")]
    public AudioSource jumpAudio;
    public AudioSource brayAudio;
    public AudioSource gallopAudio;

    private Rigidbody rb;
    private bool isGrounded;

    private float carrotStartLocalY;
    private float previousCarrotLocalY;
    private bool wasCarrotActiveLastFrame = false;

    private Vector3 seatStartLocalPos;
    private float bobTimer;

    private Vector3 lastDonkeyPosition;
    private float donkeyMoveSpeed;

    private void OnEnable()
    {
        selectAction?.action.Enable();
        leftSelectAction?.action.Enable();
    }

    private void OnDisable()
    {
        selectAction?.action.Disable();
        leftSelectAction?.action.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        seatStartLocalPos = seatAnchor != null ? seatAnchor.localPosition : Vector3.zero;
        currentSpeed = 0f;
        lastDonkeyPosition = transform.position;

        Mount();

        if (carrotStick != null && rightHandController != null)
        {
            carrotStick.transform.SetParent(rightHandController);
            carrotStick.transform.localPosition = Vector3.zero;
            carrotStick.transform.localRotation = Quaternion.identity;
            carrotStick.SetActive(false);
        }

        if (whip != null && leftHandController != null)
        {
            whip.transform.SetParent(leftHandController);
            whip.transform.localPosition = Vector3.zero;
            whip.transform.localRotation = Quaternion.identity;
            whip.SetActive(false);
        }
    }

    private void Update()
    {
        SetCarrotStickActive();
        SetWhipActive();

        bool carrotActive = carrotStick != null && carrotStick.activeSelf;

        if (carrotActive)
        {
            boostBonus = Mathf.MoveTowards(boostBonus, 0f, boostDecayRate * Time.deltaTime);
            float targetSpeed = baseSpeed + boostBonus;
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedDecayRate);
        }
        else
        {
            boostBonus = 0f;
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, Time.deltaTime * speedDecayRate);
        }

        if (carrotActive && carrot != null)
        {
            float currentCarrotLocalY = transform.InverseTransformPoint(carrot.position).y;

            if (!wasCarrotActiveLastFrame)
            {
                carrotStartLocalY = currentCarrotLocalY;
                previousCarrotLocalY = currentCarrotLocalY;
            }

            TryJump(currentCarrotLocalY);
        }

        wasCarrotActiveLastFrame = carrotActive;

        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (carrotStick != null && carrotStick.activeSelf && carrot != null && currentSpeed > 0.01f)
        {
            Move();
        }
    }

    private void LateUpdate()
    {
        UpdateDonkeyMoveSpeed();
        ApplyRideBobbing();
        HandleGallopAudio();
        KeepRigSeated();
    }

    private void Mount()
    {
        if (xrRig == null || seatAnchor == null)
            return;

        xrRig.SetParent(seatAnchor, false);
        xrRig.localPosition = Vector3.zero;
        xrRig.localRotation = Quaternion.identity;
    }

    private void KeepRigSeated()
    {
        if (xrRig == null || seatAnchor == null)
            return;

        Vector3 localPos = xrRig.localPosition;
        localPos.x = 0f;
        localPos.z = 0f;
        xrRig.localPosition = localPos;

        xrRig.localRotation = Quaternion.identity;
    }

    private void SetCarrotStickActive()
    {
        if (carrotStick == null)
            return;

        bool pressed = selectAction != null && selectAction.action.IsPressed();
        carrotStick.SetActive(pressed);

        if (pressed && rightHandController != null)
        {
            carrotStick.transform.SetParent(rightHandController);
            carrotStick.transform.localPosition = Vector3.zero;
            carrotStick.transform.localRotation = Quaternion.identity;
        }
        else if (!pressed)
        {
            currentSpeed = 0f;
            boostBonus = 0f;

#if UNITY_6000_0_OR_NEWER
            Vector3 v = rb.linearVelocity;
            v.x = 0f;
            v.z = 0f;
            rb.linearVelocity = v;
#else
            Vector3 v = rb.velocity;
            v.x = 0f;
            v.z = 0f;
            rb.velocity = v;
#endif
        }
    }

    private void SetWhipActive()
    {
        if (whip == null)
            return;

        bool pressed = leftSelectAction != null && leftSelectAction.action.IsPressed();
        whip.SetActive(pressed);

        if (pressed && leftHandController != null)
        {
            whip.transform.SetParent(leftHandController);
            whip.transform.localPosition = Vector3.zero;
            whip.transform.localRotation = Quaternion.identity;
        }
    }

    private void Move()
    {
        Vector3 offset = carrot.position - transform.position;
        offset.y = 0f;

        float distance = offset.magnitude;
        if (distance < stopDistance)
            return;

        Vector3 direction = offset.normalized;
        if (direction.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion smoothRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            turnSpeed * Time.fixedDeltaTime
        );

        Vector3 euler = smoothRotation.eulerAngles;
        smoothRotation = Quaternion.Euler(0f, euler.y, 0f);
        rb.MoveRotation(smoothRotation);

        Vector3 newPosition = rb.position + transform.forward * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }

    private void TryJump(float currentCarrotLocalY)
    {
        float currentDelta = currentCarrotLocalY - carrotStartLocalY;
        float previousDelta = previousCarrotLocalY - carrotStartLocalY;

        bool crossedThresholdUpward =
            previousDelta <= jumpHeightThreshold &&
            currentDelta > jumpHeightThreshold;

        if (crossedThresholdUpward && isGrounded)
        {
#if UNITY_6000_0_OR_NEWER
            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;
#else
            Vector3 v = rb.velocity;
            v.y = 0f;
            rb.velocity = v;
#endif

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;

            if (jumpAudio != null && jumpSound != null)
            {
                jumpAudio.PlayOneShot(jumpSound);
            }
        }

        previousCarrotLocalY = currentCarrotLocalY;
    }

    private void UpdateAnimations()
    {
        if (animator == null)
            return;

        float moveSpeed = donkeyMoveSpeed;

        bool isMoving = moveSpeed > walkThreshold;
        bool isTrotting = moveSpeed >= trotThreshold;
        bool isRunning = moveSpeed >= runThreshold;
        bool isJumping = !isGrounded;

        animator.SetBool("idle", false);
        animator.SetBool("walk", false);
        animator.SetBool("trot", false);
        animator.SetBool("run", false);
        animator.SetBool("jump", false);

        if (isJumping)
        {
            animator.SetBool("jump", true);
        }
        else if (isRunning)
        {
            animator.SetBool("run", true);
        }
        else if (isTrotting)
        {
            animator.SetBool("trot", true);
        }
        else if (isMoving)
        {
            animator.SetBool("walk", true);
        }
        else
        {
            animator.SetBool("idle", true);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckGrounded(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        CheckGrounded(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    private void CheckGrounded(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    public void ApplyWhipBoost()
    {
        if (carrotStick == null || !carrotStick.activeSelf)
            return;

        boostBonus = Mathf.Clamp(boostBonus + boostPerHit, 0f, maxBoostBonus);

        if (brayAudio != null && braySound != null)
        {
            brayAudio.PlayOneShot(braySound);
        }
    }

    private void UpdateDonkeyMoveSpeed()
    {
        Vector3 delta = transform.position - lastDonkeyPosition;
        delta.y = 0f;

        donkeyMoveSpeed = delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        lastDonkeyPosition = transform.position;
    }

    private void ApplyRideBobbing()
    {
        if (seatAnchor == null)
            return;

        bool donkeyMoving = donkeyMoveSpeed > minSpeedForBobbing;

        if (donkeyMoving)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            float bobOffsetY = Mathf.Sin(bobTimer) * bobAmount;

            seatAnchor.localPosition = seatStartLocalPos + new Vector3(0f, bobOffsetY, 0f);
        }
        else
        {
            bobTimer = 0f;
            seatAnchor.localPosition = Vector3.Lerp(
                seatAnchor.localPosition,
                seatStartLocalPos,
                Time.deltaTime * 5f
            );
        }
    }

    private void HandleGallopAudio()
    {
        if (gallopAudio == null || gallopLoop == null)
            return;

        bool isMoving = donkeyMoveSpeed > minSpeedForBobbing;

        if (isMoving)
        {
            if (!gallopAudio.isPlaying)
            {
                gallopAudio.clip = gallopLoop;
                gallopAudio.loop = true;
                gallopAudio.Play();
            }

            float maxSpeed = baseSpeed + maxBoostBonus;
            float speedFactor = Mathf.InverseLerp(baseSpeed, maxSpeed, donkeyMoveSpeed);
            gallopAudio.pitch = Mathf.Lerp(1f, 1.3f, speedFactor);
            gallopAudio.volume = Mathf.Lerp(gallopAudio.volume, 1f, Time.deltaTime * 5f);
        }
        else
        {
            gallopAudio.volume = Mathf.Lerp(gallopAudio.volume, 0f, Time.deltaTime * 5f);

            if (gallopAudio.volume <= 0.01f)
            {
                gallopAudio.Stop();
            }

            gallopAudio.pitch = 1f;
        }
    }
}