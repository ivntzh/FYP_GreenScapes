using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class EnvironmentSettingsManager : MonoBehaviourPunCallbacks, IPunObservable
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

    private PhotonView photonView;

    // Required empty implementation
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Empty but required for PhotonView observation
    }

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        InitializeDefaults();
    
        // Delay initial refresh to ensure ShopManager is ready
        StartCoroutine(DelayedInitialRefresh());
    }

    IEnumerator DelayedInitialRefresh()
    {
        // Wait until ShopManager initialization completes
        while (shopManager == null || shopManager.currentPurchasedIds == null)
        {
            yield return null;
        }
    
        // Additional safety delay
        yield return new WaitForEndOfFrame();
    
        RefreshEnvironment();
    }

    [PunRPC]
    void RequestEnvironmentSync()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RefreshEnvironmentRPC", RpcTarget.Others);
        }
    }

    [PunRPC]
    public void RefreshEnvironmentRPC()
    {
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
        if (addonsParent == null)
        {
            Debug.LogWarning("Addons parent not assigned!");
            return;
        }

        Debug.Log($"Updating addons. Purchased IDs count: {purchasedIds?.Count ?? 0}");
    
        foreach (Transform child in addonsParent)
        {
            if (child == null) continue;
        
            bool shouldActivate = purchasedIds?.Contains(child.name) ?? false;
            Debug.Log($"{child.name} activation: {shouldActivate}");

            if (PhotonNetwork.IsConnected)
            {
                if (photonView != null)
                {
                    photonView.RPC("SetActiveRPC", RpcTarget.AllBuffered, child.name, shouldActivate);
                }
                else
                {
                    Debug.LogWarning("PhotonView missing - activating locally");
                    child.gameObject.SetActive(shouldActivate);
                }
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

    [PunRPC]
    private void SetActiveRPC(string childName, bool state)
    {
        Transform child = addonsParent.Find(childName);
        if (child != null)
        {
            child.gameObject.SetActive(state);
        }
    }
}