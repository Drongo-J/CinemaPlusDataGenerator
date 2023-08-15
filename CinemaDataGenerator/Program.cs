using Bogus;
using Cinema.Entities.Models;
using CinemaDataGenerator.Helpers;
using CinemaDataGenerator.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace CinemaDataGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Uncomment this if there are no movies in Files/movies.json files
            //var movies = await GetMovies(60);
            //WriteMoviesToFile(movies); // Save Movie Data in File

            string resultPath = string.Empty;

            ShowStatusMessage("\nGenerating SQL Insert Statements for Movies . . .");
            resultPath = GenerateSqlInsertStatements(FileHelper<Movie>.ReadDataFromFile, "movies.json", "movieInsertStatements.sql");
            ShowSuccessMessage("Generated Statements for Movies Successfully!");
            ShowSuccessMessage($"Result Path : {resultPath}");

            ShowStatusMessage("\nGenerating SQL Insert Statements for Theatres . . .");
            resultPath = GenerateSqlInsertStatements(FileHelper<Theatre>.ReadDataFromFile, "theatres.json", "theatreInsertStatements.sql");
            ShowSuccessMessage("Generated Statements for Theatres Successfully!");
            ShowSuccessMessage($"Result Path : {resultPath}");

            ShowStatusMessage("\nGenerating SQL Insert Statements for Halls . . .");
            resultPath = GenerateSqlInsertStatements(FileHelper<Hall>.ReadDataFromFile, "halls.json", "hallInsertStatements.sql");
            ShowSuccessMessage("Generated Statements for Halls Successfully!");
            ShowSuccessMessage($"Result Path : {resultPath}");

            ShowStatusMessage("\nGenerating SQL Insert Statements for Languages . . .");
            resultPath = GenerateSqlInsertStatementsForLanguages(FileHelper<Movie>.ReadDataFromFile(Path.Combine("~/../../Files", "movies.json")), "languagesInsertStatements.sql");
            ShowSuccessMessage("Generated Statements for Languages Successfully!");
            ShowSuccessMessage($"Result Path : {resultPath}");

            ShowStatusMessage("\nGenerating SQL Insert Statements for Subtitles . . .");
            resultPath = GenerateSqlInsertStatementsForSubtitles(FileHelper<Movie>.ReadDataFromFile(Path.Combine("~/../../Files", "movies.json")), "subtitlesInsertStatements.sql");
            ShowSuccessMessage("Generated Statements for Subtitles Successfully!");
            ShowSuccessMessage($"Result Path : {resultPath}");

            ShowStatusMessage("\nGenerating SQL Insert Statements for Sessions . . .");
            var result = GenerateSqlInsertStatementsForSessions("sessionsInsertStatements.sql");
            resultPath = result.Path;
            ShowSuccessMessage("Generated Statements for Sessions Successfully!");
            ShowSuccessMessage($"Result Path : {resultPath}");

            ShowStatusMessage("\nGenerating SQL Insert Statements for Seats . . .");
            resultPath = GenerateSqlInsertStatementsForSeats(result.Sessions, "seatsInsertStatements.sql");
            ShowSuccessMessage("Generated Statements for Seats Successfully!");
            ShowSuccessMessage($"Result Path : {resultPath}");

            ShowStatusMessage("\nGenerating SQL Insert Statements for Tickets . . .");
            resultPath = GenerateSqlInsertStatementsForTickets(result.Sessions, "ticketsInsertStatements.sql");
            ShowSuccessMessage("Generated Statements for Tickets Successfully!");
            ShowSuccessMessage($"Result Path : {resultPath}");

            ShowStatusMessage("\nGenerating SQL Insert Statements for TheatreImages . . .");
            resultPath = GenerateSqlInsertStatementsForTheatreImages("theatreImagesInsertStatements.sql");
            ShowSuccessMessage("Generated Statements for TheatreImages Successfully!");
            ShowSuccessMessage($"Result Path : {resultPath}");
        }

        private static List<string> GetGroupNames(string[] urls, int segmentIndex)
        {
            Dictionary<string, List<string>> urlGroups = new Dictionary<string, List<string>>();

            var groupNames = new List<string>();
            foreach (string url in urls)
            {
                string[] urlParts = url.Split('/');
                if (urlParts.Length > segmentIndex)
                {
                    string groupName = urlParts[segmentIndex];
                    groupNames.Add(groupName);
                }
            }

            return groupNames.Distinct().ToList();
        }
        static string GetTheaterIdByGroupName(string groupName)
        {
            switch (groupName)
            {
                case "28-mall":
                    return "ad01d4f8-e2cf-4be7-a6f9-2b7afa0ac564";
                case "amburan-mall":
                    return "a6c7b899-9261-4857-ae3f-10bf3a77c8c1";
                case "azerbaijan-cinema":
                    return "71d9027c-cce3-4096-9a5f-55eeb899321c";
                case "deniz-mall":
                    return "d9843f0d-f129-41f4-ba84-0f6d0a96cfaa";
                case "ganja-mall":
                    return "4bae2289-8eab-47c0-ad39-9414ef47bce8";
                case "ganjlik-mall":
                    return "bc0fed18-2b75-46af-978e-e33aad82bd71";
                case "khamsa-park-ganja":
                    return "24709fc9-0940-458d-9569-1abbc2afd966";
                case "nakhchivan":
                    return "3878b117-48de-4688-a54b-55196dd49f37";
                case "shamakhi":
                    return "eae690b3-ff2a-4fe5-9cb9-67a369170db0";
                case "sumgayit":
                    return "bcdcc140-5002-4693-960c-a3284fca98c6";
                default:
                    return null;
            }
        }

        private static string GenerateSqlInsertStatements<T>(Func<string, List<T>> dataReader, string inputFileName, string ouputFileName)
        {
            var data = dataReader(Path.Combine("~/../../Files", inputFileName));
            var insertStatements = new List<string>();

            foreach (var item in data)
            {
                var insertStatement = SqlStatementGenerator.GenerateInsertStatement(item);
                insertStatements.Add(insertStatement);
            }

            var insertStatementsPath = Path.Combine("~/../../../Files/SqlInsertStatements", ouputFileName);
            FileHelper<List<string>>.WriteTextFile(insertStatementsPath, insertStatements);
            return Path.GetFullPath(insertStatementsPath);
        }

        private static string GenerateSqlInsertStatementsForLanguages(List<Movie> movies, string ouputFileName)
        {
            var insertStatements = new List<string>();
            foreach (var movie in movies)
            {
                var languages = movie.Languages.ToList();

                foreach (var language in languages)
                {
                    language.MovieId = movie.Id;
                    var insertStatement = SqlStatementGenerator.GenerateInsertStatement(language);
                    insertStatements.Add(insertStatement);
                }
            }
            var path = Path.Combine("~/../../../Files/SqlInsertStatements", ouputFileName);
            FileHelper<List<string>>.WriteTextFile(path, insertStatements);
            return Path.GetFullPath(path);
        }

        private static string GenerateSqlInsertStatementsForSubtitles(List<Movie> movies, string ouputFileName)
        {
            var insertStatements = new List<string>();
            foreach (var movie in movies)
            {
                var subtitles = movie.Subtitles.ToList();

                foreach (var subtitle in subtitles)
                {
                    subtitle.MovieId = movie.Id;
                    var insertStatement = SqlStatementGenerator.GenerateInsertStatement(subtitle);
                    insertStatements.Add(insertStatement);
                }
            }
            var path = Path.Combine("~/../../../Files/SqlInsertStatements", ouputFileName);
            FileHelper<List<string>>.WriteTextFile(path, insertStatements);
            return Path.GetFullPath(path);
        }

        private static CustomResult GenerateSqlInsertStatementsForSessions(string ouputFileName)
        {
            var rootPath = Path.GetFullPath("~/../../../Files/");
            var movies = FileHelper<Movie>.ReadDataFromFile(rootPath + "movies.json");
            var halls = FileHelper<Hall>.ReadDataFromFile(rootPath + "halls.json");
            var theatres = FileHelper<Theatre>.ReadDataFromFile(rootPath + "theatres.json");

            // In a Theatrere will be 4 movies in 1 hall. They start at :
            // 1. 12:00 2. 15:30 3.19:00 4. 22:30

            List<Session> sessions = new List<Session>();
            var insertStatements = new List<string>();
            foreach (var theatre in theatres)
            {
                foreach (var hall in halls)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        var randomMoviesForHall = movies.GetRandomDistinctItems(4);

                        TimeSpan[] timeOffsets = new TimeSpan[]
                        {
                            TimeSpan.FromHours(0),
                            TimeSpan.FromHours(3.5),
                            TimeSpan.FromHours(7),
                            TimeSpan.FromHours(10.5)
                        };

                        for (int j = 0; j < randomMoviesForHall.Count; j++)
                        {
                            var movie = randomMoviesForHall[j];
                            var session = new Session()
                            {
                                Id = Guid.NewGuid().ToString(),
                                MovieId = movie.Id,
                                HallId = hall.Id
                            };

                            DateTime baseTime = DateTime.ParseExact("12:00", "HH:mm", null);
                            DateTime specificTime = baseTime + timeOffsets[j];
                            specificTime = specificTime.AddDays(i);

                            session.StartTime = specificTime;

                            sessions.Add(session);

                            var insertStatement = SqlStatementGenerator.GenerateInsertStatement(session);
                            insertStatements.Add(insertStatement);
                        }
                    }
                }
            }

            var path = Path.Combine("~/../../../Files/SqlInsertStatements", ouputFileName);
            FileHelper<List<string>>.WriteTextFile(path, insertStatements);
            return new CustomResult()
            {
                Path = Path.GetFullPath(path),
                Sessions = sessions,
            };
        }

        private static string GenerateSqlInsertStatementsForSeats(List<Session> sessions, string ouputFileName)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("Long Process Started (Approximately 16240x196=3,183,040 seat insert statement will be generated");
            Console.ResetColor();

            var insertStatements = new List<string>();
            Random random = new Random();
            const int seatCount = 196; // 14x14

            foreach (var session in sessions)
            {
                for (int i = 0; i < seatCount; i++)
                {
                    var seat = new Seat()
                    {
                        Id = Guid.NewGuid().ToString(),
                        SessionId = session.Id,
                        Number = (i + 1).ToString(),
                        IsAvailable = random.Next(2) == 0,
                    };
                    var insertStatement = SqlStatementGenerator.GenerateInsertStatement(seat);
                    insertStatements.Add(insertStatement);
                }
            }

            var path = Path.Combine("~/../../../Files/SqlInsertStatements", ouputFileName);
            FileHelper<List<string>>.WriteTextFile(path, insertStatements);
            return Path.GetFullPath(path);
        }

        private static string GenerateSqlInsertStatementsForTickets(List<Session> sessions, string ouputFileName)
        {
            var insertStatements = new List<string>();
            var mysession = sessions.GetRandomDistinctItems(300);
            var faker = new Faker();

            foreach (var session in mysession)
            {
                var ticket = new Ticket()
                {
                    Id = Guid.NewGuid().ToString(),
                    SessionId = session.Id,
                    PurchaseDate = faker.Date.Between(new DateTime(2023, 1, 1), DateTime.Now.Date),
                    PhoneNumber = faker.Phone.PhoneNumber(),
                    Payment = faker.Finance.TransactionType(),
                    CardNumber = faker.Finance.CreditCardNumber(),
                    Email = faker.Internet.Email()
                };

                var insertStatement = SqlStatementGenerator.GenerateInsertStatement(ticket);
                insertStatements.Add(insertStatement);
            }

            var path = Path.Combine("~/../../../Files/SqlInsertStatements", ouputFileName);
            FileHelper<List<string>>.WriteTextFile(path, insertStatements);
            return Path.GetFullPath(path);
        }

        static async Task<List<Movie>> GetMovies(int movieCount)
        {
            // Retreiving Movies Names that are Trending
            ShowStatusMessage("Getting Trending Movies . . .");
            var trendingMovieNames = await MovieService.GetListOfTrendingMovieNamesAsync(movieCount);
            if (trendingMovieNames != null)
            {
                Console.WriteLine($"Movie Count : {trendingMovieNames.Count}");
                ShowSuccessMessage("Retreived Trending Movies Successfully!");

                // Retreiving Movie Trailers
                ShowStatusMessage("Getting Movie Trailers . . .");
                Dictionary<string, string> movieTrailers = new Dictionary<string, string>();
                int successfulCount = 0;
                foreach (var movieName in trendingMovieNames)
                {
                    var movieTrailerUrl = await MovieService.GetTrailerUrlAsync(movieName);
                    if (movieTrailerUrl != null)
                    {
                        movieTrailers[movieName] = movieTrailerUrl;
                        successfulCount++;
                    }
                    else
                    {
                        movieTrailers[movieName] = string.Empty;
                        ShowErrorMessage($"An error occured while getting movie trailer for \"{movieName}\". String.Empty was set for Movie Trailer . . .");
                    }
                }
                Console.WriteLine($"Movie Trailer Count Fetched Successfully : {successfulCount}");
                ShowSuccessMessage("Retrieved Movie Trailers Successfully!");


                // Get Movie Data according to name + Get All Movies
                ShowStatusMessage("Retrieving Movie Data . . .");
                List<Movie> movies = new List<Movie>();
                foreach (var movieName in trendingMovieNames)
                {
                    var movie = await MovieService.GetBestMatchingMovieAsync(movieName);
                    if (movie != null)
                    {
                        movies.Add(movie);
                    }
                }
                Console.WriteLine("Reterieved Movie Data Count : " + movies.Count);
                ShowSuccessMessage("Retrieved Movie Data Successfully!");

                return movies;
            }
            else
            {
                ShowErrorMessage("An issue occured while retrieving trending movies");
                return null;
            }
        }


        private static string GenerateSqlInsertStatementsForTheatreImages(string ouputFileName)
        {
            var theatreImagesPath = Path.Combine("~/../../../Files", "theatreImages.txt");
            var urls = FileHelper<string>.ReadTextFile(theatreImagesPath);
            List<string> groupNames = GetGroupNames(urls.ToArray(), 8);

            List<string> insertStatements = new List<string>();
            foreach (var groupName in groupNames)
            {
                var theaterImagesUrls = urls.Where(u => u.Contains(groupName)).ToList();

                foreach (var theaterImagesUrl in theaterImagesUrls)
                {
                    var theatreImage = new TheatreImage()
                    {
                        Id = Guid.NewGuid().ToString(),
                        TheatreId = GetTheaterIdByGroupName(groupName),
                        ImageUrl = theaterImagesUrl
                    };

                    var insertStatement = SqlStatementGenerator.GenerateInsertStatement(theatreImage);
                    insertStatements.Add(insertStatement);
                }
            }

            var path = Path.Combine("~/../../../Files/SqlInsertStatements", ouputFileName);
            FileHelper<List<string>>.WriteTextFile(path, insertStatements);
            return Path.GetFullPath(path);
        }
        static void ShowErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        static void ShowSuccessMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        static void ShowStatusMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        static void WriteMoviesToFile(List<Movie> movies)
        {
            ShowStatusMessage("Writing movies to file . . .");
            var path = Path.Combine("~/../../../Files", "movies.json");
            FileHelper<Movie>.Serialize(movies, path);
            ShowSuccessMessage("Movie data has been successfully written to the file!");
        }
    }
    class CustomResult
    {
        public string Path { get; set; }
        public List<Session> Sessions { get; set; }
    }
}