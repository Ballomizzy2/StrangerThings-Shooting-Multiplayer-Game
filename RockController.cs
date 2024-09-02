using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockController : MonoBehaviour
{
    private Color tintColor;

    private void Start()
    {
        TintRockRandomly();
        StartCoroutine(AnimateRock(2f));
    }

    private void TintRockRandomly()
    {
        float rand = Random.Range(0.5f, 1f);
        GetComponent<MeshRenderer>().material.color = new Color(rand, rand, rand, 1);
    }

    private IEnumerator AnimateRock(float time)
    {
        Vector3 originalScale = transform.localScale * 0;
        Vector3 destinationScale = transform.localScale;

        transform.localScale = originalScale;

        float currentTime = 0.0f;

        do
        {
            transform.localScale = Vector3.Lerp(originalScale, destinationScale, currentTime / time);
            currentTime += Time.deltaTime;
            yield return null;
        } while (currentTime <= time);
    }
}
