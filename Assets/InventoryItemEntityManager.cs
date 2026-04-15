using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 背包物品-场景实体映射管理器
/// 
/// 功能：维护背包中每个物品与场景中对应实体的一一对应关系
/// 
/// 工作流程：
/// 1. 玩家拾取物品 → CollectibleItem.Collect() → 记录该实体的引用
/// 2. 玩家点击背包物品 → 激活对应的隐藏实体到玩家面前
/// 3. 实体成功交互（如 ClockPart 安装） → 销毁实体 + 从背包移除物品
/// 4. 实体未成功交互 → 隐藏实体 + 保留背包物品
/// </summary>
public class InventoryItemEntityManager : MonoBehaviour
{
    private class ItemEntityPair
    {
        public Item item;
        public GameObject entity;
        public CollectibleItem collectibleComponent;
    }

    private static InventoryItemEntityManager instance;
    private List<ItemEntityPair> itemEntityPairs = new List<ItemEntityPair>();
    private Camera mainCamera;
    private Transform player;

    [Header("生成设置")]
    [SerializeField] private float spawnDistance = 1.5f;
    [SerializeField] private float spawnHeightOffset = 0f;

    public static InventoryItemEntityManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<InventoryItemEntityManager>();

                // 如果场景中不存在，自动创建
                if (instance == null)
                {
                    GameObject managerObj = new GameObject("InventoryItemEntityManager");
                    instance = managerObj.AddComponent<InventoryItemEntityManager>();
                    Debug.Log("[InventoryItemEntityManager] 自动创建管理器实例");
                }
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

        mainCamera = Camera.main;
        player = mainCamera?.transform;
    }

    /// <summary>
    /// 注册物品与实体的对应关系（在 CollectibleItem.Collect 时调用）
    /// </summary>
    public void RegisterItem(Item item, GameObject entity, CollectibleItem collectibleComponent)
    {
        if (item == null || entity == null)
        {
            Debug.LogWarning("[InventoryItemEntityManager] 尝试注册空物品或实体");
            return;
        }

        // 检查是否已注册
        foreach (var pair in itemEntityPairs)
        {
            if (pair.item == item && pair.entity == entity)
            {
                return; // 已注册
            }
        }

        itemEntityPairs.Add(new ItemEntityPair
        {
            item = item,
            entity = entity,
            collectibleComponent = collectibleComponent
        });

        Debug.Log($"[InventoryItemEntityManager] 注册物品: {item.ItemName}, 实体: {entity.name}");
    }

    /// <summary>
    /// 检出物品（激活隐藏实体）
    /// </summary>
    public GameObject CheckoutItem(Item item)
    {
        // 检查玩家/摄像机是否就绪
        if (player == null)
        {
            Debug.LogError("[InventoryItemEntityManager] 无法获取玩家变换，主摄像机未初始化");
            return null;
        }

        ItemEntityPair pair = FindPairByItem(item);
        if (pair == null)
        {
            Debug.LogWarning($"[InventoryItemEntityManager] 找不到物品的对应实体: {item.ItemName}");
            return null;
        }

        if (pair.entity == null)
        {
            Debug.LogWarning($"[InventoryItemEntityManager] 物品的对应实体已被销毁: {item.ItemName}");
            itemEntityPairs.Remove(pair);
            return null;
        }

        // 将实体激活到玩家面前
        Vector3 spawnPos = player.position + player.forward * spawnDistance + Vector3.up * spawnHeightOffset;
        pair.entity.transform.position = spawnPos;
        pair.entity.transform.rotation = Quaternion.identity;
        pair.entity.SetActive(true);

        Debug.Log($"[InventoryItemEntityManager] 检出物品: {item.ItemName}");
        return pair.entity;
    }

    /// <summary>
    /// 返回物品（隐藏实体但保留在背包）
    /// </summary>
    public void ReturnItem(Item item)
    {
        ItemEntityPair pair = FindPairByItem(item);
        if (pair == null || pair.entity == null)
        {
            return;
        }

        pair.entity.SetActive(false);
        Debug.Log($"[InventoryItemEntityManager] 返回物品到背包: {item.ItemName}");
    }

    /// <summary>
    /// 销毁物品（实体成功交互，如物品被使用）
    /// </summary>
    public void RemoveItem(Item item)
    {
        ItemEntityPair pair = FindPairByItem(item);
        if (pair == null)
        {
            return;
        }

        if (pair.entity != null)
        {
            Destroy(pair.entity);
        }

        itemEntityPairs.Remove(pair);

        // 从背包中删除该物品
        InventoryManager inventoryMgr = InventoryManager.Instance;
        if (inventoryMgr != null)
        {
            inventoryMgr.RemoveItem(item);
        }

        Debug.Log($"[InventoryItemEntityManager] 永久移除物品: {item.ItemName}");
    }

    /// <summary>
    /// 检查某个物品是否在背包中
    /// </summary>
    public bool HasItem(Item item)
    {
        return FindPairByItem(item) != null;
    }

    /// <summary>
    /// 根据实体查找对应的物品
    /// </summary>
    public Item FindItemByEntity(GameObject entity)
    {
        foreach (var pair in itemEntityPairs)
        {
            if (pair.entity == entity)
            {
                return pair.item;
            }
        }
        return null;
    }

    private ItemEntityPair FindPairByItem(Item item)
    {
        foreach (var pair in itemEntityPairs)
        {
            if (pair.item == item)
            {
                return pair;
            }
        }
        return null;
    }

    /// <summary>
    /// 清空所有记录（场景重置时使用）
    /// </summary>
    public void Clear()
    {
        itemEntityPairs.Clear();
    }
}
