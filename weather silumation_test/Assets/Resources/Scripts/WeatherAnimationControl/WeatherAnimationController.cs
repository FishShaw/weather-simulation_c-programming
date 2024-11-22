using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WeatherAnimationController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private WeatherMapController mapController;
    [SerializeField] private Slider timeSlider;
    
    [Header("Animation Settings")]
    [SerializeField] private float frameRate = 2f;
    [SerializeField] private bool loopAnimation = true;
    [SerializeField] private bool playOnStart = false;
    
    private bool isPlaying = false;
    private int currentHour = 0;
    private Coroutine animationCoroutine;

    private void Start()
    {
        if (timeSlider != null)
        {
            timeSlider.minValue = 0;
            timeSlider.maxValue = 23;
            timeSlider.wholeNumbers = true;
            timeSlider.onValueChanged.AddListener(OnTimeSliderChanged);
        }

        if (playOnStart)
        {
            PlayAnimation();
        }
        else
        {
            UpdateDisplay();
        }
    }

    private void OnTimeSliderChanged(float value)
    {
        if (!isPlaying)
        {
            currentHour = (int)value;
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        mapController.UpdateWeatherDisplay(currentHour);
        if (timeSlider != null)
        {
            timeSlider.value = currentHour;
        }
    }

    public void PlayAnimation()
    {
        if (!isPlaying)
        {
            isPlaying = true;
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(AnimationLoop());
        }
    }

    public void PauseAnimation()
    {
        isPlaying = false;
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    public void StopAnimation()
    {
        PauseAnimation();
        currentHour = 0;
        UpdateDisplay();
    }

    private IEnumerator AnimationLoop()
    {
        while (isPlaying)
        {
            UpdateDisplay();
            yield return new WaitForSeconds(1f / frameRate);
            
            currentHour = (currentHour + 1) % 24;
            if (!loopAnimation && currentHour == 0)
            {
                PauseAnimation();
            }
        }
    }

    public void SetHour(int hour)
    {
        currentHour = Mathf.Clamp(hour, 0, 23);
        UpdateDisplay();
    }
}
