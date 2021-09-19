using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OpenDota.Models
{
    public class Room
    {
        public int Id { get; set; }
        public int? User1Id { get; set; }
        public int? User2Id { get; set; }

        public Player User1 { get; set; }
        public Player User2 { get; set; }

        public RoomStatus RoomStatus { get; set; }
    }

    public enum RoomStatus
    {
        AwaitOnCreate, Active, Pause, Completed 
    }
}