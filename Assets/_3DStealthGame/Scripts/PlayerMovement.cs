using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    Animator m_Animator;

    public InputAction MoveAction;
    public InputAction boostAction;

    public float walkSpeed = 3.0f;  // NORMAL speed
    public float turnSpeed = 20f;
    
    // Boost variables - MAKE SURE boostSpeed > walkSpeed!
    public float boostSpeed = 6.0f;  // BOOST speed - should be HIGHER!
    public float boostDuration = 2.0f;
    public float boostCooldown = 3.0f;
    public float boostAmount = 100f;
    public bool isBoosting = false;
    
    Rigidbody m_Rigidbody;
    Vector3 m_Movement;
    Quaternion m_Rotation = Quaternion.identity;
    
    private float originalSpeed;
    private float boostTimer;
    private bool isOnCooldown = false;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        MoveAction.Enable();
        m_Animator = GetComponent<Animator>();
        
        // Set default boost key if not set
        if (boostAction.bindings.Count == 0)
        {
            boostAction.AddBinding("<Keyboard>/space");
        }
        boostAction.Enable();
        
        originalSpeed = walkSpeed;
        boostAmount = 100f;
        
        Debug.Log($"PlayerMovement Started: Walk Speed = {walkSpeed}, Boost Speed = {boostSpeed}");
    }

    void Update()
    {
        HandleSpeedBoost();
    }

    void FixedUpdate()
    {
        var pos = MoveAction.ReadValue<Vector2>();

        float horizontal = pos.x;
        float vertical = pos.y;

        m_Movement.Set(horizontal, 0f, vertical);
        m_Movement.Normalize();

        bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
        bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);
        bool isWalking = hasHorizontalInput || hasVerticalInput;
        m_Animator.SetBool("IsWalking", isWalking);

        Vector3 desiredForward = Vector3.RotateTowards(transform.forward, m_Movement, turnSpeed * Time.deltaTime, 0f);
        m_Rotation = Quaternion.LookRotation(desiredForward);

        m_Rigidbody.MoveRotation(m_Rotation);
        m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement * walkSpeed * Time.deltaTime);
        
        // Debug current speed occasionally
        if (Random.Range(0, 100) < 2) // 2% chance each frame
        {
            Debug.Log($"Current speed: {walkSpeed}, IsBoosting: {isBoosting}, Boost Amount: {boostAmount}%");
        }
    }

    void HandleSpeedBoost()
    {
        // Check if boost button is pressed, boost is available, and not on cooldown
        if (boostAction.IsPressed() && boostAmount > 0 && !isOnCooldown)
        {
            if (!isBoosting)
            {
                StartBoost();
            }
            
            // Consume boost over time
            boostAmount -= (100f / boostDuration) * Time.deltaTime;
            boostTimer += Time.deltaTime;
            
            if (boostAmount <= 0 || boostTimer >= boostDuration)
            {
                EndBoost();
                StartCoroutine(StartCooldown());
            }
        }
        else if (isBoosting)
        {
            EndBoost();
        }
        
        // Recharge boost when not in use and not on cooldown
        if (!isBoosting && boostAmount < 100f && !isOnCooldown)
        {
            boostAmount += (100f / boostCooldown) * Time.deltaTime;
            boostAmount = Mathf.Min(boostAmount, 100f);
        }
    }

    void StartBoost()
    {
        isBoosting = true;
        walkSpeed = boostSpeed;  // This should INCREASE speed!
        boostTimer = 0f;
        Debug.Log($"BOOST ACTIVATED! Speed: {walkSpeed} (was {originalSpeed})");
    }

    void EndBoost()
    {
        isBoosting = false;
        walkSpeed = originalSpeed;
        Debug.Log($"Boost ended. Speed: {walkSpeed}");
    }

    IEnumerator StartCooldown()
    {
        Debug.Log("Boost depleted! Starting cooldown...");
        isOnCooldown = true;
        yield return new WaitForSeconds(boostCooldown);
        isOnCooldown = false;
        Debug.Log("Boost cooldown finished!");
    }
}