using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;

// Fernando Rozenblit, 2018

namespace ExtractChannels2
{
    public partial class MainWindow : Form
    {
        ChannelExtractor extractor;
        FolderBrowserDialog folder;
        Task processingTask;

        private void OutputLine(string line)
        {
            txtOutput.Invoke((Action)(() => txtOutput.AppendText(line)));
        }

        private void UpdateProgress(double progress, int stimulusId)
        {
            //barProgress.GetCurrentParent().Invoke((Action)(() => barProgress.Value = (int)progress ));
            lstFiles2.Invoke((Action)(() =>
            {
                var item = lstFiles2.FindItemWithText(stimulusId.ToString(), true, 0);
                item.SubItems[2].Text = String.Format("{0:##.##%}", progress);
            }));
        }


        public MainWindow()
        {
            InitializeComponent();
            extractor = new ChannelExtractor(OutputLine, UpdateProgress);
            folder = new FolderBrowserDialog();
        }


        private void ButtonClick_ExtractBins(object sender, EventArgs e)
        {
            barProgress.Value = 0;
            barProgress.Visible = true;
            barProgress.Maximum = lstFiles2.Items.Count;
            txtLabel.Text = "Extracting...";

            List<string> paths = new List<string>();
            List<int> stimuliId = new List<int>();
            
            for (int i = 0; i < lstFiles2.Items.Count; i++)
            {
                var listItem = lstFiles2.Items[i];
                int stimulusId; // needs to be inside the loop for being properly used by the lambda below
                if (!int.TryParse(listItem.SubItems[1].Text, out stimulusId))
                    stimulusId = 1000 + i;

                paths.Add(listItem.SubItems[0].Text);
                stimuliId.Add(stimulusId);
            }

            processingTask = Task.Run(() =>
            {
                try
                {
                    for (int i = 0; i < paths.Count; i++)
                    {
                        txtCurrentfile.Invoke((Action)(() =>
                        {
                            txtCurrentfile.Text = paths[i];
                        }));

                        extractor.ExtractBins(paths[i], stimuliId[i]);

                        barProgress.GetCurrentParent().Invoke((Action)(() =>
                        {
                            barProgress.Value += 1;
                            if (barProgress.Value == barProgress.Maximum)
                            {
                                txtLabel.Text = "Ready";
                                barProgress.Visible = false;
                            }
                        }));

                    }
                }
                catch (Exception exc)
                {
                    Program.ExceptionLogger(exc);
                    throw;
                }
            });
            processingTask.SetUnhandledExceptionHandler();
            
        }

        private string[] ExplodeExperiment(string expname)
        {
            return expname.Split("_".ToCharArray(), 2);
        }

        private bool GetExperimentNumber(string expname, out int number)
        {
            string exp = ExplodeExperiment(expname)[0];
            if (!int.TryParse(exp, out number))
            {
                // slow path, used if the string before '_' is not a number
                Match numberInExp = Regex.Match(exp, "[0-9]+");
                if (numberInExp.Success)
                    return int.TryParse(numberInExp.Value, out number);

                return false;
            }
            return true;
        }

        private int ExperimentComparer(string exppath1, string exppath2)
        {
            string expname1 = Path.GetFileName(exppath1);
            string expname2 = Path.GetFileName(exppath2);

            int no_exp1 = 0;
            int no_exp2 = 0;

            if (GetExperimentNumber(expname1, out no_exp1) && GetExperimentNumber(expname2, out no_exp2))
            {
                if (no_exp1 != no_exp2)
                    return no_exp1 - no_exp2;
            }

            return String.Compare(exppath1, exppath2);
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            LoadFilesFromList(files);
        }

        private void LoadFilesFromList(string[] files)
        {
            // Sort files
            var fileList = new List<string>();
            fileList.AddRange(files);
            fileList.Sort(ExperimentComparer);

            // Add to ListView
            lstFiles2.BeginUpdate();
            lstFiles2.Items.Clear();
            for (int i = 0; i < fileList.Count; i++)
            {
                string file = fileList[i];
                ListViewItem fileItem = new ListViewItem(file)
                {
                    Checked = true
                };
                fileItem.SubItems.Add((i + 1).ToString());
                fileItem.SubItems.Add("");

                lstFiles2.Items.Add(fileItem);
            }
            lstFiles2.EndUpdate();
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            txtOutput.Clear();
            lstFiles2.Items.Clear();
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (folder.ShowDialog() == DialogResult.OK)
            {
                string[] files = Directory.GetFiles(folder.SelectedPath, "*.msrd");
                LoadFilesFromList(files);
            }
        }
    }



    static class Helper
    {

        public static Task IgnoreExceptions(this Task task)
        {
            task.ContinueWith(c => { var ignored = c.Exception; },
                TaskContinuationOptions.OnlyOnFaulted |
                TaskContinuationOptions.ExecuteSynchronously);
            return task;
        }

        public static Task SetUnhandledExceptionHandler(this Task task)
        {
            task.ContinueWith(c => Program.ExceptionLogger(c.Exception),
                TaskContinuationOptions.OnlyOnFaulted |
                TaskContinuationOptions.ExecuteSynchronously);
            return task;
        }

        public static Task FailFastOnException(this Task task)
        {
            task.ContinueWith(c => Environment.FailFast("Background task faulted", c.Exception),
                TaskContinuationOptions.OnlyOnFaulted |
                TaskContinuationOptions.ExecuteSynchronously);
            return task;
        }

    }
}