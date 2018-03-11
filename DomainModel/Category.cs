
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DomainModel
{
    public class Category
    {
        [JsonProperty("cid")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }


        /// <summary>  </summary>
        public List<Category> GetDefaultCategories()
        {
            return new List<Category>
            {
                new Category {Id = 1, Name = "Beverages"},
                new Category {Id = 2, Name = "Condiments"},
                new Category {Id = 3, Name = "Confections"}
            };
        }


        /// <summary>  </summary>
        public Category GetDefaultCategory(int number)
        {
            switch (number)
            {
                case 1:
                    return new Category { Id = 1, Name = "Beverages" };                 
                case 2:
                    return new Category { Id = 2, Name = "Condiments" };
                case 3:
                    return new Category { Id = 3, Name = "Confections" };                    
                default:
                    return null;                 
            }         
        }


    }
}
