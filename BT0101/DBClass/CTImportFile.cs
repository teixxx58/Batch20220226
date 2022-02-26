using System;

namespace BT0101.DBClass
{
    class CTImportFile
    {
        private string      _importedFileId;      // ファイルID
        private string          _importedFilePath;      // ファイルパス
        private DateTime    _impotedFileUpdateDate; // ファイル更新日時


        public string importedFileId
        {
            get { return _importedFileId; }
            set { _importedFileId = value; }
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

    }
}
