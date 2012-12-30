using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using HtmlAgilityPack;
using System.Xml;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using System.Net;

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
            this.textBox1.Text = "Starting...";

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
                    //break;
                };
            }
            

            foreach (string URL in ProductURLs)
            {
                Product myProduct = GetProduct(URL);
                if (myProduct != null)
                {
                    sb.Append("Success Getting" + URL + "\r\n");
                    InsertProductIntoDB(myProduct);
                    GetProductContent(URL, myProduct.Model);
                }
                else
                {
                    sb.Append("Failed Getting" + URL + "\r\n");
                };

            }

            //textBox1.AppendText (sb.ToString());

            //once we have the array to call of all urls, 
            //we now need to parse for each product and 
            //load into a db.

        }

        private int InsertProductIntoDB(Product aProduct)
        {

            //connect to SQL
            SqlConnection myConnection = new SqlConnection();

            //get connection string from app.config file
            myConnection.ConnectionString = ConfigurationManager.AppSettings["Target"].ToString();
                //"Data Source=(localdb)\\Projects;Initial Catalog=LincatLocalDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False";
            //ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();

            try{
                myConnection.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return 1;
            }

            //setting up each of the parameters from the Product that was submitted
 
            SqlCommand myCommand = new SqlCommand("INSERT INTO LinCatProductsTable (Model, Range, Description, Height, Width, Depth, Power, Fuel, notes) " +
                "VALUES (@ModelParam, @RangeParam, @DescriptionParam, @HeightParam, @WidthParam, @DepthParam, @PowerParam, @FuelParam, @notesParam)", myConnection);

                //"VALUES (@ModelParam, 'test', 'test', '1', '2', '3', 'test' , 'test')", myConnection);
                                    
                
            myCommand.Parameters.Add("@ModelParam", SqlDbType.NVarChar, 100);
            myCommand.Parameters["@ModelParam"].Value = aProduct.Model;

            myCommand.Parameters.Add("@RangeParam", SqlDbType.NVarChar, 100);
            myCommand.Parameters["@RangeParam"].Value = aProduct.Range;

            myCommand.Parameters.Add("@DescriptionParam", SqlDbType.NVarChar);
            myCommand.Parameters["@DescriptionParam"].Value = aProduct.Description;

            myCommand.Parameters.Add("@HeightParam", SqlDbType.Int);
            myCommand.Parameters["@HeightParam"].Value = aProduct.height;

            myCommand.Parameters.Add("@WidthParam", SqlDbType.Int);
            myCommand.Parameters["@WidthParam"].Value = aProduct.width;
            
            myCommand.Parameters.Add("@DepthParam", SqlDbType.Int);
            myCommand.Parameters["@DepthParam"].Value = aProduct.depth;
            
            myCommand.Parameters.Add("@PowerParam", SqlDbType.NVarChar, 50);
            myCommand.Parameters["@PowerParam"].Value = aProduct.power;

            myCommand.Parameters.Add("@FuelParam", SqlDbType.NVarChar, 50);
            myCommand.Parameters["@FuelParam"].Value = aProduct.powerType;

            myCommand.Parameters.Add("@notesParam", SqlDbType.NVarChar);
            myCommand.Parameters["@notesParam"].Value = aProduct.otherNotes;

            if (myCommand.ExecuteNonQuery().Equals(1))
            {

                textBox1.AppendText("\r\nInserted into DB Product: " + aProduct.Model);
            }
            else
            {
                textBox1.AppendText("\r\nFailed Insert DB Product: " + aProduct.Model);
            }

            //close up the connection
            try
            {
                myConnection.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return 1;
            }


            return 0;
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

        private Product GetProduct(string URL)
        {

            //First get a page from the lincat website....
            HtmlWeb HW = new HtmlWeb();
            Product SingleProduct = new Product();

            var document = HW.Load(URL);

            //17.08 - var ProdTags = document.DocumentNode.SelectNodes("//table[@id='product_table']//td//@href");
            var ProdTags = document.DocumentNode.SelectNodes("//section[@id='content']//table//tbody//td");
            var SpanTags = document.DocumentNode.SelectNodes("//section[@id='content']//p//span");
            var ImgTags = document.DocumentNode.SelectNodes("//div[@id='product_image']//img");
            var pdfTags = document.DocumentNode.SelectNodes("//li[@class='pdf']//a");

            if (ProdTags != null)
            {
                StringBuilder sb = new StringBuilder();
                StringBuilder SpanSb = new StringBuilder();

                int i = 0;
                
                foreach (var tag in SpanTags)
                {
                    //gets the bullet notes for the product
                    SpanSb.Append("\r\n - " + tag.InnerText);
                }



                //enumerates the table, and fills up aProduct object.
                foreach (var tag in ProdTags)
                {
                    i++;
                    switch (i)
                    {
                        case 1:
                            SingleProduct.Model = tag.InnerText;
                            break;
                        case 2:
                            SingleProduct.Range = tag.InnerText;
                            break;
                        case 3:
                            SingleProduct.Description = tag.InnerText;
                            break;
                        case 4:
                            SingleProduct.height = Convert.ToInt32(tag.InnerText);
                            break;
                        case 5:
                           SingleProduct.width = Convert.ToInt32(tag.InnerText);
                            break;
                        case 6:
                            SingleProduct.depth = Convert.ToInt32(tag.InnerText);
                            break;
                        case 7:
                            SingleProduct.power = tag.InnerText;
                            break;
                        case 8:
                            SingleProduct.powerType = tag.InnerText;
                            break;
                        default:
                            break;
                    }

                    //sb.Append("\r\n (" + i + ")" + tag.InnerText);
                }
               
                foreach (var tag in ImgTags)
                {
                    string link = tag.Attributes["SRC"].Value.ToString();
                    //gets the url for the image on the page
                    SpanSb.Append("\r\n   + " + link);
                    if (link.ToLower().Contains("jpg"))
                    {
                        GetProductContent(link, SingleProduct.Model);
                    }
                }

                foreach (var tag in pdfTags)
                {
                    string link = tag.Attributes["HREF"].Value.ToString();
                    //gets each of the pdfs for the product
                    SpanSb.Append("\r\n   + " + link);
                    //only get content if it's a pdf
                    if ( link.ToLower().Contains("pdf") )
                    {
                        GetProductContent(link, SingleProduct.Model);
                    }

                }

                SingleProduct.otherNotes = SpanSb.ToString();
                //textBox1.AppendText(SpanSb.ToString());

                //textBox1.AppendText(sb.ToString());
            }

            
            return SingleProduct;
        }

        private void GetProductContent(string URL, string product)
        {
            string RootFolder = textBox2.Text + "\\";

            //create folder
            try
            {
                Directory.CreateDirectory(RootFolder + product);
            }
            catch
            {
                MessageBox.Show("Failed to create folders");
            }
            
            //filename
            string filename = RootFolder + product + "\\" + URL.Substring(URL.LastIndexOf("/") + 1);
            
            //MessageBox.Show(filename);
            
            //get content @URL
            WebClient webClient = new WebClient();
            try
            {
                webClient.DownloadFile("http://www.lincat.co.uk" + URL, filename);
                textBox1.AppendText("\r\nDownloaded " + URL);
            }
            catch
            {
                textBox1.AppendText("\r\nFailed Downloading " + URL);
                //ignore
            }
            return;

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
            textBox1.Text = "";
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
