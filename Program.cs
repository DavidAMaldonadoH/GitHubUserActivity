using System.Text.Json.Nodes;

internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                throw new Exception("You need to provide a username to retrieve the data!");
            }

            var apiUrl = $"https://api.github.com/users/{args[0].Trim()}/events";

            string s;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "C# console program");
                s = client.GetStringAsync(apiUrl).Result;
            }

            if (string.IsNullOrEmpty(s))
            {
                throw new Exception("No data found for the user!");
            }

            var jsonArray = (JsonArray.Parse(s)?.AsArray()) ?? throw new Exception("An error occurred while parsing the JSON data!");

            if (jsonArray.Count == 0)
            {
                throw new Exception("No activity found for the user!");
            }

            foreach (var item in jsonArray)
            {
                string action;

                if (item == null)
                {
                    continue;
                }

                switch (item["type"]?.ToString())
                {
                    case "PushEvent":
                        var commits = item["payload"]?["commits"]?.AsArray();
                        action = $"Pushed {commits?.Count} commits to {item["repo"]?["name"]}";
                        break;
                    case "IssuesEvent":
                        var actionString = item["payload"]?["action"]?.ToString();
                        if (string.IsNullOrEmpty(actionString))
                        {
                            continue;
                        }
                        char firstChar = actionString[0];
                        action = $"{char.ToUpper(firstChar) + actionString.Substring(1)} a new issue in {item["repo"]?["name"]}";
                        break;
                    case "WatchEvent":
                        action = $"Starred {item["repo"]?["name"]?.ToString()}";
                        break;
                    case "ForkEvent":
                        action = $"Forked {item["repo"]?["name"]?.ToString()}";
                        break;
                    case "CreateEvent":
                        action = $"Created {item["payload"]?["ref_type"]?.ToString()} in {item["repo"]?["name"]?.ToString()}";
                        break;
                    default:
                        action = $"{item["type"]?.ToString()} in {item["repo"]?["name"]?.ToString()}";
                        break;
                }
                Console.WriteLine($"- {action}");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message ?? "An error occurred while fetching the user activity!");
        }

    }
}
