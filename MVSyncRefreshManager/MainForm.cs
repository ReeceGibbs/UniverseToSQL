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
using AutozoneSyncDataAccess;
using System.Diagnostics;
using Renci.SshNet.Common;

namespace MVSyncRefreshManager
{
    public partial class MainForm : Form
    {

        //class wide universe session variable
        private UniSession _universeSession = null;

        private ConnectionDetails connectionDetails = new ConnectionDetails();

        private RefreshDetails refreshDetails = new RefreshDetails();

        private Dictionary<string, int> dumpLog = new Dictionary<string, int>();

        private HashSet<string> tableNames = new HashSet<string>();

        public MainForm()
        {
            InitializeComponent();
        }

        //code to handle our dumpButton click event
        private async void dumpButton_Click(object sender, EventArgs e)
        {

            //we want to establish a connection with the UniVerse box through UniObjects
            try
            {

                //fetch the connection details from the config file
                string connectionConfigPath = Resources.ConnectionConfigPath;
                string refreshConfigPath = Resources.RefreshConfigPath;

                string serializedConnectionConfig = File.ReadAllText(connectionConfigPath);
                string serializedRefreshConfig = File.ReadAllText(refreshConfigPath);

                connectionDetails = JsonConvert.DeserializeObject<ConnectionDetails>(serializedConnectionConfig);
                refreshDetails = JsonConvert.DeserializeObject<RefreshDetails>(serializedRefreshConfig);

                //connection info
                _universeSession = UniObjects.OpenSession(connectionDetails.HostName,connectionDetails.UniRpcPort, connectionDetails.Username, connectionDetails.Password, connectionDetails.SourceAccount, "uvcs");

                _universeSession.Timeout = 360000;

                //call our method to dump the remote uv files
                await DumpRemoteFiles();
            }
            catch (Exception exception)
            {

                throw exception;
            }

            //if we get here, everything was a success, so we enable the fetch button
            fetchFilesButton.Enabled = true;
        }

        //when this button is clicked, the files that have been dumped will be fetched from the linux box
        private async void fetchFilesButton_Click(object sender, EventArgs e)
        {

            //we try and fetch the remote files from the linux box through the ssh protocol
            try
            {

                await FetchRemoteFiles();
            }
            catch (Exception exception)
            {

                eventLog.AppendText(exception.Message);
            }

            //enabling the bulk insert button
            bulkInsertButton.Enabled = true;
        }

        //button to perform the third and final portion of the refresh
        private async void bulkInsertButton_Click(object sender, EventArgs e)
        {

            try
            {

                await RunBulkInsert();
            }
            catch (Exception exception)
            {

                eventLog.AppendText(exception.Message);
            }

            eventLog.AppendText("Refresh completed successfully.");
        }

        //method to read from the refresh config file and run dump the necessary files on the universe system
        private async Task<bool> DumpRemoteFiles()
        {

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            try
            {

                //we iterate through each one of our branchdetails objects
                foreach (BranchDetailItem branchDetailItem in refreshDetails.BranchDetails)
                {

                    foreach (string file in branchDetailItem.Files)
                    {

                        eventLog.AppendText($"Dumping file: {file}\n");

                        await Task.Run(() =>
                        {

                            //setting up our remote subroutine call
                            UniSubroutine mvSyncDumpSubroutine = _universeSession.CreateUniSubroutine("MvSyncDump", 3);

                            mvSyncDumpSubroutine.SetArg(0, branchDetailItem.BranchName);
                            mvSyncDumpSubroutine.SetArg(1, file);
                            mvSyncDumpSubroutine.SetArg(2, "0");

                            mvSyncDumpSubroutine.Call();

                            dumpLog.Add(file, Convert.ToInt32(mvSyncDumpSubroutine.GetArg(2)));

                            tableNames.Add(file.Substring(3).Replace('.', '_'));
                        });

                        eventLog.AppendText($"Dump complete: {file}\n\n");
                    }
                }
            }
            catch (Exception exception)
            {

                eventLog.AppendText(exception.Message);

                return false;
            }

            stopwatch.Stop();

            eventLog.AppendText("File dump complete.\n" +
                                $"Time elapsed: {stopwatch.ElapsedMilliseconds}ms\n\n");

            dumpButton.BackColor = Color.Green;

            return true;
        }

