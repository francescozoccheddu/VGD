using System;

namespace Wheeled.Gameplay
{

    internal static class TimeConstants
    {

        public const double c_simulationStep = 1 / 30.0f;

        public static int SimulationSteps(this double _period)
        {
            return (int) Math.Floor(_period / c_simulationStep);
        }

        public static int CeilingSimulationSteps(this double _period)
        {
            return (int) Math.Ceiling(_period / c_simulationStep);
        }

        public static double SimulationPeriod(this int _step)
        {
            return _step * c_simulationStep;
        }

    }


}
