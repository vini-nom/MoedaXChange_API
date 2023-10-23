using Microsoft.AspNetCore.Mvc;
using MoedaXChange_API.Parameters;
using System.Net.Http.Headers;
using NewsAPI;
using NewsAPI.Models;
using NewsAPI.Constants;
using MoedaXChange_API.Models;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MoedaXChange_API.Controllers
{
    [Controller]
    [Route("Moeda")]
    public class Moeda
    { 
        UrlRequests urlRequisition = new UrlRequests();
        ChavesAPI chavesAPI = new ChavesAPI();
       
        [HttpPost]
        [Route("Cotacao")]
        public async Task<IActionResult> ObterCotacaoMoeda(string MoedaLocal, string MoedaConversao)
        {
            try
            {
                string urlParameter = $"{MoedaLocal}-{MoedaConversao}";
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(urlRequisition.URL_COTACAO_MOEDA);

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(urlParameter);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    JObject json = JObject.Parse(jsonResponse);

                    JToken result = json[$"{MoedaLocal}{MoedaConversao}"];

                    Cotacao cotacao = JsonConvert.DeserializeObject<Cotacao>(result.ToString());

                    return new JsonResult(cotacao);
                }
                else 
                {
                    return new NotFoundResult();
                }
                
            }
            catch(Exception ex) 
            {
                string msg = Convert.ToString(ex.Message);
                return new BadRequestObjectResult(msg);
            }
           
        }

        [HttpGet]
        [Route("Noticias")]
        public object ObterNoticias() {
            List<string> fontes = new List<string>();
            
            try {
                List<Noticia> noticias = new List<Noticia>();

                //fontes.Add("CNN");

                var newsApiClient = new NewsApiClient(chavesAPI.CHAVE_NEWSAPI);
                var articleResponse = newsApiClient.GetTopHeadlines(new TopHeadlinesRequest {
                    Country = Countries.US,
                    Category = Categories.Business,
                    PageSize = 3,
                    //Sources = fontes  
                });

                if (articleResponse.Status == Statuses.Ok)
                {
                    foreach (var article in articleResponse.Articles)
                    {
                        Noticia noti = new Noticia();

                        noti.Manchete = article.Title;
                        noti.Autor = article.Author;
                        noti.Conteudo = article.Description;
                        noti.Fonte = article.Url;
                        noti.Imagem = article.UrlToImage;

                        noticias.Add(noti);
                    }

                    return new JsonResult(noticias);
                }
                else {
                    return new BadRequestResult();
                }
            }
            catch (Exception ex) {
                string msg = Convert.ToString(ex.Message);
                return new BadRequestObjectResult(msg);
            }
        }


    }
}
