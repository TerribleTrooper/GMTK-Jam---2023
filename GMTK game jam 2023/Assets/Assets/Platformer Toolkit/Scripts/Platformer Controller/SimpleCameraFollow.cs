using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
	public Transform target;
	public Vector2 damping = new Vector2(500f, 500f);

	void Update()
	{
		Vector2 newPos = new Vector2(
			Mathf.Lerp(transform.position.x, target.position.x, 1f - Mathf.Pow(damping.x / 60000f, Time.deltaTime)),
			Mathf.Lerp(transform.position.y, target.position.y, 1f - Mathf.Pow(damping.y / 60000f, Time.deltaTime)));
		transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
	}
}