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
            Sessions.Add(new RoomSession(roomId));

            return roomId;
        }

        public bool JoinUser(int roomId, bool isCreator)
        {
            var inst = this[roomId];
            if(inst != null)
            {
                inst.JoinUser(isCreator);
                return true;
            }

            return false;
        }




        public RoomSession this[int id]
        {
            get => Sessions.FirstOrDefault(x => x.Id == id);
        }
    }
}