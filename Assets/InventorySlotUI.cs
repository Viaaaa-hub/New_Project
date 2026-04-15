using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 背包物品槽UI - 代表背包中的一个物品
/// 挂载到物品槽预制体上
/// 预制体应包含: Image(图标) + Button(点击) + Text(物品名)
/// </summary>
public class InventorySlotUI : MonoBehaviour
{
    private Item item;
    private InventoryUIManager uiManager;
    private Button button;
    private Image icon;
    
    // 支持两种文本组件类型
    private Text legacyText;
    private TextMeshProUGUI tmpText;

    private void Awake()
    {
        button = GetComponent<Button>();
        icon = GetComponent<Image>();
        
        // 尝试获取 TextMeshPro 文本
        tmpText = GetComponentInChildren<TextMeshProUGUI>();
        
        // 如果没有 TMP，尝试获取旧版 Text 组件
        if (tmpText == null)
        {
            legacyText = GetComponentInChildren<Text>();
        }

        if (button == null)
            Debug.LogWarning("InventorySlotUI: 找不到 Button 组件", gameObject);
        if (icon == null)
            Debug.LogWarning("InventorySlotUI: 找不到 Image 组件", gameObject);
        if (tmpText == null && legacyText == null)
            Debug.LogWarning("InventorySlotUI: 找不到 Text 或 TextMeshProUGUI 组件", gameObject);
    }

    /// <summary>
    /// 初始化槽位
    /// </summary>
    public void Initialize(Item itemData, InventoryUIManager manager)
    {
        if (itemData == null)
        {
            Debug.LogError("InventorySlotUI: 尝试初始化空物品数据", gameObject);
            return;
        }

        if (manager == null)
        {
            Debug.LogError("InventorySlotUI: UIManager 为空", gameObject);
            return;
        }

        item = itemData;
        uiManager = manager;

        Debug.Log($"[InventorySlotUI] 初始化物品槽: {itemData.ItemName}", gameObject);

        // 设置图标
        if (icon != null)
        {
            if (item.Icon != null)
            {
                icon.sprite = item.Icon;
            }
            else
            {
                Debug.LogWarning($"[InventorySlotUI] 物品 {item.ItemName} 没有图标");
            }
        }
        else
        {
            Debug.LogWarning("[InventorySlotUI] Image 组件为空，无法设置图标", gameObject);
        }

        // 设置物品名称
        SetItemName(item.ItemName);

        // 按钮点击事件
        if (button != null)
        {
            button.onClick.AddListener(OnSlotClicked);
        }
        else
        {
            Debug.LogWarning("[InventorySlotUI] Button 组件为空，无法添加点击事件", gameObject);
        }
    }

    /// <summary>
    /// 设置物品名称文本
    /// </summary>
    private void SetItemName(string name)
    {
        if (tmpText != null)
        {
            tmpText.text = name;
            Debug.Log($"[InventorySlotUI] 设置物品名称（TMP）: {name}");
        }
        else if (legacyText != null)
        {
            legacyText.text = name;
            Debug.Log($"[InventorySlotUI] 设置物品名称（Legacy）: {name}");
        }
        else
        {
            Debug.LogWarning("[InventorySlotUI] Text 组件为空，无法设置物品名称", gameObject);
        }
    }

    /// <summary>
    /// 点击槽位 - 直接使用物品
    /// </summary>
    private void OnSlotClicked()
    {
        if (uiManager == null)
        {
            Debug.LogWarning("InventorySlotUI: UIManager 未初始化");
            return;
        }

        ItemConsumer mechanism = uiManager.GetCurrentMechanism();
        
        if (mechanism != null)
        {
            // 有打开背包的机关存在，尝试使用物品
            mechanism.UseItem(item);
        }
        else
        {
            // 没有机关在使用，只是查看物品
            Debug.Log($"查看物品: {item.ItemName}");
        }
    }

    public Item GetItem() => item;

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnSlotClicked);
        }
    }
}
