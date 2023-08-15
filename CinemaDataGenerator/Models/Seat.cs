using CinemaDataGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinema.Entities.Models
{
    public class Seat : IEntity
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public bool IsAvailable { get; set; }
        public string SessionId { get; set; }
        public virtual Session Session { get; set; }
    }
}