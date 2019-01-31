using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public float moveSpeed = 5.0f;
    public float jumpVelocity = 5.0f;

    public float fallMultiplierFloat = 10f;

    private Rigidbody2D rigidbodyComponent;


	void Start () {
		rigidbodyComponent = GetComponent<Rigidbody2D>();
	}
	
	void Update () {
		Vector2 velocity = rigidbodyComponent.velocity;
		velocity.x = Input.GetAxis("Horizontal") * moveSpeed;
		if (Physics2D.Raycast(transform.position, Vector2.down, 0.1f, ~(1 << 8)) && Input.GetAxis("Jump") > 0) {
		    velocity.y = jumpVelocity;
		}
		rigidbodyComponent.velocity = velocity;
    }

    void FixedUpdate()
    {
        if (rigidbodyComponent.velocity.y < 0)
        {
            rigidbodyComponent.velocity += Vector2.up * Physics.gravity.y * (fallMultiplierFloat - 1) * Time.deltaTime;
        }
    }
}

