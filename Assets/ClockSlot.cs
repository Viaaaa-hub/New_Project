using UnityEngine;

/// <summary>
/// 钟表插槽标记 - 挂载到钟表上的零件安装位置
/// 
/// 使用方式：
/// 1. 在钟表模型上创建空物体作为插槽（齿轮位、时针位、分针位）
/// 2. 给每个插槽添加 Collider 并勾选 Is Trigger
/// 3. 设置 Tag 为 "ClockSlot"
/// 4. 挂载此脚本并选择对应的零件类型
/// </summary>
public class ClockSlot : MonoBehaviour
{
    [Tooltip("此插槽接受的零件类型")]
    [SerializeField] private ClockPartType acceptedPartType;

    /// <summary>
    /// 此插槽接受的零件类型
    /// </summary>
    public ClockPartType AcceptedPartType => acceptedPartType;
}
