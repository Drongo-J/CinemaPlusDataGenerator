using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaDataGenerator.Helpers
{
    public class FileHelper<T> where T : class
    {
        public static void Serialize(List<T> values, string filename)
        {
            var serializer = new JsonSerializer();

            using (var sw = new StreamWriter(filename))
            {
                using (var jw = new JsonTextWriter(sw))
                {
                    jw.Formatting = Formatting.Indented;
                    serializer.Serialize(jw, values);
                }
            }
        }

        public static List<T> Deserialize(string filename)
        {
            List<T> values = new List<T>();

            var serializer = new JsonSerializer();

            using (var sr = new StreamReader(filename))
            {
                using (var jr = new JsonTextReader(sr))
                {
                    values = serializer.Deserialize<List<T>>(jr);
                };
            }

            return values;
        }
        public static List<T> ReadDataFromFile(string fileName)
        {
            var path = Path.Combine("~/../../../Files", fileName);
            var data = Deserialize(path);
            return data;
        }

        public static List<string> ReadTextFile(string path)
        {
            var lines = File.ReadAllLines(path);

            return lines.ToList();
        }

        public static void WriteTextFile(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        public static void WriteTextFile(string path, List<string> lines)
        {
            File.WriteAllLines(path, lines);
        }
    }
}