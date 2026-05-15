using UnityEngine;

namespace HT.InfiniteList
{
    public interface IListElement
    {
        RectTransform UITransform { get;  }
        void OnUpdateData(InfiniteListScrollRect scrollRect, int index, object data);
        void OnClearData();
        void SetVisible(bool visible);
    }
}
