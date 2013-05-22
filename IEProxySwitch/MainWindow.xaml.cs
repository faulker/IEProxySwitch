using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace IEProxySwitch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region NotifyIcon
        System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon()
        {
            Visible = true
        };

        Icon pxConnected = Properties.Resources.plug_connect;
        Icon pxDisconnected = Properties.Resources.plug_disconnect;
        #endregion

        private System.Windows.Forms.ContextMenuStrip cm;
        proxy px = new proxy();
        bool _StayOpen = true;
        string dbPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "IESettingRefresh.exe");
        Timer timer = new Timer();
        bool proxyState;


        public MainWindow()
        {
            InitializeComponent();

            #region UnhandledException
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            #endregion

            #region ContextMenu
            cm = new ContextMenuStrip();

            // Exit
            ToolStripMenuItem menuExit = new ToolStripMenuItem();
            menuExit.Name = "exit";
            menuExit.Text = "E&xit";
            //menuExit.Image = Properties.Resources.shut_down;
            menuExit.Click += new System.EventHandler(exitForm);

            //About
            ToolStripMenuItem menuAbout = new ToolStripMenuItem();
            menuAbout.Name = "about";
            menuAbout.Text = "&About";
            //menuAbout.Image = Properties.Resources.male_user;
            menuAbout.Click += new System.EventHandler(showAbout);

            //Refresh
            ToolStripMenuItem menuRe = new ToolStripMenuItem();
            menuRe.Name = "re";
            menuRe.Text = "&Refresh";
            //menuRe.Image = Properties.Resources.male_user;
            menuRe.Click += new System.EventHandler(checkStatus);

            //Disable
            ToolStripMenuItem menuDis = new ToolStripMenuItem();
            menuDis.Name = "dis";
            menuDis.Text = "&Disable";
            //menuDis.Image = Properties.Resources.male_user;
            menuDis.Click += new System.EventHandler(notifyIconDubleClicked);

            //Enable
            ToolStripMenuItem menuEn = new ToolStripMenuItem();
            menuEn.Name = "en";
            menuEn.Text = "&Enable";
            //menuEn.Image = Properties.Resources.male_user;
            menuEn.Click += new System.EventHandler(notifyIconDubleClicked);

            if (px.isProxyEnabled())
            {
                menuEn.Enabled = false;
                menuDis.Enabled = true;
            }
            else
            {
                menuEn.Enabled = true;
                menuDis.Enabled = false;
            }

            cm.Items.Add(menuExit);
            cm.Items.Add(menuAbout);
            cm.Items.Add("-");
            cm.Items.Add(menuRe);
            cm.Items.Add("-");
            cm.Items.Add(menuDis);
            cm.Items.Add(menuEn);

            ni.ContextMenuStrip = cm;
            #endregion


            if (px.isProxyEnabled())
            {
                ni.Icon = pxConnected;
            }
            else
            {
                ni.Icon = pxDisconnected;
            }

            ni.DoubleClick += new System.EventHandler(notifyIconDubleClicked);

            mRecreateAllExecutableResources(); // Extract IESettingsRefresh.exe
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = (Exception)e.ExceptionObject;

                System.Windows.Forms.MessageBox.Show("Whoops! Please contact the developers with the following"
                      + " information:\n\n" + ex.Message + ex.StackTrace,
                      "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            finally
            {
                System.Windows.Forms.Application.Exit();
            }
        }

        private void notifyIconDubleClicked(Object sender, EventArgs e)
        {
            proxyState = px.isProxyEnabled();

            ProcessStartInfo changeState = new ProcessStartInfo();
            changeState.FileName = dbPath;
            changeState.CreateNoWindow = true;
            changeState.WindowStyle = ProcessWindowStyle.Hidden;

            Process pc = new Process();
            pc.StartInfo = changeState;
            pc.Start();
            pc.WaitForExit(100);
            pc.Close();

            timer.Interval = 100;
            timer.Tick += new EventHandler(changeIconState);
            timer.Start();
        }

        private void changeIconState(Object sender, EventArgs e)
        {
            bool cProxyState = px.isProxyEnabled();

            if (proxyState != cProxyState)
            {
                if (cProxyState)
                {
                    ni.Icon = pxConnected;
                    cm.Items["en"].Enabled = false;
                    cm.Items["dis"].Enabled = true;
                    timer.Stop();
                }
                else if (!cProxyState)
                {
                    ni.Icon = pxDisconnected;
                    cm.Items["en"].Enabled = true;
                    cm.Items["dis"].Enabled = false;
                    timer.Stop();
                }
            }
        }

        private void exitForm(Object sender, EventArgs e)
        {
            ni.Visible = false;
            _StayOpen = false;
            Close();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://faulk.me", null);
        }

        private void showAbout(Object sender, EventArgs e)
        {
            //this.Visibility = Visibility.Visible;
            this.Show();
        }

        private void checkStatus(Object sender, EventArgs e)
        {
            if (px.isProxyEnabled())
            {
                ni.Icon = pxConnected;
                cm.Items["en"].Enabled = false;
                cm.Items["dis"].Enabled = true;
            }
            else
            {
                ni.Icon = pxDisconnected;
                cm.Items["en"].Enabled = true;
                cm.Items["dis"].Enabled = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_StayOpen)
            {
                e.Cancel = true;
                this.Visibility = Visibility.Hidden;
            }
            else
            {
                ni.Visible = false;
            }
        }

        /// <summary>
        /// Extracts IESettingRefresh.exe to the IEProxySwitch exec folder
        /// This is done because there is a bug when calling 'INTERNET_OPTION_SETTINGS_CHANGED'
        /// and 'INTERNET_OPTION_REFRESH' in the wininet.dll that only lets them be called once
        /// per application execution.
        /// </summary>
        private void mRecreateAllExecutableResources()
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            string[] arrayResources = currentAssembly.GetManifestResourceNames();

            foreach (string resourceName in arrayResources)
            {
                if (resourceName.EndsWith(".exe"))
                {
                    FileInfo fileInfoOutputFile = new FileInfo(dbPath);

                    if (fileInfoOutputFile.Exists)
                    {
                        break;
                    }
                    FileStream streamToOutputFile = fileInfoOutputFile.OpenWrite();

                    Stream streamToResourceFile = currentAssembly.GetManifestResourceStream(resourceName);

                    // Save to disk
                    const int size = 4096;
                    byte[] bytes = new byte[4096];
                    int numBytes;
                    while ((numBytes = streamToResourceFile.Read(bytes, 0, size)) > 0)
                    {
                        streamToOutputFile.Write(bytes, 0, numBytes);
                    }

                    streamToOutputFile.Close();
                    streamToResourceFile.Close();
                }

            }
        }

        private void link_Clicked(object sender, EventArgs e)
        {
            var link = (Hyperlink)sender;
            Process.Start(link.NavigateUri.ToString());
        }
    }
}
