﻿using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityQuest.ApplicationServices.GameModule.Tips.Dtos
{
    public class RetrieveTipsForGameTaskInput: IInputDto
    {
        public long GameTaskId { get; set; }
    }
}
