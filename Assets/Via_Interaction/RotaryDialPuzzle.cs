using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 拨号盘密码系统（停顿自动确认）
/// 
/// 玩法：
/// 1. 点击钟表 → 指针转到下一个数字
/// 2. 停止点击超过设定时间 → 当前数字自动确认为输入
/// 3. 指针重置到12点，继续输入下一位
/// 4. 输入完整密码后自动验证
/// 
/// 使用方式：
/// 1. 将此脚本挂载到世界B的钟表物体上
/// 2. 钟表添加 XR Simple Interactable，Select 事件绑定 OnDialClicked
/// 3. 在 correctCode 中设置正确的密码序列
/// </summary>
public class RotaryDialPuzzle : MonoBehaviour
{
    [Header("密码设置")]
    [Tooltip("正确的密码序列（1-12的数字组合）")]
    [SerializeField] private List<int> correctCode = new List<int>();

    [Header("确认设置")]
    [Tooltip("停止点击多少秒后自动确认（秒）")]
    [SerializeField] private float confirmDelay = 2f;

    [Header("表盘指针")]
    [Tooltip("表盘指针物体")]
    [SerializeField] private Transform dialPointer;

    [Tooltip("指针旋转轴")]
    [SerializeField] private Vector3 rotationAxis = Vector3.forward;

    [Tooltip("指针旋转动画时长")]
    [SerializeField] private float rotationDuration = 0.4f;

    [Header("数字显示（可选）")]
    [Tooltip("显示当前指针指向数字的 TextMeshPro")]
    [SerializeField] private TMPro.TextMeshPro currentNumberText;

    [Tooltip("显示已确认输入序列的 TextMeshPro")]
    [SerializeField] private TMPro.TextMeshPro inputSequenceText;

    [Header("视觉反馈（可选）")]
    [Tooltip("确认时指针闪烁的颜色")]
    [SerializeField] private Color confirmFlashColor = Color.green;

    [Tooltip("指针的 Renderer（用于闪烁反馈）")]
    [SerializeField] private Renderer pointerRenderer;

    [Header("音效")]
    [Tooltip("指针转动音效")]
    [SerializeField] private AudioClip dialClickSound;

    [Tooltip("自动确认音效（提示玩家数字已锁定）")]
    [SerializeField] private AudioClip confirmSound;

    [Tooltip("密码正确音效")]
    [SerializeField] private AudioClip successSound;

    [Tooltip("密码错误音效")]
    [SerializeField] private AudioClip failSound;

    [Tooltip("重置音效")]
    [SerializeField] private AudioClip resetSound;

    [Header("事件")]
    [Tooltip("密码正确时触发")]
    public UnityEvent onCodeCorrect;

    [Tooltip("密码错误时触发")]
    public UnityEvent onCodeWrong;

    [Tooltip("每次自动确认时触发，参数为确认的数字")]
    public UnityEvent<int> onDigitConfirmed;

    // 内部状态
    private List<int> confirmedInput = new List<int>();
    private int currentPointerNumber = 0;
    private bool isRotating = false;
    private bool isSolved = false;
    private bool isWaitingForConfirm = false;
    private float lastClickTime = 0f;
    private Coroutine confirmCoroutine;
    private AudioSource audioSource;
    private Quaternion pointerBaseRotation;
    private Color originalPointerColor;

