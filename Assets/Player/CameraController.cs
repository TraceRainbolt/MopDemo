using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public GameObject player;
    public GameObject screenBounds;
	public float minPan = -1;
	public float maxPan = 1;
	public float minRotation = 20;
	public float maxRotation = 25;

	private Bounds bounds;

	void Start () {
		bounds = screenBounds.GetComponent<Renderer>().bounds;
	}
	
	void Update () {
		float playerPositionRangeX = (player.transform.position.x - bounds.min.x) / bounds.size.x;
		playerPositionRangeX = Mathf.Clamp(playerPositionRangeX, 0, 1);

		Vector3 cameraPosition = transform.position;
		cameraPosition.x = (maxPan - minPan) * playerPositionRangeX + minPan;
		transform.position = cameraPosition;

		Vector3 cameraRotation = transform.eulerAngles;
		cameraRotation.y = (maxRotation - minRotation) * playerPositionRangeX + minRotation;
		transform.eulerAngles = cameraRotation;
	}
}
