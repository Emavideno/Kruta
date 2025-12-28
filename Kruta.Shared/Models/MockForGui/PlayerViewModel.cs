using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Models.MockForGui
{
    public class PlayerViewModel
    {
        public string Name { get; set; }
        public int Health { get; set; } = 20;
        public int Power { get; set; } = 0;
        public List<CardViewModel> Hand { get; set; } = new();
        public CardViewModel Familiar { get; set; } // Публичный
        public string WizardProperty { get; set; } // Публичный текст
    }
}
