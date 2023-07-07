using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PlatformerController), typeof(Rigidbody2D))]
public class DashModule : MonoBehaviour
{
	PlatformerController controller;
	Rigidbody2D rb;

	Vector2 input = Vector2.right;
	Vector2 dashInput = Vector2.right;
	float prevXInput = 1f;

	[Header("Directions")]
	public KeyCode[] upButtons = new KeyCode[] { KeyCode.W, KeyCode.UpArrow };
	public KeyCode[] downButtons = new KeyCode[] { KeyCode.S, KeyCode.DownArrow };

	[Header("Dashing")]
	public KeyCode[] dashButtons = new KeyCode[] { KeyCode.X, KeyCode.LeftControl };
	public int totalDashes = 1;

	public float dashStartSpeed = 25f;
	public float dashEndSpeed = 10f;

	[Space(5f)]
	public float dashTime = 0.15f;
	public float waitTime = 0.25f;

	[Space(-5, order = 0)]
	[Header("After dash", order = 1)]
	public float reduceXSpeed = 7.5f;
	public float reduceUpSpeed = 12.5f;
	public float reduceDownSpeed = 4.5f;


	[Space(5f)]
	[SerializeField] UnityEvent onDash;
	[SerializeField] UnityEvent onStopDash;


	[HideInInspector] public int dashes;
	float _dashTimer = 0;
	float _waitTimer = 0;

	bool stoppedDashing = true;

	[HideInInspector] public bool isDashing = false;

	private void Awake()
	{
		controller = GetComponent<PlatformerController>();
		rb = GetComponent<Rigidbody2D>();
	}

	private void Update()
	{
		// Dashing deirection
		input.y = (GetAnyUpDirectionKey() ? 1 : 0) - (GetAnyDownDirectionKey() ? 1 : 0);

		if (input.y == 0)
		{
			if (controller.input > 0)
				prevXInput = 1f;
			else if (controller.input < 0)
				prevXInput = -1f;
			input.x = prevXInput;
		}
		else
		{
			input.x = controller.input;
		}
		// Rounding so there is only 1 or 0
		input.x = Mathf.Round(input.x);
		input.y = Mathf.Round(input.y);



		_dashTimer -= Time.deltaTime;
		_waitTimer -= Time.deltaTime;
		// Dashing input
		if ((_dashTimer <= 0 && _waitTimer <= 0) && dashes > 0)
		{
			for (int i = 0; i < dashButtons.Length; i++)
			{
				if (Input.GetKeyDown(dashButtons[i]))
				{
					Dash();
					break;
				}
			}
		}



		if (controller.isGrounded && (_dashTimer <= 0 || _waitTimer <= 0))
		{
			resetDash();
		}

		if (!stoppedDashing && _dashTimer <= 0)
		{
			StopDashing();
		}
	}

	private void FixedUpdate()
	{
		if (isDashing)
		{
			rb.velocity = Mathf.Lerp(dashEndSpeed, dashStartSpeed, _dashTimer / dashTime)
				* dashInput.normalized - controller.GetGravity() + controller.desiredStopVel;
		}
	}

	void Dash()
	{
		_dashTimer = dashTime;
		_waitTimer = waitTime;
		dashes--;
		stoppedDashing = false;

		isDashing = true;
		dashInput = input;
		onDash.Invoke();

		// Add dash impuse
		rb.velocity = new Vector2(rb.velocity.x + dashStartSpeed * dashInput.normalized.x,
									dashStartSpeed * dashInput.normalized.y)        // Cancel y velocity by not adding it to the velocity
					- controller.GetGravity();    // Cancel gravity
	}

	void StopDashing()
	{
		isDashing = false;
		stoppedDashing = true;
		onStopDash.Invoke();

		// Reduce speed
		rb.velocity += new Vector2(
			PlatformerController.ApproachVelocity(rb.velocity.x, 0, Mathf.Abs(dashInput.x) * reduceXSpeed) * Mathf.Abs(dashInput.normalized.x),
			rb.velocity.y < 0 ?
			PlatformerController.ApproachVelocity(rb.velocity.y, 0, Mathf.Abs(dashInput.y) * reduceDownSpeed) * Mathf.Abs(dashInput.normalized.y) :
			PlatformerController.ApproachVelocity(rb.velocity.y, 0, Mathf.Abs(dashInput.y) * reduceUpSpeed) * Mathf.Abs(dashInput.normalized.y));
	}

	public void resetDash()
	{
		dashes = totalDashes;
	}

	#region Direction Input
	public bool GetAnyUpDirectionKey()
	{
		for (int i = 0; i < upButtons.Length; i++)
			if (Input.GetKey(upButtons[i]))
				return true;
		return false;
	}
	public bool GetAnyUpDirectionKeyDown()
	{
		for (int i = 0; i < upButtons.Length; i++)
			if (Input.GetKeyDown(upButtons[i]))
				return true;
		return false;
	}
	public bool GetAnyUpDirectionKeyUp()
	{
		for (int i = 0; i < upButtons.Length; i++)
			if (Input.GetKeyUp(upButtons[i]))
				return true;
		return false;
	}


	public bool GetAnyDownDirectionKey()
	{
		for (int i = 0; i < downButtons.Length; i++)
			if (Input.GetKey(downButtons[i]))
				return true;
		return false;
	}
	public bool GetAnyDownDirectionKeyDown()
	{
		for (int i = 0; i < downButtons.Length; i++)
			if (Input.GetKeyDown(downButtons[i]))
				return true;
		return false;
	}
	public bool GetAnyDownDirectionKeyUp()
	{
		for (int i = 0; i < downButtons.Length; i++)
			if (Input.GetKeyUp(downButtons[i]))
				return true;
		return false;
	}
	#endregion
}
