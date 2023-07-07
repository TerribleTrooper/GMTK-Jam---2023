using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InterpolatedPlatformPoint
{
    public Vector2 pointA;
    public Vector2 pointB;
    public AnimationCurve curve;
    public float speed;
    public float waitTime;
}

[RequireComponent(typeof(Rigidbody2D))]
public class InterpolatedMovingPlatform : MonoBehaviour
{
    Rigidbody2D rb;
	[SerializeField] bool alwaysLinkPoints = true;
	public InterpolatedPlatformPoint[] points = new InterpolatedPlatformPoint[2];
	Vector2 originalPos;

	float curveTime = 0;
    int iteration = 0;
    bool shouldWait = false;

	// Quality of life thing
	private void OnValidate()
	{
		if (alwaysLinkPoints)
		{
			for (int i = 1; i < points.Length; i++)
			{
				points[i].pointA = points[i - 1].pointB;
			}
		}
	}

	private void Awake()
    {
		rb = GetComponent<Rigidbody2D>();

		originalPos = transform.position;
		transform.position = originalPos + points[iteration].pointA;
	}

    private void FixedUpdate()
    {
        if (shouldWait)
            return;
		//Vector2.Distance(points[iteration].pointA, points[iteration].pointB)
		//points[iteration].speed
		curveTime += (Time.deltaTime * points[iteration].speed) / Vector2.Distance(points[iteration].pointA, points[iteration].pointB);

        rb.position = originalPos + Vector2.LerpUnclamped(points[iteration].pointA, points[iteration].pointB,
            points[iteration].curve.Evaluate(curveTime));

        if (curveTime >= points[iteration].curve.keys[points[iteration].curve.length - 1].time)
        {
            StartCoroutine(EndOfIteration());
        }
    }

    IEnumerator EndOfIteration()
    {
        shouldWait = true;
        yield return new WaitForSeconds(points[iteration].waitTime);
        Iterate();
        shouldWait = false;
    }

    void Iterate()
    {
        iteration++;
        curveTime = 0;

        if (iteration >= points.Length)
        {
            iteration = 0;
			RigidbodyInterpolation2D temp = rb.interpolation;
			rb.interpolation = RigidbodyInterpolation2D.None;
			transform.position = originalPos + points[iteration].pointA;
			rb.interpolation = temp;
		}
	}



	// Gizmos
	private void OnDrawGizmosSelected()
	{
		for (int i = 0; i < points.Length; i++)
		{
			if (Application.isPlaying)
				GizmoDrawArrow(originalPos + points[i].pointA, originalPos + points[i].pointB);
			else
				GizmoDrawArrow((Vector2)transform.position + points[i].pointA, (Vector2)transform.position + points[i].pointB);
		}
	}

	public void GizmoDrawArrow(Vector3 start, Vector3 end, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
	{
		if (end - start == Vector3.zero)
			return;

		Gizmos.DrawLine(start, end);

		Vector3 right = Quaternion.LookRotation(end - start, Vector3.forward) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
		Vector3 left = Quaternion.LookRotation(end - start, Vector3.forward) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
		Gizmos.DrawRay(end, right * arrowHeadLength);
		Gizmos.DrawRay(end, left * arrowHeadLength);
	}
}