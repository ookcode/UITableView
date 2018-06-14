using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour {

	List<string> _tabData = new List<string> (){ "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"};
	List<string> _cardData = new List<string> (){"CARD 0", "CARD 1", "CARD 2", "CARD 3", "CARD 4", "CARD 5"};

	UITableView _tabTableView;
	UITableView _cardTableView;

	void Awake () {
		GameObject tabScrollView = GameObject.Find ("TabScrollView");
		GameObject cardScrollView = GameObject.Find ("CardScrollView");

		_tabTableView = tabScrollView.GetComponent<UITableView> ();
		_cardTableView = cardScrollView.GetComponent<UITableView> ();
	}
	void Start () {
		/// <summary>
		/// 
		/// 垂直scrollview
		/// 
		/// </summary>
		_tabTableView.setModel (Resources.Load ("Tab"));

		_tabTableView.onCellFill((GameObject item, int index) => {
			var textObj = item.transform.Find("Text");
			var text = textObj.GetComponent<Text>();
			text.text = _tabData[index];
		});

		_tabTableView.onCellClick((GameObject item, int index) => {
			Debug.Log(index);
		});

		_tabTableView.reload (_tabData.Count);

		/// <summary>
		/// 
		/// 水平scrollview
		/// 
		/// </summary>
		_cardTableView.setModel (Resources.Load ("Card"));

		_cardTableView.onCellFill((GameObject item, int index) => {
			var textObj = item.transform.Find("Text");
			var text = textObj.GetComponent<Text>();
			text.text = _cardData[index];
		});

		_cardTableView.onCellClick((GameObject item, int index) => {
			Debug.Log(index);
		});

		_cardTableView.reload (_cardData.Count);
	}
}
