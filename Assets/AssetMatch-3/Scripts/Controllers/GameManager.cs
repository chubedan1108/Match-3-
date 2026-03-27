using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action<eStateGame> StateChangedAction = delegate { };

    public enum eLevelMode
    {
        TIMER,
        MOVES
    }

    public enum eStateGame
    {
        SETUP,
        MAIN_MENU,
        GAME_STARTED,
        PAUSE,
        GAME_OVER,
        GAME_WIN
    }

    private eStateGame m_state;
    public eStateGame State
    {
        get { return m_state; }
        private set
        {
            m_state = value;

            StateChangedAction(m_state);
        }
    }


    private GameSettings m_gameSettings;


    private BoardController m_boardController;

    private UIMainManager m_uiMenu;

    private LevelCondition m_levelCondition;
    private bool m_isGameEnded = false;

    private void Awake()
    {
        State = eStateGame.SETUP;

        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);

        m_uiMenu = FindObjectOfType<UIMainManager>();
        m_uiMenu.Setup(this);
    }

    void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_boardController != null) m_boardController.Update();
    }


    internal void SetState(eStateGame state)
    {
        State = state;

        if(State == eStateGame.PAUSE)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }

    public void LoadLevel(eLevelMode mode)
    {
        m_isGameEnded = false;

        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        m_boardController.StartGame(this, m_gameSettings);
        m_boardController.OnWinEvent += OnWin;
        m_boardController.OnLoseEvent += OnLose;

        if (mode == eLevelMode.MOVES)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelMoves>();
            m_levelCondition.Setup(m_gameSettings.LevelMoves, m_uiMenu.GetLevelConditionView(), m_boardController);
        }
        else if (mode == eLevelMode.TIMER)
        {
            m_levelCondition = this.gameObject.AddComponent<LevelTime>();
            m_levelCondition.Setup(m_gameSettings.LevelMoves, m_uiMenu.GetLevelConditionView(), this);
        }

        m_levelCondition.ConditionCompleteEvent += OnConditionLose;

        State = eStateGame.GAME_STARTED;
    }

    private void OnWin()
    {
        if (m_isGameEnded) return;

        m_isGameEnded = true;
        StartCoroutine(EndGameRoutine(true));
    }

    private void OnLose()
    {
        if (m_isGameEnded) return;

        m_isGameEnded = true;
        StartCoroutine(EndGameRoutine(false));
    }

    private void OnConditionLose()
    {
       
        if (m_isGameEnded) return;

        m_isGameEnded = true;
        StartCoroutine(EndGameRoutine(false));
    }

   

    private IEnumerator EndGameRoutine(bool isWin)
    {
        // chờ animation xong
        while (m_boardController != null && m_boardController.IsBusy)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // set state đúng
        State = isWin ? eStateGame.GAME_WIN : eStateGame.GAME_OVER;

        CleanupLevel();
    }

   
    private void CleanupLevel()
    {
        if (m_levelCondition != null)
        {
            m_levelCondition.ConditionCompleteEvent -= OnConditionLose;
            Destroy(m_levelCondition);
            m_levelCondition = null;
        }
    }

    internal void ClearLevel()
    {
        if (m_boardController != null)
        {
            m_boardController.Clear();
            Destroy(m_boardController.gameObject);
            m_boardController = null;
        }

        CleanupLevel();
    }
}
