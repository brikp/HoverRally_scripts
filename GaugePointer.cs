using UnityEngine;
using System.Collections;

public class GaugePointer : MonoBehaviour {

	private Vector2 pivotPoint;
	private float angle;
	private float speed, maxspeed;
	private GameController gameController;
	private GameObject player;
	private Vector2 size = new Vector2();
	public Texture texture;
	Vector2 pos = new Vector2(0, 0);
	Rect rect;
	Vector2 pivot;

	void Start() {
		size.x = texture.width;
		size.y = texture.height;
		UpdateSettings();
		angle = 2;
	}

	void UpdateSettings() {
		pos = new Vector2(transform.localPosition.x, transform.localPosition.y);
		rect = new Rect(pos.x - size.x * 0.5f, pos.y - size.y * 0.5f, size.x, size.y);
		pivot = new Vector2(rect.xMin + rect.width * 0.5f, rect.yMin + rect.height * 0.5f);
		transform.localScale = new Vector3(0.25f, 0.25f, 1);
	}

	void OnGUI() {
		if (Application.isEditor) { UpdateSettings(); }
		Matrix4x4 matrixBackup = GUI.matrix;
		GUIUtility.RotateAroundPivot(angle, pivot);
		GUI.DrawTexture(rect, texture);
		GUI.matrix = matrixBackup;
	}

}