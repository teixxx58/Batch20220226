using BT0301.DBClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
                        /// DBを処理開始ステータスに変更する
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

                            Dictionary<string, Dictionary<string, List<Hashtable>>> assingedWires =
                                similarCircuit.GetSyugakiWireids(wireInfos);


                            foreach (string key in assingedWires["SYUGAKI"].Keys)
                            {
                                ////////////////////////////////////////////////////
                                /// 対象となった画像ファイルごとに朱書きを実施
                                ////////////////////////////////////////////////////
                                List<Hashtable> syugakiFigs = assingedWires["SYUGAKI"][key];
                                string pubNo = syugakiFigs[0]["pub_no"].ToString();

                                string figPath = loacleDataDir + pubNo + svgDir + key + ".svg";
                                string pdfPath = hinagataDir + searchId["create_template_id"].ToString() +
                                    "\\PDF\\" + key + ".pdf";
                                string svgPath = hinagataDir + searchId["create_template_id"].ToString() +
                                    "\\SVG\\" + key + ".svg";

                                if (!File.Exists(figPath))
                                {
                                    CLogger.Logger("ERR_FILE_READ", figPath);
                                    BatchBase.AppendErrMsg("ERR_FILE_READ", figPath);
                                    continue;
                                }
                                //朱書きファイルの読み込む
                                Syugaki syugaki = new Syugaki(figPath);
                                syugaki.RedDraws(syugakiFigs);

                                //結線削除
                                syugaki.DeleteRedDraws(assingedWires["ADDWIRES"][key]);

                                //朱書きが完了したファイル保存
                                syugaki.SVGSave(svgPath);

                                //朱書きしたファイルについてPDFを作成する
                                PDFUtil.GeneratePDF(svgPath, pdfPath);

                                //更新DB（朱書き）
                                Hashtable syugakiFile = new Hashtable
                                {
                                    {"create_svg_file_name" , svgPath},
                                    {"create_pdf_file_name",pdfPath},
                                    {"create_template_image_id", syugakiFigs[0]["create_template_image_id"].ToString()},
                                };
                                UpdateSyugaki(syugakiFile);
                            }
                            ////////////////////////////////////////////////////
                            /// 追加ファイルを作成する
                            ////////////////////////////////////////////////////
                            //SVGテンプレート
                            string templateFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @".\template\AddTemplate.svg");
                            //追加ファイル名
                            string addSvgFile = BatchBase.hinagataDir + "\\" + searchId["create_template_id"].ToString() + 
                                "\\SVG\\ADD_SVG.svg";
                            string addPdfFile = BatchBase.hinagataDir + "\\" + searchId["create_template_id"].ToString() +
                                "\\PDF\\ADD_SVG.pdf";

                            IList<Hashtable> addInfo = SearchAddFileWireInfo(assingedWires);

                            Dictionary<string, List<Hashtable>> dicAdd = new Dictionary<string, List<Hashtable>>();
                            foreach (Hashtable wire in addInfo)
                            {
                                //パーツペア辞書化
                                if (!dicAdd.Keys.Contains(wire["row_num"].ToString()))
                                {
                                    List<Hashtable> addwires = new List<Hashtable>();
                                    addwires.Add(wire);
                                    dicAdd.Add(wire["row_num"].ToString(), addwires);
                                }
                                else
                                {
                                    dicAdd[wire["row_num"].ToString()].Add(wire);
                                }
                            }
                            //追加ファイル作成
                            AddFile addfile = new AddFile(templateFile);
                            addfile.GenerateAddFigDiagramFile(dicAdd);

                            //追加ファイル保存
                            addfile.SVGSave(addSvgFile);

                            //朱書きしたファイルについてPDFを作成する
                            PDFUtil.GeneratePDF(addSvgFile, addPdfFile);

                            //更新DB（追加ファイル）
                            Hashtable addFile = new Hashtable
                            {
                                {"create_svg_file_name" , addSvgFile},
                                {"create_pdf_file_name",addPdfFile},
                                { "create_template_id", searchId["create_template_id"].ToString()},

                            };
                            UpdateAddFile(addFile);
                            
                            //状態更新
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

                    // 次のテンプレート作成IDへ
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
                BatchBase.AppendErrMsg("ERR_DB_FAILED","テンプレート作成のステータス更新");
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
                BatchBase.AppendErrMsg("ERR_DB_FAILED", "テンプレート作成のステータス更新");
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
        /// 朱書き結果の登録
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool UpdateSyugaki(Hashtable syugaki)
        {
            try
            {
                db.Update("UpdateSyugaki", syugaki);
            }
            catch(Exception ex)
            {
                CLogger.Err(ex);
                BatchBase.AppendErrMsg("ERR_DB_FAILED", "朱書きの登録");
                return false;
            }
            CLogger.Logger("INFO_SUCCESS", "朱書きの登録");
            BatchBase.AppendErrMsg("INFO_SUCCESS", "朱書きの登録");
            return true;

        }

        /// <summary>
        /// 追加ファイル結果の登録
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool UpdateAddFile(Hashtable addFile)
        {
            try
            {
                db.Update("UpdateAddFile", addFile);
            }
            catch (Exception ex)
            {
                CLogger.Err(ex);
                BatchBase.AppendErrMsg("ERR_DB_FAILED", "追加ファイルの登録");
                return false;
            }
            CLogger.Logger("INFO_SUCCESS", "追加ファイルの登録");
            BatchBase.AppendErrMsg("INFO_SUCCESS", "追加ファイルの登録");
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
                BatchBase.AppendErrMsg("ERR_DB_FAILED", "類似経線情報の取得");
                return null;
            }
            CLogger.Logger("INFO_SUCCESS", "類似経線情報の取得");
            BatchBase.AppendErrMsg("INFO_SUCCESS", "類似経線情報の取得");
            return wireInfos;

        }

        /// <summary>
        /// 追加ファイルの結線情報の取得
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private IList<Hashtable> SearchAddFileWireInfo(Dictionary<string,Dictionary<string, List<Hashtable>>> assingedWires)
        {
            List<Hashtable> selectParams = new List<Hashtable>();
            foreach (string fig in assingedWires["ADDWIRES"].Keys)
            {
                foreach (Hashtable rec in assingedWires["ADDWIRES"][fig])
                {
                    selectParams.Add(new Hashtable { { "wire_list_detail_id", rec["wire_list_detail_id"] }, });
                }
            }
            IList<Hashtable> wireInfos;
            try
            {
                wireInfos = db.QueryForList<Hashtable>("SearchAddWireInfo", selectParams);
            }
            catch (Exception ex)
            {
                CLogger.Err(ex);
                BatchBase.AppendErrMsg("ERR_DB_FAILED", "追加ファイルの結線情報の取得");
                return null;
            }
            CLogger.Logger("INFO_SUCCESS", "追加ファイルの結線情報の取得");
            BatchBase.AppendErrMsg("INFO_SUCCESS", "追加ファイルの結線情報の取得");
            return wireInfos;

        }
    }
}
