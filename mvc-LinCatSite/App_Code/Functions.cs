using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace mvc_LinCatSite.App_Code
{
    public class Functions
    {
        public string DoIT(string id)
        {
            //base images folder
            string imageFolder = @"v:\tempShare\LincatImages\";
            string imgResultFolder = "/images/";

            //first open the product folder
            imageFolder = imageFolder + id;
            string[] files = Directory.GetFiles(imageFolder, "*.jpg");
            
            foreach (string file in files)
            {
                return imgResultFolder + id + "/" + Path.GetFileName(file);
                break;
            }

            return imgResultFolder + "/default.jpg";
        }
    }
}