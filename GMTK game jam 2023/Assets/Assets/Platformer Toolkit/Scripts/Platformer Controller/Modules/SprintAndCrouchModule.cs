using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PlatformerController), typeof(Rigidbody2D))]
public class SprintAndCrouchModule : MonoBehaviour
{
	PlatformerController controller;
	Rigidbody2D rb;


	[Header("Shared")]
	public float normalMaxSpeed = 13f;
	public float normalAirMaxSpeed = 13.5f;
	enum SprintAndCrouchBehaviour
	{
		CantSprintWhileCrouching,
		CantCrouchWhileSprinting
	}
	[SerializeField] SprintAndCrouchBehaviour behaviour = SprintAndCrouchBehaviour.CantSprintWhileCrouching;



	[Header("Sprinting")]
	public KeyCode[] sprintButtons = { KeyCode.LeftShift };
	public float sprintSpeed = 16f;
	public float airSprintSpeed = 16.5f;
	[SerializeField] float sprintVelocityImpulse = 0.5f;
	[SerializeField] bool stickToGround = true;

	[Space(10f)]
	public UnityEvent onSprint;
	public UnityEvent onStopSprint;

	[HideInInspector] public bool isSprinting = false;
	[HideInInspector] public bool canSprint = true;



	[Header("Crouching")]
	public KeyCode[] crouchButtons = { KeyCode.S, KeyCode.DownArrow };
	public float crouchSpeed = 10f;
	public float airCrouchSpeed = 10f;
	[SerializeField] float airMovementMultilpier = 0.5f;
	[SerializeField][Range(0, 1f)] float crouchJumpHeight = 0.65f;
	[Space(3.5f)]
	public bool canCrouch = true;
	public bool canUncrouch = true;
	[Space(10f)]
	[SerializeField] Vector2 standingCheckSize = new Vector2(1f, 0.5f);
	[SerializeField] Vector2 standingCheckOffset = new Vector2(0, 0.25f);
	[SerializeField] Collider2D[] standingColliders;
	[SerializeField] Collider2D[] crouchingColliders;

	[Space(10f)]
	public UnityEvent onCrouch;
	public UnityEvent onUncrouch;

	[HideInInspector] public bool isCrouching = false;
	bool shouldUncrouch = false;
	bool shouldCrouch = false;

	private void Awake()
	{
		controller = GetComponent<PlatformerController>();
		rb = GetComponent<Rigidbody2D>();
	}

	private void Start()
	{
		for (int i = 0; i < standingColliders.Length; i++)
		{
			standingColliders[i].enabled = true;
		}

		for (int i = 0; i < crouchingColliders.Length; i++)
		{
			crouchingColliders[i].enabled = false;
		}
	}

	private void Update()
	{
		Crouching();

		Sprinting();
	}

	private void Sprinting()
	{
		if (behaviour == SprintAndCrouchBehaviour.CantSprintWhileCrouching && isCrouching)
			return;

		bool wasSprinting = isSprinting;
		isSprinting = GetAntSprintKey();

		if (isSprinting != wasSprinting)
		{
			if (isSprinting && canSprint) // Get key down
			{
				controller.maxSpeed = sprintSpeed;
				controller.airMaxSpeed = airSprintSpeed;
				controller.applyStickToGroundForce = stickToGround;

				onSprint.Invoke();

				rb.AddForce(Vector2.right * controller.input * sprintVelocityImpulse, ForceMode2D.Impulse);
			}
			else // Get key up
			{
				controller.maxSpeed = normalMaxSpeed;
				controller.airMaxSpeed = normalAirMaxSpeed;
				controller.applyStickToGroundForce = true;

				onStopSprint.Invoke();
			}
		}
	}

	private void Crouching()
	{
		if (behaviour == SprintAndCrouchBehaviour.CantCrouchWhileSprinting && isSprinting)
		{
			Crouch(false);
			return;
		}

		bool wasCrouching = isCrouching;
		// Input
		isCrouching = GetAnyCrouchKey();

		if (isCrouching != wasCrouching)
		{
			if (isCrouching && canCrouch) // Get key down
			{
				shouldCrouch = true;
			}
			else if (canUncrouch) // Get key up
			{
				shouldUncrouch = true;
			}
		}



		if (shouldCrouch && controller.isGrounded)
		{
			Crouch(true);
		}
		if (shouldUncrouch && controller.isGrounded)
		{
			Crouch(false);
		}
	}

	public void Crouch(bool crouch)
	{
		if (Physics2D.OverlapBox((Vector2)transform.position + standingCheckOffset, standingCheckSize,
			0, controller.groundLayer) && !crouch)
		{
			return;
		}


		// Enabilng colliders
		for (int i = 0; i < standingColliders.Length; i++)
		{
			standingColliders[i].enabled = !crouch;
		}

		for (int i = 0; i < crouchingColliders.Length; i++)
		{
			crouchingColliders[i].enabled = crouch;
		}


		// Changing speed
		if (crouch)
		{
			controller.maxSpeed = crouchSpeed;
			controller.airMaxSpeed = airCrouchSpeed;

			controller.airMovementMultiplier = airMovementMultilpier;

			controller.jumpHeightMultiplier = crouchJumpHeight;

			shouldCrouch = false;
		}
		else
		{
			controller.maxSpeed = normalMaxSpeed;
			controller.airMaxSpeed = normalAirMaxSpeed;

			controller.airMovementMultiplier = 1f;

			controller.jumpHeightMultiplier = 1f;

			shouldUncrouch = false;
		}
	}

	#region Crouch Input
	public bool GetAnyCrouchKey()
	{
		for (int i = 0; i < crouchButtons.Length; i++)
			if (Input.GetKey(crouchButtons[i]))
				return true;
		return false;
	}
	public bool GetAnyCrouchKeyDown()
	{
		for (int i = 0; i < crouchButtons.Length; i++)
			if (Input.GetKeyDown(crouchButtons[i]))
				return true;
		return false;
	}
	public bool GetAnyCrouchKeyUp()
	{
		for (int i = 0; i < crouchButtons.Length; i++)
			if (Input.GetKeyUp(crouchButtons[i]))
				return true;
		return false;
	}
	#endregion
	#region Sprint Input
	public bool GetAntSprintKey()
	{
		for (int i = 0; i < sprintButtons.Length; i++)
			if (Input.GetKey(sprintButtons[i]))
				return true;
		return false;
	}
	public bool GetAntSprintKeyDown()
	{
		for (int i = 0; i < sprintButtons.Length; i++)
			if (Input.GetKeyDown(sprintButtons[i]))
				return true;
		return false;
	}
	public bool GetAntSprintKeyUp()
	{
		for (int i = 0; i < sprintButtons.Length; i++)
			if (Input.GetKeyUp(sprintButtons[i]))
				return true;
		return false;
	}
	#endregion

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white * 0.9f;
		Gizmos.DrawWireCube((Vector2)transform.position + standingCheckOffset, standingCheckSize);
	}
}
