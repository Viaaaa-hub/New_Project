using UnityEngine;

/// <summary>
/// 钥匙脚本 - 挂载到钥匙物体上
/// 
/// 使用方式：
/// 1. 将此脚本挂载到钥匙物体上
/// 2. 给钥匙添加 XR Grab Interactable + Rigidbody + Collider
/// 3. 门物体的触发区域子物体 Tag 设为 "DoorTrigger"
/// 
/// 玩家拿着钥匙靠近门时自动触发开门
/// </summary>
public class DoorKey : MonoBehaviour
{
    [Tooltip("开门后钥匙是否消失")]
    [SerializeField] private bool hideAfterUse = true;

    private bool isUsed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isUsed) return;

        if (!other.CompareTag("DoorTrigger")) return;

        // 找到门的控制器
        DoorController door = other.GetComponentInParent<DoorController>();
        if (door == null) return;

        isUsed = true;

        // 松开钥匙
        var grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();
        if (grab != null)
            grab.enabled = false;

        // 禁用物理
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // 开门
        door.OpenWithKey();

        // 隐藏钥匙
        if (hideAfterUse)
            Invoke(nameof(HideKey), 0.5f);

        Debug.Log("[钥匙] 已使用钥匙开门");
    }

    private void HideKey()
    {
        gameObject.SetActive(false);
    }
}
