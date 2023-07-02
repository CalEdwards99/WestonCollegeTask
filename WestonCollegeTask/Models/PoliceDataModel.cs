using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WestonCollegeTask.Models
{
    public class PoliceDataModel
    {      

        public class Location
        {
            public string latitude { get; set; }
            public string longitude { get; set; }
        }

        //public DateTimeOffset Date { get; set; }
        public string month { get; set; }

        public Location location { get; set; }
        public string category { get; set; }
        public int count { get; set; }

    }

    public class CategoryCount
    {
        public string category { get; set; }
        public int count { get; set; } = 0;
    }

}
