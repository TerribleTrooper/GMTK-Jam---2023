using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class WallJumpCheck
{
	public Vector2 offset;
	public float height;
	public float length;

	public WallJumpCheck(Vector2 offset, float height, float length)
	{
		this.offset = offset;
		this.height = height;
		this.length = length;
	}
}

[RequireComponent(typeof(PlatformerController), typeof(Rigidbody2D))]
public class WallJumpModule : MonoBehaviour
{
	PlatformerController controller;
	Rigidbody2D rb;
	DashModule dashModule;
	SprintAndCrouchModule sprintAndCrouchModule;
	float slideInput = 1f;
	float jumpInput = 1f;
	float _bufferTimer = 0;
	float prevWallNormalX = 0;
	bool wasGrounded = false;

	enum SlideBehaviour
	{
		AlwaysSlide,
		MoveToSlide
	}
	[Header("Wall sliding")]
	[SerializeField] SlideBehaviour slideBehaviour = SlideBehaviour.AlwaysSlide;
	[SerializeField] Vector2 slideCheckOffset = new Vector2(0.1f, 0);
	[SerializeField] Vector2 slideCheckSize = new Vector2(0.95f, 0.9f);
	public float slideDrag = 2f;
	public float normalDrag = 0;
	public float slideMaxSpeed = 10f;
	[HideInInspector] public bool isSliding = false;

	[Header("Wall jumping")]
	public Vector2 wallJumpForce = new Vector2(10f, 13f);
	[SerializeField] float loseControlTime = 0.05f;
	[SerializeField] float jumpBufferTime = 0.15f;
	[SerializeField] float maxAngleVariation = 3.5f;
	[SerializeField] float airControlAfterJump = 0.5f;
	[SerializeField] float regainFullControlTime = 0.3f;
	float _regainFullControlTimer = 0;
	bool gainedFullControl = true;
	[Space(8f)]
	[SerializeField] bool canJumpOnSameWall = false;
	public Vector2 sameWallJumpForce = new Vector2(12f, 10f);
	[SerializeField] float loseControlSameWall = 0.15f;
	float _loseControlTimer = 0;

	[Space(10f)]
	[SerializeField] WallJumpCheck backJumpCheck = new WallJumpCheck(Vector2.zero, 0.9f, 0.55f);
	[SerializeField] WallJumpCheck frontJumpCheck = new WallJumpCheck(Vector2.zero, 0.9f, 0.55f);

	[Header("Interactions")]
	[SerializeField] bool resetDash = false;
	[SerializeField] bool uncrouch = false;

	[Header("Events")]
	[SerializeField] UnityEvent onStartSlide;
	[SerializeField] UnityEvent onStopSlide;
	[SerializeField] UnityEvent onFrontWallJump;
	[SerializeField] UnityEvent onBackWallJump;

	private void Awake()
	{
		controller = GetComponent<PlatformerController>();
		rb = GetComponent<Rigidbody2D>();

		dashModule = GetComponent<DashModule>();
		sprintAndCrouchModule = GetComponent<SprintAndCrouchModule>();
	}

	private void Update()
	{
		// Setting the input variables
		if (slideBehaviour == SlideBehaviour.AlwaysSlide)
		{
			if (controller.input > 0)
				slideInput = 1f;
			else if (controller.input < 0)
				slideInput = -1f;
		}
		else if (slideBehaviour == SlideBehaviour.MoveToSlide)
			slideInput = controller.input;


		if (controller.input > 0)
			jumpInput = 1f;
		else if (controller.input < 0)
			jumpInput = -1f;


		// Checking for jump input
		_bufferTimer -= Time.deltaTime;
		if (controller.GetAnyJumpKeyDown() && !controller.isGrounded && !controller.isSlipping)
		{
			_bufferTimer = jumpBufferTime;
		}
	}

