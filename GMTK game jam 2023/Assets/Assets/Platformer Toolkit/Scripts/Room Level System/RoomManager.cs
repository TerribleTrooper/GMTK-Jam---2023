using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Room
{
	public Vector2 position;
	public Vector2 size;

	public bool IsInBounds(Vector2 pos)
	{
		if (pos.x - position.x <= size.x * 0.5f
			&& pos.x - position.x >= -size.x * 0.5f
			&& pos.y - position.y <= size.y * 0.5f
			&& pos.y - position.y >= -size.y * 0.5f)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public float DistanceToRoomEdge(Vector2 pos)
	{
		float xd = Mathf.Abs(size.x * 0.5f - (pos.x - position.x));
		float yd = Mathf.Abs(size.y * 0.5f - (pos.y - position.y));

		return xd > yd ? yd : xd;
	}
}

public class RoomManager : MonoBehaviour
{
	public static RoomManager instance;
	public bool useMinCameraSize = false;
	public List<Room> rooms = new List<Room>();

	[Header("Gizmo Settings:")]
	public Color roomColor = Color.white;
	[HideInInspector] public int selectedRoom = -1;
	public Color selectedRoomColor = Color.yellow;
	public Color negativeSizeRoomColor = new Color(1f, 0.1f, 0.1f, 0.5f);

	public Color cornerHandleColor = Color.white;
	[Range(0.05f, 0.25f)]
	public float cornerHandleSize = 0.1f;
	[Range(0.7f, 1.5f)]
	public float scaleHandleSize = 1.2f;

	void OnValidate()
	{
		if (useMinCameraSize)
		{
			Camera cam = Camera.main;
			Vector2 minCamSize = new Vector2(cam.aspect * cam.orthographicSize, cam.orthographicSize) * 2f;
			for (int i = 0; i < rooms.Count; i++)
			{
				rooms[i].size = new Vector2(Mathf.Max(minCamSize.x, rooms[i].size.x), Mathf.Max(minCamSize.y, rooms[i].size.y));
			}
			useMinCameraSize = false;
		}
	}

	private void Awake()
	{
		instance = this;
		selectedRoom = -1;
	}

	public Room WhatRoomIsIn(Vector2 pos)
	{
		for (int i = 0; i < rooms.Count; i++)
		{
			if (rooms[i].IsInBounds(pos))
				return rooms[i];
		}
		return null;
	}

	public Room WhatRoomIsInIgnoreCurrent(Vector2 pos, Room current, bool getNull)
	{
		for (int i = 0; i < rooms.Count; i++)
		{
			if (rooms[i].IsInBounds(pos) && rooms[i] != current)
				return rooms[i];
		}
		return getNull ? null : current;
	}

	private void OnDrawGizmos()
	{
		for (int i = 0; i < rooms.Count; i++)
		{
			if (rooms[i].size.x < 0 || rooms[i].size.y < 0)
			{
				Gizmos.color = negativeSizeRoomColor;
				Gizmos.DrawCube(rooms[i].position, new Vector2(Mathf.Abs(rooms[i].size.x), Mathf.Abs(rooms[i].size.y)));
			}
			else
			{
				Gizmos.color = i == selectedRoom ? selectedRoomColor : roomColor;
				Gizmos.DrawWireCube(rooms[i].position, rooms[i].size);
			}
		}
	}
}