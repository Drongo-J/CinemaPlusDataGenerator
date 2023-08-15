using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaDataGenerator.Helpers
{
    public static class Extensions
    {
        private static Random _random = new Random();

        public static List<T> GetRandomDistinctItems<T>(this List<T> source, int amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

            int itemsToRetrieve = Math.Min(amount, source.Count);
            List<T> retrievedItems = new List<T>(itemsToRetrieve);
            HashSet<int> selectedIndices = new HashSet<int>();

            while (selectedIndices.Count < itemsToRetrieve)
            {
                int randomIndex = _random.Next(source.Count);
                if (!selectedIndices.Contains(randomIndex))
                {
                    selectedIndices.Add(randomIndex);
                    retrievedItems.Add(source[randomIndex]);
                }
            }

            return retrievedItems;
        }
    }
}
