using UnityEngine;

[System.Serializable]
public class CamShakeData : MonoBehaviour
{
    [Range(0, 1)]
    [SerializeField] private float traumaMult;
    [SerializeField] private float traumaMag;
    [SerializeField] private float traumaRotMag;
    [SerializeField] private float traumaDepthMag;
    [SerializeField] private float traumaDecay;

    public float TraumaMult => traumaMult;
    public float TraumaMag => traumaMag;
    public float TraumaRotMag => traumaRotMag;
    public float TraumaDepthMag => traumaDepthMag;
    public float TraumaDecay => traumaDecay;
}