using InfiniteList;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteListDemo : MonoBehaviour
{
    public InfiniteListScrollRect InfiniteList;

    private string[] _surnames = new string[] { "张", "李", "王", "赵", "刘", "胡", "霍", "江", "唐", "欧阳", "司徒", "慕容", "轩辕", "皇甫", "西门" };
    private string[] _names = new string[] { "三", "四", "五", "明", "强", "磊", "龙", "虎", "玉清", "博", "成", "航", "逸", "建国", "建军", "建党" };

    private void Start()
	{
        List<InfiniteListTestData> datas = new List<InfiniteListTestData>();
        for (int i = 0; i < 1000; i++)
        {
            InfiniteListTestData data = new InfiniteListTestData();
            data.Name = (i + 1) + "." + RandomName();
            datas.Add(data);
        }

        InfiniteList.AddDatas(datas);
    }

    private string RandomName()
    {
        int surname = Random.Range(0, _surnames.Length);
        int name = Random.Range(0, _names.Length);

        return _surnames[surname] + _names[name];
    }
}
