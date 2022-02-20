﻿using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT0101.DBClass
{
    [System.Diagnostics.DebuggerDisplay("部品ID:{partsId},端子名称:{terminalName}," +
        "ピン番号:{pinNo},接続点X{pointX},接続点Y{pointY},接続方向{direction}," +
        "ダイアグコード{diagCd},制御名{controlName},信号名{signalName}")]
    public class CTTerminal
    {
        private int _terminalId;
        private int _partsId;                   // 部品ID
        private string _terminalName;           // 端子名称
        private string _pinNo;                  // ピン番号
        private Single _pointX;                 // 接続点X
        private Single _pointY;                 // 接続点Y
        private string _direction;              // 接続方向
        private string _diagCd;                 // ダイアグコード
        private string _controlName;            // 制御名
        private string _signalName;             // 信号名
        private int? _insertUserId;
        private int? _updateUserId;

        public int terminalId
        {
            get { return _terminalId; }
            set { _terminalId = value; }
        }
        public int partsId
        {
            get { return _partsId; }
            set { _partsId = value; }
        }
       public string terminalName
        {
            get { return _terminalName; }
            set { _terminalName = value; }
        }
       public string pinNo
        {
            get { return _pinNo; }
            set { _pinNo = value; }
        }
        public Single pointX
        {
            get { return _pointX; }
            set { _pointX = value; }
        }
        public Single pointY
        {
            get { return _pointY; }
            set { _pointY = value; }
        }
       public string direction
        {
            get { return _direction; }
            set { _direction = value; }
        }
       public string diagCd
        {
            get { return _diagCd; }
            set { _diagCd = value; }
        }
       public string controlName
        {
            get { return _controlName; }
            set { _controlName = value; }
        }
       public string signalName
        {
            get { return _signalName; }
            set { _signalName = value; }
        }

        public int? insertUserId
        {
            get { return _insertUserId; }
            set { _insertUserId = value; }
        }
        public int? updateUserId
        {
            get { return _updateUserId; }
            set { _updateUserId = value; }
        }

        public override string ToString()
        {
            return string.Format("部品ID:{0}" +
                ",端子名称:{1}," +
                "ピン番号:{2}," +
                "接続点X:{3}," +
                "接続点Y:{4}," +
                "接続方向:{5}," +
                "ダイアグコード:{6}," +
                "制御名:{7}," +
                "信号名:{8}",
                partsId,
                terminalName,
                pinNo,
                pointX,
                pointY,
                direction,
                diagCd,
                controlName,
                signalName
                );
        }
    }
}
