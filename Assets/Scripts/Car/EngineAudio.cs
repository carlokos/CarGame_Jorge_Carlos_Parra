using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineAudio : MonoBehaviour
{
    [Header ("Running sound")]
    [SerializeField] private AudioSource runningSound;
    [SerializeField] private float runningMaxVolume;
    [SerializeField] private float runningMaxPitch;
    [Header ("Reverse sound")]
    [SerializeField] private AudioSource reverseSound;
    [SerializeField] private float reverseMaxVolume;
    [SerializeField] private float reverseMaxPitch;
    [Header ("Idle sound")]
    [SerializeField] private AudioSource idleSound;
    [SerializeField] private float idleMaxVolume;
    [Header("Start Sound")]
    [SerializeField] private AudioSource startSound;
    [Header("Settings")]
    private float limiterSound = 1f;
    private float revLimiter;
    private float limiterFrequency = 3f;
    private float limiterEngage = 0.8f;
    private float speedRatio;
    private bool isEngineRunning = false;

    private CarController carController;
    // Start is called before the first frame update
    void Start()
    {
        carController = GetComponent<CarController>();
        idleSound.volume = 0;
        runningSound.volume = 0;
        reverseSound.volume = 0;
    }

    // Update is called once per frame
    void Update()
    {
        float speedSign = 0;
        if(carController)
        {
            speedSign = Mathf.Sign(carController.GetSpeedRatio());
            speedRatio = Mathf.Abs(carController.GetSpeedRatio());
        }
        if(speedRatio > limiterEngage)
        {
            revLimiter = (Mathf.Sin(Time.time * limiterFrequency) + 1f) *  limiterSound * (speedRatio - limiterEngage);
        }
        if (isEngineRunning)
        {
            idleSound.volume = Mathf.Lerp(0.1f, idleMaxVolume, speedRatio);
            if (speedSign > 0)
            {
                reverseSound.volume = 0;
                runningSound.volume = Mathf.Lerp(0.3f, runningMaxVolume, speedRatio);
                runningSound.pitch = Mathf.Lerp(runningSound.pitch, Mathf.Lerp(0.3f, runningMaxPitch, speedRatio) + revLimiter, Time.deltaTime);
            }
            else
            {
                runningSound.volume = 0;
                reverseSound.volume = Mathf.Lerp(0f, reverseMaxVolume, speedRatio);
                reverseSound.pitch = Mathf.Lerp(reverseSound.pitch, Mathf.Lerp(0.2f, reverseMaxPitch, speedRatio) + revLimiter, Time.deltaTime);
            }
        }
        else
        {
            idleSound.volume = 0;
            runningSound.volume = 0;
            reverseSound.volume = 0;
        }
    }

    public IEnumerator StartEngine()
    {
        startSound.Play();
        carController.IsEngineRunning = 1;
        yield return new WaitForSeconds(0.6f);
        isEngineRunning = true;
        yield return new WaitForSeconds(0.4f);
        carController.IsEngineRunning = 2;
    }
}
