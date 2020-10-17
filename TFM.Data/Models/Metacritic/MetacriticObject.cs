namespace TFM.Data.Models.Metacritic
{
    public class MetacriticObject
    {
        public string Query { get; set; }
        public float ExecutionTime { get; set; }
        public MetacriticGame Result { get; set; }
    }

}
