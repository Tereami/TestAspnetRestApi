using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using TestAspnetRestApi;

internal class Program
{
    private static void Main(string[] args)
    {
        List<Person> users = new List<Person>();
        users.Add(new Person("Sasa", 33));
        users.Add(new Person("Mia", 22));
        users.Add(new Person("Баша", 11));

        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        WebApplication app = builder.Build();

        app.Run(async (context) =>
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            PathString path = request.Path;
            string expressionForGuid = @"^/api/users/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";

            bool endsWithGuid = Regex.Match(path, expressionForGuid).Success;
            string? guid = null;
            if (endsWithGuid)
                guid = path.Value?.Split('/')[3];

            if (path == "/api/users" && request.Method == "GET")
            {
                await GetAllPeople(response);
            }
            else if (path == "/api/users" && request.Method == "POST")
            {
                await CreatePerson(request, response);
            }
            else if (endsWithGuid && request.Method == "GET")
            {
                await GetPerson(guid, response);
            }
            else if (endsWithGuid && request.Method == "PUT")
            {
                await UpdatePerson(guid, response, request);
            }
            else if (endsWithGuid && request.Method == "DELETE")
            {
                await DeletePerson(guid, response);
            }
            else
            {
                response.ContentType = "text/html; charset=utf-8";
                await response.SendFileAsync("html/index.html");
            }
        });

        app.Run();

        async Task GetAllPeople(HttpResponse response)
        {
            await response.WriteAsJsonAsync(users);
        }

        async Task GetPerson(string? id, HttpResponse response)
        {
            Person? user = users.FirstOrDefault(i => i.Id == id);
            if (user != null)
            {
                await response.WriteAsJsonAsync(user);
            }
            else
            {
                response.StatusCode = 404;
                await response.WriteAsync("Get User not found");
            }
        }

        async Task CreatePerson(HttpRequest request, HttpResponse response)
        {
            try
            {
                Person? newUser = await request.ReadFromJsonAsync<Person>();
                if (newUser == null)
                {
                    throw new Exception("Create Unable to deserialize person");
                }
                else
                {
                    newUser.Id = Guid.NewGuid().ToString();
                    users.Add(newUser);
                    await response.WriteAsJsonAsync(newUser);
                }

            }
            catch (Exception ex)
            {
                response.StatusCode = 400;
                await response.WriteAsJsonAsync(new { message = "Create Incorrect data: " + ex.Message });
            }
        }

        async Task UpdatePerson(string? userId, HttpResponse response, HttpRequest request)
        {
            try
            {
                Person? userNewData = await request.ReadFromJsonAsync<Person>();
                if (userNewData == null)
                    throw new Exception("Update Incorrect data");
                if (userId == null)
                    throw new Exception("User id not defined");

                Person? userInBase = users.FirstOrDefault(i => i.Id == userId);
                if (userInBase == null)
                {
                    response.StatusCode = 404;
                    await response.WriteAsJsonAsync(new { message = "Update user not found" });
                }
                else
                {
                    userInBase.Name = userNewData.Name;
                    userInBase.Age = userNewData.Age;
                    await response.WriteAsJsonAsync(userInBase);
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 400;
                await response.WriteAsJsonAsync(new { message = "Update Incorrect data: " + ex.Message });
            }
        }

        async Task DeletePerson(string? id, HttpResponse response)
        {
            Person? user = users.FirstOrDefault(i => i.Id == id);
            if (user != null)
            {
                users.Remove(user);
                await response.WriteAsJsonAsync(user);
            }
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "Delete User not found" });
            }
        }
    }
}