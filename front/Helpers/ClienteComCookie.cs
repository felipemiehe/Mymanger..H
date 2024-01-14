namespace front.Helpers
{
    public class ClienteComCookie
    {
        private readonly HttpClient _client;
        Uri baseUrl = new Uri("https://localhost:7273");

        public ClienteComCookie(HttpRequest request)
        {
            _client = new HttpClient();
            _client.BaseAddress = baseUrl;
            ConfigureClientWithToken(request);
        }

        private void ConfigureClientWithToken(HttpRequest request)
        {
            string token = request.Cookies["X-Access-Token"];

            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Add("Cookie", $"X-Access-Token={token}");
            }
        }

        public HttpClient ConfiguredClient => _client;
    }
}
