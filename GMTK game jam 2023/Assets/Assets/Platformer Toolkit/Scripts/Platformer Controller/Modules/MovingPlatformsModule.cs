using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PlatformerController))]
public class MovingPlatformsModule : MonoBehaviour
{
	Rigidbody2D rb;
	PlatformerController controller;
	[SerializeField] string platformTag = "Moving platform";
	// [SerializeField] float launchForce = 10f;
	// [SerializeField] float launchMoveMultipllier = 0.5f;
	Vector2 prevPlatformPos = Vector2.zero;
	bool wasOnMovingPlatform = false;
	// bool wasGrounded;

	private void Awake()
	{
		controller = GetComponent<PlatformerController>();
		rb = GetComponent<Rigidbody2D>();
		controller.desiredStopVel = Vector2.zero;
	}

	private void FixedUpdate()
	{
		RaycastHit2D hit = controller.GroundCheck();

		// if (controller.isGrounded != wasGrounded)
		// {
		// 	if (controller.isGrounded)
		// 	{
		// 		controller.airMovementMultiplier = 1f;
		// 	}
		// 	else
		// 	{
		// 		rb.AddForce(controller.desiredStopVel.normalized * launchForce * Time.fixedDeltaTime, ForceMode2D.Impulse);
		// 	}
		// }
		//wasGrounded = controller.isGrounded;

		if (hit.rigidbody == null || (controller.isGrounded && !hit.rigidbody.CompareTag(platformTag)))
		{
			if (wasOnMovingPlatform)
			{
				wasOnMovingPlatform = false;
				controller.desiredStopVel = Vector2.zero;
			}
			return;
		}

		Rigidbody2D platformRb = hit.rigidbody;

		if (wasOnMovingPlatform)
		{
			controller.desiredStopVel = (platformRb.position - prevPlatformPos) / Time.fixedDeltaTime;
		}

		prevPlatformPos = platformRb.position;
		wasOnMovingPlatform = true;
		//controller.airMovementMultiplier = launchMoveMultipllier;
	}
}
