using HttpClient http = new() { BaseAddress = new Uri("https://localhost:7227") };
while (true)
{
    HttpResponseMessage response = await http.GetAsync("api/topics/latest/0");
    Console.WriteLine(response.StatusCode);
    await Task.Delay(10);
}