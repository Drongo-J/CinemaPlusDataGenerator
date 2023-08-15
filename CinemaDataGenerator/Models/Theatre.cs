using CinemaDataGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinema.Entities.Models
{
    public class Theatre : IEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string PhoneNumber { get; set; }
        public virtual List<TheatreImage> TheatreImages { get; set; }
    }
}