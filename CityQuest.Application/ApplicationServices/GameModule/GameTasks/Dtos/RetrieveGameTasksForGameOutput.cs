﻿using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityQuest.ApplicationServices.GameModule.GameTasks.Dtos
{
    public class RetrieveGameTasksForGameOutput: IOutputDto
    {
        public IList<GameTaskDto> GameTasks { get; set; }
    }
}
