using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
public class BallController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    public bool canMove = true;

    public Rigidbody rb;
    [HideInInspector] public SphereCollider sphereCollider;

    [SerializeField] public AudioSource audioSource = null;

    [SerializeField] private float rollSpeed;
    [SerializeField] private float jumpForce = 50f;
    [SerializeField] private float jumpInterval = 1f;

    [SerializeField] [ReadOnly] public float colliderVolume = 1f;
    [SerializeField] [ReadOnly] public float colliderPerceptibleVolume = 1f;

    [SerializeField] private float maxPickupVolumeFraction = 0.25f; // The collider volume won't pick up objects that are over 0.25 of its volume
    [SerializeField] private float growthMultiplier = 1.5f;

    [SerializeField] [ReadOnly] private int currentTier;
    private float[] tierVolumes = { 0.0f, 3.0f, 10f };

    private bool jumpPressed = false;
    private float jumpPressedLastTime = -5f;

    [SerializeField] private Transform unstuckTeleportPosition;

    [SerializeField] private CinemachineFreeLook cinemachineCamera1 = null;
    [SerializeField] private CinemachineFreeLook cinemachineCamera2 = null;
    [SerializeField] private CinemachineFreeLook cinemachineCamera3 = null;
    [SerializeField] private CinemachineFreeLook cinemachineCamera4 = null;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame

    private void Update()
    {
        if (!canMove)
            return;

        if(Input.GetButtonDown("Jump"))
            jumpPressed = true;

        if(Input.GetKeyDown(KeyCode.U)) {
            rb.velocity = Vector3.zero;
            transform.position = unstuckTeleportPosition.position;
        }
            
            
    }
    void FixedUpdate()
    {
        if (!canMove)
            return;

        RollFunctions();
        JumpFunctions();
    }

    private void RollFunctions()
    {
        Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")), 1f);

        Vector3 cameraPlanarProjection = Vector3.ProjectOnPlane(cameraTransform.rotation * Vector3.forward, Vector3.up).normalized;

        if (cameraPlanarProjection.sqrMagnitude == 0f)
        {
            cameraPlanarProjection = Vector3.ProjectOnPlane(cameraTransform.rotation * Vector3.up, Vector3.forward).normalized;
        }

        Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarProjection, Vector3.up);

        moveInputVector = cameraPlanarRotation * moveInputVector;
        moveInputVector.Normalize();

        rb.AddForce(moveInputVector * rollSpeed * Time.fixedDeltaTime * Mathf.Sqrt(sphereCollider.radius*2f));
    }

    private void JumpFunctions()
    {
        if(!jumpPressed) { return; }

        jumpPressed = false;

        if (Time.time > jumpPressedLastTime + jumpInterval) {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpPressedLastTime = Time.time;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<ObjectPickup>() != null)
        {
            var pickup = collision.gameObject.GetComponent<ObjectPickup>();

            if (pickup.minVolumeToPickUp == 0f) {
                if (pickup.objectVolume > colliderVolume * maxPickupVolumeFraction)
                    return;
            }

            else if (colliderVolume < pickup.minVolumeToPickUp)
                return;


            if (!pickup.removeCollisionsOnPickup) {

                bool collectTheObject = false;

                foreach (ContactPoint contact in collision.contacts) {

                    if (collectTheObject)
                        continue;

                    if (Vector3.Distance(contact.point, transform.position) < sphereCollider.radius * 2f)
                        collectTheObject = true;

                    //if (contact.thisCollider.gameObject == gameObject)
                    //    collectTheObject = true;
                    //else if (contact.thisCollider.GetComponent<ObjectPickup>().removeCollisionsOnPickup)
                    //    collectTheObject = true;
                }

                if (!collectTheObject)
                    return;
            }

            pickup.OnPickup(this);

            if(!pickup.ignoreSuck)
                AudioManager.instance.Play("vacuum_suck");

            colliderVolume += pickup.objectVolume;
            if(!pickup.dontScaleCollider)
                colliderPerceptibleVolume += pickup.objectVolume;

            ColliderResizeFunctions();
        }
    }

    private void ColliderResizeFunctions()
    {
        sphereCollider.radius = 0.5f*Mathf.Sqrt(colliderPerceptibleVolume);

        //audioSource.PlayOneShot()

        if (GameManager.instance.currentLevel > 1)
        {
            if (colliderVolume > 400f)
            {
                cinemachineCamera4.Priority = 80;
                RenderSettings.fogDensity = 0.01f;
            }

            else if (colliderVolume > 100f) {
                cinemachineCamera3.Priority = 50;
                RenderSettings.fogDensity = 0.03f;
            }

            else if (colliderVolume > 45f)
            {
                cinemachineCamera2.Priority = 20;
                RenderSettings.fogDensity = 0.04f;
            }
        }

        TiersFunctions();
    }

    private void TiersFunctions()
    {
        if (currentTier == 0) {
            if(colliderVolume >= tierVolumes[1])
                currentTier = 1;
        }

        if (currentTier == 1) {
            if (colliderVolume >= tierVolumes[2])
                currentTier = 2;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Level End Trigger")
            GameManager.instance.EndLevel();
    }
}
