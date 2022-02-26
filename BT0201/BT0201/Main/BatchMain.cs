using BT0201.DBClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace BT0201Batch
{
    class BatchMain : BatchBase
    {

        //01:実行待ち 02:実行中 03:完了 04:完了(警告あり) 05:エラー
        private const string SEARCH_STATUS_CD_RUNNING = "02";
        private const string SEARCH_STATUS_CD_RUNNING_NAME = "実行中";
        private const string SEARCH_STATUS_CD_COMPLETED = "03";
        private const string SEARCH_STATUS_CD_COMPLETED_NAME = "完了";
        private const string SEARCH_STATUS_CD_HASWARN = "04";
        private const string SEARCH_STATUS_CD_HASWARN_NAME = "完了(警告あり)";
        private const string SEARCH_STATUS_CD_FAILED = "05";
        private const string SEARCH_STATUS_CD_FAILED_NAME = "エラー";

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// 類似度計算バッチ処理
        /// </summary>
        public override bool MainProc()
        {
            //二重起動をチェックする
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                //すでに起動していると判断して終了
                CLogger.Logger("ERR_MULTIPROCESS_PROHIBIT");
                return false;
            }

            try
            {
                ////////////////////////////////////////////////////
                //類似度計算対象の取得
                ////////////////////////////////////////////////////
                IList<Hashtable> targets =GetSimilarDiagramSeachId();

                if (targets == null || targets.Count < 1)
                {
                    BatchBase.dtPubNoBatchEnd = DateTime.Now;
                    BatchBase.AppendErrMsg("INFO_NO_TARGET");
                    BatchBase.WriteErrMsg_DB();
                    CLogger.Logger("INFO_NO_TARGET");

                    return true;
                }

                ////////////////////////////////////////////////////
                /// 類似回路検索ID単位で計算
                ////////////////////////////////////////////////////
                foreach(Hashtable searchId in targets)
                {
                   try
                    {
                        dtPubNoBatchStart = DateTime.Now;
                        // 類似回路検索IDごとトランザクション開始
                        db.Begin();
                        ////////////////////////////////////////////////////
                        /// DBを処理を開始ステータスに変更する
                        ////////////////////////////////////////////////////
                        bool updateRslt = UpdateStartSearchStatus(
                            Convert.ToInt32(searchId["similar_diagram_search_id"].ToString()));

                        if (!updateRslt)
                        {
                            //ステータスさえ更新できない場合、DBに問題あるともみなし処理を終了する
                            db.Rollback();
                            //ログメッセージDB書き込み
                            BatchBase.dtPubNoBatchEnd = DateTime.Now;
                            BatchBase.WriteErrMsg_DB();
                            return false;
                        }
                        ////////////////////////////////////////////////////
                        /// 結線類似度の計算
                        ////////////////////////////////////////////////////
                        bool calculateRslt = CalculateSimilarDiagramBySeachId(Convert.ToInt32(searchId["similar_diagram_search_id"].ToString()));

                        if (!calculateRslt)
                            UpdateEndSearchStatus(Convert.ToInt32(searchId["similar_diagram_search_id"].ToString()),
                                            SEARCH_STATUS_CD_FAILED);
                        else
                        {
                            ////////////////////////////////////////////////////
                            /// 結線類似度の登録
                            ////////////////////////////////////////////////////
                            bool registRslt = UpdateSimilarDiagramSearchResult(Convert.ToInt32(searchId["similar_diagram_search_id"].ToString()));

                            UpdateEndSearchStatus(Convert.ToInt32(searchId["similar_diagram_search_id"].ToString()),
                                   SEARCH_STATUS_CD_COMPLETED);
                        }



                        //ログメッセージDB書き込み
                        BatchBase.dtPubNoBatchEnd = DateTime.Now;
                        BatchBase.WriteErrMsg_DB();
                        db.Commit();
                    }
                    catch (Exception ex)
                    {
                        db.Rollback();
                        BatchBase.AppendErrMsg(ex.Message);
                        //ログメッセージDB書き込み
                        BatchBase.dtPubNoBatchEnd = DateTime.Now;
                        db.Begin();
                        BatchBase.WriteErrMsg_DB();
                        db.Commit();
                    }

                    // 次のPUBNOへに進む
                }
            }
            catch (Exception ex)
            {
                CLogger.Err(ex);
                BatchBase.WriteErrMsg_DB();
                return false;
            }
            return true;
        }
        /// <summary>
        /// 対象サーチャーIDの取得
        /// </summary>
        /// <returns></returns>
        private IList<Hashtable> GetSimilarDiagramSeachId()
        {
            IList<Hashtable> rslt = db.QueryForList<Hashtable>("SelectSimilarDiagramSearchId", null);
            return rslt;

        }
        /// <summary>
        /// サーチャーステータスの更新(開始)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private bool UpdateStartSearchStatus(int id)
        {
            CTSimilarDiagramSearch updateParams = new CTSimilarDiagramSearch();
            updateParams.similarDiagramSearchId = id;
            //02:実行中//03:終了
            updateParams.statusCd = SEARCH_STATUS_CD_RUNNING;
            updateParams.startDt = DateTime.Now;
            updateParams.updateDt = DateTime.Now;
            updateParams.updateUserId = null;

            try
            {
                db.Update("UpdateStartSearchStatus", updateParams);
            }
            catch (Exception ex)
            {
                CLogger.Err(ex);
                BatchBase.AppendErrMsg("WNG_DB_FAILED","類似回路検索のステータス更新");
                return false;
            }
         
            CLogger.Logger("INFO_SUCCESS", "類似回路検索のステータス更新：" + SEARCH_STATUS_CD_RUNNING_NAME);
            BatchBase.AppendErrMsg("INFO_SUCCESS", "類似回路検索のステータス更新：" + SEARCH_STATUS_CD_RUNNING_NAME);
            return true;

        }
        /// <summary>
        /// サーチャーステータスの更新(終了)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private bool UpdateEndSearchStatus(int id, string status)
        {
            CTSimilarDiagramSearch updateParams = new CTSimilarDiagramSearch();
            updateParams.similarDiagramSearchId = id;
            //03:完了 04:完了(警告あり) 05:エラー
            updateParams.statusCd = status;
            updateParams.updateDt = DateTime.Now;
            updateParams.endDt = DateTime.Now;
            updateParams.updateUserId = null;

            try
            {
                db.Update("UpdateEndSearchStatus", updateParams);
            }
            catch (Exception ex)
            {
                CLogger.Err(ex);
                BatchBase.AppendErrMsg("WNG_DB_FAILED", "類似回路検索のステータス更新");
                return false;
            }
            string name = "";
            switch (status)
            {
                case SEARCH_STATUS_CD_RUNNING:
                    name = SEARCH_STATUS_CD_RUNNING_NAME;
                    break;
                case SEARCH_STATUS_CD_COMPLETED:

                    name = SEARCH_STATUS_CD_COMPLETED_NAME;
                    break;
                case SEARCH_STATUS_CD_HASWARN:
                    name = SEARCH_STATUS_CD_HASWARN_NAME;
                    break;
                case SEARCH_STATUS_CD_FAILED:
                    name = SEARCH_STATUS_CD_FAILED_NAME;
                    break;
            }

            CLogger.Logger("INFO_SUCCESS", "類似回路検索のステータス更新：" + name);
            BatchBase.AppendErrMsg("INFO_SUCCESS", "類似回路検索のステータス更新：" + name);
            return true;

        }

        /// <summary>
        /// 類似度計算結果の登録
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool UpdateSimilarDiagramSearchResult(int id)
        {
            try
            {
                db.Update("UpdateSimilarDiagramSearchResult", id);
            }
            catch(Exception ex)
            {
                CLogger.Err(ex);
                BatchBase.AppendErrMsg("WNG_DB_FAILED", "類似度計算結果の登録");
                return false;
            }
            CLogger.Logger("INFO_SUCCESS", "類似度計算結果の登録");
            BatchBase.AppendErrMsg("INFO_SUCCESS", "類似度計算結果の登録");
            return true;

        }
        /// <summary>
        /// 類似度の計算
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool CalculateSimilarDiagramBySeachId(int id)
        {
            Hashtable selectParams = new Hashtable();
            selectParams.Add("search_id", id);
            selectParams.Add("from_parts_name_point", fromPartsNameDiffPoint);
            selectParams.Add("from_pin_no_point", pinNoDiffPoint);
            selectParams.Add("from_terminal_name_point", terminalNameDiffPoint);
            selectParams.Add("from_parts_code_point", partsCodeDiffPoint);
            selectParams.Add("to_parts_name_point", toPartsNameDiffPoint);
            selectParams.Add("to_pin_no_point", pinNoDiffPoint);
            selectParams.Add("to_terminal_name_point", terminalNameDiffPoint);
            selectParams.Add("to_parts_code_point", partsCodeDiffPoint);
            selectParams.Add("color_point", wireColorDiffPoint);
            selectParams.Add("min_point", minSimilarPoint);

            try
            {
               db.Update("CalculateSimilarWireDiagram", selectParams);
            }
            catch (Exception ex)
            {
                CLogger.Err(ex);
                BatchBase.AppendErrMsg("WNG_DB_FAILED", "類似度の計算");
                return false;
            }
            CLogger.Logger("INFO_SUCCESS", "類似度の計算");
            BatchBase.AppendErrMsg("INFO_SUCCESS", "類似度の計算");
            return true;

        }
    }
}
