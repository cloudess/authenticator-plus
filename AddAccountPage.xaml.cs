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
using WP7_Barcode_Library;
using Microsoft.Phone.Shell;

namespace Authenticator
{
    public partial class AddAccountPage : PhoneApplicationPage
    {
        private App _application = null;
        bool newPageInstance = false;

        #region Construction and Navigation

        ApplicationBarIconButton save;
        ApplicationBarIconButton cancel;

        public AddAccountPage()
        {
            InitializeComponent();
            _application = (App)Application.Current;

            this.BuildApplicationBar();

            newPageInstance = true;
        }

        private void BuildApplicationBar()
        {
            save = new ApplicationBarIconButton();
            save.IconUri = new Uri("/Toolkit.Content/ApplicationBar.Check.png", UriKind.RelativeOrAbsolute);
            save.Text = "save";
            save.Click += btnSave_Click;

            cancel = new ApplicationBarIconButton();
            cancel.IconUri = new Uri("/Toolkit.Content/ApplicationBar.Cancel.png", UriKind.RelativeOrAbsolute);
            cancel.Text = "cancel";
            cancel.Click += btnCancel_Click;

            // build application bar
            ApplicationBar.Buttons.Add(save);
            ApplicationBar.Buttons.Add(cancel);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (State.ContainsKey("txtAccountName") && newPageInstance == true)
            {
                txtAccountName.Text = (string)State["txtAccountName"];
                txtSecretKey.Text = (string)State["txtSecretKey"];
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            newPageInstance = false;
            State["txtAccountName"] = txtAccountName.Text;
            State["txtSecretKey"] = txtSecretKey.Text;

            base.OnNavigatedFrom(e);
        }

        #endregion

        #region Event Handlers

        private void btnSave_Click(object sender, EventArgs e)
        {
            string tempName = txtAccountName.Text;
            string tempKey = txtSecretKey.Text;
            if (tempName != "" && tempKey != "")
            {
                AddToAccountDB(tempName, tempKey);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        private void btnScanBarcode_Click(object sender, RoutedEventArgs e)
        {
            WP7BarcodeManager.ScanMode = com.google.zxing.BarcodeFormat.QR_CODE;
            WP7BarcodeManager.ScanBarcode(ScanBarcode_Completed);
        }

        public void ScanBarcode_Completed(BarcodeCaptureResult e)
        {
            if (e.State == WP7_Barcode_Library.CaptureState.Success)
            {
                string str = e.BarcodeText;
                str = str.Replace("otpauth://totp/", "");
                string[] splitString = str.Split(Convert.ToChar("?"));
                splitString[1] = splitString[1].Replace("secret=", "");

                txtAccountName.Text = splitString[0];
                txtSecretKey.Text = splitString[1];
            }
            else
            {
                MessageBox.Show("The barcode for your account could not be read. Please try again.", "Error", MessageBoxButton.OK);
            }
        }

        #endregion

        #region Account Methods
        
        private void AddToAccountDB(string Name, string Key)
        {
            Account a = new Account();
            a.AccountName = Name;
            a.SecretKey = Key;

            CodeGenerator cg = new CodeGenerator(6, 30);
            string code = cg.computePin(a.SecretKey);

            if (code == null || code.Length != 6)
            {
                MessageBox.Show("The Secret Key you provided could not be validated. Please check your input and try again.", "Error", MessageBoxButton.OK);
                return;
            }
            else
            {
                _application.Database.Add(a);
                _application.Application_Closing(null, null);
            }

            NavigationService.GoBack();
        }

        #endregion
    }
}