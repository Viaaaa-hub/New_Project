using UnityEngine;

/// <summary>
/// 可收集物品脚本 - 挂载到场景中散落的物品上
/// 
/// 使用方式：
/// 1. 挂载此脚本到物品GameObject
/// 2. 分配 Item 数据（ScriptableObject）
/// 3. 添加 XRGrabInteractable 组件或自定义拾取检测
/// 4. 添加 Collider 和 Rigidbody
/// </summary>
public class CollectibleItem : MonoBehaviour
{
    [SerializeField] private Item itemData;
    [SerializeField] private bool destroyAfterCollection = true;

    private bool hasBeenCollected = false;

    private void Start()
    {
        if (itemData == null)
        {
            Debug.LogError($"CollectibleItem 在 {gameObject.name} 上没有指定物品数据", gameObject);
        }
    }

    /// <summary>
    /// 手动拾取物品（由 XRGrabInteractable 或其他交互系统调用）
    /// </summary>
    public void Collect()
    {
        if (hasBeenCollected || itemData == null)
        {
            return;
        }

        // 添加到背包
        InventoryManager inventoryMgr = InventoryManager.Instance;
        if (inventoryMgr != null)
        {
            inventoryMgr.AddItem(itemData);
            hasBeenCollected = true;

            // 注册物品与实体的对应关系
            InventoryItemEntityManager entityMgr = InventoryItemEntityManager.Instance;
            if (entityMgr != null)
            {
                entityMgr.RegisterItem(itemData, gameObject, this);
            }

            // 从场景移除
            if (destroyAfterCollection)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 当手柄抓取此物品时调用（如果使用XR Interaction Toolkit）
    /// 可在 Inspector 中通过 XRGrabInteractable 的事件来调用此方法
    /// 或在其他系统中手动调用
    /// </summary>
    public void OnGrabbed()
    {
        Collect();
    }

    public Item GetItemData() => itemData;
}
