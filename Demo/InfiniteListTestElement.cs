using InfiniteList;
using UnityEngine.UI;

/// <summary>
/// 无限列表测试元素
/// </summary>
public class InfiniteListTestElement : InfiniteListElement
{
    public Text Name;
    public Button RemoveButton;

    private InfiniteListScrollRect _scrollRect;
    private InfiniteListTestData _data;

    public override void UpdateData(InfiniteListScrollRect scrollRect, InfiniteListData data)
    {
        base.UpdateData(scrollRect, data);

        _scrollRect = scrollRect;
        _data = data as InfiniteListTestData;
        Name.text = _data.Name;
        RemoveButton.onClick.AddListener(() => { _scrollRect.RemoveData(_data); });
    }

    public override void ClearData()
    {
        base.ClearData();

        RemoveButton.onClick.RemoveAllListeners();
    }
}
