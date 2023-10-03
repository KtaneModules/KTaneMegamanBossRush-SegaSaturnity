using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class MegamanBossRush : MonoBehaviour {

    public KMBombModule module;
    public KMBombInfo bombInfo;
    public KMAudio audio;
	
	public KMSelectable[] b_bosses;
	public Material[] m_faces;
	public Material[] b_faces;
	public KMSelectable face;
	
	private bool isSolved;
	private static int moduleCount;
    private int moduleId;
	
	private int[] b_seq = new int[]{0,1,2,3,4,5,6,7};
	private int lastPress;
	private int firstPress;
	private int stage;
	
	void Awake () {
		moduleId = moduleCount++;
		
		reset();
		
		foreach ( KMSelectable boss in b_bosses ) {
			int index = b_bosses.ToList().IndexOf(boss);
			boss.GetComponent<MeshRenderer>().sharedMaterial = b_faces[b_seq[index]];
			boss.OnHighlight += delegate () { moveEyes(index); };
			boss.OnHighlightEnded += delegate () { moveEyes(8); };
			boss.OnInteract += delegate () { pressButton(index); return false; };
		}
		
		face.OnInteract += delegate () { pressBoss(); return false; };
	}
	
	void Start () {
		face.transform.GetChild(0).gameObject.SetActive(false);
		
		firstPress = (bombInfo.GetPorts().Count() + bombInfo.GetBatteryCount()) * bombInfo.GetSerialNumberNumbers().LastOrDefault();
		Debug.LogFormat("[Mega Man Boss Rush #{0}] Module started.", moduleId);
		Debug.LogFormat("[Mega Man Boss Rush #{0}] First press is position {1}", moduleId, firstPress % 8);
	}
	
	void moveEyes (int pos) {
		if (stage > 1) return;
		face.GetComponent<MeshRenderer>().sharedMaterial = m_faces[pos];
	}
	
	void pressButton (int pos) {
		audio.PlaySoundAtTransform("shot", transform);
		b_bosses[pos].AddInteractionPunch(0.25f);
		if (b_seq.Count(s => s == 999) == 0) {
			if (pos != firstPress % 8) {
				Debug.LogFormat("[Mega Man Boss Rush #{0}] Wrong first press! Expected {1}, pressed {2}", moduleId, firstPress % 8, pos);
				module.HandleStrike();
				return;
			}
		}
		
		if (b_seq[pos] == 999) return;
		
		string[] names = new string[] {"Spark Man", "Snake Man", "Needle Man", "Hard Man", "Top Man", "Gemini Man", "Magnet Man", "Shadow Man", "E-Tank"};
		Debug.LogFormat("[Mega Man Boss Rush #{0}] Pressed {1}", moduleId, names[b_seq[pos]]);
		
		if (!checkBoss(b_seq[pos])){
			Debug.LogFormat("[Mega Man Boss Rush #{0}] Strike! {1} wasn't weak to the last boss pressed.", moduleId, names[b_seq[pos]]);
			module.HandleStrike();
			reset();
			return;
		}
		
		lastPress = b_seq[pos];
		b_seq[pos] = 999;
		b_bosses[pos].GetComponent<MeshRenderer>().sharedMaterial = b_faces[11];
		
		
		if (b_seq.Count(s => s == 999) == 8) {
			audio.PlaySoundAtTransform("game_start", transform);
			face.transform.GetChild(0).gameObject.SetActive(true);
			getWilly();
		}
	}
	
	void pressBoss () {
		face.AddInteractionPunch(0.25f);
		if (isSolved) return;
		switch (stage) {
		case 10:
			if ((int)bombInfo.GetTime() % 2 != 0) {
				module.HandleStrike();
				Debug.LogFormat("[Mega Man Boss Rush #{0}] Last Boss was Dr. Willy, expected press at even seconds, pressed at {1}", moduleId, bombInfo.GetFormattedTime());
				return;
			}
			break;
		default:
			if ((int)bombInfo.GetTime() % 2 == 0) {
				module.HandleStrike();
				Debug.LogFormat("[Mega Man Boss Rush #{0}] Last Boss was Mr. X, expected press at odd seconds, pressed at {1}", moduleId, bombInfo.GetFormattedTime());
				return;
			}
			break;
		}
		face.GetComponent<MeshRenderer>().sharedMaterial = m_faces[9];
		Debug.LogFormat("[Mega Man Boss Rush #{0}] Defeated all bosses, module solved!", moduleId);
		module.HandlePass();
		isSolved = true;
		audio.PlaySoundAtTransform("stage_clear", transform);
	}
	
	void getWilly () {
		stage = 10;
		if (bombInfo.GetOnIndicators().Count() > bombInfo.GetOffIndicators().Count()) stage = 9;
		face.GetComponent<MeshRenderer>().sharedMaterial = b_faces[stage];
	}
	
	void reset () {
		b_seq = new int[]{0,1,2,3,4,5,6,7};
		lastPress = 999;
		stage = 1;
		
		//do the shuffle
		for (int t = 0; t < b_seq.Length; t++ ){
            int tmp = b_seq[t];
            int r = Random.Range(t, b_seq.Length);
            b_seq[t] = b_seq[r];
            b_seq[r] = tmp;
        }
		
		if (UnityEngine.Random.value < 0.1f){
			int r = Random.Range(0, b_seq.Length);
			b_seq[r] = 8;
		}
		
		foreach ( KMSelectable boss in b_bosses ) {
			int index = b_bosses.ToList().IndexOf(boss);
			boss.GetComponent<MeshRenderer>().sharedMaterial = b_faces[b_seq[index]];
		}
	}
	
	bool checkBoss(int boss_id){
		if (lastPress == 999 || lastPress == 8) {
			return true;
		}
		switch (boss_id) {
		case 0:
			if (lastPress == 7 || lastPress == 2 || lastPress == 3) return true;
			return false;
		case 1:
			if (lastPress == 2 || lastPress == 4 || lastPress == 7 || lastPress == 3) return true;
			return false;
		case 2:
			if (lastPress == 7 || lastPress == 5) return true;
			return false;
		case 3:
			if (lastPress == 6) return true;
			return false;
		case 4:
			if (lastPress == 3) return true;
			return false;
		case 5:
			if (lastPress == 1 || lastPress == 4 || lastPress == 6 || lastPress == 7 || lastPress == 3) return true;
			return false;
		case 6:
			if (lastPress == 7 || lastPress == 0) return true;
			return false;
		case 7:
			if (lastPress == 4 || lastPress == 3) return true;
			return false;
		default:
			return true;
		}
	}
}
