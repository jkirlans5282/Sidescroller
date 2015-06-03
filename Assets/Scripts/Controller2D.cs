using UnityEngine;
using System.Collections;
[RequireComponent (typeof (BoxCollider2D))]

public class Controller2D : MonoBehaviour {
	const float skinWidth = 0.015f;
	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;
	float horizontalRaySpacing;
	float verticalRaySpacing;
	float maxClimbAngle=45;
	public LayerMask collisionMask;
	public CollisionInfo collisions;
	BoxCollider2D collider; 
	RayCastOrigins rayCastOrigins;

	void Start(){
		collider = GetComponent<BoxCollider2D> ();
		CalculateRaySpacing ();

	}

	void CalculateRaySpacing(){
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2);

		horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue); //makes sure there are at least
		verticalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue); //2 vertical and 2 hor rays

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.y / (verticalRayCount - 1);

	}
	public void Move(Vector3 velocity){
		collisions.Reset ();

		UpdateRayCastOrigins ();
		if (velocity.y != 0) {
			VerticalCollisions (ref velocity);
		}
		if (velocity.x != 0) {
			HorizontalCollisions (ref velocity);
		}
			transform.Translate (velocity);
	}
	//ref keyword acts like pointer, directly editing passed variable
	void VerticalCollisions(ref Vector3 velocity){
		float directionY = Mathf.Sign (velocity.y); //down -1, up +1
		float rayLength = Mathf.Abs(velocity.y)+skinWidth;

		for (int i = 0; i<verticalRayCount; i++) {
			Vector2 rayOrigin = (directionY ==-1)?rayCastOrigins.bottomLeft:rayCastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i+velocity.x);
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up*directionY,rayLength, collisionMask);

			Debug.DrawRay(rayOrigin,Vector2.up *directionY * rayLength, Color.red);
			if(hit){
				velocity.y = (hit.distance-skinWidth) * directionY;
				rayLength = hit.distance;

				if(collisions.climbingSlope){
					velocity.x=velocity.y/Mathf.Tan (collisions.slopeAngle*Mathf.Deg2Rad)*Mathf.Sign(velocity.y);
				}
				collisions.below = directionY==-1;
				collisions.above = directionY ==1;
			}
		}
	}
	void climbSlope (ref Vector3 velocity, float slopeAngle){
		float moveDistance = Mathf.Abs (velocity.x);
		float climbVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;
		if (velocity.y <= climbVelocityY) {
			velocity.y = climbVelocityY;
			velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
			collisions.below = true;
			collisions.slopeAngle = slopeAngle;
		}

	}
	void HorizontalCollisions(ref Vector3 velocity){
		float directionX = Mathf.Sign (velocity.x); //down -1, up +1
		float rayLength = Mathf.Abs(velocity.x)+skinWidth;
		//iterates over rays. 
		for (int i = 0; i<verticalRayCount; i++) {
			Vector2 rayOrigin = (directionX ==-1)?rayCastOrigins.bottomLeft:rayCastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.right*directionX,rayLength, collisionMask);
			Debug.DrawRay(rayOrigin,Vector2.right *directionX * rayLength, Color.red);
			if(hit){
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if(i ==0 && slopeAngle <= maxClimbAngle){
					float distanceToSlope = 0;
					if(slopeAngle!=collisions.slopeAngleOld){
						distanceToSlope = hit.distance - skinWidth;
						velocity.x -= distanceToSlope * directionX;
					}
					climbSlope(ref velocity, slopeAngle);
					velocity.x += distanceToSlope * directionX;
					print(slopeAngle);
					collisions.climbingSlope = true;

				}
				if(!collisions.climbingSlope||slopeAngle>maxClimbAngle){
					velocity.x = (hit.distance-skinWidth) * directionX;

					rayLength = hit.distance;
					if(collisions.climbingSlope){
						velocity.y =Mathf.Tan(collisions.slopeAngle*Mathf.Deg2Rad)*Mathf.Abs(velocity.x);
					}

					collisions.left = directionX==-1; //which side hit.
					collisions.right = directionX ==1;
				}
			}
		}
	}
	void UpdateRayCastOrigins(){
		Bounds bounds = collider.bounds;
		bounds.Expand (skinWidth * -2);
		rayCastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		rayCastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		rayCastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
		rayCastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);

	}
	struct RayCastOrigins{
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}
	public struct CollisionInfo{
		public bool above, below, left,right,climbingSlope;
		public float slopeAngle,slopeAngleOld;
		public void Reset(){
			above = below = false;
			left = right = false;
			climbingSlope = false;
			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}
}