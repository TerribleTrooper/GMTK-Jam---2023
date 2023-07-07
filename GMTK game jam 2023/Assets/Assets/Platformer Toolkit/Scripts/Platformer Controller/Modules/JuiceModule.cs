using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PlatformerController))]
public class JuiceModule : MonoBehaviour
{
	Rigidbody2D rb;
	PlatformerController controller;
	bool wasGrounded;

	public Vector2 normalSize = Vector2.one;

	[Space(5, order = 1)]
	[Header("Turning", order = 2)]
	[SerializeField] Transform turnTransform;

	[Space(8f)]
	[SerializeField] float turnMaxAngle = 7f;
	[SerializeField] float turnDamping = 0.5f;
	float turn = 0;

	[Header("Squish and stretch")]
	[SerializeField] Transform squishStretchTransform;

	[Space(8f)]
	[SerializeField] float landSquishTime = 0.1f;
	[SerializeField] float landUnsquishTime = 0.1f;
	public Vector2 squishSize = new Vector2(1.15f, 0.5f);
	float squishTimer = 0;
	bool shouldSquish = false;

	[Space(8f)]
	[SerializeField] float jumpStretchTime = 0.1f;
	[SerializeField] float jumpUnstretchTime = 0.25f;
	public Vector2 stretchSize = new Vector2(0.65f, 1.2f);
	float stretchTimer = 0;
	bool shouldStretch = false;

	[Space(5, order = 1)]
	[Header("Falling", order = 2)]
	[SerializeField] Transform fallTransform;
	public Vector2 fallingSize = new Vector2(0.85f, 1f);
	[SerializeField] bool bidirectional = false;
	[SerializeField] float maxFallingSpeed = 20f;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		controller = GetComponent<PlatformerController>();
	}
	private void Start()
	{
		squishStretchTransform.localScale = normalSize;
		wasGrounded = controller.isGrounded;
	}


	private void Update()
	{
		DoSquishAndStretch();

		DoTurning();

		DoFalling();
	}

	void DoSquishAndStretch()
	{
		if (squishStretchTransform == null)
			return;

		if (wasGrounded != controller.isGrounded)
		{
			if (controller.isGrounded) // Land check
			{
				shouldSquish = true;
				shouldStretch = false;

				squishTimer = 0;
			}
			else if (rb.velocity.y > 0) // Jump check
			{
				shouldStretch = true;
				shouldSquish = false;

				stretchTimer = 0;
			}
		}
		wasGrounded = controller.isGrounded;



		if (shouldSquish)
		{
			squishTimer += Time.deltaTime;

			if (squishTimer > landSquishTime + landUnsquishTime)
			{
				shouldSquish = false;
			}


			if (squishTimer <= landSquishTime)
			{
				//Squish
				squishStretchTransform.localScale = easeInOutQuad(normalSize, squishSize,
					squishTimer / landSquishTime);
			}
			else
			{
				// Unsquish
				squishStretchTransform.localScale = easeInOutQuad(squishSize, normalSize,
					(squishTimer - landSquishTime) / landUnsquishTime);
			}
		}



		if (shouldStretch)
		{
			stretchTimer += Time.deltaTime;

			if (stretchTimer > jumpStretchTime + jumpUnstretchTime)
			{
				shouldStretch = false;
			}


			if (stretchTimer <= jumpStretchTime)
			{
				//Stretch
				squishStretchTransform.localScale = easeInOutQuad(normalSize, stretchSize,
					stretchTimer / jumpStretchTime);
			}
			else
			{
				// Unstretch
				squishStretchTransform.localScale = easeInOutQuad(stretchSize, normalSize,
					(stretchTimer - jumpStretchTime) / jumpUnstretchTime);
			}
		}

		squishStretchTransform.localPosition = new Vector2(squishStretchTransform.localPosition.x,
				(normalSize.y - squishStretchTransform.localScale.y) * -0.5f);

		if (!(shouldStretch || shouldSquish))
		{
			squishStretchTransform.localScale = normalSize;
		}
	}

	void DoTurning()
	{
		if (turnTransform == null)
			return;

		turn = Mathf.Lerp(turn,
			rb.velocity.x != 0 && !controller.isSlipping ?
			turnMaxAngle * System.Math.Sign(controller.input) : 0,
			1f - Mathf.Pow(turnDamping / 60000f, Time.deltaTime));
		turnTransform.localEulerAngles = new Vector3(0, 0, -turn);
	}

	void DoFalling()
	{
		if (fallTransform == null)
			return;

		if (!controller.isGrounded && !controller.isSlipping)
		{
			if (rb.velocity.y < 0)
			{
				fallTransform.localScale = Vector2.LerpUnclamped(normalSize, fallingSize, -rb.velocity.y / maxFallingSpeed);
			}
			else if (bidirectional)
			{
				fallTransform.localScale =
					Vector2.LerpUnclamped(normalSize, fallingSize, Mathf.Abs(rb.velocity.y / maxFallingSpeed));
			}
			else
			{
				fallTransform.localScale = normalSize;
			}
		}
		else
		{
			fallTransform.localScale = normalSize;
		}
	}


	Vector2 easeInOutQuad(Vector2 a, Vector2 b, float t)
	{
		return Vector2.Lerp(a, b, t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f);
	}
}
