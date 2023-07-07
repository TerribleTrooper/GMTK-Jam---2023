using UnityEngine;

[RequireComponent(typeof(PlatformerController))]
public class SpriteFlipModule : MonoBehaviour
{
	PlatformerController controller;
	[SerializeField] Transform whatToFlip;

	void Awake()
	{
		controller = GetComponent<PlatformerController>();
	}

	void Update()
	{
		if (controller.input > 0)
		{
			whatToFlip.localRotation = Quaternion.Euler(whatToFlip.localRotation.x, 0, whatToFlip.localRotation.z);
		}
		else if (controller.input < 0)
		{
			whatToFlip.localRotation = Quaternion.Euler(whatToFlip.localRotation.x, 180f, whatToFlip.localRotation.z);
		}
	}
}
