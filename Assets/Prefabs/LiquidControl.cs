using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidControl : MonoBehaviour {

    public static LiquidControl instance;

    public GameObject liquidPrefab;

    public int partitions = 50;
    public float maxDistanceToGround = 0.4f;
    public float liquidHeightOffGround = 0.03f;
    public float liquidDepthBelowGround = 0.06f;
    public float maxLedgePaint = 0.4f;
    public float minLedgeHeight = 0.1f;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    public void PlaceLiquid(Vector3 position, float left, float right)
    {
        GameObject liquid = Instantiate(liquidPrefab, Vector3.zero, Quaternion.identity);
        liquid.GetComponent<LiquidBehavior>().BuildMesh(position, left, right);
    }

}
