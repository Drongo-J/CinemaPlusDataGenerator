using CinemaDataGenerator.Models;

namespace Cinema.Entities.Models
{
    public class TheatreImage : IEntity
    {
        public string Id { get; set; }
        public string ImageUrl { get; set; }
        public string TheatreId { get; set; }
        public virtual Theatre Theatre { get; set; }
    }
}