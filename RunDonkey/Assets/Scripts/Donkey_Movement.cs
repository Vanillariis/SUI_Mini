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
    public float boostedSpeed = 5f;
    public float currentSpeed = 2f;
    public float speedDecayRate = 2f;
    public float turnSpeed = 5f;
    public float stopDistance = 0.5f;

    [Header("Jump")]
    public float jumpHeightThreshold = 0.3f;
    public float jumpForce = 5f;

    [Header("Ride Bobbing")]
    public float bobAmount = 0.03f;
    public float bobSpeed = 5f;
    public float minSpeedForBobbing = 0.01f;

    [Header("Audio")] 
    public AudioClip jumpSound;
    public AudioClip braySound;
    public AudioClip gallopLoop;
    
    [Header("Audio Sources")]
    public AudioSource jumpAudio;
    public AudioSource brayAudio;
    public AudioSource gallopAudio;

    private Rigidbody rb;
    private bool isGrounded = false;

    private float carrotStartLocalY;
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

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        seatStartLocalPos = seatAnchor.localPosition;
        currentSpeed = baseSpeed;
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

        currentSpeed = Mathf.Lerp(currentSpeed, baseSpeed, Time.deltaTime * speedDecayRate);

        bool carrotActive = carrotStick != null && carrotStick.activeSelf;

        if (carrotActive && !wasCarrotActiveLastFrame)
        {
            carrotStartLocalY = transform.InverseTransformPoint(carrot.position).y;
        }

        if (carrotActive)
        {
            TryJump();
        }

        wasCarrotActiveLastFrame = carrotActive;
    }

    private void FixedUpdate()
    {
        if (carrotStick != null && carrotStick.activeSelf)
        {
            Move();
        }
    }

    private void LateUpdate()
    {
        UpdateDonkeyMoveSpeed();
        ApplyRideBobbing();
        HandleGallopAudio();
    }

    private void Mount()
    {
        xrRig.SetParent(seatAnchor);
        xrRig.position = seatAnchor.position;

        float seatYaw = seatAnchor.eulerAngles.y;
        xrRig.rotation = Quaternion.Euler(0f, seatYaw, 0f);
    }

    private void SetCarrotStickActive()
    {
        bool pressed = selectAction != null && selectAction.action.IsPressed();

        if (carrotStick == null)
            return;

        carrotStick.SetActive(pressed);

        if (pressed)
        {
            carrotStick.transform.SetParent(rightHandController);
            carrotStick.transform.localPosition = Vector3.zero;
            carrotStick.transform.localRotation = Quaternion.identity;
        }
    }

    private void SetWhipActive()
    {
        bool pressed = leftSelectAction != null && leftSelectAction.action.IsPressed();

        if (whip == null)
            return;

        whip.SetActive(pressed);

        if (pressed)
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

    private void TryJump()
    {
        float currentCarrotLocalY = transform.InverseTransformPoint(carrot.position).y;
        float carrotHeightDelta = currentCarrotLocalY - carrotStartLocalY;

        if (carrotHeightDelta > jumpHeightThreshold && isGrounded)
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
            jumpAudio.PlayOneShot(jumpSound);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
    }

    public void ApplyWhipBoost()
    {
        currentSpeed = boostedSpeed;
        
        brayAudio.PlayOneShot(braySound);
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
        bool isMoving = donkeyMoveSpeed > minSpeedForBobbing;

        if (isMoving)
        {
            // Start loop only once
            if (!gallopAudio.isPlaying)
            {
                gallopAudio.clip = gallopLoop;
                gallopAudio.loop = true;
                gallopAudio.Play();
            }

            // Convert speed to 0–1 range (based on max boosted speed)
            float speedFactor = Mathf.InverseLerp(baseSpeed, boostedSpeed, donkeyMoveSpeed);

            // Pitch increases when faster (whip boost effect)
            gallopAudio.pitch = Mathf.Lerp(1f, 1.3f, speedFactor);

            // Smooth fade in
            gallopAudio.volume = Mathf.Lerp(gallopAudio.volume, 1f, Time.deltaTime * 5f);
        }
        else
        {
            // Smooth fade out
            gallopAudio.volume = Mathf.Lerp(gallopAudio.volume, 0f, Time.deltaTime * 5f);

            // Stop when nearly silent
            if (gallopAudio.volume <= 0.01f)
            {
                gallopAudio.Stop();
            }

            // Reset pitch so next start is clean
            gallopAudio.pitch = 1f;
        }
    }
}