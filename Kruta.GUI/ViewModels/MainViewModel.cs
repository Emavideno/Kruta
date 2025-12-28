using Kruta.GUI.Models;
using Kruta.GUI.Services;
using Kruta.Shared.Models.Cards;
using System.Collections.ObjectModel;
using System.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kruta.GUI.Models;
using Kruta.Shared.Models.Cards;
using Kruta.GUI.Services;
using System.Collections.ObjectModel;
using Kruta.Shared.Models.Enums;
using System.Linq;
using System.Threading.Tasks;

namespace Kruta.GUI.ViewModels
{
    // Состояния UI (как мы договаривались)
    public enum ViewState { Connection, GameTable }

    public partial class MainViewModel : ObservableObject
    {
        private readonly MockGameClientService _clientService = new MockGameClientService();
        private readonly MockGameLogicService _logicService = new MockGameLogicService();

        // === Общие Observable Свойства ===
        [ObservableProperty]
        private ViewState currentView = ViewState.Connection;

        [ObservableProperty]
        private string playerName = "Новый Колдун";

        [ObservableProperty]
        private string connectionStatus = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Player> opponentPlayers = new ObservableCollection<Player>();

        // === Игровое Состояние ===
        [ObservableProperty]
        private GameState currentGameState;

        [ObservableProperty]
        private Player localPlayer;

        [ObservableProperty]
        private ObservableCollection<Card> handCards = new ObservableCollection<Card>();

        [ObservableProperty]
        private ObservableCollection<Card> playedCards = new ObservableCollection<Card>();

        [ObservableProperty]
        private ObservableCollection<Card> baraholkaCards = new ObservableCollection<Card>();

        [ObservableProperty]
        private bool isMyTurn = false;

        [ObservableProperty]
        private int currentPower = 0;

        // === Конструктор ===
        public MainViewModel()
        {
            // Запускаем пустой, чтобы MVVM Toolkit мог инициализировать свойства.
            // В реальной жизни тут можно загружать настройки.
        }

        // =================================================================
        //                   МЕТОДЫ ПОДКЛЮЧЕНИЯ И НАВИГАЦИИ
        // =================================================================

        [RelayCommand]
        private async Task Connect()
        {
            ConnectionStatus = "Подключение к серверу...";

            // Заглушка: Имитация подключения и получения начального состояния
            var (success, initialState) = await _clientService.ConnectAndGetStateAsync(PlayerName);

            if (success)
            {
                ConnectionStatus = "Подключено. Ожидание начала игры...";
                CurrentGameState = initialState;
                UpdateUIState(initialState);
                CurrentView = ViewState.GameTable;

                // В однопользовательском режиме, устанавливаем ход сразу
                IsMyTurn = CurrentGameState.CurrentPlayerId == LocalPlayer.Id;
            }
            else
            {
                ConnectionStatus = "Ошибка подключения. Попробуйте снова.";
            }
        }

        [RelayCommand]
        private void ExitGame()
        {
            _clientService.Disconnect();
            CurrentView = ViewState.Connection;
            ConnectionStatus = "Отключено.";
        }

        // =================================================================
        //                     МЕТОДЫ ОБРАБОТКИ СОСТОЯНИЯ
        // =================================================================

        // В реальном приложении этот метод вызывается асинхронно из MockGameClientService.ListenForUpdatesAsync()
        private void UpdateUIState(GameState newState)
        {
            // Обновление общего состояния
            CurrentGameState = newState;

            // Находим и обновляем локального игрока
            LocalPlayer = newState.Players.FirstOrDefault(p => p.Name == PlayerName);

            // Новая логика: заполняем список оппонентов
            var opponents = newState.Players
                .Where(p => p.Id != LocalPlayer.Id)
                .ToList();

            OpponentPlayers = new ObservableCollection<Player>(opponents);

            // Обновление коллекций, привязанных к UI
            if (LocalPlayer != null)
            {
                // Рука
                HandCards = new ObservableCollection<Card>(LocalPlayer.Hand);
                // Сыгранные карты (которые лежат перед игроком)
                PlayedCards = new ObservableCollection<Card>(LocalPlayer.PlayedCardsThisTurn);
                // Мощь, набранная за ход
                CurrentPower = LocalPlayer.PowerGainedThisTurn;
            }

            // Обновление Барахолки
            BaraholkaCards = new ObservableCollection<Card>(newState.Baraholka);

            // Проверка хода
            IsMyTurn = newState.CurrentPlayerId == LocalPlayer?.Id;
        }

        // =================================================================
        //                      МЕТОДЫ ФАЗЫ РОЗЫГРЫША
        // =================================================================

