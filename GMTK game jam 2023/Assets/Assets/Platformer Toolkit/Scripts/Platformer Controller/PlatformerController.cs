using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformerController : MonoBehaviour
{
	[HideInInspector] public float input = 0;
	Rigidbody2D rb;
	RaycastHit2D hit;
	RaycastHit2D slopeHit;

	PhysicsMaterial2D zeroFrictionMat;
	PhysicsMaterial2D fullFrictionMat;

	bool keepTurning = false;

	// Public variables for the API
	[HideInInspector] public Vector2 desiredStopVel = Vector2.zero;
	[HideInInspector] public bool canJump = true;
	[HideInInspector] public float movementMultiplier = 1f;
	[HideInInspector] public float airMovementMultiplier = 1f;
	[HideInInspector] public bool isCoyote = false;
	[HideInInspector] public bool dontChangePhysicsMat = false;
	[HideInInspector] public bool applyStickToGroundForce = true;
	[HideInInspector] public float jumpHeightMultiplier = 1f;

	[Space(-10, order = 0)]
	[Header("Better controls", order = 1)]
	[Tooltip("If false the input will be gathered from the horizontal axis of the input manager")]
	[SerializeField] bool useBetterControls = true;
	[Tooltip("If you're not using the better controls should the horizontal axis return only -1, 0 and 1 or also in between values?")]
	[SerializeField] bool onlyIntegerInputValues = true;
	public KeyCode[] moveRightKeys = new KeyCode[] { KeyCode.D, KeyCode.RightArrow };
	public KeyCode[] moveLeftKeys = new KeyCode[] { KeyCode.A, KeyCode.LeftArrow };
	[Header("Movement")]
	public float acceleration = 100;
	public float deceleration = 70;
	public float turnSpeed = 170;
	public float maxSpeed = 13;
	[Space(8)]
	public float airAcceleration = 90;
	public float airDeceleration = 55;
	public float airTurnSpeed = 120;
	public float airMaxSpeed = 13.5f;
	[Space(5)]
	[SerializeField] bool useTurnAcceleration = true;
	[SerializeField] float minSpeedToTurn = 5f;

	[Header("Ground Check")]
	public LayerMask groundLayer;
	[SerializeField] Vector2 raycastOffset = Vector2.zero;
	[SerializeField] float raycastLength = 0.55f;
	public enum GroundCheckType
	{
		CircleCast,
		BoxCast
	}
	[SerializeField] GroundCheckType groundCheckType = GroundCheckType.BoxCast;
	[SerializeField] float boxWidth = 0.95f;
	[SerializeField] float circleRadius = 0.48f;
	[HideInInspector] public bool isGrounded = false;
	bool wasGrounded = false;

	[Header("Jump")]
	[Space(12)]
	public KeyCode[] jumpButtons = new KeyCode[] { KeyCode.W, KeyCode.Space, KeyCode.UpArrow };
	public int totalJumps = 1;
	[HideInInspector] public float jumpForce = 15f;
	public float jumpHeight = 5f;
	public float jumpVelocityImpulse = 0.5f;
	[SerializeField] float smallJumpMultiplier = 0.6f;
	[SerializeField] float fallGravityMultiplier = 1.2f;
	float initialGravityScale;
	bool cancelledJump = false;
	int jumps = 0;
	bool jumped = false;

	[Space(6)]
	public float maxFallSpeed = 20f;

	[Space(8)]
	[SerializeField] float bufferTime = 0.15f;
	float _bufferTimer = 0;
	[SerializeField] float coyoteTime = 0.12f;
	float _coyoteTimer = 0;

	[Header("Slopes")]
	[Space(12)]
	[SerializeField] float maxSlopeAngle = 35;
	[SerializeField] float slipForce = 15;
	[SerializeField] float maxSlipForce = 20;
	[HideInInspector] public bool isSlipping = false;
	bool isOnSlope = false;
	[SerializeField] float stickToGroundForce = 5f;

	[Header("Events")]
	[Space(12)]
	[SerializeField] UnityEvent onJump;
	[SerializeField] UnityEvent onLand;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();

		// Apply slippery physics material
		if (rb.sharedMaterial == null)
		{
			zeroFrictionMat = new PhysicsMaterial2D("Zero Friction");
			zeroFrictionMat.bounciness = 0;
			zeroFrictionMat.friction = 0;
			rb.sharedMaterial = zeroFrictionMat;
		}
		else
		{
			zeroFrictionMat = rb.sharedMaterial;
			zeroFrictionMat.name = rb.sharedMaterial.name;
		}
		fullFrictionMat = new PhysicsMaterial2D("Full Friction");
		fullFrictionMat.friction = 2147483647;
		fullFrictionMat.bounciness = zeroFrictionMat.bounciness;

		ChangeGravityScale(rb.gravityScale);
		jumps = totalJumps - 1;
	}



	void Update()
	{
		// Gather Input
		if (useBetterControls)
		{
			if (input > 0 && GetAnyLeftKeyDown())
				input = -1f;
			if (input < 0 && GetAnyRightKeyDown())
				input = 1f;

			if (input == 0 && GetAnyLeftKey())
				input = -1;
			if (input == 0 && GetAnyRightKey())
				input = 1f;

			if (input < 0 && GetAnyLeftKeyUp())
				input = 0;
			if (input > 0 && GetAnyRightKeyUp())
				input = 0;
		}
		else
		{
			input = onlyIntegerInputValues ? System.Math.Sign(Input.GetAxisRaw("Horizontal")) : Input.GetAxisRaw("Horizontal");
		}



		#region Jumping
		// Timers
		_bufferTimer -= Time.deltaTime;
		_coyoteTimer -= Time.deltaTime;



		if (GetAnyJumpKeyDown())            // Buffer time
		{
			_bufferTimer = bufferTime;
		}
		if (wasGrounded && !isGrounded)
		{
			if (rb.velocity.y <= 0)   // Coyote time
			{
				_coyoteTimer = coyoteTime;
				isCoyote = true;
			}
			else if (!jumped)
			{
				// So the player can't jump after being launched up by something
				jumps--;
			}
		}




		// Apply jump force
		jumpForce = Mathf.Sqrt(-2f * Physics2D.gravity.y * initialGravityScale * jumpHeight * jumpHeightMultiplier);
		if (_bufferTimer > 0 && jumpForce != 0 && (isGrounded || (!isCoyote && !isSlipping && jumps > 0)) && canJump) // Normal jump
		{
			jumps--;
			jumped = true;
			if (GetAnyJumpKey())
			{
				rb.velocity = new Vector2(rb.velocity.x, jumpForce);
				rb.AddForce(Vector2.right * input * jumpVelocityImpulse, ForceMode2D.Impulse);
				_bufferTimer = 0;
				onJump.Invoke();
			}
			else
			{
				// Do a lower jump if not holding jump key
				rb.velocity = new Vector2(rb.velocity.x, jumpForce * smallJumpMultiplier);
				rb.AddForce(Vector2.right * input * jumpVelocityImpulse, ForceMode2D.Impulse);
				_bufferTimer = 0;
				onJump.Invoke();
			}
		}
		else if (_coyoteTimer > 0 && GetAnyJumpKeyDown() && canJump && jumps > 0)   // Coyote jump
		{
			jumped = true;
			rb.velocity = new Vector2(rb.velocity.x, jumpForce);
			rb.AddForce(Vector2.right * input * jumpVelocityImpulse, ForceMode2D.Impulse);
			isCoyote = false;
			_coyoteTimer = 0;
			_bufferTimer = 0;
			onJump.Invoke();
			jumps--;
		}
		if (_coyoteTimer < 0 && isCoyote)
		{
			jumps--;
			isCoyote = false;
		}


		// Cancel the jump if not holding the jump key anymore
		if (GetAnyJumpKeyUp() && rb.velocity.y > 0 && !cancelledJump)
		{
			rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * smallJumpMultiplier);
			if (jumps <= 0)
				cancelledJump = true;
		}
		#endregion
		wasGrounded = isGrounded;
	}


	private void FixedUpdate()
	{
		bool canMove = true;

		#region Ground Check
		hit = GroundCheck();

		//wasGrounded = isGrounded;
		isGrounded = hit.collider != null;
		#endregion

		#region Landing
		if (isGrounded && !wasGrounded && !isSlipping)
		{
			onLand.Invoke();
			jumped = false;
			ResetJumps();
		}
		#endregion



		#region Slope Stuff
		if (Vector2.Angle(Vector2.up, hit.normal) > maxSlopeAngle && Vector2.Angle(Vector2.up, hit.normal) < 90f)
		{
			isGrounded = false;
			isSlipping = true;

			canMove = false;
		}
		else
		{
			isSlipping = false;
		}

		isOnSlope = Vector2.Angle(Vector2.up, hit.normal) > 0.01f;
		#endregion

		#region Slipping
		if (isSlipping)
		{
			rb.AddForce(Vector2.down * slipForce * Time.fixedDeltaTime, ForceMode2D.Impulse);
			if (rb.velocity.y < -maxSlipForce)
				rb.velocity = new Vector2(rb.velocity.x, -maxSlipForce);
		}
		#endregion





		#region Falling
		// More gravity when falling
		if (rb.velocity.y < 0 && !isGrounded)
			rb.gravityScale = initialGravityScale * fallGravityMultiplier;
		else
			rb.gravityScale = initialGravityScale;


		// Clamp falling speed
		if (rb.velocity.y < -maxFallSpeed && !isSlipping)
			rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
		#endregion



		#region Movement
		if (!canMove)
			return;


		float acc = isGrounded ? acceleration * movementMultiplier : airAcceleration * airMovementMultiplier;
		float dec = isGrounded ? deceleration * movementMultiplier : airDeceleration * airMovementMultiplier;
		float turn = isGrounded ? turnSpeed * movementMultiplier : airTurnSpeed * airMovementMultiplier;
		float maxSp = isGrounded ? maxSpeed : airMaxSpeed;
		Vector2 perp;


		if (hit.normal.magnitude != 0 && (input != 0 || rb.velocity.x != 0))
		{
			perp = Vector2.Perpendicular(hit.normal).normalized;
			perp = -perp; // If normal is up, perpendicular should be right, not left
		}
		else
		{
			perp = Vector2.right;
		}





		if (input != 0)
		{
			float totalXVel = rb.velocity.x - desiredStopVel.x;
			if (System.Math.Sign(input) != System.Math.Sign(totalXVel) && totalXVel != 0)
			{
				// Turning
				keepTurning = true;
				if (isGrounded && !jumped)
				{
					rb.velocity += new Vector2(
						ApproachVelocity(rb.velocity.x, desiredStopVel.x + perp.x * maxSp * System.Math.Sign(input), turn * Time.fixedDeltaTime),
						ApproachVelocity(rb.velocity.y, desiredStopVel.y + perp.y * maxSp * System.Math.Sign(input), turn * Time.fixedDeltaTime));
				}
				else
				{
					rb.velocity += new Vector2(
						ApproachVelocity(rb.velocity.x, desiredStopVel.x + maxSp * System.Math.Sign(input), turn * Time.fixedDeltaTime),
						desiredStopVel.y);
				}
			}
			else
			{
				// Acceleration
				if (isGrounded && !jumped)
				{
					rb.velocity += new Vector2(
						keepTurning && useTurnAcceleration ?
							ApproachVelocity(rb.velocity.x, desiredStopVel.x + perp.x * maxSp * input, turn * Time.fixedDeltaTime) :
							ApproachVelocity(rb.velocity.x, desiredStopVel.x + perp.x * maxSp * input, acc * Time.fixedDeltaTime),
						ApproachVelocity(rb.velocity.y, desiredStopVel.y + perp.y * maxSp * input, acc * Time.fixedDeltaTime));
				}
				else
				{
					rb.velocity += new Vector2(
						keepTurning && useTurnAcceleration ?
							ApproachVelocity(rb.velocity.x, desiredStopVel.x + maxSp * input, turn * Time.fixedDeltaTime) :
							ApproachVelocity(rb.velocity.x, desiredStopVel.x + maxSp * input, acc * Time.fixedDeltaTime),
						desiredStopVel.y);
				}
			}
		}
		else
		{
			// Deceleration
			if (isGrounded && !jumped)
			{
				rb.velocity += new Vector2(
					ApproachVelocity(rb.velocity.x, desiredStopVel.x, dec * Time.fixedDeltaTime),
					ApproachVelocity(rb.velocity.y, desiredStopVel.y, dec * Time.fixedDeltaTime));
			}
			else
			{
				rb.velocity += new Vector2(
					ApproachVelocity(rb.velocity.x, desiredStopVel.x, dec * Time.fixedDeltaTime),
					desiredStopVel.y);
			}
		}

		// Reset turning state
		if (rb.velocity.x <= minSpeedToTurn)
			keepTurning = false;
		#endregion


		// Setting the full friction material so the player doesn't slip on slopes
		if (isOnSlope && isGrounded && rb.velocity.magnitude <= 0.1f && !isSlipping && !dontChangePhysicsMat)
		{
			rb.sharedMaterial = fullFrictionMat;
		}
		else
		{
			rb.sharedMaterial = zeroFrictionMat;
		}

		#region Sticking to slopes
		if (isGrounded && !isSlipping && applyStickToGroundForce && !jumped)
		{
			rb.AddForce(GetStickToGroundForce(), ForceMode2D.Impulse);
		}
		#endregion
	}




	#region Public Functions
	public static float ApproachVelocity(float current, float target, float max)
	{
		return Mathf.Clamp(target - current, -max, max);
	}

	public RaycastHit2D GroundCheck()
	{
		RaycastHit2D hit;
		if (groundCheckType == GroundCheckType.BoxCast)
		{
			hit = Physics2D.BoxCast((Vector2)transform.position + raycastOffset, new Vector2(boxWidth, 0.0005f),
					0, Vector2.down, raycastLength, groundLayer);
		}
		else
		{
			hit = Physics2D.CircleCast((Vector2)transform.position + raycastOffset, circleRadius,
					Vector2.down, raycastLength, groundLayer);
		}

		return hit;
	}

	public void ChangeGravityScale(float gravity)
	{
		initialGravityScale = gravity;
	}

	public Vector2 GetGravity()
	{
		return Physics2D.gravity * rb.gravityScale * Time.fixedDeltaTime;
	}

	public Vector2 GetStickToGroundForce()
	{
		return -hit.normal * stickToGroundForce - GetGravity();
	}

	#region Jump Methods
	public void ResetJumps()
	{
		jumps = totalJumps;
		cancelledJump = false;
	}

	public void SetJumps(int x)
	{
		jumps = x;
		if (x != 0)
			cancelledJump = false;
	}

	public void AddJumps(int x)
	{
		jumps += x;
		if (x != 0)
			cancelledJump = false;
	}
	#endregion

	#region Button Functions
	public bool GetAnyJumpKey()
	{
		for (int i = 0; i < jumpButtons.Length; i++)
		{
			if (Input.GetKey(jumpButtons[i]))
			{
				return true;
			}
		}
		return false;
	}
	public bool GetAnyJumpKeyUp()
	{
		for (int i = 0; i < jumpButtons.Length; i++)
		{
			if (Input.GetKeyUp(jumpButtons[i]))
			{
				return true;
			}
		}
		return false;
	}
	public bool GetAnyJumpKeyDown()
	{
		for (int i = 0; i < jumpButtons.Length; i++)
		{
			if (Input.GetKeyDown(jumpButtons[i]))
			{
				return true;
			}
		}
		return false;
	}




	public bool GetAnyLeftKey()
	{
		for (int i = 0; i < moveLeftKeys.Length; i++)
		{
			if (Input.GetKey(moveLeftKeys[i]))
			{
				return true;
			}
		}
		return false;
	}
	public bool GetAnyLeftKeyDown()
	{
		for (int i = 0; i < moveLeftKeys.Length; i++)
		{
			if (Input.GetKeyDown(moveLeftKeys[i]))
			{
				return true;
			}
		}
		return false;
	}
	public bool GetAnyLeftKeyUp()
	{
		for (int i = 0; i < moveLeftKeys.Length; i++)
		{
			if (Input.GetKeyUp(moveLeftKeys[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool GetAnyRightKey()
	{
		for (int i = 0; i < moveRightKeys.Length; i++)
		{
			if (Input.GetKey(moveRightKeys[i]))
			{
				return true;
			}
		}
		return false;
	}
	public bool GetAnyRightKeyDown()
	{
		for (int i = 0; i < moveRightKeys.Length; i++)
		{
			if (Input.GetKeyDown(moveRightKeys[i]))
			{
				return true;
			}
		}
		return false;
	}
	public bool GetAnyRightKeyUp()
	{
		for (int i = 0; i < moveRightKeys.Length; i++)
		{
			if (Input.GetKeyUp(moveRightKeys[i]))
			{
				return true;
			}
		}
		return false;
	}
	#endregion
	#endregion


	private void OnDrawGizmosSelected()
	{
		// Ground check
		Gizmos.color = Color.white;
		if (groundCheckType == GroundCheckType.BoxCast)
		{
			Gizmos.DrawWireCube((Vector2)transform.position + raycastOffset + Vector2.down * raycastLength * 0.5f,
				new Vector2(boxWidth, raycastLength + 0.000025f));
		}
		else
		{
			Gizmos.DrawWireSphere((Vector2)transform.position + raycastOffset + Vector2.down * raycastLength, circleRadius);
			Gizmos.DrawWireSphere((Vector2)transform.position + raycastOffset, circleRadius);
			Gizmos.DrawWireCube((Vector2)transform.position + raycastOffset + Vector2.down * raycastLength * 0.5f,
				new Vector2(circleRadius * 2f, raycastLength));
		}

		// Normal
		Gizmos.color = Color.red;
		Gizmos.DrawLine(hit.point, hit.point + hit.normal);
	}
}