using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IBMU2.UODOTNET;
using MVSyncRefreshManager.Models;
using Newtonsoft.Json;
using Renci.SshNet;

namespace MVSyncRefreshManager
{
    public partial class MainForm : Form
    {

        //class wide universe session variable
        private UniSession _universeSession = null;

        private Dictionary<string, int> dumpLog = new Dictionary<string, int>();

        public MainForm()
        {
            InitializeComponent();
        }

        //code to handle our dumpButton click event
        private void dumpButton_Click(object sender, EventArgs e)
        {

            //we want to establish a connection with the UniVerse box through UniObjects
            try
            {

                //fetch the connection details from the config file
                string connectionConfigPath = Resources.ConnectionConfigPath;

                string serializedConfig = File.ReadAllText(connectionConfigPath);

                //deserialize the connectionConfig
                ConnectionDetails connectionDetails = JsonConvert.DeserializeObject<ConnectionDetails>(serializedConfig);

                //connection info
                _universeSession = UniObjects.OpenSession(connectionDetails.HostName,connectionDetails.Port, connectionDetails.Username, connectionDetails.Password, connectionDetails.SourceAccount, "uvcs");

                _universeSession.Timeout = 360000;

                //call our method to dump the remote uv files
                DumpRemoteFiles();
            }
            catch (Exception exception)
            {

                throw exception;
            }

            //if we get here, everything was a success, so we enable the fetch button
            fetchFilesButton.Enabled = true;
        }

        //method to transfer the dumped uv files from linux to the sql box
        private void FetchRemoteFiles()
        {

            //fetch the connection details from the config file
            string connectionConfigPath = Resources.FileFetchConfigPath;

            string serializedConfig = File.ReadAllText(connectionConfigPath);

            //deserialize the connectionConfig
            FileFetchDetails fileFetchDetailsDetails = JsonConvert.DeserializeObject<FileFetchDetails>(serializedConfig);

            //define our sftp connection with our remote linux box
            using (var sftpClient = new SftpClient(fileFetchDetailsDetails.HostName, fileFetchDetailsDetails.Port, fileFetchDetailsDetails.Username, fileFetchDetailsDetails.Password))
            {

                //establish the connection
                sftpClient.Connect();

                //we iterate through each one of the files in the dumpLog
                foreach (var item in dumpLog)
                {

                    try
                    {

                        //check to see if the connection has been made and if so, we fetch the remote files that have been dumped
                        using (Stream fileStream = File.Create($@"{Resources.LocalTransferPath}{item.Key}"))
                        {

                            //pulling the file from linux and popping it in the correct dest file
                            sftpClient.DownloadFile($"{Resources.RemoteDumpPath}{item.Key}.DAT", fileStream);
                        }
                    }
                    catch (Exception)
                    {

                        //we just carry on if we can't transfer a particular file
                        continue;
                    }
                }
            }
        }

        //method to read from the refresh config file and run dump the necessary files on the universe system
        private void DumpRemoteFiles()
        {

            //in the future we will get the config path from the form
            string refreshConfigPath = Resources.RefreshConfigPath;

            //reading in the config file
            string serializedConfig = File.ReadAllText(refreshConfigPath);

            //we deserialize our config file to our refresh details objects and sub-objects
            RefreshDetails refreshDetails = JsonConvert.DeserializeObject<RefreshDetails>(serializedConfig);

            //we iterate through each one of our branchdetails objects
            foreach (BranchDetailItem branchDetailItem in refreshDetails.BranchDetails)
            {

                foreach (string file in branchDetailItem.Files)
                {

                    //setting up our remote subroutine call
                    UniSubroutine mvSyncDumpSubroutine = _universeSession.CreateUniSubroutine("MvSyncDump", 3);

                    mvSyncDumpSubroutine.SetArg(0, branchDetailItem.BranchName);
                    mvSyncDumpSubroutine.SetArg(1, file);
                    mvSyncDumpSubroutine.SetArg(2, "0");

                    mvSyncDumpSubroutine.Call();

                    dumpLog.Add(file, Convert.ToInt32(mvSyncDumpSubroutine.GetArg(2)));
                }
            }
        }

        //when this button is clicked, the files that have been dumped will be fetched from the linux box
        private void fetchFilesButton_Click(object sender, EventArgs e)
        {

            //we try and fetch the remote files from the linux box through the ssh protocol
            try
            {

                FetchRemoteFiles();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
