using System;
using FrameSyncBattle;

namespace ConsoleApp1
{
    public class FsBattleServerLogger : ILogger
    {
        public void LogError(object msg, object param)
        {
            Console.WriteLine(msg);
        }

        public void LogWarning(object msg, object param)
        {
            Console.WriteLine(msg);
        }

        public void Log(object msg, object param)
        {
            Console.WriteLine(msg);
        }
    }
    
    public class FsBattleServerSimulator
    {
        public void TestBattle()
        {
            FsBattleLogic battleLogic = new FsBattleGame();
            battleLogic.Init(15,0,new FsBattleStartData());
            FsDebug.Set(new FsBattleServerLogger());
            FsDebug.Log("Battle Inited");
        }
    }
}