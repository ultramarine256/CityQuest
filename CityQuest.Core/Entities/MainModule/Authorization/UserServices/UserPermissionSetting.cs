﻿using CityQuest.Entities.MainModule.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityQuest.Entities.MainModule.Authorization.UserServices
{
    public class UserPermissionSetting : PermissionSetting
    {
        public virtual User User { get; set; }
        public virtual long UserId { get; set; }
    }
}