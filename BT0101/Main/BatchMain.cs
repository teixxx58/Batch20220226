using BT0101.DBClass;
using BT0101.FileClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace BT0101Batch
{
    class BatchMain : BatchBase
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// 過去データ取り込みバッチ処理
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
                // バッチの実行開始日時
                dtPubNoBatchStart = DateTime.Now;
                CFBase.SetMapper(db);

                ////////////////////////////////////////////////////
                //Server側ファイルリスト取り込み
                // 取得元ファイルリスト取得＆ファイルコピー
                ////////////////////////////////////////////////////
                FileManager.SetMapper(db);
                FileManager.UpDateLocalFiles(FileManager.fileServerPath, FileManager.localDataDir);


               ////////////////////////////////////////////////////
                //設計チェック送付Ref_No管理表データ取り込み
                ////////////////////////////////////////////////////
               if (FileManager.hasModifiedProjectFile)
                {
                    CFXLS  proFile = new CFXLS();
                    proFile.ImportFiles();
                }
                else
                {
                    BatchBase.AppendErrMsg("INFO_NO_MODIFIED_FILE",  "設計チェック送付Ref_No管理表");
                    CLogger.Logger("INFO_NO_MODIFIED_FILE", "設計チェック送付Ref_No管理表");
                }

                ////////////////////////////////////////////////////////////
                //ファイル変更のあるPubNo（ファイル新規、削除、更新）を対象
                //PubNoごとにDBへインポート
                ////////////////////////////////////////////////////////////
                var pubNos = FileManager.DiffPubNos;

                if(pubNos == null || pubNos.Count < 1)
                {
                    BatchBase.dtPubNoBatchEnd = DateTime.Now;
                    BatchBase.AppendErrMsg("INFO_NO_MODIFIED_FILE", "対象");
                    CLogger.Logger("INFO_NO_MODIFIED_FILE", "対象");
                    //終了
                    WriteErrMsg_DB();
                    return true;
                }
                // 納品フォルダのフォルダ名からPubNo取得
                ////////////////////////////////////////////////////
                /// ３－２．取得対象となったPubNoごとに下記処理を実施
                ////////////////////////////////////////////////////
                foreach (Hashtable pubNo in pubNos)
                {
                    dtPubNoBatchStart = DateTime.Now;
                    // PubNoごとトランザクション開始
                    db.Begin();
                    try
                    {

                        // PubNo使い回しのため
                        CFBase.PUBNO = pubNo["pub_no"].ToString();

                        // T_PUB_NOにデータが存在し、T_SIMILAR_DIAGRAM_SEARCHにデータが存在しない場合、
                        // T_PUB_NOとそれにつながるデータを削除
                        CFBase.DeletePubNo();

                        ////////////////////////////////////////////////////
                        ///<< T_PUB_NO>>のデータの更新と追加を実施
                        ////////////////////////////////////////////////////
                        new CFXML().ImportPubNoData();

                        ////////////////////////////////////////////////////
                        /// 対象の配線図リスト取得
                        ////////////////////////////////////////////////////
                        CFXML figs = new CFXML();
                        List<CTWiringDiagram> figsDiagramsList = figs.GetFigDiagramList();
                        ////////////////////////////////////////////////////
                        /// <<T_WIRING_DIAGRAM>>配線図ごとにデータの更新と追加を実施
                        ////////////////////////////////////////////////////
                        foreach (CTWiringDiagram fig in figsDiagramsList)
                        {
                            figs.ImportWireDiagramData(fig);


                            ////////////////////////////////////////////////////
                            /// 部品データ抽出
                            /// T_PARTSのデータの追加を実施
                            ////////////////////////////////////////////////////
                            CFSVG parts = new CFSVG();
                            CFBase.FIG_NAME = fig.figName;
                            parts.ImportPartsData(fig.figName);


                            ////////////////////////////////////////////////////
                            /// 端子・結線データ取得
                            /// T_TERMINAL、T_WIREのデータの追加を実施
                            ////////////////////////////////////////////////////
                            CFCSV terminals = new CFCSV();
                            terminals.ImportTerminalWire(fig.figName);
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
                return false;
            }
            return true;
        }
    }
}
