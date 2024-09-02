using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerCameraShake : MonoBehaviour
{
    private CinemachineBasicMultiChannelPerlin m_MultiChannelPerlin;
    [SerializeField]
    private CinemachineVirtualCamera deathCam;


    private void Start()
    {
        m_MultiChannelPerlin = this.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        deathCam.m_Priority = 9;
    }

   
    public void Shake(float seconds, float amplitude, bool isShootShake = false)
    {
        if(!isShootShake)
            StartCoroutine(IShake(seconds, amplitude));
        else
            StartCoroutine(IShootShake(seconds, amplitude));

    }

    private IEnumerator IShake(float seconds, float amplitude)
    {
        if (m_MultiChannelPerlin == null)
            yield return null;

        m_MultiChannelPerlin.m_FrequencyGain = 1;
        m_MultiChannelPerlin.m_AmplitudeGain = amplitude * 10;
        m_MultiChannelPerlin.m_PivotOffset = new Vector3(20,10,1) * amplitude;
        yield return new WaitForSeconds(seconds);
        m_MultiChannelPerlin.m_PivotOffset = new Vector3(0, 0, 0);
        m_MultiChannelPerlin.m_FrequencyGain = 0;
        m_MultiChannelPerlin.m_AmplitudeGain = 1;

        
    }

    private IEnumerator IShootShake(float seconds, float amplitude)
    {
        if (m_MultiChannelPerlin == null)
            yield return null;

        m_MultiChannelPerlin.m_FrequencyGain = 1;
        m_MultiChannelPerlin.m_AmplitudeGain = amplitude * 5;
        m_MultiChannelPerlin.m_PivotOffset = new Vector3(0, 0, 5) * amplitude;
        yield return new WaitForSeconds(seconds);
        m_MultiChannelPerlin.m_PivotOffset = new Vector3(0, 0, 0);
        m_MultiChannelPerlin.m_FrequencyGain = 0;
        m_MultiChannelPerlin.m_AmplitudeGain = 1;
    }

    public void DieAnimation()
    {
        deathCam.m_Priority = 11;
        Destroy(gameObject, 2f);
    }
}
