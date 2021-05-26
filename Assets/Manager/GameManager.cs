﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[Serializable]
public class SaveData
{
    [SerializeField]
    private int _curretnStage = 1;
    public int currentStage
    {
        get { return _curretnStage; }
        set { _curretnStage = value; }
    }
    [SerializeField]
    private int _maxReachedStage = 1;
    public int maxReachedStage
    {
        get { return _maxReachedStage; }
        set { _maxReachedStage = value; }
    }
    [SerializeField]
    private int _gold = 100;
    public int gold
    {
        get { return _gold; }
        set { _gold = value; }
    }
    // 업그레이드를 하지 않은 상태에선, 스탯 레벨은 0이다.
    [SerializeField]
    private List<int> _unitHeartLev = new List<int>();
    public List<int> unitHeartLev
    {
        get { return _unitHeartLev; }
        set { _unitHeartLev = value; }
    }
    [SerializeField]
    private List<int>  _unitApLev = new List<int>();
    public List<int> unitApLev
    {
        get { return _unitApLev; }
        set { _unitApLev = value; }
    }
    [SerializeField]
    private List<int> _unitDpLev = new List<int>();
    public List<int> unitDpLev
    {
        get { return _unitDpLev; }
        set { _unitDpLev = value; }
    }
    [SerializeField]
    private List<float> _heart = new List<float>();
    public List<float> heart
    {
        get { return _heart; }
        set { _heart = value; }
    }
    [SerializeField]
    private List<float> _ap = new List<float>();
    public List<float> ap
    {
        get { return _ap; }
        set { _ap = value; }
    }
    [SerializeField]
    private List<float> _dp = new List<float>(); // index는 유닛 ID로, 건물의 경우 아군의 건물은 0, 적의 건물은 1로한다.
    public List<float> dp
    {
        get { return _dp; }
        set { _dp = value; }
    }
    [SerializeField]
    private int _plusMoneySpeedLev = 0;
    public int plusMoneySpeedLev
    {
        get { return _plusMoneySpeedLev; }
        set { _plusMoneySpeedLev = value; }
    }
    [SerializeField]
    private int _maxPlusMoneySpeedLev = 10;
    public int maxPlusMoneySpeedLev
    {
        get { return _maxPlusMoneySpeedLev; }
        set { _maxPlusMoneySpeedLev = value; }
    }
    [SerializeField]
    private int _maxStatLev = 10; // 유닛의 업그레이드 최대레벨
    public int maxStatLev
    {
        get { return _maxStatLev; }
        set { _maxStatLev = value; }
    }
    [SerializeField]
    private int _maxFusionLev = 5; // 유닛의 fusion 최대레벨
    public int maxFusionLev
    {
        get { return _maxFusionLev; }
        set { _maxFusionLev = value; }
    }
}
public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Transform unitSpawnPosition = null;
    [SerializeField]
    private Transform enemyUnitSpawnPosition = null;
    [SerializeField]
    private Vector2 _mousePosition = Vector2.zero;
    public Vector2 mousePosition
    {
        get{return _mousePosition;}
        set{_mousePosition = value;}
    }
    private float _halfScreenSizeX = 0f;
    public float halfScreenSizeX
    {
        get{return _halfScreenSizeX;}
        set{_halfScreenSizeX = value;}
    }
    [SerializeField]
    private bool canTimeStop = true;
    [SerializeField]
    private bool canTimeDouble = true;
    [SerializeField]
    private bool _canGetOutPopUpSpawn = true;
    public bool canGetOutPopUpSpawn
    {
        get{return _canGetOutPopUpSpawn;}
        set{_canGetOutPopUpSpawn = value;}
    }
    private static GameManager instance;
    [SerializeField]
    private GameObject gameOut = null;
    private bool gameOutSpawned = false;
    [SerializeField]
    private UnitScript[] _playerUnitPrefabs;
    public UnitScript[] playerUnitPrefabs
    {
        get { return _playerUnitPrefabs; }
        set { _playerUnitPrefabs = value; }
    }
    [SerializeField]
    private SaveData saveData;
    [SerializeField]
    private Text goldText = null;
    [SerializeField]
    private float _dovalueTime = 0.4f;

    public float dovalueTime
    {
        get { return _dovalueTime; }
    }
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
                if (instance == null)
                {
                    GameObject temp = new GameObject("GameManager");
                    instance = temp.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }
    private int money = 0;
    private int _hadMoney = 0; // 처음부터 끝까지 money를 하나도 안썼을 경우의 money의 양
    public int hadMoney
    {
        get { return _hadMoney; }
        set{_hadMoney = value;}
    }
    private int plusMoney = 1;
    [SerializeField]
    private float _plusMoneyTime = 0.1f;
    public float plusMoneyTime
    {
        get { return _plusMoneyTime; }
        set
        {
            if (value >= 0f)
                _plusMoneyTime = value;
        }
    }
    [SerializeField]
    private float _minusPluseMoneyTimePerLev = 0.005f;
    public float minusPluseMoneyTimePerLev
    {
        get { return _minusPluseMoneyTimePerLev; }
        set
        {
            _minusPluseMoneyTimePerLev = value;
        }

    }
    private float soundValue = 1f;

    private bool canMoneyPlus = true;
    [SerializeField]
    private bool mapSliderMoving = false;
    private bool uiClicked = false;
    private bool _popUpIsSpawned = false;
    public bool popUpIsSpawned
    {
        get{return _popUpIsSpawned;}
        set{_popUpIsSpawned = value;}
    }
    void Awake()
    {
        halfScreenSizeX = (Screen.width / 2);
    }

    void Update()
    {
        if (canMoneyPlus)
            StartCoroutine(PlusMoney());
        TimeSet();
        CheckSpawnGetOut();

        goldText.text = saveData.gold + "";
    }

    private void CheckSpawnGetOut()
    {
        if (canGetOutPopUpSpawn)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (!gameOutSpawned)
                    gameOut.SetActive(true);
                else
                    gameOut.SetActive(false);

                gameOutSpawned = !gameOutSpawned;
            }
        }
    }

    private IEnumerator PlusMoney()
    {
        canMoneyPlus = false;

        yield return new WaitForSeconds(plusMoneyTime - (saveData.plusMoneySpeedLev * minusPluseMoneyTimePerLev));

        money += plusMoney;
        hadMoney += plusMoney;

        canMoneyPlus = true;
    }
    public int GetMoney()
    {
        return money;
    }
    public void SetMoney(int a)
    {
        money = a;
    }
    public int GetGold()
    {
        return saveData.gold;
    }
    public void SetGold(int a)
    {
        saveData.gold = a;
    }
    public SaveData GetSaveData()
    {
        return saveData;
    }
    public void SetSaveData(SaveData a)
    {
        saveData = a;
    }
    public bool GetCST()
    {
        return canTimeStop;
    }
    public bool GetCDT()
    {
        return canTimeDouble;
    }
    public void SetCSt(bool a)
    {
        canTimeStop = a;
    }
    public void SetCDT(bool a)
    {
        canTimeDouble = a;
    }
    public void SetSoundValue(float a)
    {
        soundValue = a;
    }
    public float GetSoundValue()
    {
        return soundValue;
    }
    private void TimeSet()
    {
        if (!canTimeStop)
            Time.timeScale = 0;
        else if (!canTimeDouble) // canTimeDouble이 false일 때만 Time.timeScale = 0;이 작동한다. 왜지?
            Time.timeScale = 2;
        else
            Time.timeScale = 1;
    }
    public Transform GetUnitSpawnPosition()
    {
        return unitSpawnPosition;
    }
    public Transform GetEnemyUnitSpawnPosition()
    {
        return enemyUnitSpawnPosition;
    }
    public bool GetMapSliderMoving()
    {
        return mapSliderMoving;
    }
    public bool GetUiClicked()
    {
        return uiClicked;
    }
    public void SetMapSliderMoving(bool a)
    {
        mapSliderMoving = a;
    }
    public void SetUiClicked(bool a)
    {
        uiClicked = a;
    }
}
