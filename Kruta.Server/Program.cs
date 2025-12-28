using Kruta.Server;
using Kruta.Server.Networking;
using Kruta.Shared.Models;
using Kruta.Shared.Models.Cards;
using Kruta.Shared.Models.Cards.Bespredel;
using Kruta.Shared.Models.Cards.Familiar;
using Kruta.Shared.Models.Interfaces;
using Kruta.Shared.Models.Tokens.JKSToken;
using Kruta.Shared.Services;
using System;
using System.Text.Json;

Console.Title = "Kruta Server";

var server = new GameServer2();
await server.StartAsync();

////около-заглушка логика.
////TODO: сделать красиво, перенести логику из сервера, например, по сервисам
//List<Player> players = new List<Player>();

////TODO: вместо этого сделать сервис регистрации пользователей. Через протокол и общение с клиентом
//Player player1 = new Player() { Id = 1 };
//Player player2 = new Player() { Id = 2 };
//Player player3 = new Player() { Id = 3 };
//Player player4 = new Player() { Id = 4 };

//players.Add(player1);
//players.Add(player2);
//players.Add(player3);
//players.Add(player4);

////Регистрация сервисов (возможно сделать DI тут, или вынести в какое-то другое место в будущем)
//JKSService jKSService = new JKSService(); //ЖКС
//FamiliarService familiarService = new FamiliarService(); //Фамильяр
//StickService stickService = new StickService(); //Палочки
//PshikService pshikService = new PshikService(); //Пшики
//SignService signService = new SignService(); //Знаки

////отдельные колоды карт
//LegendService legendService = new LegendService(); //стопка карт Легенд
//DeathWizardTokenService deathWizardTokenService = new DeathWizardTokenService(); //стопка жетонов дохлых колдунов


////то есть на клиенте должна быть кнопка "начать игру", чтобы выдать всем зареганым игрокам карты. не по 1 игроку при входе выдаем карты, а всем сразу за 1 раз
////начальная раздача карт (можно в сервисе сделать отдельным методом)
//foreach (var player in players)
//{
//    player.CurrentHealth = 20; //начальное здоровье = 20

//    player.WizardPropertyToken = jKSService.GetRandomJksForPlayer(); //вызываем метод который выдает рандомный ЖКС игроку
//    player.Familiar = familiarService.GetRandomFamiliarForPlayer();//вызываем метод который выдает рандомный Фамильяр игроку 

//    //добавляем их в List публичных карт конкретного игрока, которые видным остальным игрокам(возможно, удобно для UI)
//    player.PublicCards.Add(player.WizardPropertyToken);
//    player.PublicCards.Add(player.Familiar);

//    //кладем в колоду игрока 10 карт, из которым после шафла (перемешивания) он получит в руку 5
//    player.PlayerDeck.Add(stickService.GetStickForPlayer()); //добавление 1 палки

//    for (int i = 0; i < 3; i++) //добавление 3 пшиков
//    {
//        player.PlayerDeck.Add(pshikService.GetPshikForPlayer());
//    }

//    for (int i = 0; i < 6; i++) //добавление 6 знаков
//    {
//        player.PlayerDeck.Add(signService.GetSignForPlayer());
//    }


//    //Перемешиваем колоду
//    player.ShuffleDeck();

//    // Берем 5 карт из колоды и отдаем игроку в руку
//    // Мы всегда берем элемент по индексу 0, потому что после удаления
//    // следующий элемент становится новым индексом 0
//    for (int i = 0; i < 5; i++)
//    {
//        // Получаем первую карту (верх колоды)
//        var cardToMove = player.PlayerDeck[0];

//        // Добавляем ее в руку
//        player.Hand.Add(cardToMove);

//        // Удаляем ее из колоды (это безопасно, так как мы всегда удаляем 0-й элемент)
//        player.PlayerDeck.RemoveAt(0);
//    }
//}

////Заполнение общей колоды. Карты для барахолки берутся из общей колоды
//Deck deck = new Deck(); //инициализация общей колоды



////Заполнение Барахолки (туда не может входить беспредел)
//Baraholka baraholka = new Baraholka(); //инициализация барахолки

////логика заполнения барахолки из общей колоды 
//while (baraholka.AvilableCards.Count < 5)
//{
//    ICard drowCard = deck.GetCardFromDeck(); //берем 1 карту из общей колоды

//    if (drowCard is BespredelCard bespredelCard) //если выпал беспредел то он сразу должен выполняться (пока что тут метод-заглушка, напишем когда будем уже писать логику работы хода игрока, а там логику работы конкретных типов карт)
//    {
//        BespredelEffect(bespredelCard);
//    }
//    else //любая другая карта просто кладется в барахолку
//    {
//        baraholka.AvilableCards.Add(drowCard);
//    }
//}



////Заглушка логики беспредела
//void BespredelEffect(BespredelCard bespredelCard)
//{
//    Console.WriteLine("Работа логики беспредела");
//}

////...дальнешная логика игры (по флоу игры).
////3. По плану в Hoslt: создание колды Легенд, ЖДК (+мб создание класса Game куда перенести всю логику игры из Program.cs)
////TODO: также сделать красиво, растаскать по сервисам
////TODO: сделать общение с клиентом через протокол