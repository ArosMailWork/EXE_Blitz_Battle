using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpAnimUI : MonoBehaviour
{
    private Vector3 defaultPos;
    [SerializeField] private RectTransform destinationPos;

    private void Awake()
    {
        defaultPos = transform.position;
    }

    public void SmoothMovetoDestination(float arriveTime)
    {
        StartCoroutine(MoveCoroutine(arriveTime, destinationPos));
    }
    
    public void TeleToDefault()
    {
        this.transform.position = defaultPos;
    }

    private IEnumerator MoveCoroutine(float arriveTime, Transform endPoint)
    {
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;

        while (elapsedTime < arriveTime)
        {
            float t = elapsedTime / arriveTime;
            transform.position = Vector3.Lerp(startPos, endPoint.position, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Snap to the exact destination position to ensure accuracy
        transform.position = endPoint.position;
    }
}