        //method to transfer the dumped uv files from linux to the sql box
        private async Task<bool> FetchRemoteFiles()
        {

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            try
            {

                //we need to clear out our destination directory
                DirectoryInfo destinationDir = new DirectoryInfo(connectionDetails.DestinationPath);

                foreach (var file in destinationDir.GetFiles())
                {

                    file.Delete();
                }

                //we jook the system and make the remote host think someone just entered in a password manually
                KeyboardInteractiveAuthenticationMethod keyboardInteractiveAuthenticationMethod = new KeyboardInteractiveAuthenticationMethod(connectionDetails.Username);

                keyboardInteractiveAuthenticationMethod.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(HandleKeyEvent);

                ConnectionInfo connectionInfo = new ConnectionInfo(connectionDetails.HostName, connectionDetails.SshPort, connectionDetails.Username, keyboardInteractiveAuthenticationMethod);

                //define our sftp connection with our remote linux box
                using (var sftpClient = new SftpClient(connectionInfo))
                {

                    //establish the connection
                    sftpClient.Connect();

                    //we iterate through each one of the files in the dumpLog
                    foreach (var item in dumpLog)
                    {

                        try
                        {

                            eventLog.AppendText($"Downloading file: {item}\n");

                            await Task.Run(() => 
                            {

                                //check to see if the connection has been made and if so, we fetch the remote files that have been dumped
                                using (Stream fileStream = File.Create($@"{connectionDetails.DestinationPath}{item.Key}.DAT"))
                                {

                                    //pulling the file from linux and popping it in the correct dest file
                                    sftpClient.DownloadFile($"{connectionDetails.SourcePath}{item.Key}.DAT", fileStream);
                                }
                            });

                            eventLog.AppendText($"File download complete: {item}\n\n");
                        }
                        catch (Exception)
                        {

                            //we just carry on if we can't transfer a particular file
                            continue;
                        }
                    }
                }
            }
            catch (Exception exception)
            {

                eventLog.AppendText(exception.Message);

                return false;
            }

            stopwatch.Stop();

            eventLog.AppendText("File download complete.\n" +
                    $"Time elapsed: {stopwatch.ElapsedMilliseconds}ms\n\n");

            fetchFilesButton.BackColor = Color.Green;

            return true;
        }

        private void HandleKeyEvent(object sender, AuthenticationPromptEventArgs e)
        {

            foreach (AuthenticationPrompt prompt in e.Prompts)
            {
                if (prompt.Request.IndexOf("Password:", StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    prompt.Response = connectionDetails.Password;
                }
            }
        }

        private async Task<bool> RunBulkInsert()
        {

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            try
            {
                //invoking our entity model reference
                using (Entities entities = new Entities())
                {

                    //clearing the files before inserting the new data
                    foreach (var table in tableNames)
                    {

                        eventLog.AppendText($"Truncating table: {table}\n");

                        await Task.Run(() =>
                        {

                            entities.Database.ExecuteSqlCommand($"truncate table [{table}]");
                        });

                        eventLog.AppendText($"Table truncated: {table}\n\n");
                    }

                    //inserting the refresh data into the database
                    foreach (var item in dumpLog)
                    {

                        eventLog.AppendText($"Inserting into: {item.Key.Substring(3).Replace('.', '_')}\n");

                        await Task.Run(() =>
                        {

                            entities.Database.ExecuteSqlCommand($"BulkInsert '{item.Key.Substring(3).Replace('.', '_')}', '{connectionDetails.DestinationPath + item.Key}.DAT', '{item.Value}'");
                        });

                        eventLog.AppendText($"Insert completed: {item.Key.Substring(3).Replace('.', '_')}\n\n");
                    }
                }
            }
            catch (Exception exception)
            {

                eventLog.AppendText(exception.Message);

                return false;
            }

            stopwatch.Stop();

            eventLog.AppendText("Bulk insert complete.\n" +
                    $"Time elapsed: {stopwatch.ElapsedMilliseconds}ms\n\n");

            bulkInsertButton.BackColor = Color.Green;

            return true;
        }
    }
}
