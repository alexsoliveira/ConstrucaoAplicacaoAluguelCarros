namespace fnPayment.Model
{
    public class PaymantModel
    {
        public string Id { get { return Guid.NewGuid().ToString(); } }
        public string IdPayment { get { return Guid.NewGuid().ToString(); } }
        public string nome { get; set; }
        public string email { get; set; }
        public string modelo { get; set; }
        public int ano { get; set; }
        public string tempoAluguel { get; set; }
        public DateTime data { get; set; }

        public string status { get; set; }

        public DateTime? DataAprovacao { get; set; }
    }
}
