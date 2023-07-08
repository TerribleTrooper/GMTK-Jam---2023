using UnityEngine;

[System.Serializable]
struct CornerCorrectionCheck
{
	public Vector2 offset;
	public Vector2 size;

	[Space(5f)]
	public int iterations;
	public float distance;

	public CornerCorrectionCheck(Vector2 offset, Vector2 size, int iterations, float distance)
	{
		this.offset = offset;
		this.size = size;
		this.iterations = iterations;
		this.distance = distance;
	}
}

[RequireComponent(typeof(Rigidbody2D), typeof(PlatformerController))]
public class CornerCorrectionModule : MonoBehaviour
{
	Rigidbody2D rb;
	PlatformerController controller;

	[SerializeField]
	CornerCorrectionCheck upCornerCorrection = new CornerCorrectionCheck
	(new Vector2(0, 0.5f), new Vector2(1f, 0.25f), 50, 0.01f);

	[Space(10f)]
	[SerializeField]
	CornerCorrectionCheck forwardCornerCorrection = new CornerCorrectionCheck(new Vector2(0.5f, 0), new Vector2(0.1f, 0.95f), 50, 0.01f);

	[SerializeField] float addYIfGrounded = 0.1f;

	[Space(5f)]
	[SerializeField] float maxAngle = 5f;
	[SerializeField] float maxYVelocity = 3f;
	[SerializeField] float minYVelocity = -4f;

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		controller = GetComponent<PlatformerController>();
	}

	private void Update()
	{
		UpCornerCorrect();

		ForwardCornerCorrect();
	}


	void UpCornerCorrect()
	{
		if (CheckUp(Vector2.zero) && rb.velocity.y > 0)
		{
			if (rb.velocity.x <= 0)
			{
				for (int i = 1; i <= upCornerCorrection.iterations; i++)
				{
					if (!CheckUp(new Vector2(-i * upCornerCorrection.distance, 0)))
					{
						transform.position += (Vector3)new Vector2(-i * upCornerCorrection.distance, 0);
						return;
					}
				}
			}

			if (rb.velocity.x >= 0)
			{
				for (int i = 1; i <= upCornerCorrection.iterations; i++)
				{
					if (!CheckUp(new Vector2(i * upCornerCorrection.distance, 0)))
					{
						transform.position += (Vector3)new Vector2(i * upCornerCorrection.distance, 0);
						return;
					}
				}
			}
		}
	}

	void ForwardCornerCorrect()
	{
		if (CheckForward(Vector2.zero) &&
			rb.velocity.y >= minYVelocity && rb.velocity.y <= maxYVelocity && controller.desiredStopVel.y == 0 &&
			(controller.input != 0 || rb.velocity.x != 0))
		{
			for (int i = 1; i <= forwardCornerCorrection.iterations; i++)
			{
				if (!CheckForward(new Vector2(0, i * forwardCornerCorrection.distance)))
				{
					transform.position += (Vector3)new Vector2(0, i * forwardCornerCorrection.distance);

					if ((rb.velocity.y <= 0 || controller.isGrounded) && !CheckForward(new Vector2(0, addYIfGrounded)))
					{
						transform.position += (Vector3)new Vector2(0, addYIfGrounded);
					}

					return;
				}
			}
		}
	}


	bool CheckUp(Vector2 offset)
	{
		return Physics2D.OverlapBox((Vector2)transform.position + upCornerCorrection.offset + offset,
			upCornerCorrection.size, 0, controller.groundLayer) != null;
	}

	bool CheckForward(Vector2 offset)
	{
		float dir;
		if (rb.velocity.x == 0)
			dir = System.Math.Sign(controller.input);
		else
			dir = System.Math.Sign(rb.velocity.x);

		RaycastHit2D hit = Physics2D.BoxCast(new Vector2(
			transform.position.x + (forwardCornerCorrection.offset.x + offset.x) * dir,
			transform.position.y + forwardCornerCorrection.offset.y + forwardCornerCorrection.size.y * 0.5f + offset.y),
			new Vector2(forwardCornerCorrection.size.x, 0.001f), 0, Vector2.down, forwardCornerCorrection.size.y,
			controller.groundLayer);

		if (hit.collider == null)
			return false;

		if (Vector2.Angle(hit.normal, Vector2.up) <= maxAngle)
			return true;
		else
			return false;
	}


	private void OnDrawGizmosSelected()
	{
		// Up
		Gizmos.DrawWireCube((Vector2)transform.position + upCornerCorrection.offset, upCornerCorrection.size);
		// Forward
		if (Application.isPlaying)
		{
			float dir;
			if (rb.velocity.x == 0)
				dir = System.Math.Sign(controller.input);
			else
				dir = System.Math.Sign(rb.velocity.x);

			Gizmos.DrawWireCube((Vector2)transform.position + forwardCornerCorrection.offset * dir,
				forwardCornerCorrection.size + new Vector2(0, 0.001f));
		}
		else
			Gizmos.DrawWireCube((Vector2)transform.position + forwardCornerCorrection.offset,
				forwardCornerCorrection.size + new Vector2(0, 0.001f));
	}
}
