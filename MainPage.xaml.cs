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

using Microsoft.Phone.Shell;
using System.Collections;
using System.ComponentModel;
using Microsoft.Phone.Tasks;

namespace Authenticator
{
    public partial class MainPage : PhoneApplicationPage
    {
        private App _application = null;
        private ProgressIndicator _progressIndicator = null;

        #region Construction and Navigation

        ApplicationBarIconButton add;
        ApplicationBarIconButton select;
        ApplicationBarIconButton delete;

        ApplicationBarMenuItem about;
        ApplicationBarMenuItem donate;

        public MainPage()
        {
            InitializeComponent();
            _application = (App)Application.Current;

            TiltEffect.TiltableItems.Add(typeof(MultiselectItem));

            this.BuildApplicationBar();

            CodeGenerator.intervalLength = 30;
            CodeGenerator.pinCodeLength = 6;
        }

        private void BuildApplicationBar()
        {
            add = new ApplicationBarIconButton();
            add.IconUri = new Uri("/Toolkit.Content/ApplicationBar.Add.png", UriKind.RelativeOrAbsolute);
            add.Text = "add";
            add.Click += btnAdd_Click;

            select = new ApplicationBarIconButton();
            select.IconUri = new Uri("/Toolkit.Content/ApplicationBar.Select.png", UriKind.RelativeOrAbsolute);
            select.Text = "select";
            select.Click += btnSelect_Click;

            delete = new ApplicationBarIconButton();
            delete.IconUri = new Uri("/Toolkit.Content/ApplicationBar.Delete.png", UriKind.RelativeOrAbsolute);
            delete.Text = "delete";
            delete.Click += btnDelete_Click;

            about = new ApplicationBarMenuItem();
            about.Text = "about authenticator";
            about.Click += mnuAbout_Click;

            donate = new ApplicationBarMenuItem();
            donate.Text = "donate";
            donate.Click += mnuDonate_Click;

            // build application bar
            ApplicationBar.Buttons.Add(add);
            ApplicationBar.Buttons.Add(select);

            ApplicationBar.MenuItems.Add(about);
            ApplicationBar.MenuItems.Add(donate);
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            // check for unhandled exception
            if (_application.DataCorruptionException == true)
            {
                _application.DataCorruptionException = false;
                MessageBox.Show("Your account secrets were corrupted and your data has been lost. You may need to reconfigure your accounts on your phone.", "Error", MessageBoxButton.OK);
            }

            // bind display to database
            this.lstAccounts.ItemsSource = _application.Database;

            // toggle empty text display
            if (_application.Database.Count == 0)
                this.txtEmpty.Visibility = System.Windows.Visibility.Visible;
            else
                this.txtEmpty.Visibility = System.Windows.Visibility.Collapsed;

            // create progress indicator
            if (_progressIndicator == null)
            {
                _progressIndicator = new ProgressIndicator();
                SystemTray.SetProgressIndicator(this, _progressIndicator);
            }

            StartTimer();
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (this.lstAccounts.IsSelectionEnabled)
            {
                this.lstAccounts.IsSelectionEnabled = false;
                e.Cancel = true;
            }
        }

        #endregion

        #region Event Handlers

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/YourLastAboutDialog;component/AboutPage.xaml", UriKind.Relative));
        }
        
        private void mnuDonate_Click(object sender, EventArgs e)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();

