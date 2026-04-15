using UnityEngine;

/// <summary>
/// 谜题石头 - 挂载到可拾取的石头物体上
/// 
/// 使用方式：
/// 1. 将此脚本挂载到石头物体上
/// 2. 添加 XR Grab Interactable + Rigidbody（Use Gravity 勾选）+ Collider
/// 3. Rigidbody 不要勾选 Is Kinematic（需要重力让石头落到石板上）
/// </summary>
public class PuzzleStone : MonoBehaviour
{
    private bool isPlaced = false;
    private Rigidbody rb;

    /// <summary>
    /// 是否已经被正确放置
    /// </summary>
    public bool IsPlaced => isPlaced;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// 锁定石头（正确放置后调用）
    /// </summary>
    public void LockInPlace()
    {
        isPlaced = true;

        // 禁用抓取
        var grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();
        if (grab != null)
        {
            // 如果正在被抓取，先松手
            if (grab.isSelected)
                grab.enabled = false;
            else
                grab.enabled = false;
        }

        // 固定物理
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Debug.Log($"[石头] {gameObject.name} 已锁定在石板上");
    }
}
