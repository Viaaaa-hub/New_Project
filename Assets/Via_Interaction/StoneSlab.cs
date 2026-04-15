using UnityEngine;

/// <summary>
/// 石板脚本 - 挂载到地上的石板物体上
/// 
/// 使用方式：
/// 1. 将此脚本挂载到石板物体上
/// 2. 石板需要有 Collider（不勾选 Is Trigger）
/// 3. PuzzleManager 会自动查找，不需要手动填
/// </summary>
public class StoneSlab : MonoBehaviour
{
    [Tooltip("谜题管理器（可不填，运行时自动查找）")]
    [SerializeField] private StoneSlabPuzzleManager puzzleManager;

    private bool isOccupied = false;

    private void Awake()
    {
        if (puzzleManager == null)
            puzzleManager = FindObjectOfType<StoneSlabPuzzleManager>();
    }

    /// <summary>
    /// 检测石头碰撞到石板
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (isOccupied) return;

        // 检查碰撞物体是否是石头
        PuzzleStone stone = collision.gameObject.GetComponent<PuzzleStone>();
        if (stone == null) return;
        if (stone.IsPlaced) return;

        // 检查是否匹配
        if (puzzleManager != null)
        {
            bool isCorrect = puzzleManager.CheckPlacement(collision.gameObject, gameObject);

            if (isCorrect)
            {
                isOccupied = true;
                stone.LockInPlace();
            }
        }
    }
}
