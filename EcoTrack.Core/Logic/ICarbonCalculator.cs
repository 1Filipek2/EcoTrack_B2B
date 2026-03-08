namespace EcoTrack.Core.Logic;
public interface ICarbonCalculator
{
    decimal Calculate(decimal activityAmount, decimal emissionFactor);
}