using UnityEngine;

namespace FrameSyncBattle
{
    public class FsBattleUnity : MonoBehaviour
    {

        public FsBattleGame Battle { get; private set; }
        
        #region GameStart

        private void OnGUI()
        {
            if (GUILayout.Button("StartGame"))
            {
                StartGame();
            }
        }

        public void StartGame()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Battle = new FsBattleGame(20);
            Battle.StartBattle(null);
            
        }

        private void Update()
        {
            if (Battle == null) return;
            Battle.GameEngineUpdate(Time.deltaTime,GetInputCmd());
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

            if (Input.GetMouseButtonDown(0))
                cmd.Buttons |= FsButton.Fire;
            return cmd;
        }
    }
}