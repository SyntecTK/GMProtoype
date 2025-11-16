using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.GraphicsBuffer;

public class PP_Controller : MonoBehaviour
{
    [SerializeField] private Volume volume;
    [SerializeField] private Material whiteMaterialLayer01;
    [SerializeField] private Material whiteMaterialLayer02;
    [SerializeField] private Material whiteMaterialLayer03;
    [Header("Fade Settings")]
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private bool shouldFadeVignette = false;
    [SerializeField] private bool shouldFadeFilmGrain = false;
    [SerializeField] private bool shouldFadeSaturation = false;

    private Vignette vignette;
    private FilmGrain filmGrain;
    private ColorAdjustments colorAdjustments;

    //Fade PP Effect
    private Coroutine vignetteCoroutine;
    private Coroutine filmGrainCoroutine;
    private Coroutine colorAdjustmentCoroutine;
    private Coroutine materialCoroutine;

    private void Start()
    {
        if (!volume.profile.TryGet<Vignette>(out vignette))
            Debug.LogWarning("Vignette is missing");

        if (!volume.profile.TryGet<FilmGrain>(out filmGrain))
            Debug.LogWarning("FilmGrain is missing");

        if (!volume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
            Debug.LogWarning("ColorAdjustments is missing");

        GameManager.Instance.OnResourcesChanged += FadeUIChange;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnResourcesChanged -= FadeUIChange;
    }
    #region UI PostProcessing Effects
    private void FadeUIChange(float flowValue, float energyValue)
    {
        //Vignette Fade------------------
        float targetVignette = vignette != null ? 1f - (energyValue / 100f) : 0f;
        
        if(vignetteCoroutine != null)
        {
            StopCoroutine(vignetteCoroutine);
        }
        
        vignetteCoroutine = StartCoroutine(FadeVignette(targetVignette / 2f));

        //FilmGrain Fade----------------
        float targetGrain = filmGrain != null ? 1f - (energyValue / 100f) : 0f;

        if(filmGrainCoroutine != null)
        {
            StopCoroutine(filmGrainCoroutine);
        }

        filmGrainCoroutine = StartCoroutine(FadeGrain(targetGrain));

        //Color Saturation-------------

        float targetSaturation = colorAdjustments != null ? -100f * (1f - (energyValue / 100f)) : 0f;

        if(colorAdjustmentCoroutine != null)
        {
            StopCoroutine(colorAdjustmentCoroutine);
        }
        colorAdjustmentCoroutine = StartCoroutine(FadeColorAdjustments(targetSaturation));

        //Material Fade----------------
        Material targetMaterial = null;

        if (flowValue >= 70f)
        {
            targetMaterial = whiteMaterialLayer01;
        }
        else if (flowValue >= 35f)
        {
            targetMaterial = whiteMaterialLayer02;
        }
        else
        {
            targetMaterial = whiteMaterialLayer03;
        }

        if(materialCoroutine != null)
        {
            StopCoroutine(materialCoroutine);
        }
        materialCoroutine = StartCoroutine(FadeMaterial(targetMaterial));
    }
    private IEnumerator FadeVignette(float target)
    {
        while (Mathf.Abs(vignette.intensity.value - target) > 0.01f)
        {
            vignette.intensity.Override(Mathf.Lerp(vignette.intensity.value, target, Time.deltaTime * fadeSpeed));
            yield return null;
        }

        vignette.intensity.Override(target);
    }
    private IEnumerator FadeGrain(float target)
    {
        while (Mathf.Abs(filmGrain.intensity.value - target) > 0.01f)
        {
            filmGrain.intensity.Override(Mathf.Lerp(filmGrain.intensity.value, target, Time.deltaTime * fadeSpeed));
            yield return null;
        }

        filmGrain.intensity.Override(target);
    }
    private IEnumerator FadeColorAdjustments(float saturationTarget)
    {
        while (Mathf.Abs(colorAdjustments.saturation.value - saturationTarget) > 0.01f)
        {
            colorAdjustments.saturation.Override(Mathf.Lerp(colorAdjustments.saturation.value, saturationTarget, Time.deltaTime * fadeSpeed));
            yield return null;
        }

        colorAdjustments.saturation.Override(saturationTarget);
    }
    #endregion

    #region World Effects
    private IEnumerator FadeMaterial(Material mat)
    {
        float target = 1f;
        float current = mat.GetFloat("_FadeFactor");

        while (Mathf.Abs(current - target) > 0.01f)
        {
            current = Mathf.Lerp(current, target, Time.deltaTime * fadeSpeed);
            mat.SetFloat("_FadeFactor", current);
            yield return null;
        }

        mat.SetFloat("_FadeFactor", target);
    }
    #endregion
}
