using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JetBrains.Annotations;
using System.Data;

public class GameManger : MonoBehaviour
{

    [Header("Menues")]
    [SerializeField] GameObject Menu_Active;
    [SerializeField] GameObject Menu_Win;
    [SerializeField] GameObject Menu_Pause;
    [SerializeField] GameObject Menu_Lose;
    [Header("Style Meter")]
    [SerializeField] Image DopeImage;
    [SerializeField] Image CrazyImage;
    [SerializeField] Image BallerImage;
    [SerializeField] Image AwesomeSauceImage;
    [SerializeField] Image SMBar;
    [SerializeField] Image SMColor;
    [SerializeField] Slider SMslider;
    [SerializeField] Gradient SMhealthGradient;
    private float meterpoints;
    [Header("Meter Settings")]
    [SerializeField] float meterDecayRate = 0.25f; // points per second
    [SerializeField] float meterNoDecayDelay = 1.5f; // seconds after gain before decay starts
    private float lastGainTime = -Mathf.Infinity;





    [Header("other")]
    public static GameManger Instance;
    public GameObject Player;
    public Player PlayerScript;
    public Image DMG_Screen;
    public Image Player_HP_Bar;
    public Image PlayerHealthColor;
    public Slider slider;
    public Gradient healthGradient;


    [Header("audio")]
    public AudioClip[] AUDclick;
    public float AUDclickV;
    public AudioSource playerADU;
    public bool Ispaused;
    float timeScale_OG;
    float safe;
    float healthPercentage;
     public int enmenycount;

