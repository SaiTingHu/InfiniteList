using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace HT.InfiniteList
{
    /// <summary>
    /// 无限列表滚动视野
    /// </summary>
    public class InfiniteListScrollRect : ScrollRect
    {
        public class ScrollRectElementAddEvent : UnityEvent<IListElement> {}
        public class ScrollRectElementRemoveEvent : UnityEvent<IListElement> {}

        private ScrollRectElementAddEvent _onElementAdded = new ();
        public ScrollRectElementAddEvent onElementAdded { get { return _onElementAdded; } set { _onElementAdded = value; } }
        
        private ScrollRectElementRemoveEvent _onElementRemoved = new ();
        public ScrollRectElementRemoveEvent onElementRemoved { get { return _onElementRemoved; } set { _onElementRemoved = value; } }
        
        /// <summary>
        /// 元素模板
        /// </summary>
        public GameObject ElementTemplate;
        /// <summary>
        /// 元素排列方向
        /// </summary>
        public Direction ListingDirection = Direction.Vertical;
        /// <summary>
        /// 元素高度
        /// </summary>
        private float _height = 20;
        /// <summary>
        /// 元素之间的间隔
        /// </summary>
        public float Interval = 5;

        private List<object> _datas = new ();
        private HashSet<object> _dataIndexs = new ();
        private Dictionary<object, IListElement> _displayElements = new ();
        private HashSet<object> _invisibleList = new ();
        private ObjectPool<IListElement> _elementsPool;
        private RectTransform _uiTransform;

        /// <summary>
        /// UGUI变换组件
        /// </summary>
        public RectTransform UITransform
        {
            get
            {
                if (_uiTransform == null)
                {
                    _uiTransform = GetComponent<RectTransform>();
                }
                return _uiTransform;
            }
        }
        /// <summary>
        /// 当前数据数量
        /// </summary>
        public int DataCount
        {
            get
            {
                return _datas.Count;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _elementsPool = new ObjectPool<IListElement>(
                createFunc: () =>
                {
                    var go = Instantiate(ElementTemplate, content);
                    return go.GetComponent<IListElement>();
                }, 
                actionOnGet: (element) =>
                {
                    element.SetVisible(true);
                },
                actionOnRelease: (element) =>
                {
                    element.OnClearData();
                    element.SetVisible(false);
                }
            );

            // 设置滚动方向单方向匹配
            if (ListingDirection == Direction.Vertical)
            {
                horizontal = false;
                vertical = true;
            }
            else if (ListingDirection == Direction.Horizontal)
            {
                horizontal = true;
                vertical = false;
            }
            
            // 强制height来自Template的尺寸
            if (ElementTemplate != null)
            {
                var elementTemplateRectTransform = ElementTemplate.GetComponent<RectTransform>();
                if (ListingDirection == Direction.Vertical)
                {
                    _height = elementTemplateRectTransform.rect.height;
                }
                else if (ListingDirection == Direction.Horizontal)
                {
                    _height = elementTemplateRectTransform.rect.width;
                }
            }
            
            onValueChanged.AddListener((value) => { RefreshScrollView(); });
        }

        protected override void OnDestroy()
        {
            if (_elementsPool != null)
            {
                _elementsPool.Dispose();
                _elementsPool = null;
            }
            base.OnDestroy();
        }
       
        /// <summary>
        /// 添加一条新的数据到无限列表尾部
        /// </summary>
        /// <param name="data">无限列表数据</param>
        public void AddData(object data)
        {
            if (_dataIndexs.Contains(data))
            {
                Debug.LogWarning("添加数据至无限列表失败：列表中已存在该数据 " + data.ToString());
                return;
            }

            _datas.Add(data);
            _dataIndexs.Add(data);

            RefreshScrollContent();
        }
        /// <summary>
        /// 添加多条新的数据到无限列表尾部
        /// </summary>
        /// <typeparam name="T">无限列表数据类型</typeparam>
        /// <param name="datas">无限列表数据</param>
        public void AddDatas<T>(T[] datas)
        {
            for (int i = 0; i < datas.Length; i++)
            {
                if (_dataIndexs.Contains(datas[i]))
                {
                    Debug.LogWarning("添加数据至无限列表失败：列表中已存在该数据 " + datas[i].ToString());
                    continue;
                }

                _datas.Add(datas[i]);
                _dataIndexs.Add(datas[i]);
            }

            RefreshScrollContent();
        }
        /// <summary>
        /// 添加多条新的数据到无限列表尾部
        /// </summary>
        /// <typeparam name="T">无限列表数据类型</typeparam>
        /// <param name="datas">无限列表数据</param>
        public void AddDatas<T>(List<T> datas)
        {
            for (int i = 0; i < datas.Count; i++)
            {
                if (_dataIndexs.Contains(datas[i]))
                {
                    Debug.LogWarning("添加数据至无限列表失败：列表中已存在该数据 " + datas[i].ToString());
                    continue;
                }

                _datas.Add(datas[i]);
                _dataIndexs.Add(datas[i]);
            }

            RefreshScrollContent();
        }
        /// <summary>
        /// 移除一条无限列表数据
        /// </summary>
        /// <param name="data">无限列表数据</param>
        public void RemoveData(object data)
        {
            if (_dataIndexs.Contains(data))
            {
                _datas.Remove(data);
                _dataIndexs.Remove(data);

                if (_displayElements.ContainsKey(data))
                {
                    RecycleElement(_displayElements[data]);
                    _displayElements.Remove(data);
                }

                RefreshScrollContent();
            }
            else
            {
                Debug.LogWarning("从无限列表中移除数据失败：列表中不存在该数据 " + data.ToString());
            }
        }
        /// <summary>
        /// 清除所有的无限列表数据
        /// </summary>
        public void ClearData()
        {
            _datas.Clear();
            _dataIndexs.Clear();

            foreach (var element in _displayElements)
            {
                RecycleElement(element.Value);
            }
            _displayElements.Clear();

            RefreshScrollContent();
        }

        /// <summary>
        /// 刷新滚动列表内容
        /// </summary>
        protected void RefreshScrollContent()
        {
            if (ListingDirection == Direction.Vertical)
            {
                content.sizeDelta = new Vector2(content.sizeDelta.x, _datas.Count * (_height + Interval));
            }
            else
            {
                content.sizeDelta = new Vector2(_datas.Count * (_height + Interval), content.sizeDelta.y);
            }

            RefreshScrollView();
        }
        /// <summary>
        /// 刷新滚动视图
        /// </summary>
        protected void RefreshScrollView()
        {
            if (ListingDirection == Direction.Vertical)
            {
                float contentY = content.anchoredPosition.y;

                // 修复Scroll View的尺寸采用自适应父节点的方式导致渲染的BUG
                // https://discussions.unity.com/t/how-do-i-get-the-literal-width-of-a-recttransform/135984
                float viewHeight = UITransform.rect.height;

                ClearInvisibleVerticalElement(contentY, viewHeight);

                int originIndex = (int)(contentY / (_height + Interval));
                if (originIndex < 0) originIndex = 0;
                for (int i = originIndex; i < _datas.Count; i++)
                {
                    var data = _datas[i];
                    float viewY = -(i * _height + (i + 1) * Interval);
                    float realY = viewY + contentY;
                    if (realY > -viewHeight)
                    {
                        if (_displayElements.ContainsKey(data))
                        {
                            _displayElements[data].UITransform.anchoredPosition = new Vector2(0, viewY);
                            continue;
                        }
                        
                        var element = _elementsPool.Get();
                        element.UITransform.anchoredPosition = new Vector2(0, viewY);
                        element.OnUpdateData(this, i, data);
                        _displayElements.Add(data, element);
                        _onElementAdded?.Invoke(element);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                float contentX = content.anchoredPosition.x;

                // 修复Scroll View的尺寸采用自适应父节点的方式导致渲染的BUG
                // https://discussions.unity.com/t/how-do-i-get-the-literal-width-of-a-recttransform/135984
                float viewWidth = UITransform.rect.width;

                ClearInvisibleHorizontalElement(contentX, viewWidth);

                int originIndex = (int)(-contentX / (_height + Interval));
                if (originIndex < 0) originIndex = 0;
                for (int i = originIndex; i < _datas.Count; i++)
                {
                    var data = _datas[i];
                    float viewX = i * _height + (i + 1) * Interval;
                    float realX = viewX + contentX;
                    if (realX < viewWidth)
                    {
                        if (_displayElements.ContainsKey(data))
                        {
                            _displayElements[data].UITransform.anchoredPosition = new Vector2(viewX, 0);
                            continue;
                        }

                        var element = _elementsPool.Get();
                        element.UITransform.anchoredPosition = new Vector2(viewX, 0);
                        element.OnUpdateData(this, i, data);
                        _displayElements.Add(data, element);
                        _onElementAdded?.Invoke(element);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 清理并回收看不见的元素（垂直模式）
        /// </summary>
        /// <param name="contentY">滚动视图内容位置y</param>
        /// <param name="viewHeight">滚动视图高度</param>
        private void ClearInvisibleVerticalElement(float contentY, float viewHeight)
        {
            foreach (var element in _displayElements)
            {
                float realY = element.Value.UITransform.anchoredPosition.y + contentY;
                if (realY < _height && realY > -viewHeight)
                {
                    continue;
                }
                else
                {
                    _invisibleList.Add(element.Key);
                }
            }
            foreach (var item in _invisibleList)
            {
                RecycleElement(_displayElements[item]);
                _displayElements.Remove(item);
            }
            _invisibleList.Clear();
        }
        
        /// <summary>
        /// 清理并回收看不见的元素（水平模式）
        /// </summary>
        /// <param name="contentX">滚动视图内容位置x</param>
        /// <param name="viewWidth">滚动视图宽度</param>
        private void ClearInvisibleHorizontalElement(float contentX, float viewWidth)
        {
            foreach (var element in _displayElements)
            {
                float realX = element.Value.UITransform.anchoredPosition.x + contentX;
                if (realX > -_height && realX < viewWidth)
                {
                    continue;
                }
                else
                {
                    _invisibleList.Add(element.Key);
                }
            }
            foreach (var item in _invisibleList)
            {
                RecycleElement(_displayElements[item]);
                _displayElements.Remove(item);
            }
            _invisibleList.Clear();
        }
   
        
        /// <summary>
        /// 回收一个无用的无限列表元素
        /// </summary>
        /// <param name="element">无限列表元素</param>
        private void RecycleElement(IListElement element)
        {
            // element.OnClearData();
            // element.SetVisible(false);
            // _elementsPool.Enqueue(element);
            _elementsPool.Release(element);
            _onElementRemoved?.Invoke(element);
        }

        /// <summary>
        /// 方向
        /// </summary>
        public enum Direction
        {
            /// <summary>
            /// 水平
            /// </summary>
            Horizontal,
            /// <summary>
            /// 垂直
            /// </summary>
            Vertical
        }
    }
}
