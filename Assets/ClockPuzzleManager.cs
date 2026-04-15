using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 钟表谜题管理器（完整系统）
/// 
/// 流程：
/// 1. 玩家在场景中找到齿轮和两根指针（三个零件）
/// 2. 将零件放入钟表对应的插槽
/// 3. 三个零件全部安装后，钟表变为可点击
/// 4. 每次点击时针转到下一个小时
/// 5. 特定时间会触发对应的线索物体显示
/// 
/// 使用方式：
/// 1. 将此脚本挂载到钟表主体物体上
/// 2. 在 Inspector 中配置三个插槽的 Transform（零件安装位置）
/// 3. 在 hourlyClues 数组中配置每个小时对应的线索物体
/// 4. 场景中的零件物体需要挂载 ClockPart 脚本
/// </summary>
public class ClockPuzzleManager : MonoBehaviour
{
    // ===================== 零件插槽配置 =====================

    [Header("零件插槽（钟表上的安装位置）")]
    [Tooltip("齿轮安装位置")]
    [SerializeField] private Transform gearSlot;

    [Tooltip("时针安装位置")]
    [SerializeField] private Transform hourHandSlot;

    [Tooltip("分针安装位置")]
    [SerializeField] private Transform minuteHandSlot;

    // ===================== 时针旋转配置 =====================

    [Header("时针旋转设置")]
    [Tooltip("时针物体（安装后显示的时针，初始隐藏）")]
    [SerializeField] private Transform hourHandDisplay;

    [Tooltip("分针物体（安装后显示的分针，初始隐藏）")]
    [SerializeField] private Transform minuteHandDisplay;

    [Tooltip("齿轮物体（安装后显示的齿轮，初始隐藏）")]
    [SerializeField] private GameObject gearDisplay;

    [Tooltip("时针旋转轴（本地坐标）")]
    [SerializeField] private Vector3 rotationAxis = Vector3.forward;

    [Tooltip("每格旋转角度（30° = 360°/12）")]
    [SerializeField] private float degreesPerHour = 30f;

    [Tooltip("旋转动画时长（秒）")]
    [SerializeField] private float rotationDuration = 0.3f;

    // ===================== 线索配置 =====================

    [Header("各时间点的线索")]
    [Tooltip("配置每个小时触发的线索物体")]
    [SerializeField] private List<HourlyClue> hourlyClues = new List<HourlyClue>();

    // ===================== 音效配置 =====================

    [Header("音效")]
    [Tooltip("零件安装成功音效")]
    [SerializeField] private AudioClip partInstalledSound;

    [Tooltip("钟表组装完成音效")]
    [SerializeField] private AudioClip clockReadySound;

    [Tooltip("时针转动音效")]
    [SerializeField] private AudioClip tickSound;

    [Tooltip("线索出现音效")]
    [SerializeField] private AudioClip clueRevealSound;

    // ===================== 事件 =====================

    [Header("事件")]
    [Tooltip("钟表组装完成时触发")]
    public UnityEvent onClockAssembled;

    [Tooltip("时间切换时触发，参数为当前小时(1-12)")]
    public UnityEvent<int> onHourChanged;

    [Tooltip("线索显示时触发")]
    public UnityEvent<GameObject> onClueRevealed;

    // ===================== 内部状态 =====================

    private bool hasGear = false;
    private bool hasHourHand = false;
    private bool hasMinuteHand = false;
    private bool isClockReady = false;
    private bool isRotating = false;
    private int currentHour = 0; // 0=12点, 1=1点, ..., 11=11点

    private AudioSource audioSource;
    private Quaternion hourHandBaseRotation;

    /// <summary>
    /// 钟表是否已组装完成
    /// </summary>
    public bool IsClockReady => isClockReady;

    /// <summary>
    /// 当前小时（1-12）
    /// </summary>
    public int CurrentHour => currentHour == 0 ? 12 : currentHour;

