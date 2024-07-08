using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LKDS_Logger_NVRAM
{
    public class LB : INotifyPropertyChanged
    {
        private string _lbName;
        private int _lbId;
        private string _lbKey;
        private string _lbIpString;
        private int _lbPort;
        private string _lbLastChange;
        private string _lbLastDump;
        private string _lbStatus;
        private string _lBColor;

        public string LBName
        {
            get => _lbName;
            set
            {
                if (_lbName != value)
                {
                    _lbName = value;
                    OnPropertyChanged();
                }
            }
        }

        public int LBId
        {
            get => _lbId;
            set
            {
                if (_lbId != value)
                {
                    _lbId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LBKey
        {
            get => _lbKey;
            set
            {
                if (_lbKey != value)
                {
                    _lbKey = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LBIpString
        {
            get => _lbIpString;
            set
            {
                if (_lbIpString != value)
                {
                    _lbIpString = value;
                    OnPropertyChanged();
                }
            }
        }

        public int LBPort
        {
            get => _lbPort;
            set
            {
                if (_lbPort != value)
                {
                    _lbPort = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LBLastChange
        {
            get => _lbLastChange;
            set
            {
                if (_lbLastChange != value)
                {
                    _lbLastChange = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LBLastDump
        {
            get => _lbLastDump;
            set
            {
                if (_lbLastDump != value)
                {
                    _lbLastDump = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LBStatus
        {
            get => _lbStatus;
            set
            {
                if (_lbStatus != value)
                {
                    _lbStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LBColor
        {
            get => _lBColor;
            set
            {
                if (_lBColor != value)
                {
                    _lBColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}