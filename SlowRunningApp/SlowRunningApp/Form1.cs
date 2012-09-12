using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

/*
 * Using Examples from:
 * http://msdn.microsoft.com/en-us/library/system.componentmodel.backgroundworker.aspx
 * http://www.dotnetperls.com/progressbar
 * http://www.dotnetperls.com/backgroundworker
*/

namespace SlowRunningApp
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();

        }

        
        //any code you want to run when the form loads. 
        private void Form1_Load(object sender, System.EventArgs e)
        {
           
        }

        //this is where the background thread work ends up
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //cast the sender object (which represents the RunWorkerAsync in the button1_click)
            BackgroundWorker worker = sender as BackgroundWorker;

            //some work to do....
            for (int i = 1; i <= 100; i++)
            {
                //just do a quick check, make sure the user hasn't clicked cancel
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                //get on and do the work now
                else
                {
                    // Wait 100 milliseconds.
                    Thread.Sleep(50);
                    // Report progress back....
                    backgroundWorker1.ReportProgress(i);
                }

            }
        }

        //this method updates the UI everytime the report progress fires...
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Change the value of the ProgressBar to the BackgroundWorker progress.
            this.progressBar1.Value = e.ProgressPercentage;
            // Set the text.
            this.Text = e.ProgressPercentage.ToString();
        }

        //the button which fires off the async thread,
        private void button1_Click(object sender, EventArgs e)
        {
            //just a bit of UI
            this.textBox1.Text = "Calculating...";
            
            //change the buttons, so the user can only click on what he should do. 
            //he can't click go twice, and the cancel button gets ungreyed. 
            this.button1.Enabled = false;
            this.button2.Enabled = true;
           
            // Start the BackgroundWorker.
            backgroundWorker1.RunWorkerAsync();
        }

        //method to run once the background thread has finished
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //catch there error if there was one. 
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            //someone pressed the cancel button here.
            else if (e.Cancelled)
            {
                textBox1.Text = "Cancelled";
            }
            //no errors...show any results. 
            else
            {
                textBox1.Text = "Completed";
            }
            //re-enable the go button, disable the cancel button since the job
            //is complete. 
            button1.Enabled = true;
            button2.Enabled = false;

        }

        //button2 is the cancel button. 
        private void button2_Click(object sender, EventArgs e)
        {
            this.backgroundWorker1.CancelAsync();
            this.button1.Enabled = true;
            this.button2.Enabled = false;

        }

        //executes the work on the main thread NOT a background one.
        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "Notice you can't click on anything";
            for (int i = 1; i <= 100; i++)
            { 
                // Wait 100 milliseconds.
                    Thread.Sleep(50);
                    // Report progress back....
                    //backgroundWorker1.ReportProgress(i);
                    progressBar1.Value = i;

            }
        }

    }
}
