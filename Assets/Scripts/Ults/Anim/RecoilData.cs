using UnityEngine;

[System.Serializable]
public class RecoilData: MonoBehaviour
{
    [SerializeField] private float recoilX;
    [SerializeField] private float recoilY;
    [SerializeField] private float recoilZ;
    [SerializeField] private float recoilScale = 1;
    [SerializeField] private float snappiness_Hipfire;
    [SerializeField] private float firerate;

    public float RecoilX => recoilX;
    public float RecoilY => recoilY;
    public float RecoilZ => recoilZ;
    public float RecoilScale => recoilScale;
    public float Snappiness_Hipfire => snappiness_Hipfire;
    public float FireRate => firerate;
}