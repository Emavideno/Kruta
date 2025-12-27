using Kruta.Shared.Models.Cards;
using Kruta.Shared.Models.Cards.Stick;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Kruta.Shared.Services
{
    public class PshikService
    {
        private List<PshikCard> Pshiks = new List<PshikCard>();

        public PshikService()
        {
            PshikInit();
        }

        private void PshikInit() //инициализация всех Пшиков
        {
            for(int i = 0; i < 15; i++) //15 штук по правилам
            {
                Pshiks.Add(new PshikCard());
            }
        }

        public PshikCard GetPshikForPlayer()
        {
            if (Pshiks.Count > 0)
            {
                Pshiks.RemoveAt(0);
            }
            else
            {
                return null; //нет больше пшиков!
            }

            return new PshikCard();
        }
    }
}
