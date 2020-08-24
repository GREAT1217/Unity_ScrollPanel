/*************************************************************************
 *  Copyright © 2020 Great1217. All rights reserved.
 *------------------------------------------------------------------------
 *  File         :  DemoEventItem.cs
 *  Description  :  Null.
 *------------------------------------------------------------------------
 *  Author       :  Great1217
 *  Version      :  0.1.0
 *  Date         :  8/24/2020
 *  Description  :  Initial development version.
 *************************************************************************/

using UnityEngine;
using UnityEngine.UI;

public class DemoEventItem : MonoBehaviour
{
    public CanvasGroup _panel;
    public Text _textIndex;
    public Image _imgBg;
    public Color _on, _off;

    public void OnInit(int index)
    {
        _textIndex.text = index.ToString();
        _imgBg.color = _off;
    }

    public void OnCenter(bool value)
    {
        _imgBg.color = value ? _on : _off;
    }

    public void UpdataEffect(float effectValue)
    {
        _panel.alpha = effectValue;
    }
}