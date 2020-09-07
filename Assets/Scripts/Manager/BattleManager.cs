﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Struct;

public class BattleManager : MonoBehaviour
{
    // 参数
    public float time_intoBattle;


    public enum State
    {
        None,
        BattleStart,
        TurnStart,
        TurnGoOn,
        TurnEnd,
        BattleEnd
    }
    public State BattleState { get; private set; }
    


    public static BattleManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        switch (BattleState)
        {
            case State.BattleStart:BattleStart();return;
            case State.TurnStart: TurnStart(); return;
            case State.TurnGoOn: TurnGoOn(); return;
            case State.TurnEnd: TurnEnd(); return;
            case State.BattleEnd: BattleEnd(); return;
        }

    }

    // 战斗中的单位
    public List<GameObject> actorsInBattle_list = new List<GameObject>();

    private List<GameObject> actorsQueue_list = new List<GameObject>();
    public ActorMono actor_curTurn;
    private int turnIndex = 0;

    // 其他数值
    public float moveCost_varCell;

    public void OnBattleStart()
    {
        BattleState = State.BattleStart;
        GameManager.instance.gameInputMode = GameManager.InputMode.play;

        UIManager.instance.ActiveUI("BattleMode",true);
        UIManager.instance.ActiveUI("BattleStartButton",false);
        UIManager.instance.ActiveUI("UIArea", true);

        UIManager.instance.SetActorsUI(actorsInBattle_list);

        foreach (var actor in actorsInBattle_list)
        {
            if (actor.GetComponent<ActorMono>().group == ActorMono.Group.monster)
                continue;

            actor.SendMessage("OnBattle");
            actor.SendMessage("InitDeck");
            actor.SendMessage("ShuffleDeck");
        }

        // 确定对战队列
        ActorSortByAdvantage sort = new ActorSortByAdvantage();
        actorsQueue_list = new List<GameObject>(actorsInBattle_list);
        actorsQueue_list.Sort(sort);

        actor_curTurn = actorsQueue_list[0].GetComponent<ActorMono>();
    }

    private class ActorSortByAdvantage : IComparer<GameObject>
    {
        public int Compare(GameObject x, GameObject y)
        {
            if (x.GetComponent<ActorMono>().advantage > y.GetComponent<ActorMono>().advantage)
                return 1;
            else if (x.GetComponent<ActorMono>().advantage == y.GetComponent<ActorMono>().advantage)
                return 0;
            else
                return -1;
        }
    }


    private void BattleStart()
    {
        Timer timer = new Timer();
        timer.TimerTick(Time.deltaTime);
        if (timer.IsOver(time_intoBattle))
            OnTurnStart();
    }

    private void OnTurnStart()
    {
        BattleState = State.TurnStart;

        if (actor_curTurn.group == ActorMono.Group.monster)
        {
            actor_curTurn.ResumeMovePoint();
            actor_curTurn.GetComponent<MonsterController>().TurnStart();
        }
        else
        {

            actor_curTurn.DiscardFocusedCard();
            actor_curTurn.FocusUp();
            actor_curTurn.DrawStartCard();
            actor_curTurn.ResumeActionPoint();
            actor_curTurn.ResumeMovePoint();
        }

        bool isPlayerTurn = (actor_curTurn.group == ActorMono.Group.player);

            UIManager.instance.ActiveUI("ActionPoint", isPlayerTurn);
            UIManager.instance.ActiveUI("MovePoint", isPlayerTurn);
            UIManager.instance.ActiveUI("UIArea", isPlayerTurn);
            UIManager.instance.ActiveUI("Hand", isPlayerTurn);
            UIManager.instance.ActiveUI("DeckNum", isPlayerTurn);
            UIManager.instance.ActiveUI("DisCardNum", isPlayerTurn);
            UIManager.instance.ActiveUI("FocusPoint", isPlayerTurn);

    }

    private void TurnStart()
    {
        OnTurnGoOn();
    }

    private void OnTurnGoOn()
    {
        BattleState = State.TurnGoOn;
    }

    List<Vector3> path_list = new List<Vector3>();
    private void TurnGoOn()
    {
        if(actor_curTurn.group == ActorMono.Group.monster)
        {
            return;
        }

        // 处于UI交互状态 或 动画状态
        if (UIManager.instance.IsAtUIArea || UIManager.instance.IsHandUsing || GameManager.instance.gameInputMode == GameManager.InputMode.animation)
        {
            UIManager.instance.InActivePath();
            return;
        }

        // 不处于UI交互状态

        // 路径搜索与显示
        do
        {
            if (!UIManager.instance.mouse.IsCellChanged)
            {
                break;
            }
            if(actor_curTurn.IsMoving)
            {
                break;
            }
            Vector3 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetPos.z = 0;

            path_list = PathFinderManager.instance.SearchPathTo(actor_curTurn.WorldPos, targetPos);

            UIManager.instance.ShowPath(path_list);
        }
        while (false);


        // 玩家不处于移动状态 而且按下左键 便开始移动
        if (Input.GetKeyDown(KeyCode.Mouse0) && !actor_curTurn.IsMoving)
        {
            actor_curTurn.StartMoveByList(path_list);
        }

        // 移动状态下取消移动
        if (actor_curTurn.IsMoving)
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                actor_curTurn.stopMoveByList = true;
            }
            return;
        }
    }

    public void OnTurnEnd()
    {
        BattleState = State.TurnEnd;

        if(actor_curTurn.group == ActorMono.Group.player)
            actor_curTurn.DiscardEndCard();


        turnIndex += 1;

        if (turnIndex >= actorsQueue_list.Count)
            turnIndex = 0;

        actor_curTurn = actorsQueue_list[turnIndex].GetComponent<ActorMono>();

        OnTurnStart();
    }

    private void TurnEnd()
    {

    }

    public void OnBattleEnd()
    {
        //UIManager.instance.ActiveUI("BattleMode", false);
        //UIManager.instance.ActiveUI("BattleStartButton", true);
        //UIManager.instance.ActiveUI("Hand", false);
        //UIManager.instance.ActiveUI("ActionPoint", false);
        //UIManager.instance.ActiveUI("MovePoint", false); UIManager.instance.ActiveUI("DeckNum", false);
        //UIManager.instance.ActiveUI("DisCardNum", false);
    }

    private void BattleEnd()
    {

    }


}
