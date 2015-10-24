﻿using Abp.Domain.Uow;
using CityQuest.ApplicationServices.GameModule.Teams.Dtos;
using CityQuest.ApplicationServices.Shared.Dtos.Input;
using CityQuest.ApplicationServices.Shared.Dtos.Output;
using CityQuest.CityQuestConstants;
using CityQuest.Entities.GameModule.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CityQuest.Mapping;
using Abp.Application.Services.Dto;
using Abp.UI;

namespace CityQuest.ApplicationServices.GameModule.Teams
{
    public class TeamAppService : ITeamAppService
    {
        #region Injected Dependencies

        private IUnitOfWorkManager UowManager { get; set; }
        private ITeamRepository TeamRepository { get; set; }

        #endregion

        public TeamAppService(IUnitOfWorkManager uowManager, ITeamRepository teamRepository)
        {
            UowManager = uowManager;
            TeamRepository = teamRepository;
        }


        public RetrieveAllPagedResultOutput<TeamDto, long> RetrieveAllPagedResult(RetrieveAllTeamsPagedResultInput input)
        {
            if (input.IsActive ?? true)
                UowManager.Current.EnableFilter(Filters.IPassivableFilter);

            TeamRepository.Includes.Add(r => r.LastModifierUser);
            TeamRepository.Includes.Add(r => r.CreatorUser);
            TeamRepository.Includes.Add(r => r.Division);

            IQueryable<Team> teamsQuery = TeamRepository.GetAll()
                .WhereIf(!input.TeamIds.IsNullOrEmpty(), r => input.TeamIds.Contains(r.Id))
                .WhereIf(input.DivisionId != null, r => r.DivisionId == input.DivisionId)
                .WhereIf(!String.IsNullOrEmpty(input.Name), r => r.Name.ToLower().Contains(input.Name.ToLower()));

            int totalCount = teamsQuery.Count();
            IReadOnlyList<TeamDto> teamDtos = teamsQuery
                .OrderByDescending(r => r.IsActive).ThenBy(r => r.Name)
                .Skip(input.SkipCount).Take(input.MaxResultCount)
                .ToList().MapIList<Team, TeamDto>().ToList();

            TeamRepository.Includes.Clear();

            return new RetrieveAllPagedResultOutput<TeamDto, long>()
            {
                Items = teamDtos,
                TotalCount = totalCount
            };
        }

        public RetrieveAllTeamsLikeComboBoxesOutput RetrieveAllTeamsLikeComboBoxes(RetrieveAllTeamsLikeComboBoxesInput input)
        {
            if (input.IsActive ?? true)
                UowManager.Current.EnableFilter(Filters.IPassivableFilter);

            IReadOnlyList<ComboboxItemDto> teamsLikeComboBoxes = TeamRepository.GetAll().ToList()
                .Select(r => new ComboboxItemDto(r.Id.ToString(), r.Name)).ToList();

            return new RetrieveAllTeamsLikeComboBoxesOutput()
            {
                Items = teamsLikeComboBoxes
            };
        }

        public RetrieveAllOutput<TeamDto, long> RetrieveAll(RetrieveAllTeamsInput input)
        {
            if (input.IsActive ?? true)
                UowManager.Current.EnableFilter(Filters.IPassivableFilter);

            TeamRepository.Includes.Add(r => r.LastModifierUser);
            TeamRepository.Includes.Add(r => r.CreatorUser);
            TeamRepository.Includes.Add(r => r.Division);

            IList<Team> teamEntities = TeamRepository.GetAll()
                .WhereIf(!input.TeamIds.IsNullOrEmpty(), r => input.TeamIds.Contains(r.Id))
                .WhereIf(input.DivisionId != null, r => r.DivisionId == input.DivisionId)
                .WhereIf(!String.IsNullOrEmpty(input.Name), r => r.Name.ToLower().Contains(input.Name.ToLower()))
                .ToList();

            IList<TeamDto> result = teamEntities.MapIList<Team, TeamDto>();

            TeamRepository.Includes.Clear();

            return new RetrieveAllOutput<TeamDto, long>()
            {
                RetrievedEntities = result
            };
        }

        public RetrieveOutput<TeamDto, long> Retrieve(RetrieveTeamInput input)
        {
            if (input.IsActive ?? true)
                UowManager.Current.EnableFilter(Filters.IPassivableFilter);

            IList<Team> teamEntities = TeamRepository.GetAll()
                .WhereIf(input.Id != null, r => r.Id == input.Id)
                .WhereIf(!String.IsNullOrEmpty(input.Name), r => r.Name.ToLower().Contains(input.Name.ToLower()))
                .ToList();

            if (teamEntities.Count != 1)
            {
                throw new UserFriendlyException(String.Format(
                    "Can not retrieve Team with these filters."));
            }

            TeamDto teamEntity = teamEntities.Single().MapTo<TeamDto>();

            return new RetrieveOutput<TeamDto, long>()
            {
                RetrievedEntity = teamEntity
            };
        }

        public CreateOutput<TeamDto, long> Create(CreateInput<TeamDto, long> input)
        {
            Team newTeamEntity = input.Entity.MapTo<Team>();

            newTeamEntity.IsActive = true;

            TeamDto newTeamDto = (TeamRepository.Insert(newTeamEntity)).MapTo<TeamDto>();

            return new CreateOutput<TeamDto, long>()
            {
                CreatedEntity = newTeamDto
            };
        }

        public UpdateOutput<TeamDto, long> Update(UpdateInput<TeamDto, long> input)
        {
            Team newTeamEntity = input.Entity.MapTo<Team>();

            if (newTeamEntity == null)
            {
                throw new UserFriendlyException(String.Format(
                    "There is not valid Team entity. Can not update to it."));
            }

            TeamDto newTeamDto = (TeamRepository.Update(newTeamEntity)).MapTo<TeamDto>();

            return new UpdateOutput<TeamDto, long>()
            {
                UpdatedEntity = newTeamDto
            };
        }

        public DeleteOutput<long> Delete(DeleteInput<long> input)
        {
            Team teamEntityForDelete = TeamRepository.Get(input.EntityId);

            if (teamEntityForDelete == null)
            {
                throw new UserFriendlyException(String.Format(
                    "There are no Team with Id = {0}. Can not delete it.", input.EntityId));
            }

            TeamRepository.Delete(teamEntityForDelete);

            return new DeleteOutput<long>()
            {
                DeletedEntityId = input.EntityId
            };
        }

        public ChangeActivityOutput<TeamDto, long> ChangeActivity(ChangeActivityInput input)
        {
            Team teamEntity = TeamRepository.Get(input.EntityId);

            if (teamEntity == null)
            {
                throw new UserFriendlyException(String.Format(
                    "There are no Team with Id = {0}. Can not change it's activity.", input.EntityId));
            }

            teamEntity.IsActive = input.IsActive == null ? !teamEntity.IsActive : (bool)input.IsActive;

            TeamDto newTeamDto = (TeamRepository.Update(teamEntity)).MapTo<TeamDto>();

            return new ChangeActivityOutput<TeamDto, long>()
            {
                Entity = newTeamDto
            };
        }
    }
}