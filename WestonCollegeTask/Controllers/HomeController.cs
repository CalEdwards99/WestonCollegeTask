using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WestonCollegeTask.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WestonCollegeTask.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SearchPoliceData(string lat, string lng, string date)
        {
            List<CategoryCount> categoriesToAdd = new List<CategoryCount>();
            List<CategoryCount> categoriesToCount = new List<CategoryCount>();

            string policeDataAPIcall;
            policeDataAPIcall = Convert.ToString("https://data.police.uk/api/crimes-street/all-crime?lat=" + lat + "&lng=" + lng + "&date=" + date);
            
            //if theres a bad API call, fail softly
            string reader = GetPoliceData(policeDataAPIcall);
            if (reader == "400") {
                return View("Index" , categoriesToCount);
            }
            List<PoliceDataModel> policeDataList = convertReaderToPoliceData(reader);

            //seperate for eachs, 1st establish list of all the categories. ignore the count.
            foreach (PoliceDataModel pdm in policeDataList)
            {
                CategoryCount cc = new CategoryCount();

                if (pdm.category != null)
                {
                    cc.category = pdm.category.ToString();
                    cc.count = 0;

                    //if its the first category, add it to start the list
                    if (categoriesToAdd.Count == 0)
                    {
                        categoriesToAdd.Add(cc);
                    }
                    else
                    {
                        //check that the current list doesn't already contain this category
                        CategoryCount existingCategory = new CategoryCount();
                        existingCategory.category = "";

                        existingCategory = categoriesToAdd.Find(cc => (cc.category == pdm.category));

                        if (existingCategory == null) // means that it's a new category
                        {
                            categoriesToAdd.Add(cc);
                        }
                    }

                }
            }

            //2nd for each category, get a total count for each
            foreach (CategoryCount cc in categoriesToAdd)
            {
                //loop over all of crimes in the pdm, if the category matches, add to the tally
                foreach (PoliceDataModel pdm in policeDataList)
                {
                    if (pdm.category == cc.category)
                    {
                        cc.count = cc.count + 1;
                    }
                }
                categoriesToCount.Add(cc);
            }

            return View("Index", categoriesToCount);
        }

        public string GetPoliceData(string uri)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);


                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }

            catch {return "400"; }
        }

        public List<PoliceDataModel> convertReaderToPoliceData(string reader)
        {
            List<PoliceDataModel> policeDataList = new List<PoliceDataModel>();

            //convert jsonstring to a class
            policeDataList = JsonSerializer.Deserialize<List<PoliceDataModel>>(reader);

            return policeDataList;
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
