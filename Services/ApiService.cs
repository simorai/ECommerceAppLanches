using AppLanches.Models;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AppLanches.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://www.appsnacks2025.somee.com";
        private readonly ILogger<ApiService> _logger;
        JsonSerializerOptions _serializerOptions;

        public ApiService(HttpClient httpClient, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ApiResponse<bool>> RegistrarUsuario(string nome, string email, string telefone, string password)
        {
            try
            {
                var register = new Register()
                {
                    Name = nome,
                    Email = email,
                    Phone = telefone,
                    Password = password
                };

                var json = JsonSerializer.Serialize(register, _serializerOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await PostRequest("api/Users/Register", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Erro ao enviar requisição HTTP: {response.StatusCode}");
                    return new ApiResponse<bool>
                    {
                        ErrorMessage = $"Erro ao enviar requisição HTTP: {response.StatusCode}"
                    };
                }

                return new ApiResponse<bool> { Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao registrar o usuário: {ex.Message}");
                return new ApiResponse<bool> { ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> Login(string email, string password)
        {
            try
            {
                var login = new Login()
                {
                    Email = email,
                    Password = password
                };

                var json = JsonSerializer.Serialize(login, _serializerOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await PostRequest("api/Users/Login", content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Erro ao enviar requisição HTTP : {response.StatusCode}");
                    return new ApiResponse<bool>
                    {
                        ErrorMessage = $"Erro ao enviar requisição HTTP : {response.StatusCode}"
                    };
                }

                var jsonResult = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<Token>(jsonResult, _serializerOptions);

                Preferences.Set("accesstoken", result!.AccessToken);
                Preferences.Set("usuarioid", (int)result.UserId!); // had to implement because the login was not working
                Preferences.Set("usuarionome", result!.UserName ?? "");

                return new ApiResponse<bool> { Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro no login : {ex.Message}");
                return new ApiResponse<bool> { ErrorMessage = ex.Message };
            }
        }

        public async Task<ApiResponse<bool>> AdicionaItemNoCarrinho(CarrinhoCompra carrinhoCompra)
        {
            try
            {
                var json = JsonSerializer.Serialize(carrinhoCompra, _serializerOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await PostRequest("api/ShoppingCartItems", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Erro ao enviar requisição HTTP: {response.StatusCode}");
                    return new ApiResponse<bool>
                    {
                        ErrorMessage = $"Erro ao enviar requisição HTTP: {response.StatusCode}"
                    };
                }

                return new ApiResponse<bool> { Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao adicionar item no carrinho: {ex.Message}");
                return new ApiResponse<bool> { ErrorMessage = ex.Message };
            }
        }


        private async Task<HttpResponseMessage> PostRequest(string uri, HttpContent content)
        {
            var enderecoUrl = $"{_baseUrl.TrimEnd('/')}/{uri.TrimStart('/')}";

            try
            {
                var result = await _httpClient.PostAsync(enderecoUrl, content);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao enviar requisição POST para {uri}: {ex.Message}");
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        public async Task<(List<Category>? Categorias, string? ErrorMessage)> GetCategorias()
        {
            return await GetAsync<List<Category>>("api/Categories");
        }

        public async Task<(List<Product>? Produtos, string? ErrorMessage)> GetProdutos(string tipoProduto, string categoriaId)
        {
            string endpoint = $"api/Products?Search={tipoProduto}&categoryId={categoriaId}";
            return await GetAsync<List<Product>>(endpoint);
        }

        public async Task<(Product? ProdutoDetalhe, string? ErrorMessage)> GetProdutoDetalhe(int produtoId)
        {
            string endpoint = $"api/products/{produtoId}";
            return await GetAsync<Product>(endpoint);
        }

        //public async Task<(List<CarrinhoCompraItem>? CarrinhoCompraItems, string? ErrorMessage)> GetItensCarrinhoCompra(int usuarioId)
        //{
        //    var endpoint = $"api/ShoppingCartItems/{usuarioId}";
        //    return await GetAsync<List<CarrinhoCompraItem>>(endpoint);
        //}
        public async Task<(List<CarrinhoCompraItem>? CarrinhoCompraItems, string? ErrorMessage)> GetItensCarrinhoCompra(int usuarioId)
        {
            try
            {
                // basic validation for usuarioId
                if (usuarioId <= 0)
                {
                    return (null, "Usuário não autenticado ou ID inválido.");
                }

                AddAuthorizationHeader();


                var endpoint = $"{_baseUrl}/api/ShoppingCartItems/{usuarioId}";

                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var items = JsonSerializer.Deserialize<List<CarrinhoCompraItem>>(responseString, _serializerOptions);
                    return (items, null);
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Unauthorized");
                    return (null, "Unauthorized");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    // No items found for the user
                    return (new List<CarrinhoCompraItem>(), null);
                }
                else
                {
                    var errorMsg = $"Erro na requisição: {response.ReasonPhrase}";
                    _logger.LogError(errorMsg);
                    return (null, errorMsg);
                }
            }
            catch (HttpRequestException ex)
            {
                var errorMsg = $"Erro de requisição HTTP: {ex.Message}";
                _logger.LogError(ex, errorMsg);
                return (null, errorMsg);
            }
            catch (JsonException ex)
            {
                var errorMsg = $"Erro de desserialização JSON: {ex.Message}";
                _logger.LogError(ex, errorMsg);
                return (null, errorMsg);
            }
            catch (Exception ex)
            {
                var errorMsg = $"Erro inesperado: {ex.Message}";
                _logger.LogError(ex, errorMsg);
                return (null, errorMsg);
            }
        }



        private async Task<(T? Data, string? ErrorMessage)> GetAsync<T>(string endpoint)
        {
            try
            {
                AddAuthorizationHeader();

                var response = await _httpClient.GetAsync(AppConfig.BaseUrl + endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<T>(responseString, _serializerOptions);
                    return (data ?? Activator.CreateInstance<T>(), null);
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        string errorMessage = "Unauthorized";
                        _logger.LogWarning(errorMessage);
                        return (default, errorMessage);
                    }

                    string generalErrorMessage = $"Erro na requisição: {response.ReasonPhrase}";
                    _logger.LogError(generalErrorMessage);
                    return (default, generalErrorMessage);
                }
            }
            catch (HttpRequestException ex)
            {
                string errorMessage = $"Erro de requisição HTTP: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                return (default, errorMessage);
            }
            catch (JsonException ex)
            {
                string errorMessage = $"Erro de desserialização JSON: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                return (default, errorMessage);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Erro inesperado: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                return (default, errorMessage);
            }
        }

        private void AddAuthorizationHeader()
        {
            var token = Preferences.Get("accesstoken", string.Empty);
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
