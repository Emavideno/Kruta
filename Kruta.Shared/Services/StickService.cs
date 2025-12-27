using Kruta.Shared.Models.Cards.Familiar;
using Kruta.Shared.Models.Cards.Stick;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Kruta.Shared.Services
{
    public class StickService
    {
        private List<StickCard> Sticks = new List<StickCard>();

        public StickService()
        {
            StickInit();
        }

        private void StickInit() //инициализация всех Палочек
        {
            //создаем 30 копий
            for(int i = 0; i < 30; i++)
            {
                Sticks.Add(new StickCard());
            }
        }

        public StickCard GetStickForPlayer()
        {
            if(Sticks.Count > 0)
            {
                Sticks.RemoveAt(0);
            }
            else
            {
                return null; //нет больше палочек!
            }

            return new StickCard();
        }
    }
}
