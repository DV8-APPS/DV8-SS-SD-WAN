namespace DV8.Console.Services
{
    public class PlannerService
    {
        public object Recommend(string site, string intentId, int horizonDays)
            => new { site, intentId, recommendation = "scale_out", horizonDays };
    }
}
