using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnvironmentSettingsManager : MonoBehaviour
{
    [Header("Environment Addons (scene objects)")]
    [Tooltip("Parent of all your environment‐addon GameObjects")]
    public Transform addonsParent;

    [Header("Defaults (always available)")]
    [Tooltip("If empty, will use the current RenderSettings.skybox")]
    public Material  defaultSkybox;
    [Tooltip("If empty, will use the current bgmSource.clip")]
    public AudioClip defaultMusic;

    [Header("Skybox Options")]
    public TMP_Dropdown   skyboxDropdown;
    public List<Material> skyboxMaterials;   // all skyboxes you might sell

    [Header("BGM Options")]
    public TMP_Dropdown    musicDropdown;
    public List<AudioClip> bgmClips;         // all tracks you might sell
    public AudioSource     bgmSource;

    private HashSet<string> purchasedIds;

    void Start()
    {
        // fallback defaults
        if (defaultSkybox == null)
            defaultSkybox = RenderSettings.skybox;

        if (bgmSource == null)
            Debug.LogWarning("EnvironmentSettingsManager: bgmSource not assigned!");
        else if (defaultMusic == null)
            defaultMusic = bgmSource.clip;

        Refresh();
    }

    /// <summary>
    /// Activates only the purchased addons in the scene
    /// and rebuilds the skybox & music dropdowns.
    /// </summary>
    public void Refresh()
    {
        // 1) Load purchased IDs
        purchasedIds = new HashSet<string>();
        string json = PlayerPrefs.GetString("PurchasedItems", "");
        if (!string.IsNullOrEmpty(json))
            purchasedIds = new HashSet<string>(
                JsonUtility.FromJson<Serialization<string>>(json).ToList()
            );

        // 2) Environment Addons
        if (addonsParent != null)
        {
            foreach (Transform child in addonsParent)
                child.gameObject.SetActive(purchasedIds.Contains(child.name));
        }
        else Debug.LogWarning("EnvironmentSettingsManager: addonsParent not assigned!");

        // 3) Skybox Dropdown
        if (skyboxDropdown != null)
        {
            var availableSky = new List<Material>();
            if (defaultSkybox != null)
                availableSky.Add(defaultSkybox);

            if (skyboxMaterials != null)
            {
                foreach (var mat in skyboxMaterials)
                {
                    if (mat != null
                        && purchasedIds.Contains(mat.name)
                        && mat != defaultSkybox)
                    {
                        availableSky.Add(mat);
                    }
                }
            }
            else Debug.LogWarning("EnvironmentSettingsManager: skyboxMaterials not assigned!");

            if (availableSky.Count == 0 && defaultSkybox != null)
                availableSky.Add(defaultSkybox);

            skyboxDropdown.ClearOptions();
            skyboxDropdown.AddOptions(availableSky.Select(m => m.name).ToList());

            skyboxDropdown.onValueChanged.RemoveAllListeners();
            skyboxDropdown.onValueChanged.AddListener(idx =>
            {
                if (idx >= 0 && idx < availableSky.Count)
                {
                    RenderSettings.skybox = availableSky[idx];
                    DynamicGI.UpdateEnvironment();
                    PlayerPrefs.SetInt("SelectedSkybox", idx);
                }
            });

            int savedSky = Mathf.Clamp(
                PlayerPrefs.GetInt("SelectedSkybox", 0),
                0, availableSky.Count - 1
            );
            skyboxDropdown.value = savedSky;
            RenderSettings.skybox = availableSky[savedSky];
        }
        else Debug.LogWarning("EnvironmentSettingsManager: skyboxDropdown not assigned!");

        // 4) Music Dropdown
        if (musicDropdown != null && bgmSource != null)
        {
            var availableMusic = new List<AudioClip>();
            if (defaultMusic != null)
                availableMusic.Add(defaultMusic);

            if (bgmClips != null)
            {
                foreach (var clip in bgmClips)
                {
                    if (clip != null
                        && purchasedIds.Contains(clip.name)
                        && clip != defaultMusic)
                    {
                        availableMusic.Add(clip);
                    }
                }
            }
            else Debug.LogWarning("EnvironmentSettingsManager: bgmClips not assigned!");

            if (availableMusic.Count == 0 && defaultMusic != null)
                availableMusic.Add(defaultMusic);

            musicDropdown.ClearOptions();
            musicDropdown.AddOptions(availableMusic.Select(c => c.name).ToList());

            musicDropdown.onValueChanged.RemoveAllListeners();
            musicDropdown.onValueChanged.AddListener(idx =>
            {
                if (idx >= 0 && idx < availableMusic.Count)
                {
                    bgmSource.clip = availableMusic[idx];
                    bgmSource.Play();
                    PlayerPrefs.SetInt("SelectedMusic", idx);
                }
            });

            int savedMusic = Mathf.Clamp(
                PlayerPrefs.GetInt("SelectedMusic", 0),
                0, availableMusic.Count - 1
            );
            musicDropdown.value = savedMusic;
            bgmSource.clip = availableMusic[savedMusic];
            bgmSource.Play();
        }
        else
        {
            if (musicDropdown == null)
                Debug.LogWarning("EnvironmentSettingsManager: musicDropdown not assigned!");
            if (bgmSource == null)
                Debug.LogWarning("EnvironmentSettingsManager: bgmSource not assigned!");
        }
    }
}
