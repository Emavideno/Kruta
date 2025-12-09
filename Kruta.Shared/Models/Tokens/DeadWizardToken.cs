using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Models.Tokens
{
    // Жетон Дохлого Колдуна (ЖДК)
    public class DeadWizardToken
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }

        // Влияет на здоровье при получении (если не 20)
        public int NewHealthValue { get; set; } = 20;

        // Постоянный ли это эффект (для сравнения с ЖДК без эффекта)
        public bool IsPermanentEffect { get; set; } = false;

        // Штраф к ПО в конце игры (-3)
        public int VictoryPointPenalty { get; set; } = -3;
    }
}
