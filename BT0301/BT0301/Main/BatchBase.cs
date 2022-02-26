using BT0301.DBClass;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace BT0301Batch
{
    public abstract class BatchBase
    {
        /// <summary>
        /// データベースアクセス
        /// </summary>
        protected static DatabaseHelper db;

        /// バッチエラーメッセージ
        protected static List<CTBatchError> errorMessages = new List<CTBatchError>();
        //雛形回路作成の開始時間
        public static DateTime dtCreateStart;
        //雛形回路作成の終了時間
        public static DateTime dtCreateEnd;

        //朱書き対象とする最低の類似度
        public static int targetSyugakiMinPoint = Convert.ToInt32(ConfigurationManager.AppSettings["TARGET_SYUGAKI_MIN_POINT"]);
        //朱書きする際のフォントサイズ
        public static int syugakiFontSize = Convert.ToInt32(ConfigurationManager.AppSettings["SYUGAKI_FONT_SIZE"]);
        //朱書きする位置に対しての補正値		
        public static int syugakiOffsetSize = Convert.ToInt32(ConfigurationManager.AppSettings["SYUGAKI_OFFSET_SIZE"]);
        //テンプレートファイル保存場所		
        public static string loacleDataDir = ConfigurationManager.AppSettings["LOCAL_DATA_DIR"];
        //雛形ファイルを作成するフォルダ	
        public static string hinagataDir = ConfigurationManager.AppSettings["HINAGATA_DIR"];

        /// <summary>
        /// バッチメイン処理
        /// </summary>
        /// <returns>true:正常終了、false:異常終了</returns>
        public abstract bool MainProc();

        /// <summary>
        /// メイン処理を呼び出す
        /// </summary>
        public void Run()
        {
            db = new DatabaseHelper();
            try
            {
                db.Open();

                //類似度計算
                bool isSuccess = MainProc();
            }
            catch (Exception ex)
            {
                //DBエラーであれば、終了
                throw(ex);
            }
            finally
            {
                db.Close();
            }
        }

        /// <summary>
        /// エラーメッセージ書き込み処理
        /// </summary>
        /// <param/>
        public static void WriteErrMsg_DB()
        {
            // エラーメッセージ登録
            try
            {
                //終了時刻
                int errCnt = 0;
                bool swich = false;
                CTBatchError reacord = new CTBatchError();
                reacord.errorKbn = "02";
                foreach (CTBatchError rec in errorMessages)
                {
                    if (rec.errorKbn == "01")
                        errCnt++;
                    if(errCnt > 0 && !swich)
                    {
                        swich = true;
                        //エラーが存在する場合、1件目のエラー内容
                        reacord.errorTitle = rec.errorTitle + "他";
                        reacord.errorKbn = "01";
                    }
                    reacord.errorDetail += rec.errorDetail + "\r\n"; 
                }
                //警告のみの場合、1件目の警告内容
                if (errCnt < 1)
                {
                    reacord.errorKbn = "02";
                    reacord.errorTitle = errorMessages[0].errorTitle + "他";
                }
                reacord.startDt = dtCreateStart;
                reacord.endDt = dtCreateEnd;
                //SQLの実行
                db.Insert("InsertBatchError", reacord);
                ClearErrMsg();
            }
            catch (Exception ex)
            {
                CLogger.Err(ex);
            }
        }

        /// <summary>
        /// エラーメッセージ追加
        /// </summary>
        /// <param/>
        public static void AppendErrMsg(string msgID, params object[] list)
        {
            CTBatchError errRecord = new CTBatchError();

             if (msgID.StartsWith("ERR_"))
            {
                errRecord.errorKbn = "01";
                errRecord.errorTitle = "エラー";
                errRecord.errorDetail = String.Format(Messages.ResourceManager.GetString(msgID), list);
            }
            else if(msgID.StartsWith("WNG_"))
            {
                errRecord.errorKbn = "02";
                errRecord.errorTitle = "警告";
                errRecord.errorDetail = String.Format(Messages.ResourceManager.GetString(msgID), list);
            }
            else if (msgID.StartsWith("INFO_"))
            {
                errRecord.errorKbn = "02";
                errRecord.errorTitle = "インフォメーション";
                errRecord.errorDetail = String.Format(Messages.ResourceManager.GetString(msgID), list);
            }
            else if (msgID.StartsWith("Validate_"))
            {
                errRecord.errorKbn = "02";
                errRecord.errorTitle = "警告";
                errRecord.errorDetail = String.Format(Messages.ResourceManager.GetString(msgID), list);
            }
            else 
            {
                errRecord.errorKbn = "01";
                errRecord.errorTitle = "エラー";
                errRecord.errorDetail = msgID;
            }

            // バッチ開始、終了日時;
            errRecord.startDt = dtCreateStart;
            //errRecord.endDt = DateTime.Now;

            errRecord.insertUserId = null;
            errRecord.updateUserId = null;

            errorMessages.Add(errRecord);
        }

        /// <summary>
        /// エラーメッセージクリア
        /// </summary>
        public static void ClearErrMsg()
        {
            errorMessages = new List<CTBatchError>();
        }
    }
}
