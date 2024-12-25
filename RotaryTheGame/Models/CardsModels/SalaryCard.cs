﻿namespace RotaryTheGame.Models.CardsModels
{
    public class SalaryCard : ICard
    {
        public string Text { get; set; }
        public List<string> Attributes { get; }
        public bool IsOpen { get; set; } = false;
    }
}
