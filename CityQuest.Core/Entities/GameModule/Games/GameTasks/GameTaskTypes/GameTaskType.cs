﻿using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using CityQuest.Entities.MainModule.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CityQuest.Entities.GameModule.Games.GameTasks.GameTaskTypes
{
    public class GameTaskType : FullAuditedEntity<long, User>, IPassivable
    {
        #region Relations

        public virtual ICollection<GameTask> GameTasks { get; set; }

        #endregion

        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }

        public bool IsActive { get; set; }

        #region Ctors

        public GameTaskType()
        {
            GameTasks = new HashSet<GameTask>();
        }

        #endregion
    }
}
