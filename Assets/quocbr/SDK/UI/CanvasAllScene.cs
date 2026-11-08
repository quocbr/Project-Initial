using quocbr.DesignPattern;
using UnityEngine;

public class CanvasAllScene : Singleton<CanvasAllScene>
{
    public GameObject panelLoading;
    public GameObject imgFullOverlay;
    public void ShowNoti(string str)
    {
        //  UIManager.Ins.OpenUI<NotiUI>(str);
    }

}
