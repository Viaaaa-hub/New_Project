using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    [SerializeField] private ColorPlate[] plates;
    [SerializeField] private GameObject rewardPart;  // 拖入齿轮

    private bool puzzleSolved = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (rewardPart != null)
            rewardPart.SetActive(false);
    }

    public void CheckAllPlates()
    {
        if (puzzleSolved) return;

        foreach (var plate in plates)
        {
            if (!plate.IsCorrectBottlePlaced)
                return;
        }

        puzzleSolved = true;
        SpawnReward();
    }

    private void SpawnReward()
    {
        Debug.Log("谜题完成! 齿轮出现!");
        if (rewardPart != null)
            rewardPart.SetActive(true);
    }
}