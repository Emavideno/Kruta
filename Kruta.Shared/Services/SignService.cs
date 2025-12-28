using Kruta.Shared.Models.Cards;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Services
{
    public class SignService
    {
        private List<SignCard> Signs = new List<SignCard>();

        public SignService()
        {
            InitSigns();
        }

        private void InitSigns() //Инициализируем все знаки
        {
            for(int i = 0; i < 30; i++) //их 30
            {
                Signs.Add(new SignCard());
            }
        }

        public SignCard GetSignForPlayer()
        {
            if (Signs.Count > 0)
            {
                Signs.RemoveAt(0);
            }
            else
            {
                return null; //нет больше знаков!
            }

            return new SignCard();
        }
    }
}
