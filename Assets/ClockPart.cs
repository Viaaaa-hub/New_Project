using UnityEngine;

/// <summary>
/// 钟表零件脚本 - 挂载到场景中可拾取的零件物体上（齿轮、时针、分针）
/// 
/// 使用方式：
/// 1. 将此脚本挂载到零件物体上
/// 2. 设置零件类型（Gear / HourHand / MinuteHand）
/// 3. 给零件物体添加 XR Grab Interactable 组件（使其可被 VR 手柄/手势拾取）
/// 4. 给零件物体添加 Rigidbody 和 Collider
/// 5. 在钟表的插槽位置放一个带 Trigger Collider 的空物体，Tag 设为 "ClockSlot"
/// 
/// 工作原理：
/// 玩家拾取零件 → 将零件靠近钟表插槽 → 触发安装 → 零件吸附到位并锁定
/// </summary>
public class ClockPart : MonoBehaviour
{
    [Header("零件设置")]
    [Tooltip("此零件的类型")]
    [SerializeField] private ClockPartType partType;

    [Tooltip("零件安装时的吸附速度")]
    [SerializeField] private float snapSpeed = 5f;

    [Tooltip("钟表谜题管理器（可不填，运行时自动查找）")]
    [SerializeField] private ClockPuzzleManager clockManager;

    private bool isInstalled = false;
    private Transform targetSlot = null;
    private Rigidbody rb;

    // 回调函数
    private System.Action onInstalledCallback;
    private System.Action onReturnedCallback;

    /// <summary>
    /// 零件类型（外部可读取）
    /// </summary>
    public ClockPartType PartType => partType;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (clockManager == null)
            clockManager = FindObjectOfType<ClockPuzzleManager>();
    }

    /// <summary>
    /// 当零件进入插槽触发区域时自动安装
    /// 插槽物体需要有 Trigger Collider 且 Tag 为 "ClockSlot"
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (isInstalled) return;

        // 检查是否进入了对应的插槽
        if (!other.CompareTag("ClockSlot")) return;

        // 通过插槽名称或子物体匹配零件类型
        ClockSlot slot = other.GetComponent<ClockSlot>();
        if (slot == null || slot.AcceptedPartType != partType) return;

        Install(other.transform);
    }

    private void Install(Transform slot)
    {
        isInstalled = true;
        targetSlot = slot;

        // 禁用 XR 抓取（不再允许拾起）
        var grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            // 强制松手
            var interactionManager = FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>();
            if (interactionManager != null && grabInteractable.isSelected)
            {
                // 取消当前选中
                grabInteractable.enabled = false;
            }
            else
            {
                grabInteractable.enabled = false;
            }
        }

        // 禁用物理
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // 禁用碰撞体
        var collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

        // 通知钟表管理器
        if (clockManager != null)
            clockManager.InstallPart(partType);

        // 开始吸附动画
        StartCoroutine(SnapToSlot());
    }

    private System.Collections.IEnumerator SnapToSlot()
    {
        while (Vector3.Distance(transform.position, targetSlot.position) > 0.001f)
        {
            transform.position = Vector3.Lerp(transform.position, targetSlot.position, Time.deltaTime * snapSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetSlot.rotation, Time.deltaTime * snapSpeed);
            yield return null;
        }

        // 精确对齐
        transform.position = targetSlot.position;
        transform.rotation = targetSlot.rotation;
        transform.SetParent(targetSlot);

        // 安装完成后隐藏零件（由钟表上的展示模型替代显示）
        gameObject.SetActive(false);

        // 触发安装成功回调
        onInstalledCallback?.Invoke();
    }

    /// <summary>
    /// 重置零件状态（用于克隆时恢复到未安装状态）
    /// </summary>
    public void ResetState()
    {
        isInstalled = false;
        targetSlot = null;
        
        // 重新启用 XR 抓取
        var grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.enabled = true;
        }

        // 重新启用物理
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // 重新启用碰撞体
        var collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = true;

        // 确保物体是激活的
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);
    }

    /// <summary>
    /// 设置安装成功时的回调
    /// </summary>
    public void SetOnInstalledCallback(System.Action callback)
    {
        onInstalledCallback = callback;
    }

    /// <summary>
    /// 设置需要回收时的回调
    /// </summary>
    public void SetOnReturnedCallback(System.Action callback)
    {
        onReturnedCallback = callback;
    }

    /// <summary>
    /// 触发返回回调（当玩家主动收回该物品或未成功交互时调用）
    /// </summary>
    public void TriggerReturnedCallback()
    {
        onReturnedCallback?.Invoke();
    }
}
