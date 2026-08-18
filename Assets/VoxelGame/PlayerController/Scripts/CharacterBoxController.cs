//#define DRAW_EDGE_HITS

using System;
using UnityEngine;

namespace VoxelGame.Player
{
	public class CharacterBoxController : MonoBehaviour
	{
		private const float COLLISION_MARGIN = 0.01f;
		private const float NORMAL_MARGIN = 0.01f;

		public Vector3 Size = new Vector3(1, 1, 1);

		[NonSerialized] private Vector3 lastPosition;
		[NonSerialized] private LayerMask collisionLayers;

		public Vector3 Velocity { get; private set; }
		public bool IsGrounded { get; private set; }
		public bool HitHead { get; private set; }

		private void Start()
		{
			lastPosition = transform.position;

			UpdateCollisionLayers();
		}

		/// <summary>
		/// Update the collision layers based on the configured layer collision matrix.
		/// </summary>
		public void UpdateCollisionLayers()
		{
			collisionLayers = 0;
			for (int j = 0; j < 32; j++)
			{
				if (!Physics.GetIgnoreLayerCollision(gameObject.layer, j))
				{
					collisionLayers |= 1 << j;
				}
			}
		}

		public void Move(Vector3 motion)
		{
			if (motion == Vector3.zero)
			{
				Velocity = Vector3.zero;
				return;
			}

			Vector3 pos = transform.position;
			Vector3 sizeHalf = Size / 2;
			Vector3 sizeHalfM = new Vector3(sizeHalf.x - COLLISION_MARGIN, sizeHalf.y - COLLISION_MARGIN, sizeHalf.z - COLLISION_MARGIN);

			IsGrounded = false;
			HitHead = false;
			RaycastHit hit;

			// Check each axis separately
			if (motion.x != 0)
			{
				if (Physics.BoxCast(pos, new Vector3(0, sizeHalfM.y, sizeHalfM.z), Vector3.right * Mathf.Sign(motion.x), out hit, Quaternion.identity, Mathf.Abs(motion.x) + sizeHalf.x, collisionLayers, QueryTriggerInteraction.Ignore))
				{
					motion.x = Mathf.Sign(motion.x) * (hit.distance - sizeHalf.x);
				}
			}

			if (motion.y != 0)
			{
				if (Physics.BoxCast(pos, new Vector3(sizeHalfM.x, 0, sizeHalfM.z), Vector3.up * Mathf.Sign(motion.y), out hit, Quaternion.identity, Mathf.Abs(motion.y) + sizeHalf.y, collisionLayers, QueryTriggerInteraction.Ignore))
				{
					IsGrounded = motion.y < 0;
					HitHead = !IsGrounded;
					motion.y = Mathf.Sign(motion.y) * (hit.distance - sizeHalf.y);
				}
			}

			if (motion.z != 0)
			{
				if (Physics.BoxCast(pos, new Vector3(sizeHalfM.x, sizeHalfM.y, 0), Vector3.forward * Mathf.Sign(motion.z), out hit, Quaternion.identity, Mathf.Abs(motion.z) + sizeHalf.z, collisionLayers, QueryTriggerInteraction.Ignore))
				{
					motion.z = Mathf.Sign(motion.z) * (hit.distance - sizeHalf.z);
				}
			}


			// check movement in target direction (eg. check vertical collisions)
			if (Physics.BoxCast(pos, sizeHalfM, motion.normalized, out hit, Quaternion.identity, motion.magnitude, collisionLayers, QueryTriggerInteraction.Ignore))
			{

				if (Mathf.Abs(hit.normal.y) < NORMAL_MARGIN)
				{
					if (Mathf.Abs(transform.eulerAngles.y % 90) < 45)
					{
						motion.x = 0;
					}
					else
					{
						motion.z = 0;
					}

#if DRAW_EDGE_HITS && UNITY_EDITOR
					print("Horizontal edge hit: " + gameObject.name + "\n" + hit.distance + "\n" + hit.point+ "\n" + hit.normal);
					DrawBox(transform.position + motion, Size, Color.red, 10f);
#endif
				}
				else
				{
					motion.y = 0;
					
#if DRAW_EDGE_HITS && UNITY_EDITOR
					print("Vertical edge hit: " + gameObject.name + "\n" + hit.distance + "\n" + hit.point+ "\n" + hit.normal);
					DrawBox(transform.position + motion, Size, Color.yellow, 10f);
#endif
				}
				
			}


			// Apply the adjusted movement
			transform.position += motion;

			// Calculate velocity
			Velocity = (transform.position - lastPosition) / Time.deltaTime;

			// Store the new position for the next frame
			lastPosition = transform.position;
		}

#region debug

		public static void DrawHitBox(CharacterBoxController controller) => DrawHitBox(controller, Color.white);
		public static void DrawHitBox(CharacterBoxController controller, Color color, float duration = 0, bool depthTest = true) => 
			DrawBox(controller.transform.position, controller.Size, color, duration, depthTest);
		public static void DrawBox(Vector3 pos, Vector3 size) => DrawBox(pos, size, Color.white);
		public static void DrawBox(Vector3 pos, Vector3 size, Color color, float duration = 0, bool depthTest = true)
		{
			Vector3 point1 = pos + new Vector3(-size.x, -size.y, -size.z) * 0.5f;
			Vector3 point2 = pos + new Vector3(-size.x, -size.y, size.z) * 0.5f;
			Vector3 point3 = pos + new Vector3(size.x, -size.y, size.z) * 0.5f;
			Vector3 point4 = pos + new Vector3(size.x, -size.y, -size.z) * 0.5f;

			Vector3 point5 = pos + new Vector3(-size.x, size.y, -size.z) * 0.5f;
			Vector3 point6 = pos + new Vector3(-size.x, size.y, size.z) * 0.5f;
			Vector3 point7 = pos + new Vector3(size.x, size.y, size.z) * 0.5f;
			Vector3 point8 = pos + new Vector3(size.x, size.y, -size.z) * 0.5f;

			Debug.DrawLine(point1, point2, color, duration, depthTest);
			Debug.DrawLine(point2, point3, color, duration, depthTest);
			Debug.DrawLine(point3, point4, color, duration, depthTest);
			Debug.DrawLine(point4, point1, color, duration, depthTest);

			Debug.DrawLine(point5, point6, color, duration, depthTest);
			Debug.DrawLine(point6, point7, color, duration, depthTest);
			Debug.DrawLine(point7, point8, color, duration, depthTest);
			Debug.DrawLine(point8, point5, color, duration, depthTest);

			Debug.DrawLine(point1, point5, color, duration, depthTest);
			Debug.DrawLine(point2, point6, color, duration, depthTest);
			Debug.DrawLine(point3, point7, color, duration, depthTest);
			Debug.DrawLine(point4, point8, color, duration, depthTest);
		}

#if UNITY_EDITOR
		/// <summary>
		/// Draw the collider in the editor.
		/// </summary>
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireCube(transform.position, Size);
		}
#endif

#endregion
	}
}