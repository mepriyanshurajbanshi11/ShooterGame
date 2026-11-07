using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public enum Stance { Stand, Crouch, Prone }

    public float walkSpeed = 4f, sprintSpeed = 9f, crouchSpeed = 2f, proneSpeed = 1f;
    public float jumpHeight = 1.5f, gravity = -20f;
    public float maxLookAngle = 80f;
    public float stanceBlend = 10f;
    public Transform camPivot;
    public Transform body;

    public Stance CurrentStance { get; private set; } = Stance.Stand;

    // order: Stand, Crouch, Prone
    readonly float[] heights = { 2f, 1.2f, 0.6f };
    readonly float[] eyeY = { 0.7f, 0.1f, -0.5f };

    CharacterController cc;
    PlayerInputRouter input;
    Vector3 velocity;
    float xRot;
    float curHeight = 2f;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        input = GetComponent<PlayerInputRouter>();
    }

    void Update()
    {
        // look
        transform.Rotate(Vector3.up * input.Look.x);
        xRot = Mathf.Clamp(xRot - input.Look.y, -maxLookAngle, maxLookAngle);
        camPivot.localRotation = Quaternion.Euler(xRot, 0f, 0f);

        // stance
        if (input.CrouchPressed)
            CurrentStance = CurrentStance == Stance.Crouch ? Stance.Stand : Stance.Crouch;
        if (input.PronePressed)
            CurrentStance = CurrentStance == Stance.Prone ? Stance.Stand : Stance.Prone;

        bool jumpRequested = input.Jump;
        if (jumpRequested && CurrentStance != Stance.Stand)
        {
            CurrentStance = Stance.Stand; // jump key stands you up first
            jumpRequested = false;
        }
        ApplyStance();

        // speed by stance
        float speed = walkSpeed;
        if (CurrentStance == Stance.Crouch) speed = crouchSpeed;
        else if (CurrentStance == Stance.Prone) speed = proneSpeed;
        else if (input.Sprint) speed = sprintSpeed;

        // gravity + jump
        bool grounded = cc.isGrounded;
        if (grounded && velocity.y < 0) velocity.y = -2f;
        if (grounded && jumpRequested)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        velocity.y += gravity * Time.deltaTime;

        // one single move
        Vector3 move = transform.right * input.Move.x + transform.forward * input.Move.y;
        cc.Move((move * speed + Vector3.up * velocity.y) * Time.deltaTime);
    }

    void ApplyStance()
    {
        int i = (int)CurrentStance;
        float t = Time.deltaTime * stanceBlend;

        curHeight = Mathf.Lerp(curHeight, heights[i], t);
        float centerY = curHeight / 2f - 1f; // keeps feet on the same spot
        cc.height = curHeight;
        cc.center = new Vector3(0f, centerY, 0f);

        if (body != null)
        {
            body.localScale = new Vector3(1f, curHeight / 2f, 1f);
            body.localPosition = new Vector3(0f, centerY, 0f);
        }

        Vector3 p = camPivot.localPosition;
        p.y = Mathf.Lerp(p.y, eyeY[i], t);
        camPivot.localPosition = p;
    }
}