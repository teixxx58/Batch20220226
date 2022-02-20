using BT0101.DBClass;
using IBatisNet.DataMapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BT0101Batch
{
    class FileManager
    {
        private static DatabaseHelper mapper;

        public static string[] fileServerPath = (ConfigurationManager.AppSettings["FILE_SERVER_PATH"]).Split(',');
        //ローカルのファイル保存パス
        public static string localDataDir = ConfigurationManager.AppSettings["LOCAL_DATA_DIR"];
        //title.xmlのファイル保存パス
        public static string titleXmlDir = ConfigurationManager.AppSettings["TITLE_XML_DIR"];
        //*.svgのファイル保存パス
        public static string svgDir = ConfigurationManager.AppSettings["SVG_DIR"];
        //Pub.xmlのファイル保存パス
        public static string pubXmlDir = ConfigurationManager.AppSettings["PUB_XML_DIR"];
        //parts.xmlのファイル保存パス
        public static string partsXmlDir = ConfigurationManager.AppSettings["PARTS_XML_DIR"];
        //filtering.xmlのファイル保存パス
        public static string filteringXmlDir = ConfigurationManager.AppSettings["FILTERING_XML_DIR"];
        //*.csvのファイル保存パス
        public static string syugakiCsvDir = ConfigurationManager.AppSettings["SYUGAKI_CSV_DIR"];
        //{ PUB_NO}-Parts.xlsのファイル保存パス
        public static string pubNoPartsXlsDir = ConfigurationManager.AppSettings["PUB_NO_PARTS_XLS_DIR"];
        //{PUB_NO}-WireNameList.xlsのファイル保存パス
        public static string pubNoWireNameListDir = ConfigurationManager.AppSettings["PUB_NO_WIRENAMELIST_XLS_DIR"];
        //課題フォルダのパス
        public static string kadaiFileDir = ConfigurationManager.AppSettings["KADAI_FILE_DIR"];
        //* 設計チェック送付Ref No管理表.xlsの配置フォルダ
        public static string sekkeiCheckFileDir = ConfigurationManager.AppSettings["SEKKEI_CHECK_FILE_DIR"];

        private static string titlePattern = Regex.Escape(titleXmlDir) + @"title\.xml$";
        private static string NonSvgFilePattern = Regex.Escape(svgDir) + @".*([_][0-9][0-9])\.svg$";
        private static string svgFilePattern = Regex.Escape(svgDir) + @".*\.svg$";
        private static string pubFilePattern = Regex.Escape(pubXmlDir) + @"pub\.xml$";
        private static string partsFilePattern = Regex.Escape(partsXmlDir) + @"parts\.xml$";
        private static string filterFilePattern = Regex.Escape(filteringXmlDir) + @"filtering\.xml$";
        private static string csvFilePattern = Regex.Escape(syugakiCsvDir) + @".*\.csv$";
        private static string xmlPartsPattern = Regex.Escape(pubNoPartsXlsDir) + @".*\-Parts\.xls$";
        private static string wireNameListPattern = Regex.Escape(pubNoWireNameListDir) + @".*\-WireNameList\.xls$";
        private static string kadaiFileDirPattern = Regex.Escape(kadaiFileDir) + @".*\.*"; 
        private static string refNoCheckPattern = Regex.Escape(sekkeiCheckFileDir) + @".*設計チェック送付Ref No管理表\.xls$";

        //ファイルステータス 0:新規 1:削除 2:更新
        private const string STATUS_INSERT_FLG = "0";
        private const string STATUS_DELETE_FLG = "1";
        private const string STATUS_UPDATE_FLG = "2";

        // ファイル区分 0:共通 1:付随PubNo
        private const string PUBNO_FILE_TYPE = "1";
        private const string PROJECT_FILE_TYPE = "0";
        private const string PROJECT_FILE_PUBNO = @"ProComFile";


        //ファイル更新あり・なし
        public static bool hasModifiedProjectFile = false;
        public static IList<Hashtable> DiffPubNos;

        private static FileInfo fi;
        public static void SetMapper(DatabaseHelper db)
        {
            mapper = db;
        }

        /// <summary>
        /// サーバからファイルリスト取得
        /// </summary>
        private static List<FileInfo> GetSourceFileList()
        {
            //ファイルリストの取得
            List<FileInfo> serverFiles = new List<FileInfo>();
            foreach (string p in fileServerPath)
            {
                DirectoryInfo diSource = new DirectoryInfo(p);
                if (!diSource.Exists)
                {
                    //CLogger.Warn("{0}フォルダーが存在しない。", p);
                    continue;
                }
                IEnumerable<FileInfo> fileInfos = diSource.EnumerateFiles("*", SearchOption.AllDirectories)
                                        .Where(s => Regex.IsMatch(s.FullName, titlePattern) ||
                                            !Regex.IsMatch(s.FullName, NonSvgFilePattern) &&
                                            Regex.IsMatch(s.FullName, svgFilePattern) ||
                                            Regex.IsMatch(s.FullName, pubFilePattern) ||
                                            Regex.IsMatch(s.FullName, partsFilePattern) ||
                                            Regex.IsMatch(s.FullName, filterFilePattern) ||
                                            Regex.IsMatch(s.FullName, csvFilePattern) ||
                                            Regex.IsMatch(s.FullName, xmlPartsPattern) ||
                                            Regex.IsMatch(s.FullName, wireNameListPattern) ||
                                           // Regex.IsMatch(s.FullName, kadaiFileDirPattern) || 
                                            Regex.IsMatch(s.FullName, refNoCheckPattern)
                );
                foreach(FileInfo fi in fileInfos)
                {
                    serverFiles.Add(fi);
                }
            }

            return serverFiles;
        }
        /// <summary>
        /// ファイルパスリストの成形
        /// </summary>
        /// <param name="path">ルートパス</param>
        /// <param name="list">ファイルリスト</param>
        private static List<CTImportFileWk> FormatFileName(List<FileInfo> list,string[] path)
        {
            //ファイルリストの取得
            List<CTImportFileWk> fileList = new List<CTImportFileWk>();
            foreach (FileInfo file in list)
            {

                CTImportFileWk ct = new CTImportFileWk();
                string fullName = "";
                foreach (string fN in path)
                {
                    fullName = Regex.Replace(file.FullName, Regex.Escape(fN), @"");
                }
                ct.importedFilePath = fullName;
                ct.impotedFileUpdateDate = file.LastWriteTime;
                ct.fileStatusFlg = STATUS_UPDATE_FLG;
                
                //PubNo・ファイル判別
                string pattern = @"^EM[0-9a-zA-Z]+";

                if (Regex.IsMatch(fullName, pattern))
                {
                    ct.pubNo = Regex.Match(fullName, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline).Value;
                    if (ct.pubNo.Length < 11)
                    {
                        ct.fileKbn = PUBNO_FILE_TYPE;
                    }
                    else
                    {
                        ct.pubNo = PROJECT_FILE_PUBNO;
                        ct.fileKbn = PROJECT_FILE_TYPE;
                    }
                }
                else
                {
                    ct.pubNo = PROJECT_FILE_PUBNO;
                    ct.fileKbn = PROJECT_FILE_TYPE;
                }



                fileList.Add(ct);
            }
            return fileList;
        }
        /// <summary>
        /// ファイルリストのDBへ入れ込む
        /// </summary>
        /// <param name="list">ファイルリスト</param>
        private static void SaveFilesToWorkTable(List<CTImportFileWk> list)
        {
            //ファイルリストの取得
            foreach (CTImportFileWk file in list)
            {
                //追加
                mapper.Insert("InsertImportFileWk", file);
            }
        }
        /// <summary>
        /// ファイルリストのDBへ入れ込む
        /// </summary>
        /// <param name="list">ファイルリスト</param>
        private static void SaveFilesPathToImportFileTable(CTImportFile rec)
        {
            //追加
            mapper.Insert("InsertImportFile", rec);

        }
        /// <summary>
        /// ワークテーブルDrop
        /// </summary>
        private static void DropImportFileWkTable()
        {
            mapper.Update("DropImportFileWkTable");
        }
        /// <summary>
        /// ワークテーブル作成
        /// </summary>
        private static void CreateImportFileWkTable()
        {
            mapper.Update("DropImportFileWkTable");
            mapper.Update("CreateImportFileWkTable");
        }
        /// <summary>
        /// 差分ファイルリストの取得
        /// </summary>
        public static IList<Hashtable> GetDiffFromWorkTable()
        {
            //差分ファイルリストの取得
            IList<Hashtable> rslt = mapper.QueryForList<Hashtable>("selectImportFileDiff", null);
            return rslt;
        }
        /// <summary>
        /// 差分PubNoリストの取得
        /// </summar
        private static IList<Hashtable> GetDiffPubNo()
        {
            //差分ファイルリストの取得
            IList<Hashtable> rslt = mapper.QueryForList<Hashtable>("selectImportFileDiffPubNo");
            return rslt;
        }
        public static bool HasDiffProjectFiles()
        {
            //差分ファイルリストの取得
            IList<Hashtable> rslt = mapper.QueryForList<Hashtable>("selectProjectFileDiff");
            return rslt.Count > 0 ? true:false;
        }
        

        /// <summary>
        /// 削除されたファイルをDBから削除
        /// </summary>
        /// <param name="fp">ファイルス</param>
        public static void DeleteFromImportTable(string importedFilePath)
        {
            //差分ファイルリストの取得
            mapper.Delete("DeleteImportFile", importedFilePath);
        }

        /// <summary>
        /// 一括ロカールファイルの更新・削除・コピー
        /// </summary>
        /// <param name="srcPath">サーバールートパス</param>
        /// <param name="destPath">ロカールルートパス</param>
        public static void UpDateLocalFiles(string[] srcPath,string destPath)
        {
            //サーバからファイルリストの取得
            List<FileInfo> list =GetSourceFileList();
            //ファイルリストの準備
            List<CTImportFileWk> formated = FormatFileName(list, srcPath);

            //ワークテーブルに保存
            CreateImportFileWkTable();
            
            mapper.Begin();
            SaveFilesToWorkTable(formated);
            
            //差分抽出
            IList<Hashtable> files = GetDiffFromWorkTable();

            //差分PubNo取得
            DiffPubNos =  GetDiffPubNo();
            //プロジェクトファイルの更新有無
            hasModifiedProjectFile = HasDiffProjectFiles();

            //ロカールファイルの更新・削除・コピー
            foreach (Hashtable rec in files)
            {
                bool exists = false;
                foreach (string src in srcPath)
                {
                    fi = new FileInfo(src + rec["imported_file_path"].ToString());
                    if (fi.Exists)
                    {
                        exists = true;
                        break;
                    }
                }
                //ファイルが取得時と不整合
                if (!exists) { CLogger.Logger("WNG_NotTargetFile", fi.FullName); return; }
 
                if (rec["file_status_flg"].ToString().Equals(STATUS_DELETE_FLG))
                {
                    //削除
                    FileInfo del = new FileInfo(destPath + rec["imported_file_path"].ToString());
                    del.Delete();
                    //DB削除
                    DeleteFromImportTable(rec["imported_file_path"].ToString());

                }
                if (rec["file_status_flg"].ToString().Equals(STATUS_INSERT_FLG) || rec["file_status_flg"].ToString().Equals(STATUS_UPDATE_FLG))
                {

                    //新規・更新
                    string fp = destPath + rec["imported_file_path"].ToString();
                    if (!Directory.Exists(Path.GetDirectoryName(fp)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(fp));
                    }
                    fi.CopyTo(fp, true);
                    CTImportFile impFile = new CTImportFile();
                    impFile.importedFilePath = rec["imported_file_path"].ToString();
                    impFile.impotedFileUpdateDate = fi.LastWriteTime;
                    //DB更新
                    SaveFilesPathToImportFileTable(impFile);
                }
            }

            mapper.Commit();
            //DropImportFileWkTable();
        }
    }
}
