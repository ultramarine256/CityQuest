﻿using Abp.Runtime.Validation;
using CityQuest.ApplicationServices.Shared.Dtos.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityQuest.ApplicationServices.GameModule.Games.Dtos
{
    public class UpdateGameInput : UpdateInput<GameDto, long>, IShouldNormalize
    {
        public void Normalize() { }
    }
}
