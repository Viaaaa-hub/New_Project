using UnityEngine;
using System.Collections;

public class ColorPlate : MonoBehaviour
{
    [SerializeField] public BottleColor.ColorType plateColor;
    [SerializeField] private float bounceForce = 5f;
    [SerializeField] private Transform snapPoint;

    public bool IsCorrectBottlePlaced { get; private set; } = false;
    private GameObject placedBottle = null;

    private void Start()
    {
        if (snapPoint == null)
            snapPoint = transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        BottleColor bottle = other.GetComponent<BottleColor>();

        if (bottle == null)
        {
            bottle = other.GetComponentInParent<BottleColor>();
        }

        if (bottle == null) return;

        Debug.Log($"📦 [{plateColor}盘子] 检测到 {bottle.gameObject.name}(颜色={bottle.bottleColor})");

        if (IsCorrectBottlePlaced) return;

        if (bottle.bottleColor == plateColor)
        {
            Debug.Log($"✅ [{plateColor}盘子] 颜色匹配: {bottle.bottleColor} == {plateColor}");
            SnapBottle(other.gameObject);
        }
        else
        {
            Debug.Log($"❌ [{plateColor}盘子] 颜色不匹配: {bottle.bottleColor} != {plateColor}");
            BounceBottle(other);
        }
    }

    private void SnapBottle(GameObject bottle)
    {
        placedBottle = bottle;
        IsCorrectBottlePlaced = true;

        Rigidbody rb = bottle.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        Collider[] colliders = bottle.GetComponents<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        Collider[] childColliders = bottle.GetComponentsInChildren<Collider>();
        foreach (var col in childColliders)
        {
            col.enabled = false;
        }

        bottle.transform.position = snapPoint.position;
        bottle.transform.rotation = snapPoint.rotation;

        Debug.Log($"✅ {plateColor} 盘子配对成功!");

        PuzzleManager.Instance?.CheckAllPlates();
    }

    private void BounceBottle(Collider bottleCollider)
    {
        Rigidbody rb = bottleCollider.GetComponent<Rigidbody>();
        if (rb == null) return;

        if (rb.isKinematic) return;

        Vector3 direction = (bottleCollider.transform.position - transform.position).normalized;
        direction += Vector3.up * 0.8f;
        direction.Normalize();

        rb.velocity = Vector3.zero;
        rb.AddForce(direction * bounceForce, ForceMode.Impulse);

        Debug.Log($"❌ {plateColor} 盘子拒绝了错误颜色的酒瓶!");
    }
}