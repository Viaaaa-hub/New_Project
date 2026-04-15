using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 背包管理器 - 单例，管理所有背包数据
/// 挂载到场景中的 GameManager(或系统物体)
/// </summary>
public class InventoryManager : MonoBehaviour
{
    [SerializeField] private List<Item> startingItems = new List<Item>();

    private List<Item> inventoryItems = new List<Item>();
    private static InventoryManager instance;

    // 事件系统
    public delegate void ItemAddedEvent(Item item);
    public delegate void ItemRemovedEvent(Item item);
    public delegate void InventoryOpenedEvent();
    public delegate void InventoryClosedEvent();

    public static event ItemAddedEvent OnItemAdded;
    public static event ItemRemovedEvent OnItemRemoved;
    public static event InventoryOpenedEvent OnInventoryOpened;
    public static event InventoryClosedEvent OnInventoryClosed;

    public static InventoryManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InventoryManager>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        // 初始化背包
        inventoryItems.Clear();
        foreach (Item item in startingItems)
        {
            if (item != null)
            {
                inventoryItems.Add(item);
            }
        }
    }

    /// <summary>
    /// 添加物品到背包
    /// </summary>
    public void AddItem(Item item)
    {
        if (item == null)
        {
            Debug.LogWarning("尝试添加空物品");
            return;
        }

        if (!inventoryItems.Contains(item))
        {
            inventoryItems.Add(item);
            OnItemAdded?.Invoke(item);
            Debug.Log($"物品已收集: {item.ItemName}");
        }
    }

    /// <summary>
    /// 从背包移除物品
    /// </summary>
    public void RemoveItem(Item item)
    {
        if (inventoryItems.Remove(item))
        {
            OnItemRemoved?.Invoke(item);
            Debug.Log($"物品已移除: {item.ItemName}");
        }
    }

    /// <summary>
    /// 获取背包中的所有物品
    /// </summary>
    public List<Item> GetAllItems()
    {
        return new List<Item>(inventoryItems);
    }

    /// <summary>
    /// 检查背包是否包含某物品
    /// </summary>
    public bool HasItem(Item item)
    {
        return inventoryItems.Contains(item);
    }

    /// <summary>
    /// 打开背包UI
    /// </summary>
    public void OpenInventory()
    {
        OnInventoryOpened?.Invoke();
    }

    /// <summary>
    /// 关闭背包UI
    /// </summary>
    public void CloseInventory()
    {
        OnInventoryClosed?.Invoke();
    }

    /// <summary>
    /// 清空背包（场景重置时使用）
    /// </summary>
    public void Clear()
    {
        inventoryItems.Clear();
    }
}
