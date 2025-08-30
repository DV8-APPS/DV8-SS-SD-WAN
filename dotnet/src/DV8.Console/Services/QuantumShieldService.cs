namespace DV8.Console.Services
{
    public class QuantumShieldService
    {
        public int RiskScore(string status, int latency = 0)
        {
            var score = 0;
            if (status == "down") score += 70;
            if (latency > 50) score += 20;
            return score > 100 ? 100 : score;
        }
    }
}
