using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace Authenticator
{
    public partial class QRPage : PhoneApplicationPage
    {
        public QRPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void QRCodeScanner_ScanComplete(object sender, JeffWilcox.Controls.ScanCompleteEventArgs e)
        {
            (App.Current as App).QRCode = e.Result;
            NavigationService.GoBack();
        }

        private void QRCodeScanner_Error(object sender, JeffWilcox.Controls.ScanFailureEventArgs e)
        {            
            NavigationService.GoBack();
        }
    }
}