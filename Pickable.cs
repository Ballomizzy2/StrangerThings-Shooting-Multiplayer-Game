using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Pickable : MonoBehaviourPun
{
    [SerializeField]
    private PickableType pickableType;
    public void PickedUp()
    {
        Debug.Log("I was picked Up!");
        // vfx

        // sfx

        Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(ScaleOverTime(2));
    }



    public void Update()
    {
        //Animate();
    }

    IEnumerator ScaleOverTime(float time)
    {
        Vector3 originalScale = transform.localScale/2;
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

    public void Animate()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * 50);
        
        //float yWave = 2f * (Mathf.Sin(Time.deltaTime));
        //transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + yWave, transform.localPosition.z);
    }
}



public enum PickableType
{
    Gun, Eggos, Ammo, CassettePlayer, Coke, Pizza, HellfireTee
}
