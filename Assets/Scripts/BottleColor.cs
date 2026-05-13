using UnityEngine;

public class BottleColor : MonoBehaviour
{
    public enum ColorType { Red, Yellow, Blue }

    [SerializeField] public ColorType bottleColor;
}