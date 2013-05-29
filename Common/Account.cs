using System;
using System.Windows.Media;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Authenticator
{
    public class Account : INotifyPropertyChanged
    {
        public string AccountName { get; set; }
        public string SecretKey { get; set; }

        private string _Code;
        public string Code
        {
            get
            {
                return _Code;
            }

            set
            {
                _Code = value;
                NotifyPropertyChanged("Code");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
