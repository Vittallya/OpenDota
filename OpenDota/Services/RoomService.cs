using OpenDota.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenDota.Services
{
    public class RoomService
    {
        public ICollection<RoomSession> Sessions { get; } = new List<RoomSession>();
        public bool IsHeroesLoaded { get; private set; }

        public Hero[] AllHeroes { get; private set; }

        public void LoadHeroes(Hero[] heroes)
        {
            AllHeroes = heroes;
            IsHeroesLoaded = true;
        }
        private List<int> _rooms = new List<int>();

        public int CreateRoom()
        {
            Random rand = new Random();
            int roomId;
            do
            {
                roomId = rand.Next();
            }
            while (_rooms.Contains(roomId));

            _rooms.Add(roomId);
            Sessions.Add(new RoomSession(roomId, this));

            return roomId;
        }


        public ConnectToRoomResult ConnectToRoom(int roomId, bool isCreator)
        {
            if (_rooms.Contains(roomId))
            {
                var session = this[roomId];

                if (session.RoomStatus == RoomStatus.AwaitOnCreate)
                {
                    session.JoinUser(isCreator);
                    return new ConnectToRoomResult(ConnectToRoomResultType.Ok, null);
                }

                return new ConnectToRoomResult(ConnectToRoomResultType.RoomIsFilled, "Комната заполнена");
            }

            return new ConnectToRoomResult(ConnectToRoomResultType.RoomIsUndefined, "Комната не найдена");
        }


        public RoomSession this[int id]
        {
            get => Sessions.FirstOrDefault(x => x.Id == id);
        }

    }

    public enum ConnectToRoomResultType
    {
        Ok, RoomIsFilled, RoomIsUndefined
    }

    public class ConnectToRoomResult
    {
        public ConnectToRoomResult(ConnectToRoomResultType resultType, string message)
        {
            ResultType = resultType;
            Message = message;
        }
        public bool IsOk => ResultType == ConnectToRoomResultType.Ok;
        public ConnectToRoomResultType ResultType { get; }
        public string Message { get; }
    }
}