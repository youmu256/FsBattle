using System;
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
                var save = Battle.ReplaySave;
                EndGame();
                if (save == null) return;
                //replay
                InitReplayBattle(save);
                StartGame();
            }
        }

        public void EndGame()
        {
            Battle?.CleanBattle();
            Battle = null;
            Player = null;
        }

        public void InitTestBattle()
        {
            Battle = new FsBattleGame();
            var startData = new FsBattleStartData();
            //TODO 统一Entity类+逻辑组件化 
            startData.PlayerTeamUnits.Add(new FsBattleStartUnitData(){TypeId = "player",UnitInitData = new FsUnitInitData()});
            startData.EnemyTeamUnits.Add(new FsBattleStartUnitData(){TypeId = "enemy",UnitInitData = new FsUnitInitData(){Euler = Vector3.zero,Position = Vector3.forward}});
            startData.EnemyTeamUnits.Add(new FsBattleStartUnitData(){TypeId = "enemy",UnitInitData = new FsUnitInitData(){Euler = Vector3.zero,Position = Vector3.back}});
            Battle.Init(LogicFps, 0,startData);
        }

        public void InitReplayBattle(FsBattleReplay replay)
        {
            Battle = new FsBattleGame();
            Battle.InitByReplay(replay);
        }
        
        public void StartGame()
        {
            Battle.StartBattle();
            Player = Battle.EntityService.Units.Find((unit => unit.Team == FsBattleLogic.PlayerTeam));
        }

        public FsUnitLogic Player { get; private set; }

        public FsEntityView PlayerView => Player.View as FsEntityView;

        private void Update()
        {
            if (Battle == null) return;
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
                var ray = GameCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit))
                {
                    //表现层存在滞后 玩家的操作基于表现层 如果与逻辑位置计算出方位就会存在误差
                    Vector3 dir = hit.point - PlayerView.CachedTransform.position;
                    dir.y = 0;
                    dir.Normalize();
                    //Debug.DrawRay(Player.Position,dir*100,Color.red);
                    cmd.FireYaw = Vector3.SignedAngle(Vector3.forward, dir, Vector3.up);
                    cmd.FireYaw = 0;
                }
            }
            return cmd;
        }
    }
}