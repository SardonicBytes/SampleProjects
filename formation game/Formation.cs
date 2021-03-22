using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Formation : MonoBehaviour {

	//Debugging Only
	public Transform debugSpawn;
	public int howManyDebugUnits = 0;
	public GameObject debugUnit;
	public GameObject[] debugUnits;
	public Vector2Int killMe;
	//End Debug Only

	public string formationID;
	public int team;

	//Initialize
	void Start () {
		SpawnVisualizers (); //Debugging

		SpawnDebugUnits (); //Debugging


		//Initialize
		var newSpots = GetStandardSpots (transform.position, transform.forward, transform.right, width);
		ChangeShape(line,newSpots);

		InvokeRepeating("Refresh",1,Random.Range(0.05f,0.1f));
		InvokeRepeating("AutoFill",1,Random.Range(0.4f,0.7f));

		formationID = Random.Range (0,1000).ToString();

	}

	//On intervals has units move forward if there is empty spots.
	void AutoFill(){
		for (int y = 0; y < line.Count - 1; y++) {
			for (int x = 0; x < line [y].spot.Count; x++) {
				if (!HasUnit(v2(x,y)) && !IsBlocked(v2(x,y))) {
					FillThisSpot(v2(x,y));
				}
			}
		}
	}

	//Debugging only
	void SpawnDebugUnits (){
		debugUnits = new GameObject[howManyDebugUnits];
		for (int i = 0; i < howManyDebugUnits; i++) {
			debugUnits [i] = Instantiate (debugUnit, debugSpawn.position, debugSpawn.rotation);
			debugUnits [i].name = i.ToString();
			AddUnit (debugUnits[i].GetComponent<Infantry>());
			debugUnits [i].GetComponent<Soldier> ().team = team;
		}

	}

	//Debugging only
	void SpawnSingleDebugUnit (){

		var newDebugUnit = Instantiate (debugUnit, debugSpawn.position, debugSpawn.rotation);
		AddUnit (newDebugUnit.GetComponent<Infantry>());
		newDebugUnit.GetComponent<Soldier> ().team = team;


	}

	//Removes a member and destroys them.
	public void Kill ( Infantry AI ){
		ClearSpot (AI.index);
		Destroy (AI.AIBox);
		Destroy (AI.gameObject);

	}

	void WipeOutFirstLine (){
		for (int i = 0; i < line [0].spot.Count; i++) {
			var objRef = line [0].spot [i].AI.gameObject;
			ClearSpot (v2 (i, 0));
			Destroy (objRef);
		}
	}

	//Inputs
	void Update () {

		//DebugTools
		if(Input.GetKeyDown(KeyCode.Equals)){
			width++;
		}
		if(Input.GetKeyDown(KeyCode.Minus)){
			width--;
		}
		if(Input.GetKeyDown(KeyCode.Alpha5)){
			SpawnSingleDebugUnit ();
		}
		if(Input.GetKeyDown(KeyCode.Alpha4)){
			Refresh();
		}
		if(Input.GetKeyDown(KeyCode.Alpha3)){
			Engage();
		}
		if(Input.GetKeyDown(KeyCode.Alpha2)){
			WipeOutFirstLine();
		}
		if(Input.GetKeyDown(KeyCode.Alpha1)){
			Disengage();
		}
		PreviewVisualizers ();
		//EndDebugTools
	}

	[SerializeField]
	public List<Line> line;

	public float spacing = 1f;

	public bool engage = false;


	//Horizontal width (in units) of the formation.  Apply a shape change whenever this changes.
	private int _width = 8;
	public int width {
		get { return _width; }
		set {  if (_width != value) {
				_width = value;
				//var newSpots = GetStandardSpots (transform., _width);
				//ChangeShape(line,newSpots);
			}
		}
	}

	Formation () {
		line = new List<Line> ();
	}

	//Returns the number of units in the formation
	public int UnitCount (){
		int counter = 0;
		for (int y = 0; y < line.Count; y++) {
			for (int x = 0; x < line [y].spot.Count; x++) {
				if( HasUnit(new Vector2Int(x,y)) ) {
					counter++;
				}
			}
		}
//		print (counter);
		return counter;
	}

	[System.Serializable]
	public class Spot {

		private GameObject AIBox;

		private Infantry _AI;
		public Infantry AI {
			get { 
				if (_AI != null) {
					return _AI; 
				} else {
					return null;
				}
			}
			set {
				_AI = value;
				if (_AI != null) {
					//AI.RequestUpdateSpot ();

				}
			}
		}


		private Vector3 _position; 
		public Vector3 position {
			get {return _position;}
			set {
				_position = value;
			}

		}

		private bool _blocked = true; 
		public bool blocked {
			get {return _blocked;}
			set {
				_blocked = value;

			}

		}


		public Spot ( Infantry newAI, Vector3 newPosition ){
			AI = newAI;
			position = newPosition;
		}

		public Spot ( Vector3 newPosition ){
			position = newPosition;
		}

		public Spot (){
		}

	}

	[System.Serializable]
	public class Line {
		[SerializeField]
		public List<Spot> spot;

		public Line () {
			spot = new List<Spot> ();
		}

		public Line ( int length) {
			spot = new List<Spot> ();
			for(int i = 0; i < length; i ++ ) {
				spot.Add(new Spot());
			}
		}
	}

	//Returns List of units in the formation
	List<Infantry> GetAIList (){
		List<Infantry> list = new List<Infantry> ();

		for (int L = 0; L < line.Count; L ++){
			for ( int S = 0; S < line[L].spot.Count; S++){
				if (HasUnit (v2(S,L))) {
					list.Add (line [L].spot [S].AI);
				}
			}
		}
			
		return list;
	}

	//Adds unit to the formation. Used only for setting up and merging formations
	void AddUnit ( Infantry AI ) {
		//Add to an empty position in the backline if possible, else create new line for them

		RemoveEmptyBackLines ();

		if (LineHasOpen (line.Count - 1)) {
			
			for (int i = 0; i < line [line.Count - 1].spot.Count; i++) {
				Vector2Int checkSpot = new Vector2Int (i,line.Count - 1);
				if (!HasUnit (checkSpot) && !IsBlocked(checkSpot)) {

					AI.EnterFormation (this, checkSpot, formationID);
					AssignPosition (AI, checkSpot);
					return;
				}
			}
			Debug.LogError ("LineHasOpen has said there is a spot available, but no spots were available");

		} else {
			//If the backline is full, make a new one and plop him in there.

			AddBackLine(width);
			Vector2Int newSpot = new Vector2Int (0, line.Count - 1);
			AI.EnterFormation (this, newSpot, formationID );
			AssignPosition(AI, newSpot);
		}

		var newSpots = GetStandardSpots (transform.position, transform.forward, transform.right, width);
		ChangeShape(line,newSpots);

	}

	//Assigns a new position to a unit within the formation.
	public void ChangePosition ( Vector2Int oldIndex, Vector2Int newIndex) {

		//Clears the old position, assigns this unit to the new spot.  New spot should already be clear.
		var newAI = line [oldIndex.y].spot [oldIndex.x].AI;
		ClearSpot (oldIndex);

		while(newIndex.y >= line.Count){
			AddBackLine (width);
		}
			
		AssignPosition(newAI,newIndex);

		RemoveEmptyBackLines ();

	}

	void AssignPosition (Infantry AI, Vector2Int index){
		line [index.y].spot [index.x].AI = AI;
		line [index.y].spot [index.x].AI.UpdateSpot(index);
		//AI.AIBox.transform.position = line[index.y].spot[index.x].position;
	}

	public void UpdateAI( Infantry AI){
		for (int y = 0; y < line.Count; y++) {
			for (int x = 0; x < line [y].spot.Count; x++) {
				if (ReferenceEquals (AI, line[y].spot[x].AI)) {
					AI.UpdateSpot (v2 (x, y));
				}
			}
		}

	}

	void UpdateAllAI(){
		for (int y = 0; y < line.Count; y++) {
			for (int x = 0; x < line [y].spot.Count; x++) {
				if (line[y].spot[x].AI != null) {
					
					line[y].spot[x].AI.UpdateSpot (v2 (x, y));

				}
			}
		}
	}

	//Fills the spot (if possible) with the closest unit behind them.
	bool FillThisSpot (Vector2Int fillSpot) {

		if(IsBlocked(fillSpot)){
			return false;
		}

		//RemoveEmptyBackLines ();

		int a = 0;
		int mod = 0;


		//protected while Loop
		while (a < 60) {
			a++;

			//The spot we are currently checking
			Vector2Int checkSpot = new Vector2Int (fillSpot.x + mod, fillSpot.y + 1);
			if (checkSpot.y >= line.Count) {
				
				return false;
			}
			if (LineIsEmpty(checkSpot.y)){
				return false;
			}

			if (checkSpot.x >= 0 && checkSpot.x < line[checkSpot.y].spot.Count) {
				if (HasUnit (checkSpot)) {

					//Assign
					ChangePosition (checkSpot, fillSpot);

					return true;

				}

			}

			//Update mod so we can check the next spot
			if (mod > 0) {
				mod = -mod;
			}
			else if (mod < 0) {
				mod = -mod + 1;
			}
			else if (mod == 0) {
				mod = 1;
			}

		}
		print("this is a while loop crash");
		return false;

	}

	//Clear the spot
	void ClearSpot ( Vector2Int clearSpot ){
		
		line [clearSpot.y].spot [clearSpot.x].AI = null;
	}

	//Adds an empty line to the front of the formation
	public void Engage (){
		engage = true;
		line.Insert (0,new Line (width));
		line.Insert (0,new Line (width));

		//UpdateAllAI ();
		Refresh ();
	}

	//Updates the formation with new spots, in a new set location.
	public void Refresh () {

		List<Line> oldList = line;

		List<Infantry> displaced = new List<Infantry> ();

		//Transfers over AI one to one.
		line = GetStandardSpots (transform.position, transform.forward,transform.right, width);
		for (int y = 0; y < oldList.Count; y++) {
			for (int x = 0; x < oldList [y].spot.Count; x++) {

				var AI = oldList [y].spot [x].AI;

				if (HasUnit(oldList, v2(x,y))) {
					if (IsBlocked (v2 (x, y))) {

						//This spot had a unit, but is now unavailable.
						displaced.Add(AI);
					} else {
						
						AssignPosition (AI, v2 (x, y));
					}
				}
			}
		}

		for (int i = 0; i < displaced.Count; i++) {

			FindEmptySpot (displaced[i]);
		}



	}


	void FindEmptySpot ( Infantry AI ) {

		for (int y = 0; y < line.Count; y++) {
			for (int x = 0; x < line [y].spot.Count; x++) {
				var currentSpot = v2 (x, y);

				if (HasUnit (currentSpot)) {
					continue;
				}
				if (IsBlocked(currentSpot)) {
					continue;
				}


				AssignPosition( AI, currentSpot);

				return;
			}
		}

		AddBackLine (width);
		AssignPosition (AI, v2 (0,line.Count));

	}
		

	public void Disengage (){
		engage = false;
		Refresh ();
		//UpdateAllAI ();
	}

	//Adds an empty line to the back of the formation
	void AddBackLine (int size){
		line.Add (new Line (width));
	}

	//Delete the backline if it is empty. Loop until the backline is not empty.
	void RemoveEmptyBackLines (){

		while (LineIsEmpty(line.Count - 1)) {

			if (line.Count == 1) {
				Debug.Log ("Formation is down to 1.");
				return;
			}


			line.RemoveAt (line.Count - 1);
		}
	}

	//Is the line empty
	bool LineIsEmpty (int index) {
		
		if (index < 0) {
			return false;
		}

		for (int x = 0; x < line [index].spot.Count; x++) {
			
			if(HasUnit(v2(x,index))){
				
				return false;
			}
				
		}
		return true;
	}

	//Does the line have at least one open spot?
	public bool LineHasOpen (int index) {

		if (index < 0) {
			return false;
		}
		for (int i = 0; i < line [index].spot.Count; i++) {
			//if(line [line.Count - 1].spot[i].AI == null && !line [line.Count - 1].spot[i].blocked ){
			if(!HasUnit( v2( i, index ) ) && !IsBlocked( v2( i, index ) ) ){
				return true;
			}
		}
		return false;
	}

	//Does this specific spot have a unit in it? Main formation.
	public bool HasUnit ( Vector2Int index ){
		//print(index);
		if(line [index.y].spot[index.x].AI != null){
			return true;
		}
		return false;
	}
	//Does this specific spot have a unit in it? Virtual formation.
	public bool HasUnit ( List<Line> virtualFormation, Vector2Int index ){
		//print(index);
		if(virtualFormation [index.y].spot[index.x].AI != null){
			return true;
		}
		return false;
	}

	//Is this spot blocked by obstacles. Main Formation
	public bool IsBlocked ( Vector2Int index ){

		if (line [index.y].spot [index.x].blocked) {
			return true;
		}

		if (HitAIBox (line [index.y].spot [index.x].position)) {
			return true;
		}

		return false;

	}
	//Is this spot blocked by obstacles. Virtual Formation
	public bool IsBlocked ( List<Line> virtualFormation, Vector2Int index ){

		if (virtualFormation [index.y].spot [index.x].blocked) {
			return true;
		}

		if (HitAIBox (virtualFormation [index.y].spot [index.x].position)) {
			
			return true;
		}

		return false;
	}

	//Applies new positions to a formation when its shape has changed
	public void ChangeShape ( List<Line> oldShape, List<Line>newShape ) {

		//Store the old formation to pull AI from
		var AIList = GetAIList ();

		//Assign a new shape
		line = newShape;

		//HyperPrimitive method of reforming the formation
		int a = 0;
		for (int y = 0; y < line.Count; y++) {

			for (int x = 0; x < line [y].spot.Count; x++) {

				if(a >= AIList.Count){
					return;
				}

				if (!IsBlocked(v2(x,y))) {

					AssignPosition(AIList[a],v2(x,y));
					a++;
					
				}

			}
		}

	}

	public LayerMask terrainMask;
	//Returns an empty formation shape
	public List<Line> GetStandardSpots ( Vector3 anchorPoint, Vector3 forward, Vector3 right, int theWidth){
		List<Line> newShape = new List<Line> ();

		float xOffset = theWidth / -2f * spacing;
		float yOffset = engage ? 2 * spacing : 0;

		//Create 15 lines with positions
		for (int y = 0; y < 15; y++) {

			newShape.Add (new Line ());

			for (int x = 0; x < theWidth; x++) {

				Vector3 rightPosition = right * (x * spacing + xOffset);
				Vector3 vertPosition = (forward * (y * -spacing + yOffset));

				Vector3 p = anchorPoint + rightPosition + vertPosition;

				newShape[y].spot.Add (new Spot());


				RaycastHit rayHit;
				if (Physics.Raycast (p + (Vector3.up * 6), -Vector3.up, out rayHit, 12f, terrainMask)) { 
					p = rayHit.point;
				} else {
					continue;
				}


				UnityEngine.AI.NavMeshHit hit;
				if (UnityEngine.AI.NavMesh.SamplePosition (p, out hit, .9f, UnityEngine.AI.NavMesh.AllAreas)) {

					newShape [y].spot[x].position = hit.position;
					newShape [y].spot[x].blocked = false;

				} 
			}
		}

		return newShape;
	}

	public LayerMask AIBoxLayer;
	public bool HitAIBox ( Vector3 position ) {

		Collider[] hitColliders = Physics.OverlapSphere (position, 0.5f, AIBoxLayer);
		for (int i = 0; i < hitColliders.Length; i++) {
			if (hitColliders [i].name != formationID) {
				return true;
			}
		}
		return false;

	}

	public GameObject visualizer;
	GameObject[] visualizers;
	//Debugging Only.  Spawns the visualizers for debug.
	void SpawnVisualizers () {

		visualizers = new GameObject[80];
		for (int i = 0; i < visualizers.Length; i++) {

			visualizers [i] = Instantiate (visualizer, transform);
			visualizers [i].transform.position = transform.position;

		}

	}

	//Debugging Only. Displays the visualizers in the current spots
	void PreviewVisualizers () {
		int a = 0;
		while(true) {
			visualizers [a].transform.position = Vector3.zero;
			a++;
			if (a == visualizers.Length) {
				break;
			}
		}
		a = 0;
		for (int y = 0; y < line.Count; y++) {
			for (int x = 0; x < line[y].spot.Count; x++) {
				if(!IsBlocked(v2(x,y))){
					visualizers [a].transform.position = line [y].spot [x].position;
					a++;
					if (a == visualizers.Length) {
						return;
					}
				}
			}
		}
	}


	public Vector2Int v2 ( int x, int y) {
		return new Vector2Int(x,y);
	}

	public Vector3 GetFormationCenter () {

		Vector3 average = Vector3.zero;

		var v3List = new List<Vector3>();
		for (int y = 0; y < line.Count; y++) {
			for (int x = 0; x < line [y].spot.Count; x++) {
				if (HasUnit (v2 (x, y))) {
					v3List.Add (line[y].spot[x].position);
				}
			}
		}


		for (int i = 0; i < v3List.Count; i++) {
			average += v3List [i];
		}

		average = average / v3List.Count;

		return average;

	}

	public List<Vector3> GetPreviewSpots (Vector3 anchorPoint, Vector3 forward, Vector3 right, int theWidth) {
		List <Vector3> returnList = new List<Vector3>();

		List<Line> tempSpots = GetStandardSpots (anchorPoint, forward, right, theWidth);
	
		for (int y = 0; y < tempSpots.Count; y++) {
			for (int x = 0; x < tempSpots [y].spot.Count; x++) {
				if(!IsBlocked(tempSpots,v2(x,y))){
					returnList.Add( tempSpots[y].spot[x].position );
				}
			}

		}

		return returnList;

	}
}

