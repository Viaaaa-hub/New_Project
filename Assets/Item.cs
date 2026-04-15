using UnityEngine;

/// <summary>
/// 物品数据结构 - ScriptableObject
/// 所有可收集物品都对应一个 Item 资源文件
/// 在 Assets 中右键 → Create → Inventory → Item 来创建物品
/// </summary>
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string itemName;
    [SerializeField] private Sprite icon;
    [SerializeField] private string description;

    public string ItemId => itemId;
    public string ItemName => string.IsNullOrEmpty(itemName) ? name : itemName;
    public Sprite Icon => icon;
    public string Description => description;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(itemId))
        {
            itemId = name.Replace(" ", "_").ToLower();
        }

        if (string.IsNullOrEmpty(itemName))
        {
            itemName = name;
        }
    }
}
