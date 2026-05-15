using HT.InfiniteList;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteListDemo : MonoBehaviour
{
    public InfiniteListScrollRect InfiniteList;

    private string[] _surnames = new string[] { "张", "李", "王", "赵", "刘", "胡", "霍", "江", "唐", "欧阳", "司徒", "慕容", "轩辕", "皇甫", "西门" };
    private string[] _names = new string[] { "三", "四", "五", "明", "强", "磊", "龙", "虎", "玉清", "博", "成", "航", "逸", "建国", "建军", "建党" };

    private void Start()
	{
        AddTwoData();

        //请注意，由于此事件会在列表到达尾部时添加2条数据，而如果删除数据，会收缩列表，导致列表到达尾部，从而又触发添加数据，导致删除数据无效
        //所以，如果有这个需求（列表滚动到尾部自动添加数据），则最好禁用删除数据功能
        InfiniteList.onValueChanged.AddListener((value) =>
        {
            if (InfiniteList.ListingDirection == InfiniteListScrollRect.Direction.Vertical && value.y <= 0)
            {
                Debug.Log("到达列表尾部，在尾部添加数据2条！");
                AddTwoData();
            }
            else if (InfiniteList.ListingDirection == InfiniteListScrollRect.Direction.Horizontal && value.x >= 1)
            {
                Debug.Log("到达列表尾部，在尾部添加数据2条！");
                AddTwoData();
            }
        });
    }
    
    private string RandomName()
    {
        int surname = Random.Range(0, _surnames.Length);
        int name = Random.Range(0, _names.Length);

        return _surnames[surname] + _names[name];
    }

    /// <summary>
    /// 添加两条新数据
    /// </summary>
    public void AddTwoData()
    {
        List<InfiniteListTestData> datas = new List<InfiniteListTestData>();
        for (int i = 0; i < 2; i++)
        {
            InfiniteListTestData data = new InfiniteListTestData();
            data.Name = (i + 1) + "." + RandomName();
            datas.Add(data);
        }
        InfiniteList.AddDatas(datas);

        Debug.Log("当前列表数据总量：" + InfiniteList.DataCount);
    }
}
