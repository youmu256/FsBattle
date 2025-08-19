using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrameSyncBattle
{
    public class UnityLog : ILogger
    {
        public void LogError(object msg, object param)
        {
            Debug.LogError(msg,(UnityEngine.Object) param);
        }
        public void LogWarning(object msg, object param)
        {
            Debug.LogWarning(msg,(UnityEngine.Object) param);
        }
        public void Log(object msg, object param)
        {
            Debug.Log(msg,(UnityEngine.Object) param);
        }
    }

    public class FsBattleUnity : MonoBehaviour
    {
        public static FsBattleUnity Instance { get; private set; }

        #region PrefabRefs

        public GameObject PlayerModel;

        public GameObject BulletModel;

        #endregion
        
        public Camera GameCamera { get; private set; }
        
        private void Awake()
        {
            FsDebug.Set(new UnityLog());
            Instance = this;
            if (LimitRate)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 60;
            }
            GameCamera = Camera.main;
        }

        public FsBattleGame Battle { get; private set; }

        public int LogicFps = 20;
        public bool LimitRate = false;

        #region GameStart

        private void OnGUI()
        {
            if (GUILayout.Button("StartGame"))
            {
                EndGame();
                InitTestBattle();
                StartGame();
            }
            if (GUILayout.Button("EndGame&Replay"))
            {
                var save = Battle?.ReplaySave;
                EndGame();
                if (save == null) return;
                //replay
                InitReplayBattle(save);
                StartGame();
            }
            GUILayout.Button($"Lerp : {Battle?.ViewLerp}");
        }

        public void EndGame()
        {
            if (Battle != null)
            {
                FsDebug.Log("PLAY END!");
                FsDebug.Log(Battle.GetGameStateMsg());
                Battle.EndBattle();
            }
            Battle = null;
            Player = null;
        }

        #region TestData
        public static FsUnitPropertyInitData TestPlayerData = new FsUnitPropertyInitData()
        {
            HpMax = 100,
            MpMax = 100,
            Attack = 1,
            AttackRange = 10,
            BaseAttackInterval = 0.1f,
            Defend = 0,
            MoveSpeed = 5,
        };
        public static FsUnitPropertyInitData TestEnemyData = new FsUnitPropertyInitData()
        {
            HpMax = 10,
            MpMax = 10,
            Attack = 1,
            AttackRange = 5,
            BaseAttackInterval = 1f,
            Defend = 0,
            MoveSpeed = 2,
        };
        #endregion

        public void InitTestBattle()
        {
            Battle = new FsBattleGame();
            var startData = new FsBattleStartData();
            //TODO 统一Entity类+逻辑组件化 
            startData.PlayerTeamUnits.Add(new FsBattleStartUnitData()
            {
                TypeId = "player",
                UnitInitData = new FsUnitInitData()
                    {PropertyInitData = TestPlayerData, Euler = Vector3.zero, Position = Vector3.zero}
            });
            startData.EnemyTeamUnits.Add(new FsBattleStartUnitData()
            {
                TypeId = "enemy",
                UnitInitData = new FsUnitInitData()
                    {PropertyInitData = TestEnemyData, Euler = Vector3.zero, Position = Vector3.forward}
            });
            Battle.Init(LogicFps, 0, startData);
        }

        public void InitReplayBattle(FsBattleReplay replay)
        {
            Battle = new FsBattleGame();
            Battle.InitByReplay(replay);
        }
        
        public void StartGame()
        {
            FsDebug.Log("TestLogReDiction");
            Debug.Log("TestLogNormal");
            Battle.StartBattle();
            var list = new List<FsUnitLogic>();
            Battle.EntityService.CollectUnits(list);
            Player = list.Find((unit => unit.Team == FsBattleLogic.PlayerTeam));
        }

        public FsUnitLogic Player { get; private set; }

        public FsEntityView PlayerView
        {
            get
            {
                if(Battle.EntityViewsMap.ContainsKey(Player.Id))
                    return Battle.EntityViewsMap[Player.Id];
                return null;
            }
        }

        private void Update()
        {
            if (Battle == null) return;
            if (Battle.IsPlayEnd) return;
            FsCmd cmd = null;
            if (Battle.IsReplayMode == false)
                cmd = GetInputCmd();
            Battle.GameEngineUpdate(Time.deltaTime,cmd);
            var view = PlayerView;
            if (view != null)
            {
                var position = view.transform.position;
                GameCamera.transform.position = new Vector3(position.x, 10, position.z);
            }
        }

        #endregion
        
        public FsCmd GetInputCmd()
        {
            FsCmd cmd = new FsCmd();
            cmd.LogicFrameIndex = this.Battle.FrameIndex;
            if (Input.GetKey(KeyCode.W))
                cmd.Buttons |= FsButton.W;

            if (Input.GetKey(KeyCode.S))
                cmd.Buttons |= FsButton.S;

            if (Input.GetKey(KeyCode.A))
                cmd.Buttons |= FsButton.A;

            if (Input.GetKey(KeyCode.D))
                cmd.Buttons |= FsButton.D;

            if (Input.GetMouseButton(0))
            {
                cmd.Buttons |= FsButton.Fire;
                if (PlayerView != null)
                {
                    var ray = GameCamera.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out var hit))
                    {
                        //表现层存在滞后 玩家的操作基于表现层 如果与逻辑位置计算出方位就会存在误差
                        Vector3 dir = hit.point - PlayerView.CachedTransform.position;
                        dir.y = 0;
                        dir.Normalize();
                        cmd.FireYaw = Vector3.SignedAngle(Vector3.forward, dir, Vector3.up);
                    }

                }
            }
            return cmd;
        }
    }
}