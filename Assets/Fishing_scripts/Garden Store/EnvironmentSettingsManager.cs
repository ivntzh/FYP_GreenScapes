using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class EnvironmentSettingsManager : MonoBehaviourPunCallbacks
{
    [Header("Environment Addons (scene objects)")]
    public Transform addonsParent;

    [Header("Defaults (always available)")]
    public Material defaultSkybox;
    public AudioClip defaultMusic;

    [Header("Skybox Options")]
    public TMP_Dropdown skyboxDropdown;
    public List<Material> skyboxMaterials;

    [Header("BGM Options")]
    public TMP_Dropdown musicDropdown;
    public List<AudioClip> bgmClips;
    public AudioSource bgmSource;

    [Header("References")]
    public ShopManager shopManager; // Reference to your ShopManager

    public HashSet<string> purchasedIds;
    private List<Material> availableSky;
    private List<AudioClip> availableMusic;

    void Start()
    {
        InitializeDefaults();
        RefreshEnvironment();
    }

    void InitializeDefaults()
    {
        if (defaultSkybox == null)
            defaultSkybox = RenderSettings.skybox;

        if (bgmSource == null)
            Debug.LogWarning("bgmSource not assigned!");
        else if (defaultMusic == null)
            defaultMusic = bgmSource.clip;
    }

    public void RefreshEnvironment()
    {
        UpdatePurchasedData();
        UpdateAddons();
        UpdateSkyboxOptions();
        UpdateMusicOptions();
    }

    void UpdatePurchasedData()
    {
        if (PhotonNetwork.IsConnected && shopManager != null)
        {
            purchasedIds = new HashSet<string>(shopManager.currentPurchasedIds);
        }
        else
        {
            string json = PlayerPrefs.GetString("PurchasedItems", "");
            purchasedIds = string.IsNullOrEmpty(json) 
                ? new HashSet<string>() 
                : new HashSet<string>(JsonUtility.FromJson<Serialization<string>>(json).ToList());
        }
    }

    void UpdateAddons()
    {
        if (addonsParent == null) return;

        foreach (Transform child in addonsParent)
        {
            bool shouldActivate = purchasedIds.Contains(child.name);
            
            if (PhotonNetwork.IsConnected)
            {
                // Networked activation
                child.gameObject.GetPhotonView().RPC("SetActiveRPC", RpcTarget.AllBuffered, shouldActivate);
            }
            else
            {
                child.gameObject.SetActive(shouldActivate);
            }
        }
    }

    void UpdateSkyboxOptions()
    {
        if (skyboxDropdown == null) return;

        availableSky = new List<Material> { defaultSkybox };
        availableSky.AddRange(skyboxMaterials.Where(m => 
            m != null && purchasedIds.Contains(m.name) && m != defaultSkybox
        ));

        UpdateDropdown(skyboxDropdown, 
            availableSky.Select(m => m.name).ToList(),
            PlayerPrefs.GetInt("SelectedSkybox", 0),
            (index) => {
                RenderSettings.skybox = availableSky[index];
                DynamicGI.UpdateEnvironment();
                PlayerPrefs.SetInt("SelectedSkybox", index);
            }
        );
    }

    void UpdateMusicOptions()
    {
        if (musicDropdown == null || bgmSource == null) return;

        availableMusic = new List<AudioClip> { defaultMusic };
        availableMusic.AddRange(bgmClips.Where(c => 
            c != null && purchasedIds.Contains(c.name) && c != defaultMusic
        ));

        UpdateDropdown(musicDropdown,
            availableMusic.Select(c => c.name).ToList(),
            PlayerPrefs.GetInt("SelectedMusic", 0),
            (index) => {
                bgmSource.clip = availableMusic[index];
                bgmSource.Play();
                PlayerPrefs.SetInt("SelectedMusic", index);
            }
        );
    }

    void UpdateDropdown(TMP_Dropdown dropdown, List<string> options, int savedIndex, UnityEngine.Events.UnityAction<int> callback)
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        
        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.onValueChanged.AddListener(callback);
        
        int clampedIndex = Mathf.Clamp(savedIndex, 0, options.Count - 1);
        dropdown.value = clampedIndex;
        callback?.Invoke(clampedIndex);
    }

    // Add this to any networked objects you want to control
    [PunRPC]
    public void SetActiveRPC(bool state)
    {
        gameObject.SetActive(state);
    }
}