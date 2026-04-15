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

    [Header("成功提示文字")]
    [Tooltip("密码正确时要显示的文字物体（例如 'Which planet will u be next time...'）")]
    [SerializeField] private GameObject successTextObject;

    [Tooltip("成功文字的 TextMeshProUGUI 组件（Canvas 下的 UI 文字，用于淡入和动态修改文字内容）")]
    [SerializeField] private TMPro.TextMeshProUGUI successTextMesh;

    [Tooltip("成功时显示的文字内容（留空则使用 TextMeshPro 中已有的文字）")]
    [TextArea(2, 4)]
    [SerializeField] private string successMessage = "Which planet will u be next time...";

    [Tooltip("文字淡入时长（秒），0 表示直接显示")]
    [SerializeField] private float textFadeInDuration = 1.5f;

    [Tooltip("成功音效播放后延迟多久显示文字（秒）")]
    [SerializeField] private float textShowDelay = 0.3f;

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

        // 启动时确保成功文字是隐藏的
        if (successTextObject != null)
            successTextObject.SetActive(false);
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

        // 显示成功提示文字
        StartCoroutine(ShowSuccessText());
    }

    /// <summary>
    /// 在成功音效后显示提示文字（带淡入效果）
    /// </summary>
    private IEnumerator ShowSuccessText()
    {
        // 等待一小段时间，让音效先响起
        yield return new WaitForSeconds(textShowDelay);

        // 如果指定了 TextMeshPro 组件，且填写了文字内容，则更新文字
        if (successTextMesh != null && !string.IsNullOrEmpty(successMessage))
        {
            successTextMesh.text = successMessage;
        }

        // 激活文字物体
        if (successTextObject != null)
        {
            successTextObject.SetActive(true);

            // 如果设置了淡入时长且有 TextMeshPro，则做淡入动画
            if (textFadeInDuration > 0f && successTextMesh != null)
            {
                Color startColor = successTextMesh.color;
                startColor.a = 0f;
                successTextMesh.color = startColor;

                float elapsed = 0f;
                while (elapsed < textFadeInDuration)
                {
                    elapsed += Time.deltaTime;
                    float alpha = Mathf.Clamp01(elapsed / textFadeInDuration);
                    Color c = successTextMesh.color;
                    c.a = alpha;
                    successTextMesh.color = c;
                    yield return null;
                }

                Color finalColor = successTextMesh.color;
                finalColor.a = 1f;
                successTextMesh.color = finalColor;
            }
        }
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