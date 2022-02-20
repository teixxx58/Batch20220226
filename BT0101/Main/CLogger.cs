using BT0101;
using log4net;
using System;
using System.Reflection;

namespace BT0101Batch
{
    class CLogger
    {
        /// <summary>
        /// ロガー取得
        /// </summary>
        /// <returns></returns>
        private static ILog GetLogger()
        {
            ILog logger = LogManager.GetLogger(Assembly.GetExecutingAssembly().GetName().Name);
            return logger;
        }
        /// <summary>
        /// ロガーログ出力
        /// </summary>
        /// <param name="msgID"></param>
        /// <param name="list"></param>
        public static void Logger(string msgID,params object[] list)
        {

            if (msgID.Contains("DBG_"))
            {
                CLogger.GetLogger().Debug(String.Format(Messages.ResourceManager.GetString(msgID), list));
            }
            else if (msgID.Contains("INFO_"))
            {
                CLogger.GetLogger().Info(String.Format(Messages.ResourceManager.GetString(msgID), list));
            }
            else if (msgID.Contains("WNG_"))
            {
                CLogger.GetLogger().Warn(String.Format(Messages.ResourceManager.GetString(msgID), list));
            }
            else if (msgID.Contains("ERR_"))
            {
                CLogger.GetLogger().Error(String.Format(Messages.ResourceManager.GetString(msgID), list));
            }
            else
            {
                CLogger.GetLogger().Error(msgID);
            }

        }
        /// <summary>
        /// Fatalログ
        /// </summary>
        /// <param name="ex"></param>
        public static void Fatal(Exception ex)
        {
            CLogger.GetLogger().Fatal(ex);
        }
        /// <summary>
        /// Errログ
        /// </summary>
        /// <param name="ex"></param>
        public static void Err(Exception ex)
        {
            CLogger.GetLogger().Error(ex);
        }
        /// <summary>
        /// Warnログ
        /// </summary>
        /// <param name="ex"></param>
        public static void Warn(Exception ex)
        {
            CLogger.GetLogger().Warn(ex);
        }
        /// <summary>
        /// Infoログ
        /// </summary>
        /// <param name="ex"></param>
        public static void Info(Exception ex)
        {
            CLogger.GetLogger().Info(ex);
        }
        /// <summary>
        /// Debugログ
        /// </summary>
        /// <param name="ex"></param>
        public static void Debug(Exception ex)
        {
            CLogger.GetLogger().Debug(ex);
        }
        /// <summary>
        /// Fatalメッセージログ
        /// </summary>
        /// <param name="message"></param>

        public static void Fatal(string message)
        {
            CLogger.GetLogger().Fatal(message);
        }
        /// <summary>
        /// エラーメッセージログ
        /// </summary>
        /// <param name="message"></param>
        public static void Err(string message)
        {
            CLogger.GetLogger().Error(message);
        }
        /// <summary>
        /// Warnメッセージログ
        /// </summary>
        /// <param name="message"></param>
        public static void Warn(string message)
        {
            CLogger.GetLogger().Warn(message);
        }
        /// <summary>
        /// INFOメッセージログ
        /// </summary>
        /// <param name="message"></param>
        public static void Info(string message)
        {
            CLogger.GetLogger().Info(message);
        }
        /// <summary>
        /// Debugメッセージログ
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(string message)
        {
            CLogger.GetLogger().Debug(message);
        }

    }

}

