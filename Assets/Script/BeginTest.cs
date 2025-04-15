using UnityEngine;
using UnityEngine.UI; // 引入UI命名空间来处理UI元素

public class ButtonHandler : MonoBehaviour
{
    public Button yourButton; // 可以在Unity Editor中设置此公共变量

    void Start()
    {
        yourButton.onClick.AddListener(OnButtonClick); // 给按钮添加点击事件的监听器
    }

    void OnButtonClick()
    {
        Debug.Log("按钮已点击！");
        // 这里添加点击按钮时要执行的代码
    }
}