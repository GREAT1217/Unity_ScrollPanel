/*************************************************************************
 *  Copyright © 2020 Great1217. All rights reserved.
 *------------------------------------------------------------------------
 *  File         :  DemoEventScrollManager.cs
 *  Description  :  Null.
 *------------------------------------------------------------------------
 *  Author       :  Great1217
 *  Version      :  0.1.0
 *  Date         :  8/24/2020
 *  Description  :  Initial development version.
 *************************************************************************/

using UnityEngine;

public class DemoEventScrollManager : MonoBehaviour
{
    public GreatScrollPanel _scrollPanel;
    public GreatScrollEffect _scrollEffect;

    int _centerIndex = -1;

    void Awake()
    {
        _scrollPanel.OnItemInit.AddListener(OnItemInit);
        _scrollPanel.OnItemCenter.AddListener(OnItemCenter);
        _scrollEffect.OnCustomEffect.AddListener(UpdateScrollEffect);
    }

    void OnItemInit(Transform item, int index)
    {
        item.GetComponent<DemoEventItem>().OnInit(index);
    }

    void OnItemCenter(int index)
    {
        if (_centerIndex != -1)
        {
            _scrollPanel.ItemsRT[_centerIndex].GetComponent<DemoEventItem>().OnCenter(false);
        }
        _scrollPanel.ItemsRT[index].GetComponent<DemoEventItem>().OnCenter(true);
        _centerIndex = index;
    }

    void UpdateScrollEffect(Transform item, float effectValue)
    {
        item.GetComponent<DemoEventItem>().UpdataEffect(effectValue);
    }
}