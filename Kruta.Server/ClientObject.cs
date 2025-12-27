using Kruta.Protocol;
using Kruta.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Kruta.Server
{
    public class ClientObject
    {
        public string Id { get; private set; } = Guid.NewGuid().ToString(); //id клиента/сессии
        public string Username { get; set; } = "Player"; //изначально просто ставим имя Player

        public Player PlayerData { get; set; } //полноценная модель игрока для игры
        public bool IsReady { get; set; } //готов ли к игре после регистрации

        private List<byte> _accumulatedData = new List<byte>(); //чтобы не было потери данных при возможном чтении всего буффера сокета не до конца

        public GameServer2 _gameServer2;
        Socket _socket;

        private Thread _clientThread;



        public ClientObject(Socket socket, GameServer2 gameServer2)
        {
            _socket = socket;
            _gameServer2 = gameServer2;

            // Запускаем поток прослушивания конкретно для этого клиента
            _clientThread = new Thread(Listen);
            _clientThread.Start();
        }

        private void Listen()
        {
            try
            {
                Console.WriteLine($"[ClientObject] Listen started for client {Id}. SocketConnected={_socket?.Connected}");
                byte[] buffer = new byte[1024];
                while (_socket.Connected)
                {
                    int bytesRead = _socket.Receive(buffer); //получаем байты с клиента
                    Console.WriteLine($"[ClientObject] Client {Id} received bytes: {bytesRead}");
                    if (bytesRead <= 0) break;

                    //собираем пришедшие к нам байты в "пакеты" чтобы не было ошибок
                    //когда не все байты дошли а мы уже их пытаемся читать на другой стороне
                    byte[] receivedData = new byte[bytesRead];
                    Array.Copy(buffer, receivedData, bytesRead);
                    _accumulatedData.AddRange(receivedData);
                    Console.WriteLine($"[ClientObject] Client {Id} accumulated buffer size: {_accumulatedData.Count}");

                    while (_accumulatedData.Count >= 6) //6 - минимальный размер пакеа (EA/CS + type, subtype + AE)
                    {
                        byte[] currentBuffer = _accumulatedData.ToArray();
                        EAPacket packet = EAPacket.Parse(currentBuffer);

                        if (packet == null) break;

                        // Считаем размер: 
                        // 4 (заголовок EA/CS + Type + Subtype) 
                        // + сумма всех полей (ID + Size + Content) 
                        // + 2 (конец AE)
                        int totalSize = 4;
                        foreach (var field in packet.Fields)
                        {
                            totalSize += 2 + field.FieldSize;
                        }
                        totalSize += 2;

                        Console.WriteLine($"[ClientObject] Parsed {packet.PacketType}:{packet.PacketSubtype}. Size: {totalSize}");

                        _gameServer2.OnMessageReceived(this, packet);

                        // Удаляем ровно столько байт, сколько занял ОДИН пакет
                        _accumulatedData.RemoveRange(0, totalSize);
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"Ошибка: {ex.Message}"); }
            finally { Close(); }
        }

        //закрытие подключения
        protected internal void Close()
        {
            try
            {
                // Сообщаем серверу, что мы уходим (если еще не сообщили)
                _gameServer2.RemoveConnection(Id);

                // Закрываем сокет
                if (_socket != null)
                {
                    _socket.Shutdown(SocketShutdown.Both); // Прекращаем и чтение, и отправку
                    _socket.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при закрытии клиента {Id}: {ex.Message}");
            }
        }


        public void Send(EAPacket packet)
        {
            try
            {
                if (_socket != null && _socket.Connected)
                {
                    // 1. Защищаем пакет (шифруем и добавляем заголовки)
                    EAPacket encryptedPacket = packet.Encrypt();

                    // 2. Превращаем объект в поток байт
                    byte[] data = encryptedPacket.ToPacket();

                    // Логируем отправку — важно для диагностики
                    Console.WriteLine($"[ClientObject] Отправка {data.Length} байт клиенту {Id}. SocketConnected={_socket?.Connected}");

                    // 3. Отправляем в сокет
                    _socket.Send(data);
                }
                else
                {
                    Console.WriteLine($"[ClientObject] Попытка отправки, но сокет не подключён (ClientId={Id})");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке пакета клиенту {Id}: {ex.Message}");
            }
        }


    }
}
