/*************************************************************************
 *  Copyright © 2020 Great1217. All rights reserved.
 *------------------------------------------------------------------------
 *  File         :  GreatScrollPanel.cs
 *  Description  :  Null.
 *------------------------------------------------------------------------
 *  Author       :  Great1217
 *  Version      :  0.1.0
 *  Date         :  7/14/2020
 *  Description  :  Initial development version.
 *************************************************************************/

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 滑动面板
/// </summary>
public class GreatScrollPanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public enum ScrollAxis
    {
        Horizontal,
        Vertical,
        Free,
    }

    public enum ItemSizeType
    {
        Custom, //自定义
        Fit //自适应（与Viewport同大小）
    }

    public enum ItemInitType
    {
        Dynamic, //动态生成
        Static //静态生成（使用子物体）
    }

    public class ItemInitEvent : UnityEvent<Transform, int>
    {
    };

    public class ItemCenterEvent : UnityEvent<int>
    {
    };

    public ScrollAxis _scrollAxis = ScrollAxis.Horizontal; //滑动轴向

    public ItemInitType _itemInitType = ItemInitType.Static; //子对象初始化类型
    public GameObject _itemPrefab; //子对象预制体
    public int _itemsCount; //子对象数量
    public bool _itemAutoLayout = true; //子对象自动布局
    public ItemSizeType _itemSizeType = ItemSizeType.Custom; //子对象大小类型
    public Vector2 _itemSize = new Vector2(120, 170); //子对象大小
    public float _itemSpacing = 20f; //子对象间隔
    public bool _infinite = false; //无限滑动

    public float _snapSpeed = 10f; //对齐速度
    public float _snapThreshold = 0.01f; //对齐阈值

    public bool _inertia = false; //使用惯性
    public int _startingIndex = 0; //开始子对象索引

    private Canvas _canvas;
    private RectTransform _canvasRT;
    private CanvasScaler _canvasScaler;
    private ScrollRect _scrollRect;
    private Vector2 _infiniteContentSize;
    private Vector2 _scaledContentSize;
    private Vector2 _centerPos;
    private Vector2 _targetPos;
    private bool _pressing;
    private bool _dragging;
    private bool _snapping;
    private ItemInitEvent _onItemInit;
    private ItemCenterEvent _onItemCenter;

    public RectTransform Content
    {
        get
        {
            return _scrollRect.content;
        }
    }

    public RectTransform Viewport
    {
        get
        {
            return _scrollRect.viewport;
        }
    }

    public RectTransform[] ItemsRT
    {
        get;
        set;
    }

    public int ItemsCount
    {
        get
        {
            return _itemsCount;
        }
    }

    public int CenterIndex
    {
        get;
        set;
    }

    public ItemInitEvent OnItemInit
    {
        get
        {
            if (_onItemInit == null) _onItemInit = new ItemInitEvent();
            return _onItemInit;
        }
    }

    public ItemCenterEvent OnItemCenter
    {
        get
        {
            if (_onItemCenter == null) _onItemCenter = new ItemCenterEvent();
            return _onItemCenter;
        }
    }

    void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _canvas = GetComponentInParent<Canvas>();
        _canvasScaler = _canvas.GetComponent<CanvasScaler>();
        _canvasRT = _canvas.GetComponent<RectTransform>();
    }

    void Start()
    {
        Init();
        SnapItem(_startingIndex);
    }

    void Update()
    {
        if (ItemsCount == 0) return;
        OnSnapScrolling();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _snapping = false;
        _dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnInfiniteScrolling();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _dragging = false;
        SnapNearestCenterItem();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pressing = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pressing = false;
    }

    void Init()
    {
        //设置ScrollRect
        _scrollRect.horizontal = _scrollAxis != ScrollAxis.Vertical;
        _scrollRect.vertical = _scrollAxis != ScrollAxis.Horizontal;
        _scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
        _scrollRect.inertia = _inertia;
        _scrollRect.scrollSensitivity = 0;
        //设置Viewport：设置轴心点居中，用于检查最近距离时使用世界坐标位置
        Viewport.pivot = new Vector2(0.5f, 0.5f);
        //设置Items
        InitItems();
        //设置Content
        if (_itemAutoLayout)
        {
            //根据移动方向设置Content锚点、轴心点、大小
            Content.anchorMin = new Vector2(_scrollAxis == ScrollAxis.Horizontal ? 0f : 0.5f, _scrollAxis == ScrollAxis.Vertical ? 0f : 0.5f);
            Content.anchorMax = new Vector2(_scrollAxis == ScrollAxis.Horizontal ? 0f : 0.5f, _scrollAxis == ScrollAxis.Vertical ? 0f : 0.5f);
            Content.pivot = new Vector2(_scrollAxis == ScrollAxis.Horizontal ? 0f : 0.5f, _scrollAxis == ScrollAxis.Vertical ? 0f : 0.5f);

            float contentWidth = (_scrollAxis == ScrollAxis.Horizontal) ? ItemsCount * _itemSize.x + (ItemsCount - 1) * _itemSpacing : _itemSize.x;
            float contentHeight = (_scrollAxis == ScrollAxis.Vertical) ? ItemsCount * _itemSize.y + (ItemsCount - 1) * _itemSpacing : _itemSize.y;
            Content.sizeDelta = new Vector2(contentWidth, contentHeight);
        }
        if (_infinite)
        {
            //infiniteContentSize需要增加一个首尾子对象之间的间隔
            _infiniteContentSize = Content.sizeDelta + new Vector2(_scrollAxis == ScrollAxis.Horizontal ? _itemSpacing : 0f, _scrollAxis == ScrollAxis.Vertical ? _itemSpacing : 0f);
            //Content的缩放，要依据缩放计算子物体之间的距离
            _scaledContentSize.x = Content.sizeDelta.x * Content.lossyScale.x;
            _scaledContentSize.y = Content.sizeDelta.y * Content.lossyScale.y;
            if (_canvasScaler != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                _scaledContentSize.x /= _canvasRT.localScale.x;
                _scaledContentSize.y /= _canvasRT.localScale.y;
            }
        }
        //中心位置
        float centerX = _scrollAxis != ScrollAxis.Vertical ? Viewport.rect.width / 2f : 0f;
        float centerY = _scrollAxis != ScrollAxis.Horizontal ? Viewport.rect.height / 2 : 0f;
        _centerPos = new Vector2(centerX, centerY);
        //根据开始子对象的位置设置Content位置
        Content.anchoredPosition = _centerPos - ItemsRT[_startingIndex].anchoredPosition;
        CenterIndex = _startingIndex;
    }

    void InitItems()
    {
        _itemSize = _itemSizeType == ItemSizeType.Custom ? _itemSize : new Vector2(Viewport.rect.width, Viewport.rect.height);
        if (_itemInitType == ItemInitType.Static)
        {
            _itemsCount = Content.childCount;
            ItemsRT = new RectTransform[ItemsCount];
            for (int i = 0; i < ItemsCount; i++)
            {
                InitItem(i, Content.GetChild(i).transform as RectTransform);
            }
        }
        else
        {
            ItemsRT = new RectTransform[ItemsCount];
            for (int i = 1; i < ItemsCount; i++)
            {
                InitItem(i, Instantiate(_itemPrefab, Content).transform as RectTransform);
            }
            InitItem(0, _itemPrefab.transform as RectTransform);
        }
    }

    void InitItem(int index, RectTransform item)
    {
        ItemsRT[index] = item;
        //设置轴心点、锚点：横向滑动时左侧居中，纵向滑动时底部居中
        ItemsRT[index].pivot = new Vector2(0.5f, 0.5f);
        ItemsRT[index].anchorMin = new Vector2(_scrollAxis != ScrollAxis.Vertical ? 0f : 0.5f, _scrollAxis != ScrollAxis.Horizontal ? 0f : 0.5f);
        ItemsRT[index].anchorMax = new Vector2(_scrollAxis != ScrollAxis.Vertical ? 0f : 0.5f, _scrollAxis != ScrollAxis.Horizontal ? 0f : 0.5f);
        if (_itemAutoLayout)
        {
            ItemsRT[index].sizeDelta = _itemSize;
            //设置位置
            float itemPosX = (_scrollAxis == ScrollAxis.Horizontal) ? index * (_itemSpacing + _itemSize.x) + (_itemSize.x / 2f) : 0f; //子物体轴心点在中间
            float itemPosY = (_scrollAxis == ScrollAxis.Vertical) ? index * (_itemSpacing + _itemSize.y) + (_itemSize.y / 2f) : 0f;
            ItemsRT[index].anchoredPosition = new Vector2(itemPosX, itemPosY);
        }
        OnItemInit.Invoke(item, index);
    }

    /// <summary>
    /// 对齐滑动
    /// </summary>
    void OnSnapScrolling()
    {
        //根据当前子对象位置更新Content位置
        if (_dragging || _pressing) return;
        if (!_snapping) return;
        OnInfiniteScrolling();
        Content.anchoredPosition = Vector2.Lerp(Content.anchoredPosition, _targetPos, Time.deltaTime * _snapSpeed);
        if (Vector2.Distance(Content.anchoredPosition, _targetPos) < _snapThreshold)
        {
            Content.anchoredPosition = _targetPos;
            _snapping = false;
        }
    }

    /// <summary>
    /// 无限滑动
    /// </summary>
    void OnInfiniteScrolling()
    {
        //检查超出Content区域的子对象更新位置
        if (!_infinite) return;
        for (int i = 0; i < ItemsCount; i++)
        {
            float space = SpaceFromCenter(ItemsRT[i].position);
            if (_scrollAxis == ScrollAxis.Horizontal)
            {
                if (space > _scaledContentSize.x / 2)
                    ItemsRT[i].anchoredPosition -= new Vector2(_infiniteContentSize.x, 0);
                else if (space < -_scaledContentSize.x / 2)
                    ItemsRT[i].anchoredPosition += new Vector2(_infiniteContentSize.x, 0);
            }
            else if (_scrollAxis == ScrollAxis.Vertical)
            {
                if (space > _scaledContentSize.y / 2f)
                    ItemsRT[i].anchoredPosition -= new Vector2(0, _infiniteContentSize.y);
                else if (space < -1f * _scaledContentSize.y / 2f)
                    ItemsRT[i].anchoredPosition += new Vector2(0, _infiniteContentSize.y);
            }
        }
    }

    /// <summary>
    /// 对齐最近子对象
    /// </summary>
    void SnapNearestCenterItem()
    {
        int nearestIndex = 0;
        float minSpace = Mathf.Abs(SpaceFromCenter(ItemsRT[0].position));
        for (int i = 1; i < ItemsCount; i++)
        {
            float tempSpace = Mathf.Abs(SpaceFromCenter(ItemsRT[i].position));
            if (tempSpace < minSpace)
            {
                nearestIndex = i;
                minSpace = tempSpace;
            }
        }
        SnapItem(nearestIndex);
    }

    /// <summary>
    /// 对齐子对象
    /// </summary>
    /// <param name="index"></param>
    public void SnapItem(int index)
    {
        CenterIndex = index;
        OnItemCenter.Invoke(CenterIndex);
        _targetPos = _centerPos - ItemsRT[index].anchoredPosition;
        _snapping = true;
    }

    public float SpaceFromCenter(Vector2 point)
    {
        if (_scrollAxis == ScrollAxis.Horizontal)
        {
            return (point.x - Viewport.position.x) / _canvasRT.localScale.x;
        }
        else if (_scrollAxis == ScrollAxis.Vertical)
        {
            return (point.y - Viewport.position.y) / _canvasRT.localScale.y;
        }
        else
        {
            return Vector2.Distance(point, Viewport.position) / _canvasRT.localScale.x;
        }
    }
}
