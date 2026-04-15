using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 简单传送器 - 直接调用即可传送玩家
/// 
/// 使用方式：
/// 1. 将此脚本挂载到空物体上
/// 2. 拖入 XR Origin 和目标位置
/// 3. 在任何事件中绑定 Teleport() 方法即可触发传送
/// 
/// 例如：钟表组装完成后传送
/// ClockPuzzleManager 的 On Clock Assembled 事件 → SimpleTeleport → Teleport
/// </summary>
public class SimpleTeleport : MonoBehaviour
{
    [Header("传送设置")]
    [Tooltip("XR Origin（玩家）")]
    [SerializeField] private Transform xrOrigin;

    [Tooltip("传送目标位置")]
    [SerializeField] private Transform teleportTarget;

    [Header("渐变效果（可选）")]
    [Tooltip("传送前屏幕渐黑时长（秒），设为0跳过")]
    [SerializeField] private float fadeOutDuration = 1f;

    [Tooltip("传送后屏幕渐亮时长（秒），设为0跳过")]
    [SerializeField] private float fadeInDuration = 1f;

    [Tooltip("屏幕遮罩 CanvasGroup（可选，不填则无渐变直接传送）")]
    [SerializeField] private CanvasGroup screenFade;

    [Header("音效")]
    [Tooltip("传送音效")]
    [SerializeField] private AudioClip teleportSound;

    [Header("事件")]
    [Tooltip("传送完成后触发")]
    public UnityEvent onTeleportComplete;

    private bool isTeleporting = false;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (screenFade != null)
        {
            screenFade.alpha = 0f;
            screenFade.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 执行传送（直接在任何事件中调用此方法）
    /// </summary>
    public void Teleport()
    {
        if (isTeleporting) return;
        StartCoroutine(TeleportSequence());
    }

    private IEnumerator TeleportSequence()
    {
        isTeleporting = true;

        if (teleportSound != null)
            audioSource.PlayOneShot(teleportSound);

        // 屏幕渐黑
        if (screenFade != null && fadeOutDuration > 0)
        {
            screenFade.gameObject.SetActive(true);
            yield return StartCoroutine(Fade(0f, 1f, fadeOutDuration));
        }

        // 传送
        if (xrOrigin != null && teleportTarget != null)
        {
            xrOrigin.position = teleportTarget.position;
            xrOrigin.rotation = teleportTarget.rotation;
            Debug.Log("[传送] 玩家已传送到目标位置");
        }

        yield return null;

        // 屏幕渐亮
        if (screenFade != null && fadeInDuration > 0)
        {
            yield return StartCoroutine(Fade(1f, 0f, fadeInDuration));
            screenFade.gameObject.SetActive(false);
        }

        isTeleporting = false;
        onTeleportComplete?.Invoke();
        Debug.Log("[传送] 传送完成");
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            screenFade.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        screenFade.alpha = to;
    }
}
