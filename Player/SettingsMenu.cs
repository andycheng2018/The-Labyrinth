using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Unity.Netcode;

namespace AC
{
    public class SettingsMenu : MonoBehaviour
    {
        [Header("Gameplay Settings")]
        public Camera playerCam;
        public Slider sensitivitySlider;
        public TMP_Text sensitivityText;
        public Slider renderDistanceSlider;
        public TMP_Text renderDistanceText;
        public GameObject postProcessing;
        public Toggle postProcessingToggle;
        public Toggle antiAliasingToggle;

        [Header("Control Settings")]
        public TMP_Text jump;
        public TMP_Text sprint;
        public TMP_Text crouch;
        public TMP_Text interact;
        public TMP_Text reload;

        [Header("Video Settings")]
        public Resolution[] resolutions;
        public TMP_Dropdown resolutionDropdown;
        public TMP_Dropdown graphicsDropdown;
        public Toggle fullScreenToggle;
        public Toggle VSyncToggle;

        [Header("Audio Settings")]
        public Slider masterSlider;
        public TMP_Text masterText;
        public AudioMixer musicMixer;
        public Slider musicSlider;
        public TMP_Text musicText;

        [Header("Character Skins")]
        public int skinIndex;

        [Header("Coins")]
        public TMP_Text coinsText;
        public int coinsAmount;

        public Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();
        private GameObject currentKey;
        private int highestIndex;
        private AudioSource audioSource;
        private Player player;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            audioSource = gameObject.GetComponent<AudioSource>();
            player = GetComponentInParent<Player>();

            //Gameplay
            sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
            renderDistanceSlider.onValueChanged.AddListener(SetRenderDistance);
            postProcessingToggle.onValueChanged.AddListener(SetPostProcessing);
            antiAliasingToggle.onValueChanged.AddListener(SetAntiAliasing);

            //Controls
            keys.Add("Jump", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Jump", "Space")));
            keys.Add("Sprint", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Sprint", "LeftShift")));
            keys.Add("Crouch", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Crouch", "C")));
            keys.Add("Interact", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Interact", "F")));
            keys.Add("Reload", (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Reload", "R")));
            jump.text = keys["Jump"].ToString();
            sprint.text = keys["Sprint"].ToString();
            crouch.text = keys["Crouch"].ToString();
            interact.text = keys["Interact"].ToString();
            reload.text = keys["Reload"].ToString();

            //Video
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
            graphicsDropdown.onValueChanged.AddListener(SetQuality);
            fullScreenToggle.onValueChanged.AddListener(SetFullScreen);
            VSyncToggle.onValueChanged.AddListener(SetVSync);

            //Resolutions
            resolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();
            foreach (Resolution resolution in resolutions)
            {
                resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(resolution.ToString()));
            }
            int currentResolutionIndex = GetCurrentResolutionIndex();
            highestIndex = currentResolutionIndex;
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();

            //Audio
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
            musicSlider.onValueChanged.AddListener(SetMusicVoume);

            //Coins
            coinsText.text = coinsAmount.ToString();

            LoadData();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && Cursor.visible && Cursor.lockState == CursorLockMode.Confined)
            {
                audioSource.Play();
            }
        }

        //Gameplay
        private void SetSensitivity(float sensitivityValue)
        {
            if (playerCam != null)
            {
                player.sensitivity = sensitivityValue;
            }
            sensitivityText.text = sensitivityValue.ToString();
            sensitivitySlider.value = sensitivityValue;
            PlayerPrefs.SetFloat("Sensitivity", sensitivityValue);
        }

        private void SetRenderDistance(float renderDistance)
        {
            if (playerCam != null)
            {
                playerCam.farClipPlane = renderDistance;
            }
            renderDistanceText.text = ((int)renderDistance).ToString();
            renderDistanceSlider.value = renderDistance;
            PlayerPrefs.SetFloat("RenderDistance", renderDistance);
        }

        private void SetPostProcessing(bool enablePostProcessing)
        {
            if (enablePostProcessing)
            {
                if (postProcessing != null)
                    postProcessing.GetComponent<Volume>().enabled = true;
                postProcessingToggle.isOn = true;
            }
            else
            {
                if (postProcessing != null)
                    postProcessing.GetComponent<Volume>().enabled = false;
                postProcessingToggle.isOn = false;
            }
            PlayerPrefs.SetInt("PostProcessing", boolToInt(enablePostProcessing));
        }

        private void SetAntiAliasing(bool enableAntiAliasing)
        {
            if (enableAntiAliasing)
            {
                if (playerCam != null)
                    playerCam.allowMSAA = true;
                antiAliasingToggle.isOn = true;
            }
            else
            {
                if (playerCam != null)
                    playerCam.allowMSAA = false;
                antiAliasingToggle.isOn = false;
            }
            PlayerPrefs.SetInt("AntiAliasing", boolToInt(enableAntiAliasing));
        }

        //Controls
        public void ChangeKey(GameObject clicked)
        {
            if (currentKey != null)
            {
                currentKey.GetComponent<Image>().color = Color.white;
            }

            currentKey = clicked;
            currentKey.GetComponent<Image>().color = Color.red;
        }

        private void OnGUI()
        {
            if (currentKey != null)
            {
                Event e = Event.current;
                if (e.isKey && e.keyCode != KeyCode.Escape && !keys.ContainsValue(e.keyCode))
                {
                    keys[currentKey.name] = e.keyCode;
                    currentKey.GetComponentInChildren<TMP_Text>().text = e.keyCode.ToString();
                    currentKey.GetComponent<Image>().color = Color.white;
                    currentKey = null;
                }
            }

            foreach (var key in keys)
            {
                PlayerPrefs.SetString(key.Key, key.Value.ToString());
            }
        }

        //Video
        public void SetQuality(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex);
            graphicsDropdown.value = qualityIndex;
            PlayerPrefs.SetInt("Graphics", graphicsDropdown.value);
        }

        public void SetResolution(int resolutionIndex)
        {
            Resolution resolution = resolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            resolutionDropdown.value = resolutionIndex;
            PlayerPrefs.SetInt("Resolution", resolutionDropdown.value);
        }

        private int GetCurrentResolutionIndex()
        {
            Resolution currentResolution = Screen.currentResolution;

            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == currentResolution.width &&
                    resolutions[i].height == currentResolution.height)
                {
                    return i;
                }
            }

            return 0;
        }

