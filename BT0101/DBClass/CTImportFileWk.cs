using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT0101.DBClass
{
    class CTImportFileWk
    {
        private string      _pubNo;                 // PubNo
        private string      _importedFilePath;      // ファイルパス
        private DateTime    _impotedFileUpdateDate; // ファイル更新日時
        private string      _fileStatusFlg;         // ファイルステータス 0:新規 1:削除 2:更新
        private string      _fileKbn;               // ファイル区分 0:共通 1:付随PubNo


        public string pubNo
        {
            get { return _pubNo; }
            set { _pubNo = value; }
        }
        public string importedFilePath
        {
            get { return _importedFilePath; }
            set { _importedFilePath = value; }
        }

        public DateTime impotedFileUpdateDate
        {
            get { return _impotedFileUpdateDate; }
            set { _impotedFileUpdateDate = value; }
        }
        public string fileStatusFlg
        {
            get { return _fileStatusFlg; }
            set { _fileStatusFlg = value; }
        }
        public string fileKbn
        {
            get { return _fileKbn; }
            set { _fileKbn = value; }
        }

    }
}
