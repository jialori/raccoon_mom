﻿using System.Collections;
using System.Text; // to get class StringBuilder
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class BillDisplay : MonoBehaviour
{
	public TextMeshProUGUI history;
	public TextMeshProUGUI newest;

    public int maxLogNum;

    [Header("Text Pop Up Effect")]
	private int newLogFontSize_1;
	private int newLogFontSize_2;
	public int newLogFontSize_3;

	// Templates for bill statements
    private string[] billLogsTemplates_BREAK = {
    	"+{0:C2} | You broke the {1}!!! "
    };

    private string[] billLogsTemplates_KNOCK = {
		"+{0:C2} |  You knocked the {1} over!!!"
    };

	// Dictionary and Random shuffler to select a random phrase from templates
   	// private Dictionary<ScoreManager.ActionTypes, string[]> actionToStats;
	private System.Random rnd = new System.Random();

    private List<string> billLogs = new List<string>();
    private StringBuilder historyLogs;
    private string newLog;
    private int newLogLength;
    private int curLogNum;

    private int first_display;
    private int first_display_idx;
    private int last_display_idx;

    void Awake()
    {
    	newLogFontSize_1 = newLogFontSize_3 - 12;
    	newLogFontSize_2 = newLogFontSize_3 + 2;

       	newest.text = "";
    	history.text = "";

    	newLog = "";
	    newLogLength = 0;
    	historyLogs = new StringBuilder();
	    // first_display = 0; // lol
	    first_display_idx = 0;
	    last_display_idx = 0;
	    curLogNum = 0;
		// actionToStats = new Dictionary<ScoreManager.ActionTypes, string[]>() {
		// 	[ScoreManager.ActionTypes.BREAK] = billLogsTemplates_BREAK,
		// 	[ScoreManager.ActionTypes.KNOCK] = billLogsTemplates_KNOCK
		// };
    }

    // void Update()
    // {
    // 	StringBuilder bb = new StringBuilder();
    // 	bb.Append("hello\n<lol>");
    // 	Debug.Log(bb.Length);
    // 	Debug.Log(bb.ToString(2, bb.Length-2));
    // }

    public void AddBillLog(string item, ScoreManager.ActionTypes aType, float damage)
    {
    	// history log
       	first_display = (curLogNum > maxLogNum + 1) ? (first_display + 1) : 0;
		first_display_idx = (curLogNum > maxLogNum) ? (first_display_idx + billLogs[first_display].Length) : 0;
		last_display_idx += newLogLength;
    	history.text = historyLogs.ToString(first_display_idx, last_display_idx - first_display_idx);

    	// newest log
    	newLog = FormatLogPopUp(item, aType, damage);
       	newest.text = newLog;
    	newLog = FormatLog(item, aType, damage); // reformat
    	billLogs.Add(newLog);
    	newLogLength = newLog.Length;
       	historyLogs.Append(newLog);
       	StopCoroutine(PopUpText(newest));
       	StartCoroutine(PopUpText(newest));
   		curLogNum++;
    }

    string FormatLog(string item, ScoreManager.ActionTypes aType, float damage)
    {
    	// string s = actionToStats[aType][rnd.Next(actionToStats[aType].Length)];
    	// string s = "<color=orange>-{0:C2}</color>  |  You {1}ed a {2}!\n";
    	// note font used is not monospaced!
    	// string s = "<color=orange>-{0,6}</color>  |     *** {1,1} ***\n";
    	string s = "<color=orange>-{0,4}</color> | *** {1} ***\n";
    	// return string.Format(s, damage*GameManager.instance.scoreMultiplier, aType.ToString(), item);
    	return string.Format(s, damage*ScoreManager.instance.scoreMultiplier, item);
    }

    string FormatLogPopUp(string item, ScoreManager.ActionTypes aType, float damage)
    {
    	// string s = actionToStats[aType][rnd.Next(actionToStats[aType].Length)];
    	// string s = "<color=orange>-{0:C2}</color>  |  You {1}ed a {2}!\n";
    	// note font used is not monospaced!
    	// string s = "<color=orange>-{0,6}</color>  |     *** {1,1} ***\n";
    	string s = "<color=orange>-  {0}</color>   * {1} *\n";
    	// return string.Format(s, damage*GameManager.instance.scoreMultiplier, aType.ToString(), item);
    	return string.Format(s, damage*ScoreManager.instance.scoreMultiplier, item);
    }

    IEnumerator PopUpText(TextMeshProUGUI newest)
    {
    	// 34, 44, 42
		// TODO: according to ammount of score, change how big the pop up is 
    	newest.fontSize = newLogFontSize_1;
    	yield return new WaitForSeconds(0.05f);

    	newest.fontSize = newLogFontSize_2;
    	yield return new WaitForSeconds(0.05f);

    	newest.fontSize = newLogFontSize_3;
    }

}