    void Awake()
    {
        Instance = this;
        timeScale_OG = Time.timeScale;
        Player = GameObject.FindGameObjectWithTag("Player");
        PlayerScript = Player.GetComponent<Player>();
        // initialize meter UI to hidden
        if (DopeImage != null) DopeImage.gameObject.SetActive(false);
        if (CrazyImage != null) CrazyImage.gameObject.SetActive(false);
        if (BallerImage != null) BallerImage.gameObject.SetActive(false);
        if (AwesomeSauceImage != null) AwesomeSauceImage.gameObject.SetActive(false);

        Debug.Log("GameManger Awake: meter images initialized");

        // Initialize meter slider if present
        if (SMslider != null)
        {
            SMslider.maxValue = 4f;
            SMslider.value = meterpoints;
            if (SMslider.fillRect != null && SMhealthGradient != null)
            {
                var img = SMslider.fillRect.GetComponent<Image>();
                if (img != null) img.color = SMhealthGradient.Evaluate(0f);
            }
        }

        // initialize optional images for meter bar and color
        if (SMBar != null)
        {
            SMBar.fillAmount = Mathf.Clamp01(meterpoints / 4f);
            SMBar.gameObject.SetActive(false);
        }
        if (SMColor != null && SMhealthGradient != null)
        {
            SMColor.color = SMhealthGradient.Evaluate(Mathf.Clamp01(meterpoints / 4f));
            SMColor.gameObject.SetActive(false);
        }
        if (SMslider != null)
        {
            SMslider.gameObject.SetActive(false);
        }

    }




    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (Menu_Active == null)
            {
                StartPause();
                Menu_Active = Menu_Pause;
                Menu_Active.SetActive(Ispaused);
            }
            else if (Menu_Active == Menu_Pause)
            {
                startUnPause();
            }
        }
        UpdateHealthBar();

        // handle meter decay over time and update UI
        UpdateMeterDecay();

    }


    public void StartPause()
    {
        Ispaused = !Ispaused;
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        Player.GetComponentInChildren<PlayerCamera>().enabled = false;
        PlayerScript.enabled = false;
        PlayClickSound();   

    }


    public void startUnPause()
    {
        Ispaused = !Ispaused;
        Time.timeScale = timeScale_OG;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Menu_Active.SetActive(Ispaused);
        Menu_Active = null;
        PlayerScript.enabled = true;
        Player.GetComponentInChildren<PlayerCamera>().enabled = true;
        PlayClickSound();

    }

    public void UpdateHealthBar()
    {
       
        setHealth();
        SetMaxHealth();
    }

    public void setHealth()
    {
        slider.value = PlayerScript.health;
        PlayerHealthColor.color = healthGradient.Evaluate(slider.normalizedValue);

    }

    public void SetMaxHealth()
    {
        slider.maxValue = PlayerScript.maxHealth;
        slider.value = PlayerScript.health;
        PlayerHealthColor.color = healthGradient.Evaluate(1f);

    }
    //public IEnumerator dmgflash()
    //{

    //    DMG_Screen.SetActive(true);
    //    yield return new WaitForEndOfFrame();
    //    DMG_Screen.SetActive(false);
    //}11
    public void StartLose()
    {
        StartPause();
        Menu_Active = Menu_Lose;
        Menu_Lose.SetActive(true);
    }


    public void updateGameGoal(int goal)
    {
        enmenycount += goal;
        if (enmenycount <= 0)
        {

            StartPause();
            Menu_Active = Menu_Win;
            Menu_Win.SetActive(Ispaused);
        }
    }


    public void PlayClickSound()
    {
        playerADU.PlayOneShot(AUDclick[Random.Range(0, AUDclick.Length)], AUDclickV);
    }

    public void StopClickSound() {
        playerADU.Stop();
    }
    //make it so the points are 0.25 or something like that and make one whole number correspand to an image and make it so we dont get at the same time as the other images and make it so we can only have one image at a time and make it so the images are in order
    // also make it so whatever image is active they will go away in a few seconds so they dont stay on the screen forever when we get no new points
    public void MeterPointAddage()
    {
        // add 0.5 per call so it takes two increments to advance one full stage
        meterpoints = Mathf.Clamp(meterpoints + 0.5f, 0f, 4f);
        lastGainTime = Time.time;
        Debug.Log($"MeterPointAddage called, meterpoints={meterpoints}");

        // update UI to reflect new fractional value
        UpdateMeterUI();
    }

    void UpdateMeterDecay()
    {
        if (Time.time - lastGainTime < meterNoDecayDelay) return;

        if (meterpoints > 0f)
        {
            meterpoints = Mathf.Clamp(meterpoints - meterDecayRate * Time.deltaTime, 0f, 4f);
            UpdateMeterUI();
        }
    }
    //bar doesn't fill up, need to fix this later, also make it so the bar and color only show when we have at least 0.25 points or something like that and make it so they go away when we have no points
    void UpdateMeterUI()
    {
        // update slider fill
        if (SMslider != null)
        {
            SMslider.maxValue = 4f;
            SMslider.value = meterpoints;
            if (SMslider.fillRect != null && SMhealthGradient != null)
            {
                var img = SMslider.fillRect.GetComponent<Image>();
                if (img != null) img.color = SMhealthGradient.Evaluate(Mathf.Clamp01(meterpoints / 4f));
            }
        }
        int stage = Mathf.FloorToInt(meterpoints);
        // show or hide meter UI based on whether any stage image is active
        bool anyStageActive = stage >= 1;
        if (SMBar != null)
        {
            SMBar.gameObject.SetActive(anyStageActive);
            SMBar.fillAmount = Mathf.Clamp01(meterpoints / 4f);
        }
        if (SMColor != null && SMhealthGradient != null)
        {
            SMColor.gameObject.SetActive(anyStageActive);
            SMColor.color = SMhealthGradient.Evaluate(Mathf.Clamp01(meterpoints / 4f));
        }
        if (SMslider != null)
        {
            SMslider.gameObject.SetActive(anyStageActive);
        }

        // update stage images based on full stage (only one active at a time)
        //int stage = Mathf.FloorToInt(meterpoints);
        if (DopeImage != null) DopeImage.gameObject.SetActive(stage == 1);
        if (CrazyImage != null) CrazyImage.gameObject.SetActive(stage == 2);
        if (BallerImage != null) BallerImage.gameObject.SetActive(stage == 3);
        if (AwesomeSauceImage != null) AwesomeSauceImage.gameObject.SetActive(stage == 4);
    }
}
