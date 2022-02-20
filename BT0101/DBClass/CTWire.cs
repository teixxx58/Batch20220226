﻿using BT0101Batch;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BT0101.DBClass
{
    class CTWire
    {
        private int _wireId;
        private int _fromTerminalId;            // FROM端子ID
        private int _toTerminalId;              // TO端子ID
        private int _svgLineId;                 // SVG結線ID
        private string _wireColor;              // 線色
        private int? _insertUserId;             // NULL
        private int? _updateUserId;             // NULL

        public int wireId
        {
            get { return _wireId; }
            set { _wireId = value; }
        }
        public int fromTerminalId
        {
            get { return _fromTerminalId; }
            set { _fromTerminalId = value; }
        }
        public int toTerminalId
        {
            get { return _toTerminalId; }
            set { _toTerminalId = value; }
        }
        [RegularExpression("^[0-9]+$", ErrorMessageResourceName = "Validate_Regx", ErrorMessageResourceType = typeof(Messages))]
        public int svgLineId
        {
            get { return _svgLineId; }
            set { _svgLineId = value; }
        }
        public string wireColor
        {
            get { return _wireColor; }
            set { _wireColor = value; }
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
            return string.Format("FROM端子ID:{0}," +
                "TO端子ID:{1}," +
                "SVG結線ID:{2}," +
                "線色:{3}",
                fromTerminalId,
                toTerminalId,
                svgLineId,
                wireColor
                );
        }
    }
}
