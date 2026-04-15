using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 石板谜题管理器
/// 
/// 四块石头一一对应放在四块石板上，全部正确后显示浮空照片线索
/// 
/// 使用方式：
/// 1. 将此脚本挂载到一个空物体上
/// 2. 在 stonePairs 中配置每对石头和石板
/// 3. 把照片物体拖入 photoClue 槽位（初始会自动隐藏）
/// 4. 石板物体需要挂载 StoneSlab 脚本
/// 5. 石头物体需要挂载 PuzzleStone 脚本
/// </summary>
public class StoneSlabPuzzleManager : MonoBehaviour
{
    [Header("石头与石板配对")]
    [Tooltip("每对石头和石板的配置")]
    [SerializeField] private List<StonePair> stonePairs = new List<StonePair>();

    [Header("照片线索")]
    [Tooltip("浮空照片物体（Quad 或 Plane，贴上照片材质）")]
    [SerializeField] private GameObject photoClue;

    [Header("音效")]
    [Tooltip("放对时的音效")]
    [SerializeField] private AudioClip correctSound;

    [Tooltip("照片出现时的音效")]
    [SerializeField] private AudioClip photoRevealSound;

    [Header("事件")]
    [Tooltip("全部放对时触发")]
    public UnityEvent onPuzzleComplete;

    [Tooltip("每放对一块时触发，参数为已完成数量")]
    public UnityEvent<int> onStoneCorrect;

    private int correctCount = 0;
    private AudioSource audioSource;

    /// <summary>
    /// 已正确放置的数量
    /// </summary>
    public int CorrectCount => correctCount;

    /// <summary>
    /// 是否全部完成
    /// </summary>
    public bool IsComplete => correctCount >= stonePairs.Count;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // 隐藏照片
        if (photoClue != null)
            photoClue.SetActive(false);
    }

    /// <summary>
    /// 检查石头是否放在了正确的石板上（由 StoneSlab 调用）
    /// </summary>
    public bool CheckPlacement(GameObject stone, GameObject slab)
    {
        foreach (var pair in stonePairs)
        {
            if (pair.stone == stone && pair.slab == slab)
            {
                // 匹配正确
                correctCount++;

                PlaySound(correctSound);
                onStoneCorrect?.Invoke(correctCount);

                Debug.Log($"[石板谜题] 放置正确！({correctCount}/{stonePairs.Count})");

                // 检查是否全部完成
                if (IsComplete)
                {
                    RevealPhoto();
                }

                return true;
            }
        }

        Debug.Log("[石板谜题] 放置错误，不匹配");
        return false;
    }

    private void RevealPhoto()
    {
        if (photoClue != null)
        {
            photoClue.SetActive(true);
            PlaySound(photoRevealSound);
        }

        onPuzzleComplete?.Invoke();
        Debug.Log("[石板谜题] 全部完成！照片线索已显示");
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}

/// <summary>
/// 石头与石板的配对
/// </summary>
[System.Serializable]
public class StonePair
{
    [Tooltip("石头物体")]
    public GameObject stone;

    [Tooltip("对应的石板物体")]
    public GameObject slab;
}