    public int CurrentPointerNumber => currentPointerNumber == 0 ? 12 : currentPointerNumber;
    public bool IsSolved => isSolved;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }

        if (dialPointer != null)
            pointerBaseRotation = dialPointer.localRotation;

        if (pointerRenderer != null)
            originalPointerColor = pointerRenderer.material.color;
    }

    private void Start()
    {
        UpdateDisplay();
    }

    /// <summary>
    /// 点击钟表，指针转到下一个数字（供 XR 交互事件调用）
    /// </summary>
    public void OnDialClicked()
    {
        if (isSolved || isRotating) return;

        // 取消之前的自动确认计时
        if (confirmCoroutine != null)
            StopCoroutine(confirmCoroutine);

        // 前进一个数字
        currentPointerNumber = (currentPointerNumber % 12) + 1;

        PlaySound(dialClickSound);

        if (dialPointer != null)
            StartCoroutine(AnimatePointer(currentPointerNumber));

        UpdateDisplay();

        Debug.Log($"[拨号盘] 指针转到: {currentPointerNumber}");

        // 重新开始计时
        lastClickTime = Time.time;
        confirmCoroutine = StartCoroutine(AutoConfirmCountdown());
    }

    private IEnumerator AutoConfirmCountdown()
    {
        isWaitingForConfirm = true;

        // 等待确认延迟时间
        yield return new WaitForSeconds(confirmDelay);

        // 确认当前数字
        ConfirmCurrentNumber();
    }

    private void ConfirmCurrentNumber()
    {
        if (currentPointerNumber == 0 || isSolved) return;

        isWaitingForConfirm = false;

        // 锁定数字
        int confirmedNumber = currentPointerNumber;
        confirmedInput.Add(confirmedNumber);

        PlaySound(confirmSound);

        // 视觉反馈：指针闪烁
        if (pointerRenderer != null)
            StartCoroutine(FlashPointer());

        onDigitConfirmed?.Invoke(confirmedNumber);

        Debug.Log($"[拨号盘] 自动确认: {confirmedNumber}, 已输入序列: {GetInputString()}");

        // 重置指针到12点，准备输入下一位
        currentPointerNumber = 0;
        StartCoroutine(ResetPointerAfterDelay());

        UpdateDisplay();

        // 检查密码
        CheckCode();
    }

    private IEnumerator ResetPointerAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);

        if (dialPointer != null && !isSolved)
            StartCoroutine(AnimatePointer(0));
    }

    private void CheckCode()
    {
        for (int i = 0; i < confirmedInput.Count; i++)
        {
            if (i >= correctCode.Count || confirmedInput[i] != correctCode[i])
            {
                StartCoroutine(WrongCodeSequence());
                return;
            }
        }

        if (confirmedInput.Count == correctCode.Count)
        {
            StartCoroutine(CorrectCodeSequence());
        }
    }

    private IEnumerator CorrectCodeSequence()
    {
        isSolved = true;
        yield return new WaitForSeconds(0.5f);

        PlaySound(successSound);
        onCodeCorrect?.Invoke();

        Debug.Log("[拨号盘] 密码正确！");
    }

    private IEnumerator WrongCodeSequence()
    {
        yield return new WaitForSeconds(0.3f);

        PlaySound(failSound);
        onCodeWrong?.Invoke();

        Debug.Log($"[拨号盘] 密码错误！输入了: {GetInputString()}");

        yield return new WaitForSeconds(1f);

        ResetInput();
    }

    /// <summary>
    /// 重置所有输入
    /// </summary>
    public void ResetInput()
    {
        if (confirmCoroutine != null)
            StopCoroutine(confirmCoroutine);

        confirmedInput.Clear();
        currentPointerNumber = 0;
        isWaitingForConfirm = false;

        PlaySound(resetSound);

        if (dialPointer != null)
            StartCoroutine(AnimatePointer(0));

        UpdateDisplay();

        Debug.Log("[拨号盘] 已重置");
    }

    // ===================== 动画 =====================

    private IEnumerator AnimatePointer(int targetNumber)
    {
        isRotating = true;

        Quaternion startRotation = dialPointer.localRotation;
        float targetAngle = -targetNumber * 30f;
        Quaternion targetRotation = pointerBaseRotation * Quaternion.AngleAxis(targetAngle, rotationAxis);

        float elapsed = 0f;
        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / rotationDuration);
            dialPointer.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        dialPointer.localRotation = targetRotation;
        isRotating = false;
    }

    private IEnumerator FlashPointer()
    {
        if (pointerRenderer == null) yield break;

        Material mat = pointerRenderer.material;

        // 闪烁两次
        for (int i = 0; i < 2; i++)
        {
            mat.color = confirmFlashColor;
            yield return new WaitForSeconds(0.15f);
            mat.color = originalPointerColor;
            yield return new WaitForSeconds(0.15f);
        }
    }

    // ===================== 显示 =====================

    private void UpdateDisplay()
    {
        if (currentNumberText != null)
        {
            currentNumberText.text = currentPointerNumber == 0 ? "-" : currentPointerNumber.ToString();
        }

        if (inputSequenceText != null)
        {
            string confirmed = confirmedInput.Count > 0 ? GetInputString() : "";
            string current = currentPointerNumber > 0 ? $" [{currentPointerNumber}?]" : "";
            inputSequenceText.text = confirmed + current;

            if (string.IsNullOrEmpty(inputSequenceText.text))
                inputSequenceText.text = "等待输入...";
        }
    }

    private string GetInputString()
    {
        return string.Join(" - ", confirmedInput);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}
