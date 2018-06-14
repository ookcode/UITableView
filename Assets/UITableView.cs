using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 
/// 无限复用型ScrollView
/// 
/// 
/// </summary>
public class UITableView : MonoBehaviour {
	Action<GameObject, int> _onCellFill; // 填充回调函数
	Action<GameObject, int> _onCellClick; // 点击回调函数

	Transform _content;
	RectTransform _contentRect;
	RectTransform _scrollViewRect;

	bool _isVertical; // 是否垂直滚动，false为水平滚动
	GameObject _model; // 复用的cell原型
	Vector2 _cellSize; // 复用的cell大小
	Dictionary<int, RectTransform> _cells; // cell复用字典

	int _displayCellCount; // 不滚动时最大显示的cell个数
	int _totalCellCount; // cell总个数
	int _lastMinIndex = 0; // 记录上一次滚动的最小序号

	void Awake () {
		_content = transform.Find ("Viewport/Content");
		_contentRect = _content.GetComponent<RectTransform> ();
		_scrollViewRect = transform.GetComponent<RectTransform> ();

		// 监听滚动事件
		var scrollRect = transform.GetComponent<ScrollRect> ();
		scrollRect.onValueChanged.AddListener(onScroll);

		// 滚动方向
		_isVertical = scrollRect.vertical;

		// 重置content布局
		if (_isVertical) { 
			// 垂直滚动，宽度固定
			_contentRect.anchorMin = new Vector2 (0, 1);
			_contentRect.anchorMax = new Vector2 (1, 1);
		} else { 
			// 水平滚动，高度固定
			_contentRect.anchorMin = new Vector2 (0, 0);
			_contentRect.anchorMax = new Vector2 (0, 1);
		}
		_contentRect.anchoredPosition = new Vector2 (0, 0);
		_contentRect.sizeDelta = new Vector2 (0, 0);
	}

	// 滚动事件
	void onScroll(Vector2 value) {
		float contentValue = _isVertical ? _contentRect.anchoredPosition.y : -_contentRect.anchoredPosition.x;
		float cellValue = _isVertical ? _cellSize.y : _cellSize.x;

		float min = contentValue < 0 ? 0 : contentValue;
		int minIndex = Mathf.FloorToInt(min / cellValue);
		int diff = minIndex - _lastMinIndex;
		if (diff > 0) {
			for (int i = _lastMinIndex + 1; i <= minIndex; ++i) {
				reuse (i, i + _displayCellCount);
			}
		} else if(diff < 0) {
			for (int i = _lastMinIndex - 1; i >= minIndex; --i) {
				reuse (i, i + _displayCellCount);
			}
		}
		_lastMinIndex = minIndex;
	}

	// 复用cell
	void reuse(int minIndex, int maxIndex) {
		if (maxIndex >= _totalCellCount)
			maxIndex = _totalCellCount - 1;
		Action<int, int> doReuse = (int reuseIndex, int targetIndex) => {
			var cell = _cells [reuseIndex];
			if (_isVertical) {
				cell.anchoredPosition = new Vector2 (cell.anchoredPosition.x, targetIndex * -_cellSize.y);
			} else {
				cell.anchoredPosition = new Vector2 (targetIndex * _cellSize.x, cell.anchoredPosition.y);
			}
			cell.gameObject.name = targetIndex.ToString ();
			_cells [targetIndex] = cell;
			_cells.Remove (reuseIndex);
			if (_onCellFill != null) {
				_onCellFill (cell.gameObject, targetIndex);
			}
		};
		if (!_cells.ContainsKey(maxIndex)) {
			int reuseIndex = minIndex - 1;
			doReuse (reuseIndex, maxIndex);

		} else if (!_cells.ContainsKey (minIndex)) {
			int reuseIndex = maxIndex + 1;
			doReuse (reuseIndex, minIndex);
		}
	}

	// cell点击事件
	void onClick(GameObject sender) {
		if (_onCellClick != null) {
			int index = Convert.ToInt32 (sender.name);
			_onCellClick (sender, index);
		}
	}

	/// <summary>
	/// API
	/// </summary>

	// 设置填充回调
	public void onCellFill(Action<GameObject, int> onCellFill) {
		_onCellFill = onCellFill;
	}

	// 设置点击回调
	public void onCellClick(Action<GameObject, int> onCellClick) {
		_onCellClick = onCellClick;
	}

	// 设置cell原型（必须调用）
	// 为方便适配，传入的原型prefab会自动修改为左上角为锚点布局
	public void setModel(UnityEngine.Object obj) {
		_model = obj as GameObject;
		var trans = _model.GetComponent<RectTransform> ();
		_cellSize = trans.sizeDelta;
	}
		
	// 加载数据（必须调用）
	public void reload(int size) {
		// clean
		_cells = new Dictionary<int, RectTransform> ();
		foreach(Transform child in _content) {
			Destroy (child.gameObject);
		}

		_totalCellCount = size;

		if (_isVertical) {
			_contentRect.sizeDelta = new Vector2 (_contentRect.sizeDelta.x, _totalCellCount * _cellSize.y);
			_displayCellCount = Mathf.CeilToInt(_scrollViewRect.rect.height / _cellSize.y);
		} else {
			_contentRect.sizeDelta = new Vector2 (_totalCellCount * _cellSize.x, _contentRect.sizeDelta.y);
			_displayCellCount = Mathf.CeilToInt(_scrollViewRect.rect.width / _cellSize.x);
		}

		// 初始化复用cell
		for (int i = 0; i < _displayCellCount + 1; ++i) {
			var cell = Instantiate (_model) as GameObject;
			cell.transform.SetParent (_content, false);
			var trans = cell.GetComponent<RectTransform> ();
			if (_isVertical) {
				trans.anchoredPosition = new Vector2 (trans.anchoredPosition.x, -_cellSize.y * i);
			} else {
				trans.anchoredPosition = new Vector2 (_cellSize.x * i, trans.anchoredPosition.y);
			}
			_cells [i] = trans;
			trans.gameObject.name = i.ToString ();
			if (_onCellFill != null) {
				_onCellFill (cell.gameObject, i);
			}
			cell.GetComponent<Button> ().onClick.AddListener (()=>{
				onClick(cell);
			});
		}
	}
}
