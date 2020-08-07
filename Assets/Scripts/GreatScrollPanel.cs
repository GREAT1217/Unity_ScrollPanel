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
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 滑动面板
/// </summary>
public class GreatScrollPanel : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public enum ScrollAxis
    {
        Horizontal,
        Vertical
    }
    public enum ItemSizeType
    {
        Manual,//自定义
        Fit//自适应（与Viewport同大小）
    }

    public ScrollAxis _scrollAxis = ScrollAxis.Horizontal;//滑动轴向
    public bool _autoLayout = true;//子对象自动布局
    public float _layoutSpacing = 20f;//子对象间隔
    public ItemSizeType _itemSizeType = ItemSizeType.Fit;//子对象大小类型
    public Vector2 _itemSize = new Vector2(120, 170);//子对象大小
    public bool _infinite = false;//无限滑动
    public bool _inertia = false;//使用惯性
    public int _startingIndex = 0;//开始子对象索引
    [Range(0, 10)]
    public float _snapSpeed = 10f;//对齐速度
    [Range(0.01f, 0.1f)]
    public float _snapThreshold = 0.01f;//对齐阈值

    private Canvas _canvas;
    private RectTransform _canvasRT;
    private CanvasScaler _canvasScaler;
    private ScrollRect _scrollRect;
    private Vector2 _contentSize;
    private Vector2 _scaledContentSize;
    private bool _pressing;
    private bool _dragging;
    private bool _snapping;

    public RectTransform Content { get { return _scrollRect.content; } }
    public RectTransform Viewport { get { return _scrollRect.viewport; } }
    public RectTransform[] ItemsRT { get; set; }
    public int ItemsCount { get { return Content.childCount; } }
    public int CenterIndex { get; set; }

    void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _canvas = GetComponentInParent<Canvas>();
        _canvasScaler = _canvas.GetComponent<CanvasScaler>();
        _canvasRT = _canvas.GetComponent<RectTransform>();
    }

    void Start()
    {
        Setup();
        OnInfiniteScrolling();
        SnapNearestCenterItem();
    }

    void Update()
    {
        if (ItemsCount == 0) return;
        OnSnapScrolling();
        OnInfiniteScrolling();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _snapping = false;
        _dragging = true;
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

    void Setup()
    {
        if (ItemsCount < 4 && _infinite)
        {
            _infinite = false;
            Debug.Log("无限滑动须4个及以上数量的子对象");
        }
        //设置ScrollRect
        _scrollRect.horizontal = _scrollAxis == ScrollAxis.Horizontal;
        _scrollRect.vertical = _scrollAxis == ScrollAxis.Vertical;
        _scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
        _scrollRect.inertia = _inertia;
        _scrollRect.scrollSensitivity = 0;
        //设置Viewport：设置轴心点居中，用于检查最近距离时使用世界坐标位置
        Viewport.pivot = new Vector2(0.5f, 0.5f);
        //设置Items
        _itemSize = _itemSizeType == ItemSizeType.Manual ? _itemSize : new Vector2(Viewport.rect.width, Viewport.rect.height);
        ItemsRT = new RectTransform[ItemsCount];
        for (int i = 0; i < ItemsCount; i++)
        {
            ItemsRT[i] = Content.GetChild(i).transform as RectTransform;
            if (_autoLayout)
            {
                //设置锚点：横向滑动时左侧居中，纵向滑动时底部居中
                ItemsRT[i].anchorMin = new Vector2(_scrollAxis == ScrollAxis.Horizontal ? 0f : 0.5f, _scrollAxis == ScrollAxis.Vertical ? 0f : 0.5f);
                ItemsRT[i].anchorMax = new Vector2(_scrollAxis == ScrollAxis.Horizontal ? 0f : 0.5f, _scrollAxis == ScrollAxis.Vertical ? 0f : 0.5f);
                //设置轴心点，大小
                ItemsRT[i].pivot = new Vector2(0.5f, 0.5f);
                ItemsRT[i].sizeDelta = _itemSize;
                //设置位置
                float panelPosX = (_scrollAxis == ScrollAxis.Horizontal) ? i * (_layoutSpacing + _itemSize.x) + (_itemSize.x / 2f) : 0f;//子物体轴心点在中间
                float panelPosY = (_scrollAxis == ScrollAxis.Vertical) ? i * (_layoutSpacing + _itemSize.y) + (_itemSize.y / 2f) : 0f;
                ItemsRT[i].anchoredPosition = new Vector2(panelPosX, panelPosY);
            }
        }
        //设置Content
        if (_autoLayout)
        {
            //根据移动方向设置Content锚点、轴心点、大小
            Content.anchorMin = new Vector2(_scrollAxis == ScrollAxis.Horizontal ? 0f : 0.5f, _scrollAxis == ScrollAxis.Vertical ? 0f : 0.5f);
            Content.anchorMax = new Vector2(_scrollAxis == ScrollAxis.Horizontal ? 0f : 0.5f, _scrollAxis == ScrollAxis.Vertical ? 0f : 0.5f);
            Content.pivot = new Vector2(_scrollAxis == ScrollAxis.Horizontal ? 0f : 0.5f, _scrollAxis == ScrollAxis.Vertical ? 0f : 0.5f);

            float contentWidth = (_scrollAxis == ScrollAxis.Horizontal) ? ItemsCount * _itemSize.x + (ItemsCount - 1) * _layoutSpacing : _itemSize.x;
            float contentHeight = (_scrollAxis == ScrollAxis.Vertical) ? ItemsCount * _itemSize.y + (ItemsCount - 1) * _layoutSpacing : _itemSize.y;
            Content.sizeDelta = new Vector2(contentWidth, contentHeight);
        }
        if (_infinite)
        {
            //contentSize增加一个间隔（首尾子对象之间的间隔）
            _contentSize = Content.sizeDelta + new Vector2(_scrollAxis == ScrollAxis.Horizontal ? _layoutSpacing : 0f, _scrollAxis == ScrollAxis.Vertical ? _layoutSpacing : 0f);
            //Content在WorldSpace渲染模式下设置缩放
            _scaledContentSize.x = Content.sizeDelta.x * Content.lossyScale.x;
            _scaledContentSize.y = Content.sizeDelta.y * Content.lossyScale.y;
            if (_canvasScaler != null && _canvas.renderMode != RenderMode.WorldSpace)
            {
                _scaledContentSize.x /= _canvasRT.localScale.x;
                _scaledContentSize.y /= _canvasRT.localScale.y;
            }
        }
        //根据开始子对象的索引设置Content位置
        float xOffset = (_scrollAxis == ScrollAxis.Horizontal) ? Viewport.rect.width / 2f : 0f;
        float yOffset = (_scrollAxis == ScrollAxis.Vertical) ? Viewport.rect.height / 2 : 0f;
        Content.anchoredPosition = -ItemsRT[_startingIndex].anchoredPosition + new Vector2(xOffset, yOffset);
        CenterIndex = _startingIndex;
    }

    /// <summary>
    /// 无限滑动
    /// </summary>
    void OnInfiniteScrolling()
    {
        //检查超出Content区域的子对象更新位置
        if (!_infinite) return;
        if (_scrollAxis == ScrollAxis.Horizontal)
        {
            for (int i = 0; i < ItemsCount; i++)
            {
                float dis = ItemsRT[i].position.x - Viewport.position.x;
                if (dis > _scaledContentSize.x / 2)
                {
                    ItemsRT[i].anchoredPosition -= new Vector2(_contentSize.x, 0);
                }
                else if (dis < -_scaledContentSize.x / 2)
                {
                    ItemsRT[i].anchoredPosition += new Vector2(_contentSize.x, 0);
                }
            }
        }
        else
        {
            for (int i = 0; i < ItemsCount; i++)
            {
                float dis = ItemsRT[i].position.y - Viewport.position.y;
                if (dis > _scaledContentSize.y / 2f)
                {
                    ItemsRT[i].anchoredPosition -= new Vector2(0, _contentSize.y);
                }
                else if (dis < -1f * _scaledContentSize.y / 2f)
                {
                    ItemsRT[i].anchoredPosition += new Vector2(0, _contentSize.y);
                }
            }
        }
    }

    /// <summary>
    /// 对齐滑动
    /// </summary>
    void OnSnapScrolling()
    {
        //根据当前子对象位置更新Content位置
        if (_dragging || _pressing) return;
        if (!_snapping) return;
        float xOffset = (_scrollAxis == ScrollAxis.Horizontal) ? Viewport.rect.width / 2f : 0f;
        float yOffset = (_scrollAxis == ScrollAxis.Vertical) ? Viewport.rect.height / 2 : 0f;
        Vector2 offset = new Vector2(xOffset, yOffset);
        Vector2 targetPosition = -ItemsRT[CenterIndex].anchoredPosition + offset;//子对象与Viewport的间距
        Content.anchoredPosition = Vector2.Lerp(Content.anchoredPosition, targetPosition, Time.deltaTime * _snapSpeed);
        if (Vector2.Distance(Content.anchoredPosition, targetPosition) < _snapThreshold)
        {
            Content.anchoredPosition = targetPosition;
            _snapping = false;
        }
    }

    /// <summary>
    /// 对齐最近子对象
    /// </summary>
    void SnapNearestCenterItem()
    {
        int nearestIndex = 0;
        float minDistance = _scrollAxis == ScrollAxis.Horizontal ? Mathf.Abs(ItemsRT[0].position.x - Viewport.position.x) : Mathf.Abs(ItemsRT[0].position.y - Viewport.position.y);
        for (int i = 1; i < ItemsCount; i++)
        {
            float tempDistance = _scrollAxis == ScrollAxis.Horizontal ? Mathf.Abs(ItemsRT[i].position.x - Viewport.position.x) : Mathf.Abs(ItemsRT[i].position.y - Viewport.position.y);
            if (Mathf.Min(minDistance, tempDistance) == tempDistance)
            {
                nearestIndex = i;
                minDistance = tempDistance;
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
        _snapping = true;
    }

}