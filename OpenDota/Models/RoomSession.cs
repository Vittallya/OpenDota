using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenDota.Models
{
    public class RoomSession
    {
        public int Id { get; }

        public RoomSession(int id)
        {
            Id = id;
        }

        public RoomStatus RoomStatus { get; private set; }
        public ICollection<UserModel> Users { get; } = new List<UserModel>();

        public void JoinUser(bool isCreator)
        {
            Users.Add(new UserModel(isCreator));
        }

    }
}