    // ===================== 初始化 =====================

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D 空间音效
        }

        // 隐藏钟表上的展示用指针和齿轮（等安装后再显示）
        if (hourHandDisplay != null) hourHandDisplay.gameObject.SetActive(false);
        if (minuteHandDisplay != null) minuteHandDisplay.gameObject.SetActive(false);
        if (gearDisplay != null) gearDisplay.SetActive(false);

        // 记录时针初始旋转
        if (hourHandDisplay != null)
            hourHandBaseRotation = hourHandDisplay.localRotation;

        // 初始时隐藏所有线索物体
        HideAllClues();
    }

    // ===================== 零件安装 =====================

    /// <summary>
    /// 安装零件（由 ClockPart 脚本在零件进入插槽时调用）
    /// </summary>
    public void InstallPart(ClockPartType partType)
    {
        switch (partType)
        {
            case ClockPartType.Gear:
                if (hasGear) return;
                hasGear = true;
                if (gearDisplay != null) gearDisplay.SetActive(true);
                Debug.Log("[钟表谜题] 齿轮已安装");
                break;

            case ClockPartType.HourHand:
                if (hasHourHand) return;
                hasHourHand = true;
                if (hourHandDisplay != null) hourHandDisplay.gameObject.SetActive(true);
                Debug.Log("[钟表谜题] 时针已安装");
                break;

            case ClockPartType.MinuteHand:
                if (hasMinuteHand) return;
                if (minuteHandDisplay != null) minuteHandDisplay.gameObject.SetActive(true);
                hasMinuteHand = true;
                Debug.Log("[钟表谜题] 分针已安装");
                break;
        }

        PlaySound(partInstalledSound);
        CheckClockReady();
    }

    private void CheckClockReady()
    {
        if (isClockReady) return;

        if (hasGear && hasHourHand && hasMinuteHand)
        {
            isClockReady = true;
            PlaySound(clockReadySound);
            onClockAssembled?.Invoke();
            Debug.Log("[钟表谜题] 钟表组装完成！现在可以点击切换时间");
        }
    }

    // ===================== 时间切换 =====================

    /// <summary>
    /// 点击钟表，切换到下一个小时（供 XR 交互事件调用）
    /// </summary>
    public void OnClockClicked()
    {
        if (!isClockReady)
        {
            Debug.Log("[钟表谜题] 钟表尚未组装完成，无法操作");
            return;
        }

        if (isRotating) return;

        // 前进一小时
        currentHour = (currentHour + 1) % 12;

        PlaySound(tickSound);

        // 旋转时针
        StartCoroutine(RotateHourHand());
    }

    private IEnumerator RotateHourHand()
    {
        isRotating = true;

        Quaternion startRotation = hourHandDisplay.localRotation;
        Quaternion targetRotation = hourHandBaseRotation *
            Quaternion.AngleAxis(-currentHour * degreesPerHour, rotationAxis);

        float elapsed = 0f;
        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / rotationDuration);
            hourHandDisplay.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        hourHandDisplay.localRotation = targetRotation;
        isRotating = false;

        // 触发时间变更事件
        onHourChanged?.Invoke(CurrentHour);

        // 检查并显示对应线索
        RevealCluesForCurrentHour();
    }

    // ===================== 线索系统 =====================

    private void RevealCluesForCurrentHour()
    {
        // 先隐藏所有线索
        HideAllClues();

        // 显示当前时间对应的线索
        foreach (var hourlyClue in hourlyClues)
        {
            if (hourlyClue.hour == CurrentHour)
            {
                foreach (var clueObj in hourlyClue.clueObjects)
                {
                    if (clueObj != null)
                    {
                        clueObj.SetActive(true);
                        onClueRevealed?.Invoke(clueObj);
                    }
                }

                PlaySound(clueRevealSound);
                Debug.Log($"[钟表谜题] {CurrentHour}点 - 显示 {hourlyClue.clueObjects.Count} 个线索");
                return;
            }
        }
    }

    private void HideAllClues()
    {
        foreach (var hourlyClue in hourlyClues)
        {
            foreach (var clueObj in hourlyClue.clueObjects)
            {
                if (clueObj != null)
                    clueObj.SetActive(false);
            }
        }
    }

    // ===================== 工具方法 =====================

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    /// <summary>
    /// 重置钟表到初始状态
    /// </summary>
    public void ResetClock()
    {
        currentHour = 0;
        if (hourHandDisplay != null)
            hourHandDisplay.localRotation = hourHandBaseRotation;
        HideAllClues();
    }
}

// ===================== 数据结构 =====================

/// <summary>
/// 零件类型枚举
/// </summary>
public enum ClockPartType
{
    Gear,       // 齿轮
    HourHand,   // 时针
    MinuteHand  // 分针
}

/// <summary>
/// 每个小时对应的线索配置
/// </summary>
[Serializable]
public class HourlyClue
{
    [Tooltip("触发线索的时间（1-12）")]
    public int hour;

    [Tooltip("该时间点显示的线索物体列表")]
    public List<GameObject> clueObjects = new List<GameObject>();
}
