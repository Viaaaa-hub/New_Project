using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 钟表零件背包管理器 - 协调背包中钟表零件与场景实体的交互
/// 
/// 工作流程：
/// 1. 玩家从背包点击钟表零件
/// 2. 本脚本生成一个临时实体在玩家面前
/// 3. 玩家将实体放入对应的插槽
/// 4. ClockPart 检测到成功安装，通知本脚本
/// 5. 本脚本从背包中移除该物品
/// 6. 如果未成功安装，实体可被收回背包
/// </summary>
public class ClockPartInventoryManager : MonoBehaviour
{
    [Header("零件预制体/模板")]
    [Tooltip("可以为空 - 系统会在运行时从场景中查找对应的零件模板")]
    [SerializeField] private GameObject gearTemplate;
    [SerializeField] private GameObject hourHandTemplate;
    [SerializeField] private GameObject minuteHandTemplate;

    [Header("生成设置")]
    [Tooltip("在玩家面前生成物品的距离")]
    [SerializeField] private float spawnDistance = 1.5f;

    [Tooltip("在玩家面前生成物品的高度偏移")]
    [SerializeField] private float spawnHeightOffset = 0f;

    private static ClockPartInventoryManager instance;
    private Camera mainCamera;
    private Transform player;
    
    // 正在使用中的零件实体（来自背包）
    private Dictionary<ClockPartType, GameObject> activePartInstances = new Dictionary<ClockPartType, GameObject>();
    // 对应的 Item 数据
    private Dictionary<ClockPartType, Item> activePartItems = new Dictionary<ClockPartType, Item>();

    public static ClockPartInventoryManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ClockPartInventoryManager>();
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

        // 如果预制体为空，尝试从场景中查找模板
        FindTemplateParts();
    }

    /// <summary>
    /// 在场景中查找对应的零件作为模板（如果未在 Inspector 中指定）
    /// </summary>
    private void FindTemplateParts()
    {
        if (gearTemplate == null || hourHandTemplate == null || minuteHandTemplate == null)
        {
            // 从场景中所有 ClockPart 中查找对应类型的第一个实例作为模板
            ClockPart[] allParts = FindObjectsOfType<ClockPart>();
            foreach (ClockPart part in allParts)
            {
                switch (part.PartType)
                {
                    case ClockPartType.Gear:
                        if (gearTemplate == null) gearTemplate = part.gameObject;
                        break;
                    case ClockPartType.HourHand:
                        if (hourHandTemplate == null) hourHandTemplate = part.gameObject;
                        break;
                    case ClockPartType.MinuteHand:
                        if (minuteHandTemplate == null) minuteHandTemplate = part.gameObject;
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 从背包检出钟表零件，在玩家面前生成临时实体
    /// </summary>
    public GameObject CheckoutClockPart(Item item, ClockPartType partType)
    {
        if (item == null)
        {
            Debug.LogWarning("[ClockPartInventoryManager] 尝试检出空物品");
            return null;
        }

        // 如果该类型零件已存在活跃实体，先收回
        if (activePartInstances.ContainsKey(partType) && activePartInstances[partType] != null)
        {
            ReturnPartToInventory(partType);
        }

        // 选择对应的模板
        GameObject template = GetTemplateForPartType(partType);
        if (template == null)
        {
            Debug.LogWarning($"[ClockPartInventoryManager] 找不到 {partType} 的模板（预制体未指定，也未在场景中找到）");
            return null;
        }

        // 检查玩家/摄像机是否就绪
        if (player == null)
        {
            Debug.LogError("[ClockPartInventoryManager] 无法获取玩家变换，主摄像机未初始化");
            return null;
        }

        // 在玩家面前生成实体（克隆模板）
        Vector3 spawnPos = player.position + player.forward * spawnDistance + Vector3.up * spawnHeightOffset;
        GameObject instance = Instantiate(template, spawnPos, Quaternion.identity);

        // 获取 ClockPart 组件并重置状态
        ClockPart clockPart = instance.GetComponent<ClockPart>();
        if (clockPart != null)
        {
            // 如果是克隆，需要重置状态
            clockPart.ResetState();
            clockPart.SetOnInstalledCallback(() => OnPartInstalled(partType));
            clockPart.SetOnReturnedCallback(() => ReturnPartToInventory(partType));
        }
        else
        {
            Debug.LogWarning($"[ClockPartInventoryManager] 克隆的零件缺少 ClockPart 组件");
            Destroy(instance);
            return null;
        }

        // 记录该实体
        activePartInstances[partType] = instance;
        activePartItems[partType] = item;

        Debug.Log($"[ClockPartInventoryManager] 检出零件: {partType}");
        return instance;
    }

    /// <summary>
    /// 零件成功安装时调用（由 ClockPart 触发）
    /// </summary>
    private void OnPartInstalled(ClockPartType partType)
    {
        if (!activePartItems.ContainsKey(partType))
        {
            return;
        }

        Item item = activePartItems[partType];

        // 从背包中移除该物品
        InventoryManager inventoryMgr = InventoryManager.Instance;
        if (inventoryMgr != null)
        {
            inventoryMgr.RemoveItem(item);
        }

        // 清理记录
        activePartInstances.Remove(partType);
        activePartItems.Remove(partType);

        Debug.Log($"[ClockPartInventoryManager] 零件 {partType} 已安装并从背包移除");
    }

    /// <summary>
    /// 将零件回收到背包（如果未成功安装）
    /// </summary>
    public void ReturnPartToInventory(ClockPartType partType)
    {
        if (!activePartInstances.ContainsKey(partType) || activePartInstances[partType] == null)
        {
            return;
        }

        GameObject instance = activePartInstances[partType];
        Destroy(instance);
        activePartInstances.Remove(partType);
        activePartItems.Remove(partType);

        Debug.Log($"[ClockPartInventoryManager] 零件 {partType} 已回收到背包");
    }

    /// <summary>
    /// 根据零件类型获取模板
    /// </summary>
    private GameObject GetTemplateForPartType(ClockPartType partType)
    {
        return partType switch
        {
            ClockPartType.Gear => gearTemplate,
            ClockPartType.HourHand => hourHandTemplate,
            ClockPartType.MinuteHand => minuteHandTemplate,
            _ => null
        };
    }

    /// <summary>
    /// 检查某个物品是否是钟表零件
    /// </summary>
    public static bool IsClocklPart(Item item)
    {
        return item != null && (item.ItemId.Contains("gear") || item.ItemId.Contains("hand") || item.ItemId.Contains("clock"));
    }

    /// <summary>
    /// 从物品 ID 推断零件类型
    /// </summary>
    public static ClockPartType InferPartType(string itemId)
    {
        if (itemId.Contains("gear") || itemId.Contains("Gear"))
            return ClockPartType.Gear;
        if (itemId.Contains("hour") || itemId.Contains("Hour"))
            return ClockPartType.HourHand;
        if (itemId.Contains("minute") || itemId.Contains("Minute"))
            return ClockPartType.MinuteHand;
        return ClockPartType.Gear; // 默认
    }
}
