using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 背包UI管理器 - 管理UI显示和交互
/// 挂载到 Canvas 或单独物体上
/// </summary>
public class InventoryUIManager : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private KeyCode toggleKey = KeyCode.Menu;

    private List<InventorySlotUI> slots = new List<InventorySlotUI>();
    private ItemConsumer currentMechanism;

    private void Start()
    {
        ValidateSetup();

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        InventoryManager.OnItemAdded += RefreshUI;
        InventoryManager.OnInventoryOpened += Open;
        InventoryManager.OnInventoryClosed += Close;
        InventoryManager.OnItemRemoved += RefreshUI;

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }
    }

    private void OnDestroy()
    {
        InventoryManager.OnItemAdded -= RefreshUI;
        InventoryManager.OnInventoryOpened -= Open;
        InventoryManager.OnInventoryClosed -= Close;
        InventoryManager.OnItemRemoved -= RefreshUI;

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
        }
    }

    private void ValidateSetup()
    {
        if (inventoryPanel == null)
            Debug.LogError("InventoryUIManager: inventoryPanel 未指定", gameObject);

        if (itemContainer == null)
            Debug.LogError("InventoryUIManager: itemContainer 未指定", gameObject);

        if (itemSlotPrefab == null)
            Debug.LogError("InventoryUIManager: itemSlotPrefab 未指定", gameObject);
    }

    private void Update()
    {
        // 按键切换背包
        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }
    }

    /// <summary>
    /// 切换背包显示
    /// </summary>
    public void Toggle()
    {
        if (inventoryPanel == null)
        {
            Debug.LogError("InventoryUIManager: inventoryPanel 为空，无法切换");
            return;
        }

        if (inventoryPanel.activeInHierarchy)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    /// <summary>
    /// 打开背包
    /// </summary>
    public void Open()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
            RefreshUI(null);
        }
    }

    /// <summary>
    /// 关闭背包
    /// </summary>
    public void Close()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 刷新UI显示
    /// </summary>
    private void RefreshUI(Item changedItem)
    {
        if (itemContainer == null)
        {
            Debug.LogError("InventoryUIManager: itemContainer 为空，无法刷新UI");
            return;
        }

        if (itemSlotPrefab == null)
        {
            Debug.LogError("InventoryUIManager: itemSlotPrefab 未指定，无法创建物品槽", gameObject);
            return;
        }

        // 验证预制体是否有 InventorySlotUI 组件
        InventorySlotUI prefabSlotComponent = itemSlotPrefab.GetComponent<InventorySlotUI>();
        if (prefabSlotComponent == null)
        {
            Debug.LogError("InventoryUIManager: itemSlotPrefab 上缺少 InventorySlotUI 组件", itemSlotPrefab);
            return;
        }

        // 清除旧槽位
        foreach (InventorySlotUI slot in slots)
        {
            Destroy(slot.gameObject);
        }
        slots.Clear();

        // 创建新槽位
        InventoryManager inventoryMgr = InventoryManager.Instance;
        if (inventoryMgr == null)
        {
            Debug.LogError("InventoryUIManager: InventoryManager 实例为空");
            return;
        }

        List<Item> items = inventoryMgr.GetAllItems();
        foreach (Item item in items)
        {
            if (item == null)
            {
                Debug.LogWarning("InventoryUIManager: 背包中发现空物品");
                continue;
            }

            GameObject slotGO = Instantiate(itemSlotPrefab, itemContainer);
            InventorySlotUI slot = slotGO.GetComponent<InventorySlotUI>();

            if (slot != null)
            {
                slot.Initialize(item, this);
                slots.Add(slot);
            }
            else
            {
                Debug.LogError("物品槽预制体缺少 InventorySlotUI 组件", itemSlotPrefab);
                Destroy(slotGO);
            }
        }
    }

    /// <summary>
    /// 设置当前机关（当打开背包时调用）
    /// </summary>
    public void SetCurrentMechanism(ItemConsumer mechanism)
    {
        currentMechanism = mechanism;
    }

    /// <summary>
    /// 获取当前机关
    /// </summary>
    public ItemConsumer GetCurrentMechanism()
    {
        return currentMechanism;
    }
}
