using System;
using NLog;

namespace OhmGraphite
{
    public static class LoggerUtils
    {
        public static void LogAction(this Logger logger, string msg, Action action)
        {
            try
            {
                logger.Debug("Starting: {0}", msg);
                action();
                logger.Debug("Finished: {0}", msg);
            }
            catch (Exception e)
            {
                logger.Error(e, "Exception: {0}", msg);
                throw new Exception($"Exception with {msg}", e);
            }
        }

        public static T LogFunction<T>(this Logger logger, string msg, Func<T> fn)
        {
            try
            {
                logger.Debug("Starting: {0}", msg);
                var result = fn();
                logger.Debug("Finished: {0}", msg);
                return result;
            }
            catch (Exception e)
            {
                logger.Error(e, "Exception: {0}", msg);
                throw new Exception($"Exception with {msg}", e);
            }
        }
    }
}