	private void FixedUpdate()
	{
		#region Wall jumping
		// Just landed
		_regainFullControlTimer -= Time.fixedDeltaTime;
		if ((controller.isGrounded && !wasGrounded))
		{
			prevWallNormalX = 0;
		}
		if (((controller.isGrounded && !wasGrounded) || _regainFullControlTimer <= 0) && !gainedFullControl)
		{
			controller.airMovementMultiplier = 1f;
			gainedFullControl = true;
		}

		// Actual jump
		RaycastHit2D front =
			Physics2D.BoxCast((Vector2)transform.position + frontJumpCheck.offset, new Vector2(0.001f, frontJumpCheck.height),
			0, Vector2.right * jumpInput, frontJumpCheck.length, controller.groundLayer);
		RaycastHit2D back =
			Physics2D.BoxCast((Vector2)transform.position + backJumpCheck.offset, new Vector2(0.001f, frontJumpCheck.height),
			0, Vector2.left * jumpInput, frontJumpCheck.length, controller.groundLayer);


		if (((front.collider != null && Vector2.Angle(front.normal, Vector2.left * jumpInput) <= maxAngleVariation
					&& (canJumpOnSameWall || front.normal.x != prevWallNormalX)) ||
			back.collider != null && Vector2.Angle(back.normal, Vector2.right * jumpInput) <= maxAngleVariation
					&& (canJumpOnSameWall || back.normal.x != prevWallNormalX))
			&&
			_bufferTimer > 0 && !controller.isGrounded)
		{
			rb.drag = normalDrag;
			StopSliding();


			controller.airMovementMultiplier = 0;
			if (System.Math.Sign(front.normal.x) == -System.Math.Sign(controller.input))
			{
				_loseControlTimer = loseControlSameWall;
			}
			else
			{
				_loseControlTimer = loseControlTime;
			}


			// Force adding
			if (front.collider != null && back.collider != null)
			{
				rb.velocity = new Vector2(rb.velocity.x, wallJumpForce.y);
				onFrontWallJump.Invoke();
				onBackWallJump.Invoke();
				prevWallNormalX = 0;
			}
			else if (front.collider != null)
			{
				if (System.Math.Sign(front.normal.x) == -System.Math.Sign(controller.input))
					rb.velocity = new Vector2(sameWallJumpForce.x * -jumpInput, sameWallJumpForce.y);
				else
					rb.velocity = new Vector2(wallJumpForce.x * -jumpInput, wallJumpForce.y);
				onFrontWallJump.Invoke();
				prevWallNormalX = front.normal.x;
			}
			else if (back.collider != null)
			{
				if (System.Math.Sign(back.normal.x) == System.Math.Sign(controller.input))
					rb.velocity = new Vector2(sameWallJumpForce.x * jumpInput, sameWallJumpForce.y);
				else
					rb.velocity = new Vector2(wallJumpForce.x * jumpInput, wallJumpForce.y);
				onBackWallJump.Invoke();
				prevWallNormalX = back.normal.x;
			}

			_regainFullControlTimer = regainFullControlTime;
			gainedFullControl = false;
			_bufferTimer = 0;
		}

		wasGrounded = controller.isGrounded;

		_loseControlTimer -= Time.fixedDeltaTime;
		if (_loseControlTimer <= 0 && !gainedFullControl)
		{
			controller.airMovementMultiplier = airControlAfterJump;
		}
		#endregion



		#region Sliding
		bool wasSliding = isSliding;
		if (Physics2D.OverlapBox((Vector2)transform.position + slideCheckOffset * slideInput,
			slideCheckSize, 0, controller.groundLayer)
			&& !controller.isGrounded && !controller.isSlipping && rb.velocity.y < 0 && gainedFullControl)
		{
			rb.drag = slideDrag;
			if (rb.velocity.y < -slideMaxSpeed)
				rb.velocity = new Vector2(rb.velocity.x, -slideMaxSpeed);
			isSliding = true;
		}
		else
		{
			rb.drag = normalDrag;
			isSliding = false;
		}

		if (isSliding != wasSliding)
		{
			if (isSliding)
			{
				StartSliding();
			}
			else
			{
				StopSliding();
			}
		}
		#endregion
	}

	void StartSliding()
	{
		onStartSlide.Invoke();
		controller.canJump = false;
	}

	void StopSliding()
	{
		onStopSlide.Invoke();
		controller.canJump = true;
		if (dashModule != null && resetDash)
			dashModule.resetDash();
		if (sprintAndCrouchModule != null && uncrouch)
			sprintAndCrouchModule.Crouch(false);
		controller.SetJumps(controller.totalJumps - 1);
	}

	private void OnDrawGizmosSelected()
	{
		// Sliding gizmos
		Gizmos.color = Color.magenta;
		if (Application.isPlaying)
			Gizmos.DrawWireCube((Vector2)transform.position + slideCheckOffset * slideInput, slideCheckSize);
		else
			Gizmos.DrawWireCube((Vector2)transform.position + slideCheckOffset, slideCheckSize);


		// Wall jumping gizmos
		Gizmos.color = Color.cyan;
		if (Application.isPlaying)
		{
			Gizmos.DrawWireCube(((Vector2)transform.position + (frontJumpCheck.offset +
				new Vector2(frontJumpCheck.length * jumpInput, 0)) * new Vector2(0.5f, 1f)),
				new Vector2(frontJumpCheck.length, frontJumpCheck.height));

			Gizmos.DrawWireCube(((Vector2)transform.position + (backJumpCheck.offset -
				new Vector2(backJumpCheck.length * jumpInput, 0)) * new Vector2(0.5f, 1f)),
				new Vector2(backJumpCheck.length, backJumpCheck.height));
		}
		else
		{
			Gizmos.DrawWireCube(((Vector2)transform.position + (frontJumpCheck.offset +
				new Vector2(frontJumpCheck.length, 0)) * new Vector2(0.5f, 1f)),
				new Vector2(frontJumpCheck.length, frontJumpCheck.height));

			Gizmos.DrawWireCube(((Vector2)transform.position + (backJumpCheck.offset -
				new Vector2(backJumpCheck.length, 0)) * new Vector2(0.5f, 1f)),
				new Vector2(backJumpCheck.length, backJumpCheck.height));
		}
	}
}
