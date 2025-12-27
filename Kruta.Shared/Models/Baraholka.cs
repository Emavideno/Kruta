using Kruta.Shared.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Models
{
    public class Baraholka
    {
        //Допступные карты (5 штук должно быть всегда)
        public List<ICard> AvilableCards { get; set; } = new();

        //Уничтоженные карты
        public List<ICard> DestroyedCards { get; set; } = new();

        public Baraholka()
        {
            
        }
 
    }
}
