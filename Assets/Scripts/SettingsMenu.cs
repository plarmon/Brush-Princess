using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.Rendering;


public class SettingsMenu : MonoBehaviour
{
    void Start()
    {
        dropdown.value = QualitySettings.GetQualityLevel();
    }

    public AudioMixer audioMixer;
    public RenderPipelineAsset[] qualityLevels;
    public TMP_Dropdown dropdown;

    public void SetVolume (float volume)
    {
        audioMixer.SetFloat("volume", volume);
    }

    public void ChangeQualityLevel(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        QualitySettings.renderPipeline = qualityLevels[qualityIndex];
    }

    public void SetFullscreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }
}