        [RelayCommand]
        private async Task PlayCard(Card card)
        {
            if (!IsMyTurn) return;

            // 1. Имитация отправки действия на сервер
            await _clientService.SendActionAsync(new { Type = "PlayCard", CardId = card.Id });

            // 2. Локальное обновление UI (в реальной игре это делает сервер)
            // Здесь мы имитируем, что сервер разрешил и вернул новое состояние:
            LocalPlayer.Hand.Remove(card);
            LocalPlayer.PlayedCardsThisTurn.Add(card);
            LocalPlayer.PowerGainedThisTurn += card.PowerValue;

            // 3. Обновление UI через Observable-свойства
            UpdateUIState(CurrentGameState);
        }

        [RelayCommand]
        private async Task TargetPlayer(Player target)
        {
            // Выбор цели для атаки. Отправляется на сервер.
            if (!IsMyTurn) return;
            await _clientService.SendActionAsync(new { Type = "TargetPlayer", TargetId = target.Id });

            // UI обновится после ответа сервера
        }

        [RelayCommand]
        private async Task UseFamiliarAttack()
        {
            // Проверяем, что ход игрока и что в ЕГО КОЛОДЕ (Deck) или СБРОСЕ (Discard)
            // есть купленный Фамильяр. Фамильяр лежит в колоде/сбросе после покупки.

            if (!IsMyTurn || !LocalPlayer.Deck.Any(c => c.Type == CardType.Familiar) && !LocalPlayer.Discard.Any(c => c.Type == CardType.Familiar))
            {
                return;
            }

            await _clientService.SendActionAsync(new { Type = "UseFamiliarAttack" });
        }

        // =================================================================
        //                      МЕТОДЫ ФАЗЫ ПОКУПКИ
        // =================================================================

        [RelayCommand]
        private async Task BuyCardFromBaraholka(Card card)
        {
            if (!IsMyTurn || CurrentPower < card.BuyCost) return;

            await _clientService.SendActionAsync(new { Type = "BuyBaraholka", CardId = card.Id });

            // Имитация:
            CurrentPower -= card.BuyCost;
            CurrentGameState.Baraholka.Remove(card);
            LocalPlayer.Discard.Add(card); // Карта идет в сброс
            UpdateUIState(CurrentGameState);
        }

        [RelayCommand]
        private async Task BuyWildMagic()
        {
            const int cost = 3;
            if (!IsMyTurn || CurrentPower < cost) return;

            await _clientService.SendActionAsync(new { Type = "BuyWildMagic" });

            // Имитация:
            CurrentPower -= cost;
            // Имитируем взятие карты из стопки WildMagic
            var wmCard = _logicService.CreateInitialMockState(PlayerName).WildMagicStack.First();
            LocalPlayer.Discard.Add(wmCard);
            UpdateUIState(CurrentGameState);
        }

        [RelayCommand]
        private async Task AttackLegend()
        {
            var legend = CurrentGameState.CurrentLegend;
            if (!IsMyTurn || legend == null || CurrentPower < legend.BuyCost) return;

            await _clientService.SendActionAsync(new { Type = "AttackLegend" });

            // Имитация (сервер обновляет: легенда перемещается в сброс, открывается новая, групповая атака)
            CurrentPower = 0;
            LocalPlayer.Discard.Add(legend);

            // Обновляем UI, чтобы показать, что фаза покупки закончена
            UpdateUIState(CurrentGameState);
        }

        // =================================================================
        //                          КОНЕЦ ХОДА
        // =================================================================

        [RelayCommand]
        private async Task EndTurn()
        {
            if (!IsMyTurn) return;

            await _clientService.SendActionAsync(new { Type = "EndTurn" });

            // Имитация фазы конца хода (в реальной игре это делает сервер)
            // 1. Очистка руки и сыгранных карт в сброс
            LocalPlayer.Discard.AddRange(LocalPlayer.Hand);
            LocalPlayer.Discard.AddRange(LocalPlayer.PlayedCardsThisTurn);
            LocalPlayer.Hand.Clear();
            LocalPlayer.PlayedCardsThisTurn.Clear();
            LocalPlayer.PowerGainedThisTurn = 0;

            // 2. Имитация взятия 5 новых карт
            LocalPlayer.Deck.AddRange(LocalPlayer.Discard);
            LocalPlayer.Discard.Clear();
            // Тут должна быть сложная логика Shuffle, но для заглушки просто берем 5
            LocalPlayer.Hand.AddRange(LocalPlayer.Deck.Take(5));
            LocalPlayer.Deck.RemoveRange(0, 5);

            // 3. Передача хода (имитация)
            CurrentGameState.CurrentPlayerId = CurrentGameState.Players.FirstOrDefault(p => p.Id != LocalPlayer.Id)?.Id ?? 1;

            // Обновление UI
            UpdateUIState(CurrentGameState);
        }
    }
}
