using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RoomCameraFollow : MonoBehaviour
{
	Camera cam;
	Vector2 targetPos;
	Room currentRoom;

	bool changeRoom = false;

	// Camera movement
	public Transform target;
	public bool canBeInNoRoom = true;
	public Vector2 damping = new Vector2(500f, 500f);

	[Space(10f)]
	public float roomChangeTimeScale = 0.1f;
	public float normalTimeScale = 1f;
	public float minDistanceToChangeTimeScale = 2f;

	private void Awake()
	{
		cam = GetComponent<Camera>();
	}

	private void Start()
	{
		currentRoom = RoomManager.instance.WhatRoomIsIn(target.position);
		if (currentRoom == null)
			changeRoom = true;
	}

	void Update()
	{
		targetPos = target.position;

		if (currentRoom != null)
		{
			Vector2 clampVector = currentRoom.size * 0.5f - new Vector2(cam.aspect * cam.orthographicSize, cam.orthographicSize);
			targetPos = currentRoom.position + new Vector2(
				Mathf.Clamp(targetPos.x - currentRoom.position.x, -clampVector.x, clampVector.x),
				Mathf.Clamp(targetPos.y - currentRoom.position.y, -clampVector.y, clampVector.y));
			if (cam.orthographicSize > currentRoom.size.y * 0.5f)
			{
				targetPos.y = currentRoom.position.y;
			}
			if (cam.aspect * cam.orthographicSize > currentRoom.size.x * 0.5f)
			{
				targetPos.x = currentRoom.position.x;
			}
		}

		Vector2 newPos = new Vector2(
			Mathf.Lerp(transform.position.x, targetPos.x, 1f - Mathf.Pow(damping.x / 60000f, Time.unscaledDeltaTime)),
			Mathf.Lerp(transform.position.y, targetPos.y, 1f - Mathf.Pow(damping.y / 60000f, Time.unscaledDeltaTime)));
		transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);



		// Change time scale when entering a new room so that the player has time to see what's coming
		if (Time.timeScale != normalTimeScale && Vector2.Distance(newPos, targetPos) < minDistanceToChangeTimeScale)
		{
			Time.timeScale = normalTimeScale;
		}


		// Change room when leaving the bounds of the current one
		Room tempRoom;
		if (changeRoom)
		{
			tempRoom = RoomManager.instance.WhatRoomIsIn(target.position);
			if (currentRoom != tempRoom && tempRoom != null)
			{
				currentRoom = tempRoom;
				changeRoom = false;
				Time.timeScale = roomChangeTimeScale;
			}
		}
		else if (!currentRoom.IsInBounds(target.position))
		{
			tempRoom = RoomManager.instance.WhatRoomIsIn(target.position);
			if (tempRoom == null && canBeInNoRoom)
			{
				currentRoom = null;
			}
			changeRoom = true;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(targetPos, 0.15f);
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(target.position, 0.15f);
		if (Application.isPlaying)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawSphere(new Vector2(cam.transform.position.x, cam.transform.position.y), 0.15f);
		}
	}
}
