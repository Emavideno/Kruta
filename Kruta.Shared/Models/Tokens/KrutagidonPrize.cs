using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Models.Tokens
{
    // Главный Приз Крутагидона (ГПК)
    public class KrutagidonPrize
    {
        public int CurrentHolderPlayerId { get; set; } = -1; // -1 если никто не владеет
        public string Text { get; set; } = "Постоянка: в конце каждого своего хода игрок берет +1 карту из своего сброса при наборе новой руки и затем сбрасывает 1 карту на выбор";
        public bool IsPermanent { get; set; } = true;
    }
}
