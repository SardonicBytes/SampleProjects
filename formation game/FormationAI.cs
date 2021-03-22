using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FormationAI : MonoBehaviour {


	bool ignoreEngage = false;
	bool engaging = false;


	public KeyCode commandKey;

	Formation myFormation;

	private Vector3 downLocation;
	public Vector3 goalLocation;
	private Vector3 goalDirection;
	private int goalWidth;

	bool previewing;

	NavMeshAgent NMA;

	// Use this for initialization
	void Start () {
		myFormation = GetComponent<Formation> ();

		//myFormation.InvokeRepeating ("Refresh", 0.1f, 0.2f);

		SpawnTPV ();

		NMA = GetComponent<NavMeshAgent> ();
		NMA.updateRotation = false;
	}
		
	void Update () {

		if (Vector3.Distance (transform.position, goalLocation) < 8f && !engaging) {
			NMA.updateRotation = false;

			transform.rotation = Quaternion.Slerp( transform.rotation, Quaternion.Euler(goalDirection), Time.deltaTime * 2 );

		} else {
			NMA.updateRotation = true;
		}


		if (Input.GetKeyDown (commandKey)) {
			StartPreview ();
			return;
		}
		if (previewing) {

			if(Input.GetMouseButtonDown(1)){
				CancelPreview ();
				return;
			}
				
			Vector3 nowLocation;
			Ray cameraRay = Camera.main.ScreenPointToRay (Input.mousePosition);
			Plane groundPlane = new Plane (Vector3.up,downLocation);
			float hit;

			if (groundPlane.Raycast (cameraRay, out hit)) {
				nowLocation = cameraRay.GetPoint (hit);

				if (Input.GetKeyUp (commandKey)) {

					ManualMovementOrder (nowLocation);
					return;
				}

				PreviewSpots (nowLocation);
			} else {
				print ("there we go");
			}
		}
	}
		
	void StartPreview () {

		previewing = true;
		Ray cameraRay = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast (cameraRay, out hit)) {
			downLocation = hit.point;
		}

	}


	void PreviewSpots(Vector3 nowLocation) {

		//Calculate width
		int width = Mathf.FloorToInt(Vector3.Distance(downLocation, nowLocation));
		width = Mathf.Clamp (width, 4, 14);

		//Set up an anchor
		Transform anchor = new GameObject ().transform;
		anchor.position = (nowLocation + downLocation) / 2;

		Vector3 vDir = nowLocation - downLocation;
		float yAngle = Vector3.SignedAngle (Vector3.forward, vDir, Vector3.up) - 90;
		anchor.rotation = Quaternion.Euler(0,yAngle,0);

		int unitCount = myFormation.UnitCount ();


		List<Vector3> spots = myFormation.GetPreviewSpots (anchor.position, anchor.forward, anchor.right, width);

			for (int a = 0; a < TPV.Length; a++) {
				TPV [a].SetActive (a < unitCount);
				if (a < unitCount) {
					TPV [a].transform.position = spots[a];
				}
			}

		Destroy(anchor.gameObject);

	}

	public GameObject TempPositionVisualizer;
	public GameObject[] TPV;

	public void SpawnTPV () {
		TPV = new GameObject[30];
		for (int i = 0; i < TPV.Length; i++) {
			TPV [i] = Instantiate (TempPositionVisualizer);
		}
	}

	void ManualMovementOrder (Vector3 nowLocation) {

		//Set up an anchor
		Transform anchor = new GameObject ().transform;
		anchor.position = (nowLocation + downLocation) / 2;

		Vector3 vDir = nowLocation - downLocation;
		float yAngle = Vector3.SignedAngle (Vector3.forward, vDir, Vector3.up) - 90;
		anchor.rotation = Quaternion.Euler(0,yAngle,0);



		previewing = false;
		for (int i = 0; i < TPV.Length; i++) {
			TPV [i].SetActive (false);
		}
			
		goalDirection = anchor.rotation.eulerAngles;
		goalLocation = anchor.position;
		goalWidth = Mathf.Clamp(Mathf.FloorToInt (Vector3.Distance (nowLocation, downLocation) / myFormation.spacing),4,14);
		myFormation.width = goalWidth;

		var newShape = myFormation.GetStandardSpots (anchor.position, anchor.forward, anchor.right, goalWidth);
		myFormation.ChangeShape (myFormation.line, newShape);

		NMA.SetDestination (goalLocation);
	}

	void CancelPreview (){
		previewing = false;
		for (int i = 0; i < TPV.Length; i++) {
			TPV [i].SetActive (false);
		}

	}

	public void HitCol( GameObject otherFormation ){

		var otherFormationScript = otherFormation.GetComponent<Formation> ();

		if (otherFormationScript.team == myFormation.team) {
			return;
		}

		if (ignoreEngage) {
			return;
		}

		var enemyFormationCenter = otherFormationScript.GetFormationCenter ();
		//RotateToFace (enemyFormationCenter);

		var relPos = enemyFormationCenter - transform.position;
		relPos = relPos.normalized;

		NMA.SetDestination (transform.position + relPos);
		myFormation.Engage ();
		engaging = true;

	}

	//Pass Along
	void RotateToFace ( Vector3 enemyFormationCenter) {
		//StartCoroutine (IRotateToFace (enemyFormationCenter));
	}

	float rotationSpeed = 2f;

}
