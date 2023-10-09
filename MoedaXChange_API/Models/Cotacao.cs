namespace MoedaXChange_API.Models
{
    public class Cotacao
    {
        public string MoedaLocal { get; set; }
        public string MoedaConversao { get; set; }
        public int variacaoAlta { get; set; }
        public int variacaoBaixa { get; set;}
    }
}
