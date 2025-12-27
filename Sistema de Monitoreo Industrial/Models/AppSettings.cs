using System;
using System.Collections.Generic;
using System.Text;

namespace Sistema_de_Monitoreo_Industrial.Models
{
    public class AppSettings
    {
        public string InfluxUrl { get; set; } = "http://localhost:8086";
        public string InfluxToken { get; set; } = "5iOdKV3tTGLSg7RrXXQbJuyUXwKR2Yxx0Kr-dS_Zz-4Jv_EfOGexzxNjdN3AvsK0pTF_G44NlPgDnuDNkiPyiQ==";
        public string InfluxOrg { get; set; } = "Vertex-IoT";
        public string InfluxBucket { get; set; } = "texts";
    }
}
