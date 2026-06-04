using System;

namespace BisectionApp.Models
{
    /// <summary>
    /// Модель результата вычисления корня
    /// </summary>
    public class CalculationResult
    {
        public int Id { get; set; }
        public double LeftBound { get; set; }
        public double RightBound { get; set; }
        public double Root { get; set; }
        public double FunctionValue { get; set; }
        public int Iterations { get; set; }
        public double Accuracy { get; set; }
        public DateTime CalculationDate { get; set; }
    }
}