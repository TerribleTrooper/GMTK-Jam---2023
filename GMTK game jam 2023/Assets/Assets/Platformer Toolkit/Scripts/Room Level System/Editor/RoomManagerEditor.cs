using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomManager))]
public class RoomManagerEditor : Editor
{
	private void OnSceneGUI()
	{
		RoomManager manager = (RoomManager)target;
		bool noRoomSelected = true;
		Undo.RecordObject(target, "Room change");

		for (int i = 0; i < manager.rooms.Count; i++)
		{
			Room r = manager.rooms[i];

			// Size corner handles
			Handles.color = manager.cornerHandleColor;

			Vector2 urCorner = Handles.Slider2D(r.position + r.size * 0.5f, Vector3.forward, Vector2.right, Vector2.up,
				HandleUtility.GetHandleSize(r.position + r.size * 0.5f) * manager.cornerHandleSize,
				Handles.DotHandleCap, EditorSnapSettings.move);
			Vector2 dlCorner = Handles.Slider2D(r.position - r.size * 0.5f, Vector3.forward, Vector2.right, Vector2.up,
				HandleUtility.GetHandleSize(r.position + r.size * 0.5f) * manager.cornerHandleSize,
				Handles.DotHandleCap, EditorSnapSettings.move);

			r.position = dlCorner + (urCorner - dlCorner) * 0.5f;
			r.size = urCorner - dlCorner;
			// Vector2 urCorner = Handles.Slider2D(r.position + r.size * 0.5f, Vector3.forward, Vector2.right, Vector2.up,
			// 	HandleUtility.GetHandleSize(r.position + r.size * 0.5f) * manager.cornerHandleSize, Handles.DotHandleCap, EditorSnapSettings.move);
			// Vector2 dlCorner = Handles.Slider2D(r.position - r.size * 0.5f, Vector3.forward, Vector2.right, Vector2.up,
			// 	HandleUtility.GetHandleSize(r.position + r.size * 0.5f) * manager.cornerHandleSize, Handles.DotHandleCap, EditorSnapSettings.move);
			// Vector2 ulCorner = Handles.Slider2D(r.position + new Vector2(-r.size.x, r.size.y) * 0.5f, Vector3.forward, Vector2.right, Vector2.up,
			// 	HandleUtility.GetHandleSize(r.position + r.size * 0.5f) * manager.cornerHandleSize, Handles.DotHandleCap, EditorSnapSettings.move);
			// Vector2 drCorner = Handles.Slider2D(r.position + new Vector2(r.size.x, -r.size.y) * 0.5f, Vector3.forward, Vector2.right, Vector2.up,
			// 	HandleUtility.GetHandleSize(r.position + r.size * 0.5f) * manager.cornerHandleSize, Handles.DotHandleCap, EditorSnapSettings.move);

			// Vector2 top = (urCorner - ulCorner) * 0.5f;
			// Vector2 bottom = (drCorner - dlCorner) * 0.5f;
			// Vector2 right = (urCorner - drCorner) * 0.5f;
			// Vector2 left = (ulCorner - dlCorner) * 0.5f;

			// r.position = (urCorner + dlCorner + drCorner + ulCorner) * 0.25f;
			// r.size = right + top;

			// Center handle
			r.size = Handles.DoScaleHandle(r.size, r.position, Quaternion.identity,
				HandleUtility.GetHandleSize(r.position) * manager.scaleHandleSize);
			r.position = Handles.DoPositionHandle(r.position, Quaternion.identity);

			// Change room color to yellow if the mouse is hovering over it
			if (r.IsInBounds(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin))
			{
				manager.selectedRoom = i;
				noRoomSelected = false;
			}
		}

		if (noRoomSelected)
			manager.selectedRoom = -1;
	}

	private void OnDisable()
	{
		Tools.hidden = false;
		RoomManager manager = (RoomManager)target;
		manager.selectedRoom = -1;
	}

	private void OnEnable()
	{
		Tools.hidden = true;
	}
}