        public void SetFullScreen(bool enableFullScreen)
        {
            if (enableFullScreen)
            {
                Screen.fullScreen = true;
                fullScreenToggle.isOn = true;
            }
            else
            {
                Screen.fullScreen = false;
                fullScreenToggle.isOn = false;
            }
            PlayerPrefs.SetInt("FullScreen", boolToInt(enableFullScreen));
        }

        public void SetVSync(bool enableVSync)
        {
            if (enableVSync)
            {
                QualitySettings.vSyncCount = 1;
                VSyncToggle.isOn = true;
            }
            else
            {
                QualitySettings.vSyncCount = 0;
                VSyncToggle.isOn = false;
            }
            PlayerPrefs.SetInt("VSync", boolToInt(enableVSync));
        }

        //Audio
        public void SetMasterVolume(float volume)
        {
            masterText.text = volume.ToString();
            masterSlider.value = volume;
            AudioListener.volume = volume/100;
            PlayerPrefs.SetFloat("MasterVolume", volume);
        }

        public void SetMusicVoume(float volume)
        {
            musicText.text = volume.ToString();
            musicSlider.value = volume;
            musicMixer.SetFloat("CaveMasterVol", Mathf.Log10(volume/100) * 20);
            PlayerPrefs.SetFloat("MusicVolume", volume);
        }

        //Save Skins
        public void SetSkin(int skinIndex)
        {
            this.skinIndex = skinIndex;
            PlayerPrefs.SetInt("SelectedSkinIndex", skinIndex);
        }

        //Add Coins
        public void AddCoins(int coins)
        {
            coinsAmount += coins;
            coinsText.text = coinsAmount.ToString();
            PlayerPrefs.SetInt("CoinsAmount", coinsAmount);
        }

        //Save/Load
        private int boolToInt(bool val)
        {
            if (val)
                return 1;
            else
                return 0;
        }

        private bool intToBool(int val)
        {
            if (val != 0)
                return true;
            else
                return false;
        }

        public void ResetData()
        {
            //Gameplay
            SetSensitivity(50);
            SetRenderDistance(100);
            SetPostProcessing(true);
            SetAntiAliasing(true);

            //Controls
            keys["Jump"] = KeyCode.Space;
            keys["Sprint"] = KeyCode.LeftShift;
            keys["Crouch"] = KeyCode.C;
            keys["Interact"] = KeyCode.F;
            keys["Reload"] = KeyCode.R;
            jump.text = keys["Jump"].ToString();
            sprint.text = keys["Sprint"].ToString();
            crouch.text = keys["Crouch"].ToString();
            interact.text = keys["Interact"].ToString();
            reload.text = keys["Reload"].ToString();

            //Video
            SetResolution(highestIndex);
            SetQuality(2);
            SetFullScreen(true);
            SetVSync(true);

            //Audio
            SetMasterVolume(50f);
            SetMusicVoume(50f);

            //Skin
            SetSkin(1);
        }

        public void LoadData()
        {
            if (PlayerPrefs.HasKey("Sensitivity"))
            {
                //Gameplay
                SetSensitivity(PlayerPrefs.GetFloat("Sensitivity"));
                SetRenderDistance(PlayerPrefs.GetFloat("RenderDistance"));
                SetPostProcessing(intToBool(PlayerPrefs.GetInt("PostProcessing", 0)));
                SetAntiAliasing(intToBool(PlayerPrefs.GetInt("AntiAliasing", 0)));

                //Video
                SetResolution(PlayerPrefs.GetInt("Resolution", 0));
                SetQuality(PlayerPrefs.GetInt("Graphics", 0));
                SetFullScreen(intToBool(PlayerPrefs.GetInt("FullScreen", 0)));
                SetVSync(intToBool(PlayerPrefs.GetInt("VSync", 0)));

                //Audio
                SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume"));
                SetMusicVoume(PlayerPrefs.GetFloat("MusicVolume"));

                //Skin
                SetSkin(PlayerPrefs.GetInt("SelectedSkinIndex", skinIndex));

                //Coins
                AddCoins(PlayerPrefs.GetInt("CoinsAmount", coinsAmount));
            }
            else
            {
                ResetData();
            }
        }

        //Menu Buttons
        public void Back()
        {
            if (SceneManager.GetActiveScene().name != "(1) Main Menu")
            {
                player.UpdateCursor(false);
            }
        }

        public void Menu()
        {
            SceneManager.LoadScene("(1) Main Menu");
            NetworkManager.Singleton.Shutdown(true);
            
            if (NetworkManager.Singleton.gameObject != null)
            {
                Destroy(NetworkManager.Singleton.gameObject);
            }

            if (LobbySaver.instance.gameObject != null)
            {
                Destroy(LobbySaver.instance.gameObject);
            }
        }

        public void Quit() {
            Application.Quit();
        }
    }
}