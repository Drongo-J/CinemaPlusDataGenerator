using CinemaDataGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinema.Entities.Models
{
    public class Ticket : IEntity
    {
        public string Id { get; set; }
        public string SessionId { get; set; }
        public virtual Session Session { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Payment { get; set; }
        public string CardNumber { get; set; }
    }
}