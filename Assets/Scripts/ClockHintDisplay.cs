using UnityEngine;
using TMPro;

public class ClockHintDisplay : MonoBehaviour
{
    public TextMeshProUGUI hintText;
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.5f;
    public float triggerDistance = 5f;
    
    private CanvasGroup canvasGroup;
    private bool isPlayerNear = false;
    private float fadeTimer = 0f;
    private Transform playerTransform;

    private void Start()
    {
        // 尝试找到玩家
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
            Debug.Log("找到玩家: " + playerObject.name);
        }
        else
        {
            Debug.LogError("找不到标签为 'Player' 的物体！请检查玩家是否有 Player 标签");
        }

        // 初始化文本和CanvasGroup
        if (hintText == null)
        {
            Debug.LogError("ClockHintDisplay: 请在Inspector中绑定 Hint Text!");
            return;
        }

        canvasGroup = hintText.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = hintText.gameObject.AddComponent<CanvasGroup>();
            Debug.Log("自动添加 CanvasGroup 组件");
        }
        
        canvasGroup.alpha = 0f;
        Debug.Log("ClockHintDisplay 初始化完成");
    }

    private void Update()
    {
        if (hintText == null || canvasGroup == null) return;

        // 检查玩家距离
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            
            // 如果在范围内
            if (distanceToPlayer < triggerDistance)
            {
                if (!isPlayerNear)
                {
                    isPlayerNear = true;
                    fadeTimer = 0f;
                    Debug.Log("玩家进入范围，距离: " + distanceToPlayer);
                }
            }
            else
            {
                if (isPlayerNear)
                {
                    isPlayerNear = false;
                    fadeTimer = fadeOutDuration;
                    Debug.Log("玩家离开范围，距离: " + distanceToPlayer);
                }
            }
        }

        // 处理淡入淡出
        if (isPlayerNear)
        {
            fadeTimer += Time.deltaTime;
            if (fadeTimer >= fadeInDuration)
            {
                canvasGroup.alpha = 1f;
            }
            else
            {
                canvasGroup.alpha = fadeTimer / fadeInDuration;
            }
        }
        else
        {
            fadeTimer -= Time.deltaTime;
            if (fadeTimer <= 0)
            {
                canvasGroup.alpha = 0f;
            }
            else
            {
                canvasGroup.alpha = fadeTimer / fadeOutDuration;
            }
        }
    }

    // 备用方案：使用Collider触发
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Collider触发: 玩家进入!");
            isPlayerNear = true;
            fadeTimer = 0f;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // 保持玩家在范围内
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Collider触发: 玩家离开!");
            isPlayerNear = false;
            fadeTimer = fadeOutDuration;
        }
    }
}