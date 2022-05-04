namespace TurnTheGameOn.ArrowWaypointer
{
	using UnityEngine;
	using UnityEngine.Events;
	using System;
	using System.Collections;
	using System.Collections.Generic;

	[ExecuteInEditMode]
	public class WaypointController : MonoBehaviour
	{

		public enum Switch { Off, On }

		[System.Serializable]
		public class WaypointComponents
		{
			public string waypointName = "Waypoint Name";
			public Waypoint waypoint;
			public UnityEvent waypointEvent;
		}


		public Transform player;
		public Switch configureMode;
		[Range(0.0001f,20)]public float arrowTargetSmooth; // controls how fast the arrow should smoothly target the next waypoint
		[Range(1,100)]public int TotalWaypoints; // controls how many Waypoints should be used
		public WaypointComponents[] waypointList;
		private GameObject newWaypoint;
		private string newWaypointName;
		private int nextWP;
		private Transform waypointArrow; //Transform used to reference the Waypoint Arrow
		private Transform currentWaypoint; //Transforms used to identify the Waypoint Arrow's target
		private Transform arrowTarget;
    	public float currTime;
		public float totalTime;

		void Start () {
			currTime = 0;
			if(Application.isPlaying){
				GameObject newObject = new GameObject();
				newObject.name = "Arrow Target";
				newObject.transform.parent = gameObject.transform;
				arrowTarget = newObject.transform;
				newObject = null;
			}
			nextWP = 0;
			ChangeTarget ();
		}

        [ContextMenu("Reset")] public void Reset()
        {
            nextWP = 0;
            ChangeTarget();
        }

        void Update () {
			currTime = currTime + Time.deltaTime;
			totalTime= totalTime + Time.deltaTime;
			if (configureMode == Switch.Off) {
				TotalWaypoints = waypointList.Length;
			}
			//Check if the script is being executed in the Unity Editor
			#if UNITY_EDITOR
			if (configureMode == Switch.On) {
				CalculateWaypoints ();
			}
			#endif
			//Keep the Waypoint Arrow pointed at the Current Waypoint
			if (arrowTarget != null) {
				arrowTarget.localPosition = Vector3.Lerp (arrowTarget.localPosition, currentWaypoint.localPosition, arrowTargetSmooth * Time.deltaTime);
				arrowTarget.localRotation = Quaternion.Lerp (arrowTarget.localRotation, currentWaypoint.localRotation, arrowTargetSmooth * Time.deltaTime);
			} else {
				arrowTarget = currentWaypoint;
			}
			if (waypointArrow == null)
				FindArrow ();
			waypointArrow.LookAt(arrowTarget);
		}

		public void WaypointEvent(int waypointEvent){
			waypointList [waypointEvent - 1].waypointEvent.Invoke ();
		}

		public void ChangeTarget(){
			TimeSpan timecurr = TimeSpan.FromSeconds(currTime);
			TimeSpan timetot = TimeSpan.FromSeconds(totalTime);
			print(currentWaypoint + " TOTAL TIME: " + timetot.ToString(@"mm\:ss\:fff") + " ||| TIME FROM LAST CP: " + timecurr.ToString(@"mm\:ss\:fff"));
			currTime = 0;
			int check = nextWP;
			if (check < TotalWaypoints) {
				if (currentWaypoint == null)
					currentWaypoint = waypointList [0].waypoint.transform;
				currentWaypoint.gameObject.SetActive (false);
				currentWaypoint = waypointList [nextWP].waypoint.transform;
				currentWaypoint.gameObject.SetActive (true);
				nextWP += 1;
			}
			if (check == TotalWaypoints) {
				Destroy (waypointArrow.gameObject);
				Destroy (gameObject);
				Application.Quit();
			}
		}

		public void CreateArrow(){
			GameObject instance = Instantiate(Resources.Load("Waypoint Arrow", typeof(GameObject))) as GameObject;
			instance.name = "Waypoint Arrow";
			instance = null;
		}

		public void FindArrow(){
			GameObject arrow = GameObject.Find ("Waypoint Arrow");
			if (arrow == null) {
				CreateArrow ();
				waypointArrow = GameObject.Find ("Waypoint Arrow").transform;
			}else{
				waypointArrow = arrow.transform;
			}
		}

		public void CalculateWaypoints(){
			if (configureMode == Switch.On) {
				System.Array.Resize (ref waypointList, TotalWaypoints);
				if (waypointArrow == null)	FindArrow ();
				for (var i = 0; i < TotalWaypoints; i++) {
					if (waypointList [i] != null && waypointList[i].waypoint == null) {
						newWaypointName = "Waypoint " + (i + 1);
						waypointList [i].waypointName = newWaypointName;
						//setup waypoint reference
						foreach (Transform child in transform) {
							if (child.name == newWaypointName) {		waypointList[i].waypoint = child.GetComponent<Waypoint> ();			}
						}
						if (waypointList [i].waypoint == null) {
							newWaypoint = Instantiate (Resources.Load<GameObject>("Waypoint")) as GameObject;
							newWaypoint.name = newWaypointName;
							newWaypoint.GetComponent<Waypoint> ().waypointNumber = i + 1;
							newWaypoint.transform.parent = gameObject.transform;
							waypointList[i].waypoint = newWaypoint.GetComponent<Waypoint> ();
							waypointList [i].waypoint.waypointController = this;
							Debug.Log ("Waypoint Controller created a new Waypoint: " + newWaypointName);
						}
						currentWaypoint = waypointList [0].waypoint.transform;
					}
				}
				CleanUpWaypoints ();
			}
		}
		
		public void CleanUpWaypoints(){
			if (configureMode == Switch.On) {
				if(Application.isPlaying){
					Debug.LogWarning ("ARROW WAYPOINTER: Turn Off 'Configure Mode' on the Waypoint Controller");
				}
				if (transform.childCount > waypointList.Length) {
					foreach (Transform oldChild in transform) {
						if (oldChild.GetComponent<Waypoint> ().waypointNumber  > waypointList.Length) {
							DestroyImmediate (oldChild.gameObject);
						}
					}
				}
			}
		}



		#if UNITY_EDITOR
		//Draws a Gizmo in the scene view window to show the Waypoints
		public void OnDrawGizmosSelected(int radius) {
			for(var i = 0; i < waypointList.Length; i++){
				if (waypointList [i] != null) {
					if (waypointList [i].waypoint != null) {
						Gizmos.DrawWireSphere (waypointList [i].waypoint.transform.position, waypointList [i].waypoint.radius);
					}
				}
			}
		}
		#endif

	}
}