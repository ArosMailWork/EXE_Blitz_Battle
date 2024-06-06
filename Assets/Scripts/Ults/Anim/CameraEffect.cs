using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
//using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class CameraEffect : MonoBehaviour
{
    public static CameraEffect Instance { get; private set; }
    
    //public Volume volume;
    private LensDistortion lensDistortion;
    [SerializeField] Camera _camera;
    [ReadOnly] public float default_FOV; //If allow player change FOV when Play, make sure u save this one again XD
    [FormerlySerializedAs("lerp_time")]
    [Space(5)]
    [SerializeField] float lerp_time_Start = 0.2f;
    [SerializeField] float lerp_time_End = 0.25f;
    [SerializeField] float LD_Amount = -0.2f;
    [SerializeField] float Fov_Amount = 8f;

    bool delayEnd;
    
    private void Awake()
    {
        Instance = this;
        //volume.profile.TryGet(out lensDistortion);
    }

    void Start()
    {
        default_FOV = _camera.fieldOfView;
        lensDistortion.intensity.value = 0f;
    }

    private Coroutine lensDistortionCoroutine;
    private Coroutine cameraFOVCoroutine;

    #region Base
    public void Sprint()
    {
        CancelFOVChange();
        
        lensDistortionCoroutine = StartCoroutine(StartSmoothDamp(lensDistortion.intensity.value,
            LD_Amount, lerp_time_Start, (lerpedValue) => lensDistortion.intensity.value = lerpedValue));
        cameraFOVCoroutine = StartCoroutine(StartSmoothDamp(_camera.fieldOfView,
            default_FOV + Fov_Amount, lerp_time_Start, (lerpedValue) => _camera.fieldOfView = lerpedValue));
    }
    public void EndSprint()
    {
        CancelFOVChange();

        lensDistortionCoroutine = StartCoroutine(StartSmoothDamp(lensDistortion.intensity.value,
            0f, lerp_time_End, (lerpedValue) => lensDistortion.intensity.value = lerpedValue));
        cameraFOVCoroutine = StartCoroutine(StartSmoothDamp(_camera.fieldOfView,
            default_FOV, lerp_time_End, (lerpedValue) => _camera.fieldOfView = lerpedValue));
    }
    public void LODEffectStart_Custom(float time_Start, float LDAmount, float FovAmount)
    {
        CancelFOVChange();
        
        
        lensDistortionCoroutine = StartCoroutine(StartSmoothDamp(lensDistortion.intensity.value,
            LDAmount, lerp_time_Start, (lerpedValue) => lensDistortion.intensity.value = lerpedValue));
        cameraFOVCoroutine = StartCoroutine(StartSmoothDamp(_camera.fieldOfView,
            default_FOV + Fov_Amount, lerp_time_Start, (lerpedValue) => _camera.fieldOfView = lerpedValue));
    }
    public void LODEffectEnd_Custom(float time_End)
    {
        if (lensDistortionCoroutine != null)
            StopCoroutine(lensDistortionCoroutine);
        if (cameraFOVCoroutine != null)
            StopCoroutine(cameraFOVCoroutine);

        lensDistortionCoroutine = StartCoroutine(StartSmoothDamp(lensDistortion.intensity.value,
            0f, time_End, (lerpedValue) => lensDistortion.intensity.value = lerpedValue));
        cameraFOVCoroutine = StartCoroutine(StartSmoothDamp(_camera.fieldOfView,
            default_FOV, time_End, (lerpedValue) => _camera.fieldOfView = lerpedValue));
    }
    //------------------------Calculate and Ults---------------------------\\
    private IEnumerator StartSmoothDamp(float startValue, float targetValue, float duration, System.Action<float> setValue)
    {
        float currentVelocity = 0f;

        delayEnd = true;

        while (Mathf.Abs(targetValue - startValue) > 0.01f)
        {
            // Smoothly damp the startValue towards the targetValue
            startValue = Mathf.SmoothDamp(startValue, targetValue, ref currentVelocity, duration);

            // Update the value using the provided setValue action
            setValue(startValue);

            yield return null;
        }

        // Ensure the final value is set correctly
        setValue(targetValue);
    }
    private void CancelFOVChange()
    {
        if (lensDistortionCoroutine != null)
            StopCoroutine(lensDistortionCoroutine);
        if (cameraFOVCoroutine != null)
            StopCoroutine(cameraFOVCoroutine);
    }
    #endregion

    #region Quick Effect

    public void BlinkEffect()
    {
        Debug.Log("Blinked");
        LODEffectStart_Custom(0.02f, -0.025f, 0.25f);
        Invoke(nameof(EndLODEffect), 0.075f);
    }

    private void EndLODEffect()
    {
        LODEffectEnd_Custom(0.02f);
    }

    #endregion
}
