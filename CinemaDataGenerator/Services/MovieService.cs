using Cinema.Entities.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;

namespace CinemaDataGenerator.Services
{
    public class MovieService
    {
        private static string TheMovieDbApiKey = "9e25147377bef94f642b5d239d9b7ae1";
        public static async Task<Movie> GetBestMatchingMovieAsync(string movieName)
        {
            HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync($"http://www.omdbapi.com/?apikey=d5254632&s={movieName}&plot=full");
            var str = await response.Content.ReadAsStringAsync();
            dynamic searchData = JsonConvert.DeserializeObject(str);

            if (searchData.Response == "True" && searchData.Search.Count > 0)
            {
                dynamic bestMatch = searchData.Search[0];

                response = await httpClient.GetAsync($"http://www.omdbapi.com/?apikey=d5254632&i={bestMatch.imdbID}&plot=full");
                str = await response.Content.ReadAsStringAsync();
                dynamic singleData = JsonConvert.DeserializeObject(str);

                Movie mymovie = new Movie
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = GetValue(singleData.Title),
                    Year = GetValue(singleData.Year),
                    Released = GetValue(singleData.Released),
                    RunTime = GetValue(singleData.Runtime),
                    Genre = GetValue(singleData.Genre),
                    Director = GetValue(singleData.Director),
                    Writer = GetValue(singleData.Writer),
                    Actors = GetValue(singleData.Actors),
                    Plot = GetValue(singleData.Plot),
                    ProducerCountry = GetValue(singleData.Country),
                    Awards = GetValue(singleData.Awards),
                    ImdbRating = GetValue(singleData.imdbRating),
                };

                if (singleData.Poster == "N/A" || singleData.Poster == null)
                {
                    mymovie.PosterUrl = "https://media.aykhan.net/assets/images/step-it-academy/diploma-project/noImage.jpg";
                }
                else
                {
                    var posterUrl = await GetMoviePosterUrlAsync(singleData.Poster.ToString());
                    if (posterUrl == null)
                    {
                        posterUrl = singleData.Poster.ToString();
                    }
                    var url = await CloudinaryService.UploadImageFromUrlAsync(posterUrl);
                    if (url == null || url == string.Empty || url == "N/A")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Post is missing for : {mymovie.Title}");
                        Console.ResetColor();
                    }
                    mymovie.PosterUrl = url;
                }

                var imageSize = 512;
                var flagUrlRoot = $"https://media.aykhan.net/assets/images/step-it-academy/diploma-project/flags/{imageSize}/";
                var imageFormat = ".png";

                string languages = singleData.Language.ToString();
                mymovie.Languages = languages.Split(',').Select(l => new Language
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = l.Trim(),
                    FlagUrl = GetLanguageCode(l.Trim()) == null ? "https://media.aykhan.net/assets/images/step-it-academy/diploma-project/flags/512/not-found.png" : flagUrlRoot + GetLanguageCode(l.Trim()) + imageFormat
                }).ToList();
                mymovie.Price = GetMoviePrice(mymovie);
                mymovie.AgeLimit = GetAgeLimit(GetValue(singleData.Rated));
                mymovie.Subtitles = GetSubtitles(mymovie.Id);

