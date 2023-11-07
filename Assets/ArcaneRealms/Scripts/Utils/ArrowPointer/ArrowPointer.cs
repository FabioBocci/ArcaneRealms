using System;
using System.Collections.Generic;
using ArcaneRealms.Scripts.Interfaces;
using UnityEngine;

namespace ArcaneRealms.Scripts.Utils.ArrowPointer {
	public class ArrowPointer : MonoBehaviour {

		public float tailSegmentSpacing = 0.6f;
		public float tailCurveHeight = 1f;
		public float tailSpeed = 2f;

		public GameObject tailSegmentPrefab;
		public LayerMask terreinLayerMask;
		public LayerMask monsterLayerMask;


		public Transform arrowStartingPosition = null;
		public Action<ITargetable> action = null; //TODO - change MonsterInGameController with an interface Targetable
		public Predicate<ITargetable> predicate = null;

		private List<GameObject> tailSegments = new List<GameObject>();

		private int counter = 0;
		
		void Update() {
			if(arrowStartingPosition == null || action == null || predicate == null) {
				return;
			}

			counter = 0;

			// Get the mouse position in world space
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if(Physics.Raycast(ray, out hit, Mathf.Infinity, terreinLayerMask)) {
				// Update the position of the arrow pointer to the mouse position on the floor
				transform.position = hit.point + Vector3.up * 1;
				counter += 2;
			}


			if(Physics.Raycast(ray, out hit, Mathf.Infinity, monsterLayerMask)) {
				transform.position = hit.transform.position + Vector3.up * 1;
				counter += 5;
			}

			if(Input.GetMouseButtonDown(0)) {
				//callback with monster hit
				action.Invoke(counter < 5 ? null : hit.transform.GetComponent<ITargetable>());
				action = null;
				arrowStartingPosition = null;
				DestroyTail();
				Destroy(gameObject);
				return;
			}




			// Update the positions of the tail segments to create a segmented tail that follows the arc path
			float distance = Vector3.Distance(arrowStartingPosition.position, transform.position);
			int numSegments = Mathf.CeilToInt(distance / tailSegmentSpacing);
			for(int i = 0; i < numSegments; i++) {
				Vector3 segmentPosition = Vector3.Lerp(arrowStartingPosition.position, transform.position, (float) i / numSegments);
				segmentPosition += Vector3.up * (Mathf.Sin((float) i / numSegments * Mathf.PI) * distance) / 10f; // Add arc path to segment position

				if(i >= tailSegments.Count) {
					GameObject segment = Instantiate(tailSegmentPrefab, segmentPosition, Quaternion.identity);
					segment.name = "Segment-" + i;
					segment.transform.SetParent(transform, false);
					tailSegments.Add(segment);
				} else {
					tailSegments[i].transform.position = segmentPosition;
				}

				if(i > 0) {
					Vector3 prevSegmentPosition = tailSegments[i - 1].transform.position;
					Vector3 segmentDirection = segmentPosition - prevSegmentPosition;
					if(segmentDirection.magnitude > 0.001f) {
						tailSegments[i].transform.rotation = Quaternion.LookRotation(segmentDirection, Vector3.up);
					}

					// Rotate the segment to align with the curve
					Vector3 forwardDirection = tailSegments[i].transform.forward;
					float angle = Vector3.SignedAngle(forwardDirection, segmentDirection, Vector3.up);
					tailSegments[i].transform.Rotate(Vector3.right, angle);
				} else {
					// First segment should point towards the monster
					Vector3 segmentDirection = segmentPosition - arrowStartingPosition.position;
					if(segmentDirection.magnitude > 0.001f) {
						tailSegments[i].transform.rotation = Quaternion.LookRotation(segmentDirection, Vector3.up);
					}
				}

			}

			// Remove any extra tail segments
			while(tailSegments.Count > numSegments) {
				int lastIndex = tailSegments.Count - 1;
				Destroy(tailSegments[lastIndex]);
				tailSegments.RemoveAt(lastIndex);
			}
		}

		private void DestroyTail() {
			for(int i = tailSegments.Count - 1; i >= 0; i--) {
				Destroy(tailSegments[i]);
				tailSegments.RemoveAt(i);
			}
		}

	}

}
