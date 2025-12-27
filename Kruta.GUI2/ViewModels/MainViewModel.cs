using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kruta.GUI2.ViewModels;
using Kruta.Shared.Models;
using Kruta.Shared.Models.Cards.Bespredel;
using Kruta.Shared.Models.Interfaces;
using Kruta.Shared.Services;
using System.Collections.ObjectModel;

namespace Kruta.GUI2.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {

    }
}










//// Помечаем коллекции как ObservableCollection, чтобы UI видел добавление/удаление карт
//public ObservableCollection<Player> Players { get; } = new();
//public ObservableCollection<ICard> BaraholkaCards { get; } = new();

////// Флаги для управления UI (например, показ экрана Беспредела)
////[ObservableProperty] private bool isBespredelActive = false;
////[ObservableProperty] private BespredelCard currentBespredel;
////[ObservableProperty] private Player currentPlayer;

////Регистрация сервисов (возможно сделать DI тут, или вынести в какое-то другое место в будущем)
//JKSService jKSService = new JKSService(); //ЖКС
//FamiliarService familiarService = new FamiliarService(); //Фамильяр
//StickService stickService = new StickService(); //Палочки
//PshikService pshikService = new PshikService(); //Пшики
//SignService signService = new SignService(); //Знаки

////отдельные колоды карт
//LegendService legendService = new LegendService(); //стопка карт Легенд
//DeathWizardTokenService deathWizardTokenService = new DeathWizardTokenService(); //стопка жетонов дохлых колдунов


//Deck deck = new Deck(); //инициализация общей колоды
//Baraholka baraholka = new Baraholka(); //инициализация барахолки

//public MainViewModel()
//{
//    StartGame();
//}

//private void StartGame()
//{
//    //TODO: вместо этого сделать сервис регистрации пользователей. Через протокол и общение с клиентом
//    Player player1 = new Player() { Id = 1, Name = "Egor" };
//    Player player2 = new Player() { Id = 2, Name = "Albert" };
//    Player player3 = new Player() { Id = 3, Name = "Boris" };
//    Player player4 = new Player() { Id = 4, Name = "Roman" };

//    Players.Add(player1);
//    Players.Add(player2);
//    Players.Add(player3);
//    Players.Add(player4);

//    foreach (var player in Players)
//    {
//        player.CurrentHealth = 20; //начальное здоровье = 20

//        player.WizardPropertyToken = jKSService.GetRandomJksForPlayer(); //вызываем метод который выдает рандомный ЖКС игроку
//        player.Familiar = familiarService.GetRandomFamiliarForPlayer();//вызываем метод который выдает рандомный Фамильяр игроку 

//        //добавляем их в List публичных карт конкретного игрока, которые видным остальным игрокам(возможно, удобно для UI)
//        player.PublicCards.Add(player.WizardPropertyToken);
//        player.PublicCards.Add(player.Familiar);

//        //кладем в колоду игрока 10 карт, из которым после шафла (перемешивания) он получит в руку 5
//        player.PlayerDeck.Add(stickService.GetStickForPlayer()); //добавление 1 палки

//        for (int i = 0; i < 3; i++) //добавление 3 пшиков
//        {
//            player.PlayerDeck.Add(pshikService.GetPshikForPlayer());
//        }

//        for (int i = 0; i < 6; i++) //добавление 6 знаков
//        {
//            player.PlayerDeck.Add(signService.GetSignForPlayer());
//        }


//        //Перемешиваем колоду
//        player.ShuffleDeck();

//        // Берем 5 карт из колоды и отдаем игроку в руку
//        // Мы всегда берем элемент по индексу 0, потому что после удаления
//        // следующий элемент становится новым индексом 0
//        for (int i = 0; i < 5; i++)
//        {
//            // Получаем первую карту (верх колоды)
//            var cardToMove = player.PlayerDeck[0];

//            // Добавляем ее в руку
//            player.Hand.Add(cardToMove);

//            // Удаляем ее из колоды (это безопасно, так как мы всегда удаляем 0-й элемент)
//            player.PlayerDeck.RemoveAt(0);
//        }
//    }

//    CurrentPlayer = Players.FirstOrDefault();
//    RefillBaraholka();
//}

//[RelayCommand]
//public async Task RefillBaraholka()
//{
//    while (baraholka.AvilableCards.Count < 5)
//    {
//        ICard drowCard = deck.GetCardFromDeck(); //берем 1 карту из общей колоды

//        if (drowCard is BespredelCard bespredelCard) //если выпал беспредел то он сразу должен выполняться (пока что тут метод-заглушка, напишем когда будем уже писать логику работы хода игрока, а там логику работы конкретных типов карт)
//        {
//            await BespredelEffect(bespredelCard);
//        }
//        else //любая другая карта просто кладется в барахолку
//        {
//            baraholka.AvilableCards.Add(drowCard);
//        }
//    }
//}

////Заглушка логики беспредела
//private async Task BespredelEffect(BespredelCard bespredelCard)
//{
//    CurrentBespredel = bespredelCard;
//    IsBespredelActive = true;

//    // Имитируем ожидание, чтобы игрок успел испугаться
//    await Task.Delay(3000);

//    IsBespredelActive = false;
//}

////заглушка чтобы на ui закрыть беспредел пока что
//[RelayCommand]
//private void CloseBespredel() => IsBespredelActive = false;

    