                return mymovie;
            }

            return null;
        }

        static string GetLanguageCode(string languageName)
        {
            // Mandarin does not work
            if (languageName == "Mandarin")
            {
                return "cn";
            }

            CultureInfo cultureInfo = CultureInfo.GetCultures(CultureTypes.AllCultures)
                                                  .FirstOrDefault(ci => ci.EnglishName.Equals(languageName, StringComparison.OrdinalIgnoreCase));

            if (cultureInfo != null)
            {
                return cultureInfo.TwoLetterISOLanguageName;
            }
            return null;
        }

        static async Task<string> GetMoviePosterUrlAsync(string movieTitle)
        {
            string apiKey = "c33503297ea966f7a225401807afed8b"; // Replace with your actual JustWatch API key

            using (HttpClient client = new HttpClient())
            {
                string apiUrl = $"https://apis.justwatch.com/content/titles/en_US/popular?apiKey={apiKey}&query={Uri.EscapeDataString(movieTitle)}";

                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic searchData = JObject.Parse(responseBody);

                    if (searchData != null && searchData.items != null && searchData.items.Count > 0)
                    {
                        string posterUrl = searchData.items[0].poster;
                        return posterUrl;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
        static List<Subtitle> GetSubtitles(string movieId)
        {
            int minSubtitles = 1;
            int maxSubtitles = 3;

            List<string> subtitleImages = new List<string>
            {
                 "https://media.aykhan.net/assets/images/step-it-academy/diploma-project/subtitles/en-sub.png",
                 "https://media.aykhan.net/assets/images/step-it-academy/diploma-project/subtitles/es-sub.png",
                 "https://media.aykhan.net/assets/images/step-it-academy/diploma-project/subtitles/fr-sub.png",
                 "https://media.aykhan.net/assets/images/step-it-academy/diploma-project/subtitles/de-sub.png",
                 "https://media.aykhan.net/assets/images/step-it-academy/diploma-project/subtitles/it-sub.png",
                 "https://media.aykhan.net/assets/images/step-it-academy/diploma-project/subtitles/ru-sub.png",
                 "https://media.aykhan.net/assets/images/step-it-academy/diploma-project/subtitles/az-sub.png",
                 "https://media.aykhan.net/assets/images/step-it-academy/diploma-project/subtitles/tr-sub.png"
            };

            List<Subtitle> randomSubtitles = new List<Subtitle>();

            Random random = new Random();
            int maxIndex = subtitleImages.Count - 1;

            int numberOfSubtitles = random.Next(minSubtitles, maxSubtitles + 1);

            for (int i = 0; i < numberOfSubtitles; i++)
            {
                int randomIndex = random.Next(0, maxIndex + 1);
                var subtitle = new Subtitle()
                {
                    Id = Guid.NewGuid().ToString(),
                    ImageUrl = subtitleImages.ElementAt(randomIndex),
                    MovieId = movieId
                };
                randomSubtitles.Add(subtitle);
            }

            return randomSubtitles;
        }
        static string GetAgeLimit(string rating)
        {
            switch (rating.ToUpper())
            {
                case "G":
                    return "0+";
                case "PG":
                    return "10+";
                case "PG-13":
                    return "13+";
                case "R":
                    return "17+";
                case "NC-17":
                    return "18+";
                default:
                    return "0+";
            }
        }
        static int GetMoviePrice(Movie movie)
        {
            int basePrice = 100;

            // Price adjustment based on IMDb rating
            int imdbPriceAdjustment = 0;
            if (movie.ImdbRating != "N/A" && decimal.TryParse(movie.ImdbRating, out decimal imdbRating))
            {
                imdbPriceAdjustment = (int)(imdbRating * 150);
            }

            // Price adjustment based on awards
            int awardsPriceAdjustment = 0;
            if (movie.Awards != "N/A" && movie.Awards.Contains("Oscar"))
            {
                awardsPriceAdjustment = 500;
            }

            // Price adjustment based on release year
            int yearPriceAdjustment = 0;
            if (movie.Year != "N/A" && int.TryParse(movie.Year, out int releaseYear))
            {
                int currentYear = DateTime.Now.Year;
                int age = currentYear - releaseYear;
                if (age <= 1)
                {
                    yearPriceAdjustment = 300;
                }
                else if (age <= 5)
                {
                    yearPriceAdjustment = 200;
                }
                else if (age <= 10)
                {
                    yearPriceAdjustment = 100;
                }
            }

            // Total price adjustment
            int totalPriceAdjustment = imdbPriceAdjustment + awardsPriceAdjustment + yearPriceAdjustment;

            // Calculate and return final price
            return (basePrice + totalPriceAdjustment) / 100;
        }
        static string GetValue(dynamic value)
        {
            if (value == null)
                return "N/A";

            return value.ToString();
        }
        public static async Task<List<string>> GetListOfTrendingMovieNamesAsync(int numberOfMovies)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    List<string> movieNames = new List<string>();
                    int moviesFetched = 0;
                    int page = 1;

                    while (moviesFetched < numberOfMovies)
                    {
                        string endpoint = $"https://api.themoviedb.org/3/trending/movie/week?api_key={TheMovieDbApiKey}&page={page}";

                        HttpResponseMessage response = await client.GetAsync(endpoint);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();

                            // Parse the JSON response
                            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);

                            foreach (var movie in data["results"])
                            {
                                if (moviesFetched >= numberOfMovies)
                                    break;

                                string title = movie["title"];
                                movieNames.Add(title);
                                moviesFetched++;
                            }

                            page++; // Move to the next page for the next iteration
                        }
                        else
                        {
                            return null;
                        }
                    }

                    return movieNames;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        public static async Task<string> GetTrailerUrlAsync(string movieName)
        {
            string apiKey = TheMovieDbApiKey;
            string baseUrl = "https://api.themoviedb.org/3/search/movie";

            using (HttpClient httpClient = new HttpClient())
            {
                string queryParams = $"?api_key={apiKey}&query={movieName}";
                string searchUrl = baseUrl + queryParams;

                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(searchUrl);
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    JObject data = JObject.Parse(jsonResponse);

                    if (data["results"].HasValues)
                    {
                        int movieId = (int)data["results"][0]["id"];
                        string movieDetailsUrl = $"https://api.themoviedb.org/3/movie/{movieId}?api_key={apiKey}&append_to_response=videos";

                        HttpResponseMessage movieResponse = await httpClient.GetAsync(movieDetailsUrl);
                        string movieJsonResponse = await movieResponse.Content.ReadAsStringAsync();
                        JObject movieData = JObject.Parse(movieJsonResponse);

                        JArray videos = (JArray)movieData["videos"]["results"];
                        foreach (var video in videos)
                        {
                            if ((string)video["type"] == "Trailer")
                            {
                                string youtubeKey = (string)video["key"];
                                string trailerUrl = $"https://www.youtube.com/watch?v={youtubeKey}";
                                return trailerUrl;
                            }
                        }
                    }

                    return null;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}
