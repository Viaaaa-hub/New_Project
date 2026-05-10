using UnityEngine;

public class ClockHintTrigger : MonoBehaviour
{
    [SerializeField] private GameObject hintText;
    [SerializeField] private float triggerDistance = 5f;

    private Transform playerCamera;

    private void Start()
    {
        if (Camera.main != null)
            playerCamera = Camera.main.transform;
        else
            Debug.LogWarning("找不到 Main Camera！");

        if (hintText != null)
            hintText.SetActive(false);
    }

    private void Update()
    {
        if (playerCamera == null || hintText == null) return;

        float distance = Vector3.Distance(transform.position, playerCamera.position);
        bool shouldShow = distance < triggerDistance;

        if (hintText.activeSelf != shouldShow)
        {
            hintText.SetActive(shouldShow);
        }
    }
}