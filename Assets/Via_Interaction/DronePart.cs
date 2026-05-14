using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class DronePart : MonoBehaviour
{
    [Header("配置")]
    public DroneController droneController;
    public int slotIndex = 0;

    [Header("吸附参数")]
    public float attractSpeed = 6f;
    public float snapThreshold = 0.08f;

    private XRGrabInteractable _grab;
    private Rigidbody _rb;
    private bool _isSnapped = false;
    private bool _isAttracting = false;

    void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
        _rb = GetComponent<Rigidbody>();
        _grab.selectExited.AddListener(OnReleased);
    }

    void OnDestroy()
    {
        if (_grab != null)
            _grab.selectExited.RemoveListener(OnReleased);
    }

    void Update()
    {
        if (_isSnapped) return;
        if (droneController == null) return;

        if (_isAttracting)
        {
            Transform snapPoint = droneController.GetSnapPoint(slotIndex);

            transform.position = Vector3.Lerp(
                transform.position, snapPoint.position, attractSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(
                transform.rotation, snapPoint.rotation, attractSpeed * Time.deltaTime);

            float distToSnap = Vector3.Distance(transform.position, snapPoint.position);

            // 每帧打印当前距离（前几帧用于诊断）
            if (Time.frameCount % 30 == 0)
                Debug.Log($"[DronePart {slotIndex}] 吸附中，距离吸附点 = {distToSnap}");

            if (distToSnap < snapThreshold)
            {
                SnapToDrone();
            }
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        if (_isSnapped) return;

        float dist = Vector3.Distance(transform.position, droneController.transform.position);
        Debug.Log($"[DronePart {slotIndex}] 松手！距离无人机 = {dist}，吸附范围 = {droneController.attractionRadius}");

        if (dist <= droneController.attractionRadius)
        {
            Debug.Log($"[DronePart {slotIndex}] 在范围内，开始吸附");
            StartAttracting();
        }
        else
        {
            Debug.Log($"[DronePart {slotIndex}] 超出范围，不吸附");
        }
    }

    private void StartAttracting()
{
    _isAttracting = true;

    // 脱离任何父物体（防止还被手柄拽着）
    transform.SetParent(null);

    if (!_rb.isKinematic)
    {
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
    }
    _rb.isKinematic = true;

    _grab.enabled = false;
}

    private void SnapToDrone()
    {
        _isSnapped = true;
        _isAttracting = false;

        Transform snapPoint = droneController.GetSnapPoint(slotIndex);
        transform.position = snapPoint.position;
        transform.rotation = snapPoint.rotation;

        transform.SetParent(droneController.transform, worldPositionStays: true);

        Debug.Log($"[DronePart {slotIndex}] ✅ 吸附完成！通知 DroneController");
        droneController.RegisterPartSnapped(slotIndex);

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Debug.Log($"[DronePart {slotIndex}] 找到 {renderers.Length} 个 Renderer，全部隐藏");
        foreach (var r in renderers)
        {
            r.enabled = false;
        }
    }
}
