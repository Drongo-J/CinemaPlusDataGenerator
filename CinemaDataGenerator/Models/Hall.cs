using CinemaDataGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinema.Entities.Models
{
    public class Hall : IEntity
    {
        public string Id { get; set; }
        public string TheatreId { get; set; }
        public string Name { get; set; }
        public virtual List<Seat> Seats { get; set; }
    }
}