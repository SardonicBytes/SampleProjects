using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Infantry : MonoBehaviour {

	public Vector2Int index;
	public Formation myFormation;
	public GameObject AIBox;
	Soldier sol;
	NavMeshAgent NMA;

	// Use this for initialization
	void Start () {
		InvokeRepeating("Execute",1,Random.Range(0.3f,0.8f));
		NMA = GetComponent<NavMeshAgent> ();
		AIBox = Instantiate (AIBox, transform.position, transform.rotation);
		AIBox.name = myFormation.formationID;
		sol = GetComponent<Soldier> ();
	}

	void Update () {
		sol.anim.SetFloat("Speed",NMA.velocity.magnitude);
	}

	void Execute () {

		NMA.SetDestination (GetNMADest());

		var enemies = ScanForEnemies ();

		if (enemies.Count > 0) {

			FaceTarget (enemies [0].transform.position);
			Attack (enemies [0]);
		}


	}

	Vector3 GetNMADest () {
		Vector3 destination;

		var formPos = myFormation.line[index.y].spot[index.x].position;

		destination = formPos;
		return destination;
	}
		

	public void EnterFormation (Formation theFormation, Vector2Int theIndex, string formationID){
		index = theIndex;
		myFormation = theFormation;

	}

	public void UpdateSpot (Vector2Int theIndex) {
		index = theIndex;
		AIBox.transform.position = myFormation.line[theIndex.y].spot[theIndex.x].position;
	}

	public void RequestUpdateSpot () {
		myFormation.UpdateAI (this);


	}

	public LayerMask AIMask;
	List<GameObject> ScanForEnemies (){
		var hit = Physics.OverlapSphere (transform.position, sol.attackRange, AIMask);
		List<GameObject> enemies = new List<GameObject> ();
		for (int i = 0; i < hit.Length; i++) {
			if (hit [i].gameObject.GetComponent<Soldier> ().team != sol.team) {
				enemies.Add (hit [i].gameObject);
			}
		}
		return enemies;
	}

	void FaceTarget (Vector3 target ){
		target = new Vector3 (target.x, transform.position.y, target.z);
		transform.LookAt (target);
	}

	public void Attack (GameObject target) {

		if (sol.stunTimer != 0) {
			return;
		}
		if (sol.curCooldown != 0) {
			return;
		} 
			
		StartCoroutine(MeleeAttack(target));

	}

	public IEnumerator MeleeAttack (GameObject target) {

		sol.anim.SetTrigger ("Strike");
		sol.curCooldown = sol.maxCooldown;
		yield return new WaitForSeconds (sol.hitFrame);
		if (sol.stunTimer == 0){
			if (target != null) {
				target.GetComponent<Soldier> ().GetHit (sol.damage, sol.stunTimer);
			}
		}
	}

}
