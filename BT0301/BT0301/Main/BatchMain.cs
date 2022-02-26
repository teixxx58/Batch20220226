using BT0301.DBClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace BT0301Batch
{
    class BatchMain : BatchBase
    {

        //01:実行待ち 02:実行中 03:完了 04:完了(警告あり) 05:エラー
        private const string STATUS_CD_RUNNING = "02";
        private const string STATUS_CD_RUNNING_NAME = "実行中";
        private const string STATUS_CD_COMPLETED = "03";
        private const string STATUS_CD_COMPLETED_NAME = "完了";
        private const string STATUS_CD_HASWARN = "04";
        private const string STATUS_CD_HASWARN_NAME = "完了(警告あり)";
        private const string STATUS_CD_FAILED = "05";
        private const string STATUS_CD_FAILED_NAME = "エラー";

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// 雛形回路作成バッチ処理
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
                //雛形回路作成対象の取得
                ////////////////////////////////////////////////////
                IList<Hashtable> targets = GetCreateTemplateId();

                if (targets == null || targets.Count < 1)
                {
                    BatchBase.dtCreateEnd = DateTime.Now;
                    BatchBase.AppendErrMsg("INFO_NO_TARGET");
                    BatchBase.WriteErrMsg_DB();
                    CLogger.Logger("INFO_NO_TARGET");

                    return true;
                }

                ////////////////////////////////////////////////////
                /// テンプレート作成ID単位で計算
                ////////////////////////////////////////////////////
                foreach(Hashtable searchId in targets)
                {
                   try
                    {
                        dtCreateStart = DateTime.Now;
                        // テンプレート作成IDごとトランザクション開始
                        db.Begin();
                        ////////////////////////////////////////////////////
                        /// DBを処理を開始ステータスに変更する
                        ////////////////////////////////////////////////////
                        bool updateRslt = UpdateStartCreateTemplateStatus(
                            Convert.ToInt32(searchId["create_template_id"].ToString()));

                        if (!updateRslt)
                        {
                            //ステータスさえ更新できない場合、DBに問題あるともみなし処理を終了する
                            db.Rollback();
                            //ログメッセージDB書き込み
                            BatchBase.dtCreateEnd = DateTime.Now;
                            BatchBase.WriteErrMsg_DB();
                            return false;
                        }
                        ////////////////////////////////////////////////////
                        /// 類似WIRE_IDに対して、配線図と、結線の情報についての取得
                        ////////////////////////////////////////////////////
                        IList<Hashtable> wireInfos = SearchSimilarWireInfo(Convert.ToInt32(searchId["create_template_id"].ToString()));

                        if (wireInfos == null)
                            UpdateEndCreateTemplateStatus(Convert.ToInt32(searchId["create_template_id"].ToString()),
                                            STATUS_CD_FAILED);
                        else
                        {
                            ////////////////////////////////////////////////////
                            /// 類似度が高いものから結線IDの割り当て
                            ////////////////////////////////////////////////////
                            SimilarCircuit similarCircuit = new SimilarCircuit();
                            IList<Hashtable>  assingedWires = similarCircuit.AssignWireId(wireInfos);


                            foreach (Hashtable wire in assingedWires)
                            {
                                ////////////////////////////////////////////////////
                                /// 対象となった画像ファイルごとに朱書きを実施
                                ////////////////////////////////////////////////////
                                if (SimilarCircuit.ASSING_FLG_TRUE.Equals(wire["assingFlg"].ToString()) )
                                {


                                }
                                ////////////////////////////////////////////////////
                                /// 追加結線のファイルを作成する
                                ////////////////////////////////////////////////////
                                else
                                {


                                }


                            }

                            ///






                            bool registRslt = UpdateSimilarDiagramSearchResult(Convert.ToInt32(searchId["create_template_id"].ToString()));

                            UpdateEndCreateTemplateStatus(Convert.ToInt32(searchId["create_template_id"].ToString()),
                                   STATUS_CD_COMPLETED);
                        }



                        //ログメッセージDB書き込み
                        BatchBase.dtCreateEnd = DateTime.Now;
                        BatchBase.WriteErrMsg_DB();
                        db.Commit();
                    }
                    catch (Exception ex)
                    {
                        db.Rollback();
                        BatchBase.AppendErrMsg(ex.Message);
                        //ログメッセージDB書き込み
                        BatchBase.dtCreateEnd = DateTime.Now;
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
        /// テンプレート作成対象IDの取得
        /// </summary>
        /// <returns></returns>
        private IList<Hashtable> GetCreateTemplateId()
        {
            IList<Hashtable> rslt = db.QueryForList<Hashtable>("SelectCreateTemplateId");
            return rslt;

        }
        /// <summary>
        /// テンプレート作成ステータスの更新(開始)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private bool UpdateStartCreateTemplateStatus(int id)
        {
            CTCreateTemplate updateParams = new CTCreateTemplate();
            updateParams.createTemplate_id = id;
            //02:実行中//03:終了
            updateParams.statusCd = STATUS_CD_RUNNING;
            updateParams.startDt = DateTime.Now;
            updateParams.updateDt = DateTime.Now;
            updateParams.updateUserId = null;

            try
            {
                db.Update("UpdateStartCreateTemplateStatus", updateParams);
            }
            catch (Exception ex)
            {
                CLogger.Err(ex);
                BatchBase.AppendErrMsg("WNG_DB_FAILED","テンプレート作成のステータス更新");
                return false;
            }
         
            CLogger.Logger("INFO_SUCCESS", "テンプレート作成のステータス更新：" + STATUS_CD_RUNNING_NAME);
            BatchBase.AppendErrMsg("INFO_SUCCESS", "テンプレート作成のステータス更新：" + STATUS_CD_RUNNING_NAME);
            return true;

        }
        /// <summary>
        /// テンプレート作成ステータスの更新(終了)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private bool UpdateEndCreateTemplateStatus(int id, string status)
        {
            CTCreateTemplate updateParams = new CTCreateTemplate();
            updateParams.createTemplate_id = id;
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
                BatchBase.AppendErrMsg("WNG_DB_FAILED", "テンプレート作成のステータス更新");
                return false;
            }
            string name = "";
            switch (status)
            {
                case STATUS_CD_RUNNING:
                    name = STATUS_CD_RUNNING_NAME;
                    break;
                case STATUS_CD_COMPLETED:

                    name = STATUS_CD_COMPLETED_NAME;
                    break;
                case STATUS_CD_HASWARN:
                    name = STATUS_CD_HASWARN_NAME;
                    break;
                case STATUS_CD_FAILED:
                    name = STATUS_CD_FAILED_NAME;
                    break;
            }

            CLogger.Logger("INFO_SUCCESS", "テンプレート作成のステータス更新：" + name);
            BatchBase.AppendErrMsg("INFO_SUCCESS", "テンプレート作成のステータス更新：" + name);
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
        /// 類似経線情報の取得
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private IList<Hashtable> SearchSimilarWireInfo(int id)
        {
            Hashtable selectParams = new Hashtable();
            selectParams.Add("createTplId", id);

            selectParams.Add("min_point", targetSyugakiMinPoint);
            IList<Hashtable> wireInfos;
            try
            {
               wireInfos =  db.QueryForList<Hashtable>("SearchSimilarWireInfo", selectParams);
            }
            catch (Exception ex)
            {
                CLogger.Err(ex);
                BatchBase.AppendErrMsg("WNG_DB_FAILED", "類似経線情報の取得");
                return null;
            }
            CLogger.Logger("INFO_SUCCESS", "類似経線情報の取得");
            BatchBase.AppendErrMsg("INFO_SUCCESS", "類似経線情報の取得");
            return wireInfos;

        }
    }
}
