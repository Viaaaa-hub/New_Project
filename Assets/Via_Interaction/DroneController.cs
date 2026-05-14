using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DroneController : MonoBehaviour
{
    [Header("零件吸附点")]
    public Transform snapPoint0;
    public Transform snapPoint1;

    [Header("吸附范围")]
    public float attractionRadius = 1.5f;

    [Header("飞行设置")]
    public float takeoffDelay = 1.2f;
    public float animationDuration = 5f;
    public float fadeStartTime = 3f;

    [Header("XR Origin")]
    public Transform xrOrigin;
    public GameObject locomotionSystem;  // 新增这一行

    [Header("结尾 UI")]
    public Image fadePanel;
    public TextMeshProUGUI escapeText;
    public float fadeDuration = 1.5f;
    public float textDelay = 0.8f;

    [Header("音效（可不填）")]
    public AudioSource audioSource;
    public AudioClip snapSound;
    public AudioClip engineSound;

    private bool[] _snapped = new bool[2];
    private int _snappedCount = 0;
    private Animator _animator;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        Debug.Log($"[DroneController] Awake，Animator = {_animator}");

        if (fadePanel != null) SetAlpha(fadePanel, 0f);
        if (escapeText != null) SetTextAlpha(escapeText, 0f);
    }

    public Transform GetSnapPoint(int index)
    {
        return index == 0 ? snapPoint0 : snapPoint1;
    }

    public void RegisterPartSnapped(int slotIndex)
    {
        if (_snapped[slotIndex]) return;
        _snapped[slotIndex] = true;
        _snappedCount++;

        Debug.Log($"[DroneController] 收到吸附通知：slot {slotIndex}，当前已吸附数量 = {_snappedCount}");

        if (audioSource != null && snapSound != null)
            audioSource.PlayOneShot(snapSound);

        if (_snappedCount >= 2)
        {
            Debug.Log("[DroneController] 🚀 两件齐全，启动飞行流程");
            StartCoroutine(FlightSequence());
        }
    }

    private IEnumerator FlightSequence()
    {
        Debug.Log("[DroneController] FlightSequence 开始");

        yield return new WaitForSeconds(takeoffDelay);

        if (audioSource != null && engineSound != null)
        {
            audioSource.clip = engineSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        Debug.Log("[DroneController] 把 XR Origin 绑到无人机");
        // 禁用玩家移动
        if (locomotionSystem != null)
            locomotionSystem.SetActive(false);

        xrOrigin.SetParent(this.transform);

        Debug.Log("[DroneController] 触发 Takeoff 动画");
        _animator.SetTrigger("Takeoff");

        yield return new WaitForSeconds(fadeStartTime);

        Debug.Log("[DroneController] 开始渐黑");
        yield return StartCoroutine(FadeToBlack());

        yield return StartCoroutine(ShowEscapeText());

        if (audioSource != null) audioSource.Stop();
        xrOrigin.SetParent(null);

        Debug.Log("[DroneController] 🏁 成功逃脱！");
    }

    private IEnumerator FadeToBlack()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(fadePanel, Mathf.Clamp01(elapsed / fadeDuration));
            yield return null;
        }
        SetAlpha(fadePanel, 1f);
    }

    private IEnumerator ShowEscapeText()
    {
        yield return new WaitForSeconds(textDelay);
        float elapsed = 0f;
        float textFadeDuration = 1f;
        while (elapsed < textFadeDuration)
        {
            elapsed += Time.deltaTime;
            SetTextAlpha(escapeText, Mathf.Clamp01(elapsed / textFadeDuration));
            yield return null;
        }
        SetTextAlpha(escapeText, 1f);
    }

    private void SetAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    private void SetTextAlpha(TextMeshProUGUI tmp, float alpha)
    {
        if (tmp == null) return;
        Color c = tmp.color;
        c.a = alpha;
        tmp.color = c;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawSphere(transform.position, attractionRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
    }
}
