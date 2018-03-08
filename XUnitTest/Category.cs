using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace XUnitTest
{
           internal class Category
        {
            [JsonProperty("cid")]
            public int Id { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
        }
   
}
