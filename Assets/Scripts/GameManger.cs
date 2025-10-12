using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JetBrains.Annotations;

public class GameManger : MonoBehaviour
{

    [Header("Menues")]
    [SerializeField] GameObject Menu_Active;
    [SerializeField] GameObject Menu_Win;
    [SerializeField] GameObject Menu_Pause;
    [SerializeField] GameObject Menu_Lose;

    [Header("other")]
    public static GameManger Instance;
    public GameObject Player;
    public Player PlayerScript;


    [Header("audio")]
    public AudioClip[] AUDclick;
    public float AUDclickV;
    public AudioSource playerADU;
    public bool paused;
    float timeScale_OG;

    void Awake()
    {
        Instance = this;
        timeScale_OG = Time.timeScale;
        Player = GameObject.FindGameObjectWithTag("Player");
        PlayerScript = Player.GetComponent<Player>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
