using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : MonoBehaviour {


	[HideInInspector]
	public Animator anim;
	public Infantry AI;

	public int team;


	// Use this for initialization
	void Start () {
		anim = GetComponentInChildren<Animator> ();
		AI = GetComponent<Infantry> ();
	}

	// Update is called once per frame
	void Update () {

		curCooldown = Mathf.Clamp (curCooldown - Time.deltaTime, 0, maxCooldown);
		stunTimer = Mathf.Clamp (stunTimer - Time.deltaTime, 0, 3);

	}

	public float attackRange = 0.8f;
	public float health = 10;
	public float maxCooldown = 1f; 
	public float curCooldown = 0f;
	public float hitFrame = 0.4f;

	public float stunTimer = 0f;

	public float damage = 1;

	public float rotationLerpSpeed = 2f;

	public void GetHit ( float theDamage, float stunDuration) {

		stunTimer = stunDuration;
		anim.SetTrigger ("GetHit");

		health -= theDamage;
		if(health <= 0) {
			AI.myFormation.Kill (AI);
		}
	}
}

