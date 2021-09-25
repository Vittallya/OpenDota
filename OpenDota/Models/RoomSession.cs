using OpenDota.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OpenDota.Models
{
    public class RoomSession
    {
        public int Id { get; }

        public int TotalSteps { get; private set; } = 1;



        /// <summary>
        /// Полльзователь выбирает для себя персонажа
        /// </summary>
        /// <param name="isCreator"></param>
        /// <param name="heroId"></param>
        public void SelectHero(bool isCreator, int heroId)
        {
            var user = this[isCreator];
            user.SelectedHeroes.Add(heroId);            

            TotalSteps++;
        }


        /// <summary>
        /// Можно ли дальше ходить?
        /// </summary>
        /// <returns></returns>
        public bool CanChoose()
        {
            return TotalSteps < 25;
        }

        /// <summary>
        /// Блокировка персонажа
        /// </summary>
        /// <param name="isCreator"></param>
        /// <param name="heroId"></param>
        public void BanHero(bool isCreator, int heroId)
        {
            var user = this[isCreator];
            user.BannedHeroes.Add(heroId);            

            TotalSteps++;
        }


        /// <summary>
        /// Те герои, которых уже выбрал пользователь
        /// </summary>
        /// <param name="isCreator"></param>
        /// <returns></returns>
        public Hero[] GetAllHeroesFor(bool isCreator)
        {
            return this[isCreator].SelectedHeroes.Select(x => roomService.AllHeroes.First(y => y.id == x)).ToArray();
        }

        /// <summary>
        /// Все те персонажи, кого заблокировал пользователь
        /// </summary>
        /// <param name="isCreator"></param>
        /// <returns></returns>
        public Hero[] GetAllHeroesBlocked(bool isCreator)
        {
            return this[isCreator].BannedHeroes.Select(x => roomService.AllHeroes.First(y => y.id == x)).ToArray();
        }

        private readonly RoomService roomService;

        /// <summary>
        /// Те герои, которых может выбрать польз. на след. ходу
        /// </summary>
        /// <param name="isCreator"></param>
        /// <returns></returns>
        public Hero[] GetHeroesFor(bool isCreator)
        {
            var user = GetUser(isCreator);
            var opponent = GetUser(!isCreator);

            return roomService.AllHeroes.Where(x => !user.SelectedHeroes.Contains(x.id) &&
             !user.BannedHeroes.Contains(x.id) &&
            !opponent.SelectedHeroes.Contains(x.id) && 
            !opponent.BannedHeroes.Contains(x.id)).ToArray();
        }

        /// <summary>
        /// Дать понять, ждем или ходим
        /// </summary>
        /// <param name="isCreator"></param>
        /// <returns></returns>
        public bool IsAwaitOpponent(bool isCreator)
        {
            if (TotalSteps == 15)
                return isCreator;
            if (TotalSteps == 16)
                return !isCreator;


            return isCreator && TotalSteps % 2 == 0 || !isCreator && TotalSteps % 2 != 0;
        }


        /// <summary>
        /// Какой сейчас режим? trrue - блокирока, false - выборка
        /// </summary>
        /// <returns></returns>
        public bool IsBanMode()
        {
            return TotalSteps <= 4 || (TotalSteps >= 9 && TotalSteps < 15) || (TotalSteps >= 19 && TotalSteps < 23);
        }

        public bool IsSavingProccesNow()
        {
            return ResultCalled && !IsResultSetted;
        }

        public RoomSession(int id, RoomService roomService)
        {
            Id = id;
            RoomStatus = RoomStatus.AwaitOnCreate;
            this.roomService = roomService;
        }

        public RoomStatus RoomStatus { get; private set; }
        public ICollection<UserModel> Users { get; } = new List<UserModel>();


        /// <summary>
        /// Подключить пользователя
        /// </summary>
        /// <param name="isCreator"></param>
        public void JoinUser(bool isCreator)
        {
            Users.Add(new UserModel(isCreator));

            if (Users.Count == 2)
                RoomStatus = RoomStatus.Active;
        }

        public int Result { get; private set; }
        public bool IsResultSetted { get; private set; }
        public bool ResultCalled { get; internal set; }

        internal void CompleteCompetition()
        {
            IsResultSetted = true;
            RoomStatus = RoomStatus.Completed;
        }

        public UserModel this[bool isCreator]
        {
            get => Users.First(x => x.IsCreator == isCreator);
        }

        public UserModel GetUser(bool isCreator)
        {
            return Users.First(x => x.IsCreator == isCreator);
        }

        internal void SetResult(int res)
        {
            Result = res;
        }
    }
    public enum RoomStatus
    {
        AwaitOnCreate, Active, Pause, Completed
    }
}