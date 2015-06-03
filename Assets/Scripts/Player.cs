using UnityEngine;
using System.Collections;
[RequireComponent (typeof(Controller2D))]
public class Player : MonoBehaviour {
	public float moveSpeed=6;
	public float jumpHeight=5;
	public float timeToApex=0.4f;
	public float smoothTimeConstant;
	float accelerationTimeAirborn=0.5f;
	float accelerationTimeGrounded=0.1f;
	float velocityXSmoothing;
	float gravity;
	float jumpVelocity;
	Vector3 velocity;
	Controller2D controller;
	// Use this for initialization

	void Start () {
		controller = GetComponent<Controller2D>();
		gravity = -(2 * jumpHeight) / Mathf.Pow (timeToApex, 2);
		jumpVelocity = Mathf.Abs(gravity) * timeToApex;
	}
	void Update(){
		if (controller.collisions.above || controller.collisions.below) {
			velocity.y =0;
		}
		if (Input.GetKeyDown (KeyCode.UpArrow) && controller.collisions.below) {
			velocity.y = jumpVelocity;
		}
		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		float targetVelocityX = (input.x * moveSpeed);
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX,ref velocityXSmoothing,(controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborn );
		//Debug.Log (input.x*moveSpeed);
		//Debug.Log (input.x);
		velocity.y += gravity * Time.deltaTime;
		controller.Move(velocity * Time.deltaTime);
	}


}
