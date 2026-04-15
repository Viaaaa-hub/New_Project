using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 钥匙开门控制器
/// 
/// 使用方式：
/// 1. 将此脚本挂载到门物体上
/// 2. 给门物体添加一个子物体作为触发区域（Box Collider，勾选 Is Trigger，Tag 设为 "DoorTrigger"）
/// 3. 钥匙物体需要挂载 DoorKey 脚本
/// 4. 在 Inspector 中设置门的开启角度
/// 
/// 注意：门的旋转轴默认是 Y 轴（左右推开），如果你的门是其他方向，修改 rotationAxis
/// </summary>
public class DoorController : MonoBehaviour
{
    [Header("门设置")]
    [Tooltip("门的开启角度")]
    [SerializeField] private float openAngle = 90f;

    [Tooltip("门的旋转轴（本地坐标）")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;

    [Tooltip("开门动画时长（秒）")]
    [SerializeField] private float openDuration = 1.5f;

    [Header("音效")]
    [Tooltip("开门音效")]
    [SerializeField] private AudioClip openSound;

    [Tooltip("钥匙插入音效")]
    [SerializeField] private AudioClip keyInsertSound;

    [Header("事件")]
    [Tooltip("门打开时触发")]
    public UnityEvent onDoorOpened;

    private bool isOpen = false;
    private bool isAnimating = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private AudioSource audioSource;

    private void Awake()
    {
        // 记录关闭状态的旋转
        closedRotation = transform.localRotation;
        openRotation = closedRotation * Quaternion.AngleAxis(openAngle, rotationAxis);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }
    }

    /// <summary>
    /// 用钥匙开门（由 DoorKey 脚本在进入触发区域时调用）
    /// </summary>
    public void OpenWithKey()
    {
        if (isOpen || isAnimating) return;

        if (keyInsertSound != null)
            audioSource.PlayOneShot(keyInsertSound);

        StartCoroutine(OpenDoor());
    }

    private IEnumerator OpenDoor()
    {
        isAnimating = true;

        // 等一下模拟钥匙插入
        yield return new WaitForSeconds(0.5f);

        if (openSound != null)
            audioSource.PlayOneShot(openSound);

        float elapsed = 0f;
        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / openDuration);
            transform.localRotation = Quaternion.Slerp(closedRotation, openRotation, t);
            yield return null;
        }

        transform.localRotation = openRotation;
        isOpen = true;
        isAnimating = false;

        onDoorOpened?.Invoke();
        Debug.Log("[门] 门已打开！");
    }
}
