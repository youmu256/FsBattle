namespace FrameSyncBattle
{
    public interface ILogger
    {
        public void LogError(object msg, object param);
        public void LogWarning(object msg, object param);
        public void Log(object msg, object param);
    }
    
    public static class FsDebug
    {
        private static ILogger _helper;

        public static void Set(ILogger logger)
        {
            _helper = logger;
        }
        
        public static void LogError(object msg,object param = null)
        {
            _helper?.LogError(msg,param);
        }

        public static void LogWarning(object msg, object param = null)
        {
            _helper?.LogWarning(msg,param);
        }
        
        public static void Log(object msg,object param = null)
        {
            _helper?.Log(msg,param);
        }
    }
    
}