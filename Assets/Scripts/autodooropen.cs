using UnityEngine;

public class AutoDoorOpen : MonoBehaviour
{
    [SerializeField] private Transform doorPivot;       // 合页（要旋转的物体）
    [SerializeField] private float triggerDistance = 3f; // 触发距离
    [SerializeField] private float openAngle = 75f;      // 打开角度
    [SerializeField] private float openSpeed = 2f;       // 开门速度
    [SerializeField] private Vector3 rotationAxis = Vector3.up;  // 旋转轴（Y轴 = 左右开）

    private Transform playerCamera;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool hasOpened = false;

    private void Start()
    {
        if (Camera.main != null)
            playerCamera = Camera.main.transform;

        if (doorPivot != null)
        {
            closedRotation = doorPivot.localRotation;
            // 计算打开后的旋转
            openRotation = closedRotation * Quaternion.AngleAxis(openAngle, rotationAxis);
        }
    }

    private void Update()
    {
        if (playerCamera == null || doorPivot == null) return;

        // 距离检测
        float distance = Vector3.Distance(transform.position, playerCamera.position);

        if (!hasOpened && distance < triggerDistance)
        {
            hasOpened = true;  // 标记为已打开，永远不再关闭
        }

        // 如果已经触发过，就一直开门方向旋转
        if (hasOpened)
        {
            doorPivot.localRotation = Quaternion.Slerp(
                doorPivot.localRotation,
                openRotation,
                Time.deltaTime * openSpeed
            );
        }
    }
}