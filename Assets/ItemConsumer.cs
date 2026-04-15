using UnityEngine;

/// <summary>
/// 物品消费者基类 - 机关/谜题使用此类消费物品
/// 
/// 使用方式：
/// 1. 创建派生类并实现 OnCorrectItem() 和 OnWrongItem()
/// 2. 挂载到机关GameObject
/// 3. 指定所需物品和机制ID
/// 4. 添加 Collider 和交互触发
/// </summary>
public abstract class ItemConsumer : MonoBehaviour
{
    [SerializeField] protected string mechanismId;
    [SerializeField] protected Item requiredItem;

    protected bool isSolved = false;
    protected bool isPlayerNearby = false;

    protected virtual void Start()
    {
        ValidateSetup();
    }

    protected virtual void ValidateSetup()
    {
        if (string.IsNullOrEmpty(mechanismId))
        {
            Debug.LogWarning($"ItemConsumer 在 {gameObject.name} 上没有指定 mechanismId", gameObject);
        }

        if (requiredItem == null)
        {
            Debug.LogWarning($"ItemConsumer 在 {gameObject.name} 上没有指定 requiredItem", gameObject);
        }

        if (GetComponent<Collider>() == null)
        {
            Debug.LogWarning($"ItemConsumer 在 {gameObject.name} 上没有 Collider 组件", gameObject);
        }
    }

    protected virtual void Update()
    {
        // 按E键打开背包（可根据需要调整按键）
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    /// <summary>
    /// 检测玩家靠近
    /// </summary>
    protected virtual void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            OnPlayerNearby();
        }
    }

    protected virtual void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            OnPlayerLeave();
        }
    }

    protected virtual void OnPlayerNearby()
    {
        Debug.Log("按E可与机关交互");
    }

    protected virtual void OnPlayerLeave()
    {
    }

    /// <summary>
    /// 交互 - 打开背包
    /// </summary>
    public virtual void Interact()
    {
        if (isSolved)
        {
            OnMechanismAlreadySolved();
            return;
        }

        InventoryManager inventoryMgr = InventoryManager.Instance;
        if (inventoryMgr != null)
        {
            inventoryMgr.OpenInventory();

            // 告诉UI当前打开背包的是哪个机关
            InventoryUIManager uiMgr = FindObjectOfType<InventoryUIManager>();
            if (uiMgr != null)
            {
                uiMgr.SetCurrentMechanism(this);
            }
        }

        Debug.Log($"打开背包以使用物品 (机关: {mechanismId})");
    }

    /// <summary>
    /// 使用物品 - 由 InventorySlotUI 调用
    /// </summary>
    public void UseItem(Item selectedItem)
    {
        if (isSolved)
        {
            OnMechanismAlreadySolved();
            return;
        }

        if (selectedItem == null)
        {
            Debug.LogWarning("尝试使用空物品");
            return;
        }

        if (requiredItem == null)
        {
            Debug.LogError($"ItemConsumer 在 {gameObject.name} 上的 requiredItem 未设置");
            return;
        }

        // 检查是否是正确物品
        if (selectedItem.ItemId == requiredItem.ItemId)
        {
            // 正确物品 - 解决机关
            OnCorrectItem(selectedItem);
        }
        else
        {
            // 错误物品
            OnWrongItem(selectedItem);
        }
    }

    /// <summary>
    /// 处理正确物品 - 派生类实现具体逻辑
    /// </summary>
    protected virtual void OnCorrectItem(Item item)
    {
        isSolved = true;

        // 移除物品
        InventoryManager inventoryMgr = InventoryManager.Instance;
        if (inventoryMgr != null)
        {
            inventoryMgr.RemoveItem(item);
            inventoryMgr.CloseInventory();
        }

        Debug.Log($"机关 {mechanismId} 已解决!");
    }

    /// <summary>
    /// 处理错误物品 - 派生类可自定义反馈
    /// </summary>
    protected virtual void OnWrongItem(Item item)
    {
        Debug.Log($"错误! {item.ItemName} 不能使用在这个机关上");
    }

    /// <summary>
    /// 机关已解决时的处理
    /// </summary>
    protected virtual void OnMechanismAlreadySolved()
    {
        Debug.Log($"机关 {mechanismId} 已经被解决过了");
    }

    public bool IsSolved() => isSolved;
    public string GetMechanismId() => mechanismId;
}
