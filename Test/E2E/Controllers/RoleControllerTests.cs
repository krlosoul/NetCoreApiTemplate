namespace Test.E2E.Controllers
{
    using Microsoft.AspNetCore.Mvc.Testing;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Xunit;
    using FluentAssertions;
    using Api;
    using Microsoft.Extensions.Configuration;
    using System.IO;
    using System.Net.Http.Json;
    using Core.Dtos.ResponsesDto;
    using Core.Dtos.Jwt;

    public class RoleControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public RoleControllerTests(WebApplicationFactory<Program> factory)
        {
            var testFactory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.test.json", optional: false, reloadOnChange: true);
                });
            });

            _client = testFactory.CreateClient();
        }

        [Fact]
        public async Task GetAllRoles_Ok()
        {
            var token = await GetAuthTokenAsync();
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var response = await _client.GetAsync("/api/V1/Role/GetAllRoles");
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateRole_WhenNoToken_ShouldReturnUnauthorized()
        {
            var newRole = new { Description = "Admin" };
            var content = new StringContent(JsonConvert.SerializeObject(newRole), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/V1/Role/CreateRole", content);
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        }
    
        private async Task<string> GetAuthTokenAsync()
        {
            var loginRequest = new
            {
                Email = "prueba@gmail.com",
                Password = "1234"
            };
            var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/V1/User/Authenticate", content);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<Result<TokenDto>>();

            return result?.Data.Token;
        }
    }
}
