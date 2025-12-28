using Kruta.Shared.Mini;
using Kruta.Shared.Mini.Cards;

namespace Kruta.Server.Handlers
{
    public static class CardFactoryMini
    {
        public static ICardMini CreateCardById(int id)
        {
            return id switch
            {
                1 => new LimpWandCard(),
                2 => new BattleSaxCard(),
                3 => new FizzleCard(),
                4 => new SnotKnightCard(),
                5 => new TwinsCard(),
                6 => new WandCard(),
                7 => new SignCard(),
                8 => new WildMagicCard(),
                9 => new InfernoCard(),
                10 => new KrutagidonCard(),
                _ => new SomeCardMini { CardId = id, Name = "Неизвестная карта", Cost = 0 }
            };
        }
    }
}
