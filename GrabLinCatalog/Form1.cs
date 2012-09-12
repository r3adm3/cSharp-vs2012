﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using HtmlAgilityPack;
using System.Xml;

namespace GrabLinCatalog
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        public static List<string> ProductURLs = new List<string>();

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


        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Change the value of the ProgressBar to the BackgroundWorker progress.
            this.progressBar1.Value = e.ProgressPercentage;
            // Set the text.
            this.Text = e.ProgressPercentage.ToString();
        }

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

        private void button1_Click(object sender, EventArgs e)
        {
            //just a bit of UI
            this.textBox1.Text = "Calculating...";

            //change the buttons, so the user can only click on what he should do. 
            //he can't click go twice, and the cancel button gets ungreyed. 
            this.button1.Enabled = false;
            this.button2.Enabled = true;

            // Start the BackgroundWorker.
            //backgroundWorker1.RunWorkerAsync();

            StringBuilder sb = new StringBuilder();
            
            
            foreach (string URL in GetURLs())
            {
                //here we want to load all category urls into an array/collection of some sort. 
                //sb.Append(URL + " " + AreThereProducts(URL).ToString() + "\r\n");
                if (AreThereProducts(URL))
                {
                    //remove this break to do a full run. 
                    break;
                };
            }
            

            foreach (string URL in ProductURLs)
            {
                sb.Append(URL + "\r\n");
            }

            textBox1.Text = sb.ToString();

            //once we have the array to call of all urls, 
            //we now need to parse for each product and 
            //load into a db.

        }

        private bool AreThereProducts(string URL)
        {
            //first check if there are any products on the page
            //update global list
            //if not...

            List<string> URLs = new List<string>();

            //First get a page from the lincat website....
            HtmlWeb HW = new HtmlWeb();

            var document = HW.Load(URL);

            //17.08 - var ProdTags = document.DocumentNode.SelectNodes("//table[@id='product_table']//td//@href");
            var ProdTags = document.DocumentNode.SelectNodes("//table[@id='product_table']//td//@href");

            if (ProdTags != null)
            {
                //17.08 - ProductURLs.Add(URL);
                foreach (var tag in ProdTags)
                {
                    if (tag.Attributes["HREF"] != null)
                    { 

                        //check its not already been saved. 
                        bool isItThere = false;
                        foreach (string ProdURL in ProductURLs)
                        {
                            if (ProdURL == tag.Attributes["HREF"].Value.ToString())
                            {
                                isItThere = true;
                            }
                        }

                        //save it
                        if (!isItThere)
                        {
                            ProductURLs.Add(tag.Attributes["HREF"].Value.ToString());
                        }
                    }
                }

                return true;
            }
            else
            {
                return false;
            }

        }

        private void UpdateProductList(string URL)
        {

        }


        private List<string> GetURLs()
        {
            List<string> URLs = new List<string>();
        
            //First get a page from the lincat website....
            HtmlWeb HW = new HtmlWeb();

            var document = HW.Load("http://www.lincat.co.uk");

            var spanTags = document.DocumentNode.SelectNodes("//div[@class='prod_nav']//@href");

            if (spanTags != null)
            {
                foreach (var tag in spanTags)
                {
                    if (tag.Attributes["HREF"] != null)
                    {
                        URLs.Add("http://www.lincat.co.uk" + tag.Attributes["HREF"].Value.ToString());
                    }
                }
            }

            return URLs;

        }



        private void button2_Click(object sender, EventArgs e)
        {
            this.backgroundWorker1.CancelAsync();
            this.button1.Enabled = true;
            this.button2.Enabled = false;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}