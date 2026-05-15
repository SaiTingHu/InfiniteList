using HT.InfiniteList;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 无限列表测试元素
/// </summary>
public class InfiniteListTestElement : MonoBehaviour, IListElement
{
    public Text Name;
    public Button RemoveButton;

    private InfiniteListScrollRect _scrollRect;
    private InfiniteListTestData _data;

    public RectTransform UITransform => transform as RectTransform;
    
    public void SetVisible(bool visible)
    {
        if (gameObject.activeSelf != visible)
        {
            gameObject.SetActive(visible);
        }
    }

    public void OnUpdateData(InfiniteListScrollRect scrollRect, int index, object data)
    {
        _scrollRect = scrollRect;
        _data = data as InfiniteListTestData;
        Name.text = $"{index}. {_data?.Name}";
        RemoveButton.onClick.AddListener(() => { _scrollRect.RemoveData(_data); });
    }

    public void OnClearData()
    {
        RemoveButton.onClick.RemoveAllListeners();
    }


}
