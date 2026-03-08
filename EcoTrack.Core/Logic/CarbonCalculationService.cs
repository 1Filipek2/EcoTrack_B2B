namespace EcoTrack.Core.Logic;
public class CarbonCalculationService : ICarbonCalculator
{
    public decimal Calculate(decimal activityAmount, decimal emissionFactor)
    {
        return activityAmount * emissionFactor;
    }
}