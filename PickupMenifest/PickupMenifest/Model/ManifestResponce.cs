using System;
using System.Collections.Generic;
using System.Text;

namespace PickupMenifest.Model
{
    public class ManifestResponce
    {
        public string status { get; set; }
        public string manifestUrl { get; set; }
        public string message { get; set; }
        public int status_code { get; set; }
    }
}
