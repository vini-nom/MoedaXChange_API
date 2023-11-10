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

        [HttpPost]
        [Route("TaxaCambio")]
        public async Task<IActionResult> ObterTaxaCambio(string MoedaLocal, string MoedaVariada, string DataVariacao) 
        {
            try {
                //CALCULO DE QUANTIDADE DE DIAS
                DateTime DataAtual = DateTime.Now.Date;
                DateTime dtVariacao = Convert.ToDateTime(DataVariacao);
                DateTime dtAtual = Convert.ToDateTime(DataAtual.ToString("yyyy-MM-dd"));

                TimeSpan NumDias = dtAtual - dtVariacao;

                string urlParameters = $"{MoedaVariada}-{MoedaLocal}/{NumDias.Days}";
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(urlRequisition.URL_TAXA_CAMBIO);

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(urlParameters);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    JArray json = JArray.Parse(jsonResponse);

                    JToken result = json;

                    List<Cotacao> cotacao = JsonConvert.DeserializeObject<List<Cotacao>>(result.ToString());

                    return new JsonResult(cotacao);
                }
                else
                {
                    return new NotFoundResult();
                }
            }
            catch(Exception ex){
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