            webBrowserTask.Uri = new Uri("http://mbmccormick.com/donate/", UriKind.Absolute);
            webBrowserTask.Show();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AddAccountPage.xaml", UriKind.Relative));
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            this.lstAccounts.IsSelectionEnabled = true;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (this.lstAccounts.SelectedItems.Count == 1)
            {
                if (MessageBox.Show("Are you sure you want to delete the selected account?", "Delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    while (this.lstAccounts.SelectedItems.Count > 0)
                    {
                        _application.Database.Remove((Account)this.lstAccounts.SelectedItems[0]);
                    }

                    _application.Application_Closing(null, null);
                }
            }
            else if (this.lstAccounts.SelectedItems.Count > 1)
            {
                if (MessageBox.Show("Are you sure you want to delete the selected accounts?", "Delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    while (this.lstAccounts.SelectedItems.Count > 0)
                    {
                        _application.Database.Remove((Account)this.lstAccounts.SelectedItems[0]);
                    }

                    _application.Application_Closing(null, null);
                }
            }

            // toggle empty text display
            if (_application.Database.Count == 0)
                this.txtEmpty.Visibility = System.Windows.Visibility.Visible;
            else
                this.txtEmpty.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void lstAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MultiselectList target = (MultiselectList)sender;
            ApplicationBarIconButton i = (ApplicationBarIconButton)ApplicationBar.Buttons[0];

            if (target.IsSelectionEnabled)
            {
                if (target.SelectedItems.Count > 0)
                {
                    i.IsEnabled = true;
                }
                else
                {
                    i.IsEnabled = false;
                }
            }
            else
            {
                i.IsEnabled = true;
            }
        }

        private void lstAccounts_IsSelectionEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            while (ApplicationBar.Buttons.Count > 0)
            {
                ApplicationBar.Buttons.RemoveAt(0);
            }

            while (ApplicationBar.MenuItems.Count > 0)
            {
                ApplicationBar.MenuItems.RemoveAt(0);
            }

            if ((bool)e.NewValue)
            {
                ApplicationBar.Buttons.Add(delete);
                ApplicationBarIconButton i = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
                i.IsEnabled = false;
            }
            else
            {
                ApplicationBar.Buttons.Add(add);
                ApplicationBar.Buttons.Add(select);

                ApplicationBar.MenuItems.Add(about);
                ApplicationBar.MenuItems.Add(donate);
            }
        }

        private void ItemContent_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Account item = ((FrameworkElement)sender).DataContext as Account;
            if (this.lstAccounts.IsSelectionEnabled)
            {
                MultiselectItem container = this.lstAccounts.ItemContainerGenerator.ContainerFromItem(item) as MultiselectItem;
                if (container != null)
                {
                    container.IsSelected = !container.IsSelected;
                }
            }
        }

        private Account MostRecentAccountClick
        {
            get;
            set;
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement)
            {
                FrameworkElement frameworkElement = (FrameworkElement)e.OriginalSource;
                if (frameworkElement.DataContext is Account)
                {
                    MostRecentAccountClick = (Account)frameworkElement.DataContext;
                }
            }

            base.OnMouseLeftButtonDown(e);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem target = (MenuItem)sender;
            ContextMenu parent = (ContextMenu)target.Parent;

            if (target.Header.ToString() == "copy")
            {
                Clipboard.SetText(MostRecentAccountClick.Code);
            }
            else if (target.Header.ToString() == "delete")
            {
                if (MessageBox.Show("Are you sure you want to delete the selected account?", "Delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    _application.Database.Remove(MostRecentAccountClick);
                    
                    _application.Application_Closing(null, null);
                }
            }
        }

        #endregion

        #region Timer Methods

        private void StartTimer()
        {
            System.Windows.Threading.DispatcherTimer myDispatcherTimer = new System.Windows.Threading.DispatcherTimer();

            myDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            myDispatcherTimer.Tick += new EventHandler(Timer_Tick);
            myDispatcherTimer.Start();
        }        

        private void Timer_Tick(object o, EventArgs sender)
        {
            foreach (Account a in _application.Database)
            {
                CodeGenerator cg = new CodeGenerator(6, 30);
                string code = cg.computePin(a.SecretKey);

                if (a.Code != code)
                {
                    a.Code = code;
                }
            }

            if (_application.Database.Count > 0)
            {
                _progressIndicator.IsVisible = true;
            }
            else
            {
                _progressIndicator.IsVisible = false;
            }

            _progressIndicator.IsIndeterminate = false;
            _progressIndicator.Value = CodeGenerator.numberSecondsLeft() / 30.0;
        }

        #endregion
    }
}