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
using CityQuest.CityQuestPolicy.GameModule.Teams;
using CityQuest.Exceptions;
using CityQuest.Entities.GameModule.PlayerCareers;

namespace CityQuest.ApplicationServices.GameModule.Teams
{
    [Abp.Authorization.AbpAuthorize]
    public class TeamAppService : ITeamAppService
    {
        #region Injected Dependencies

        private IUnitOfWorkManager UowManager { get; set; }
        private ITeamRepository TeamRepository { get; set; }
        private ITeamPolicy TeamPolicy { get; set; }
        private IPlayerCareerRepository PlayerCareerRepository { get; set; }

        #endregion

        #region ctors

        public TeamAppService(IUnitOfWorkManager uowManager, 
            ITeamRepository teamRepository, 
            ITeamPolicy teamPolicy,
            IPlayerCareerRepository playerCareerRepository)
        {
            UowManager = uowManager;
            TeamRepository = teamRepository;
            TeamPolicy = teamPolicy;
            PlayerCareerRepository = playerCareerRepository;
        }

        #endregion

        [Abp.Authorization.AbpAuthorize]
        public RetrieveAllPagedResultOutput<TeamDto, long> RetrieveAllPagedResult(RetrieveAllTeamsPagedResultInput input)
        {
            if (input.IsActive ?? true)
                UowManager.Current.EnableFilter(Filters.IPassivableFilter);

            TeamRepository.Includes.Add(r => r.LastModifierUser);
            TeamRepository.Includes.Add(r => r.CreatorUser);
            TeamRepository.Includes.Add(r => r.PlayerCareers);

            IQueryable<Team> teamsQuery = TeamPolicy.CanRetrieveManyEntities( 
                TeamRepository.GetAll()
                .WhereIf(!input.TeamIds.IsNullOrEmpty(), r => input.TeamIds.Contains(r.Id))
                .WhereIf(input.DivisionId != null, r => r.DivisionId == input.DivisionId)
                .WhereIf(!String.IsNullOrEmpty(input.Name), r => r.Name.ToLower().Contains(input.Name.ToLower())));

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

        [Abp.Authorization.AbpAuthorize]
        public RetrieveAllTeamsLikeComboBoxesOutput RetrieveAllTeamsLikeComboBoxes(RetrieveAllTeamsLikeComboBoxesInput input)
        {
            if (input.IsActive ?? true)
                UowManager.Current.EnableFilter(Filters.IPassivableFilter);

            IReadOnlyList<ComboboxItemDto> teamsLikeComboBoxes = TeamPolicy.CanRetrieveManyEntities(
                TeamRepository.GetAll()
                .WhereIf(input.DivisionId != null, r => r.DivisionId == input.DivisionId)).ToList()
                .Select(r => new ComboboxItemDto(r.Id.ToString(), r.Name))
                .OrderBy(r => r.DisplayText).ToList();

            return new RetrieveAllTeamsLikeComboBoxesOutput()
            {
                Items = teamsLikeComboBoxes
            };
        }

        [Abp.Authorization.AbpAuthorize]
        public RetrieveAllOutput<TeamDto, long> RetrieveAll(RetrieveAllTeamsInput input)
        {
            if (input.IsActive ?? true)
                UowManager.Current.EnableFilter(Filters.IPassivableFilter);

            TeamRepository.Includes.Add(r => r.LastModifierUser);
            TeamRepository.Includes.Add(r => r.CreatorUser);
            TeamRepository.Includes.Add(r => r.PlayerCareers);

            IList<Team> teamEntities = TeamPolicy.CanRetrieveManyEntities( 
                TeamRepository.GetAll()
                .WhereIf(!input.TeamIds.IsNullOrEmpty(), r => input.TeamIds.Contains(r.Id))
                .WhereIf(input.DivisionId != null, r => r.DivisionId == input.DivisionId)
                .WhereIf(!String.IsNullOrEmpty(input.Name), r => r.Name.ToLower().Contains(input.Name.ToLower())))
                .ToList();

            IList<TeamDto> result = teamEntities.MapIList<Team, TeamDto>();

            TeamRepository.Includes.Clear();

            return new RetrieveAllOutput<TeamDto, long>()
            {
                RetrievedEntities = result
            };
        }

        [Abp.Authorization.AbpAuthorize]
        public RetrieveOutput<TeamDto, long> Retrieve(RetrieveTeamInput input)
        {
            if (input.IsActive ?? true)
                UowManager.Current.EnableFilter(Filters.IPassivableFilter);

            TeamRepository.Includes.Add(r => r.LastModifierUser);
            TeamRepository.Includes.Add(r => r.CreatorUser);
            TeamRepository.Includes.Add(r => r.PlayerCareers);

            IList<Team> teamEntities = TeamRepository.GetAll()
                .WhereIf(input.Id != null, r => r.Id == input.Id)
                .WhereIf(input.UserId != null, r => r.PlayerCareers.Any(e => e.CareerDateEnd == null && e.UserId == input.UserId))
                .WhereIf(!String.IsNullOrEmpty(input.Name), r => r.Name.ToLower().Contains(input.Name.ToLower()))
                .ToList();

            if (teamEntities.Count != 1)
                throw new CityQuestItemNotFoundException(CityQuestConsts.CityQuestItemNotFoundExceptionMessageBody, "\"Team\"");

            if (!TeamPolicy.CanRetrieveEntity(teamEntities.Single()))
                throw new CityQuestPolicyException(CityQuestConsts.CQPolicyExceptionRetrieveDenied, "\"Team\"");

            TeamDto teamEntity = teamEntities.Single().MapTo<TeamDto>();

            TeamRepository.Includes.Clear();

            return new RetrieveOutput<TeamDto, long>()
            {
                RetrievedEntity = teamEntity
            };
        }

        [Abp.Authorization.AbpAuthorize]
        public CreateOutput<TeamDto, long> Create(CreateTeamInput input)
        {
            Team newTeamEntity = input.Entity.MapTo<Team>();

            if (!TeamPolicy.CanCreateEntity(newTeamEntity))
                throw new CityQuestPolicyException(CityQuestConsts.CQPolicyExceptionCreateDenied, "\"Team\"");

            newTeamEntity.IsActive = true;
            newTeamEntity.IsDefault = false;

            TeamRepository.Includes.Add(r => r.LastModifierUser);
            TeamRepository.Includes.Add(r => r.CreatorUser);
            TeamRepository.Includes.Add(r => r.PlayerCareers);

            TeamDto newTeamDto = (TeamRepository.Insert(newTeamEntity)).MapTo<TeamDto>();

            TeamRepository.Includes.Clear();

            return new CreateOutput<TeamDto, long>()
            {
                CreatedEntity = newTeamDto
            };
        }

        [Abp.Authorization.AbpAuthorize]
        public UpdateOutput<TeamDto, long> Update(UpdateTeamInput input)
        {
            TeamRepository.Includes.Add(r => r.LastModifierUser);
            TeamRepository.Includes.Add(r => r.CreatorUser);
            TeamRepository.Includes.Add(r => r.PlayerCareers);

            Team currentTeamEntity = TeamRepository.Get(input.Entity.Id);

            if (currentTeamEntity == null)
                throw new CityQuestItemNotFoundException(CityQuestConsts.CityQuestItemNotFoundExceptionMessageBody, "\"Team\"");

            if (!TeamPolicy.CanUpdateEntity(currentTeamEntity))
                throw new CityQuestPolicyException(CityQuestConsts.CQPolicyExceptionUpdateDenied, "\"Team\"");

            TeamRepository.Detach(currentTeamEntity);

            Team newTeamEntity = input.Entity.MapTo<Team>();
            newTeamEntity.IsDefault = currentTeamEntity.IsDefault;


            TeamRepository.Update(newTeamEntity);
            TeamDto newTeamDto = (TeamRepository.Get(newTeamEntity.Id)).MapTo<TeamDto>();

            TeamRepository.Includes.Clear();

            return new UpdateOutput<TeamDto, long>()
            {
                UpdatedEntity = newTeamDto
            };
        }

        [Abp.Authorization.AbpAuthorize]
        public DeleteOutput<long> Delete(DeleteInput<long> input)
        {
            Team teamEntityForDelete = TeamRepository.Get(input.EntityId);

            if (teamEntityForDelete == null)
                throw new CityQuestItemNotFoundException(CityQuestConsts.CityQuestItemNotFoundExceptionMessageBody, "\"Team\"");

            if (!TeamPolicy.CanDeleteEntity(teamEntityForDelete))
                throw new CityQuestPolicyException(CityQuestConsts.CQPolicyExceptionDeleteDenied, "\"Team\"");

            TeamRepository.Delete(teamEntityForDelete);

            return new DeleteOutput<long>()
            {
                DeletedEntityId = input.EntityId
            };
        }

        [Abp.Authorization.AbpAuthorize]
        public ChangeActivityOutput<TeamDto, long> ChangeActivity(ChangeActivityInput input)
        {
            TeamRepository.Includes.Add(r => r.LastModifierUser);
            TeamRepository.Includes.Add(r => r.CreatorUser);
            TeamRepository.Includes.Add(r => r.PlayerCareers);

            Team teamEntity = TeamRepository.Get(input.EntityId);

            if (teamEntity == null)
                throw new CityQuestItemNotFoundException(CityQuestConsts.CityQuestItemNotFoundExceptionMessageBody, "\"Team\"");

            if (!TeamPolicy.CanChangeActivityForEntity(teamEntity))
                throw new CityQuestPolicyException(CityQuestConsts.CQPolicyExceptionChangeActivityDenied, "\"Team\"");

            teamEntity.IsActive = input.IsActive == null ? !teamEntity.IsActive : (bool)input.IsActive;
            TeamDto newTeamDto = teamEntity.MapTo<TeamDto>();

            TeamRepository.Update(teamEntity);

            #region Closing player careers

            if (teamEntity.CurrentPlayers.Count() > 0) 
            {
                DateTime currDT = DateTime.Now;
                var players = teamEntity.CurrentPlayers.ToList();
                foreach(var item in players)
                {
                    item.CareerDateEnd = currDT;
                    PlayerCareerRepository.Update(item);
                }
            }

            #endregion

            TeamRepository.Includes.Clear();

            return new ChangeActivityOutput<TeamDto, long>()
            {
                Entity = newTeamDto
            };
        }

        [Abp.Authorization.AbpAuthorize]
        public ChangeCaptainOutput ChangeCaptain(ChangeCaptainInput input)
        {
            TeamRepository.Includes.Add(r => r.PlayerCareers);

            Team teamEntity = TeamRepository.Get(input.TeamId);

            if (teamEntity == null)
                throw new CityQuestItemNotFoundException(CityQuestConsts.CityQuestItemNotFoundExceptionMessageBody, "\"Team\"");

            if (!TeamPolicy.CanChangeCaptainForEntity(teamEntity))
                throw new CityQuestPolicyException(CityQuestConsts.CQPolicyExceptionUpdateDenied, "\"Team\"");

            PlayerCareer newCap = teamEntity.PlayerCareers.SingleOrDefault(r => r.Id == input.NewCaptainCareerId);
            if (newCap != null)
            {
                var oldCap = teamEntity.Captain;

                if (oldCap.Id != newCap.Id)
                {
                    newCap.IsCaptain = true;
                    PlayerCareerRepository.Update(newCap);

                    oldCap.IsCaptain = false;
                    PlayerCareerRepository.Update(oldCap);
                }
            }

            return new ChangeCaptainOutput();
        }
    }
}
