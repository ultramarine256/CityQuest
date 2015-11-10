﻿using Abp.Application.Services.Dto;
using Abp.Domain.Uow;
using Abp.UI;
using CityQuest.ApplicationServices.GameModule.Games.Dtos;
using CityQuest.ApplicationServices.Shared.Dtos.Input;
using CityQuest.ApplicationServices.Shared.Dtos.Output;
using CityQuest.CityQuestConstants;
using CityQuest.CityQuestPolicy.GameModule.Games;
using CityQuest.Entities.GameModule.Games;
using CityQuest.Exceptions;
using CityQuest.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityQuest.ApplicationServices.GameModule.Games
{
    public class GameAppService : IGameAppService
    {
        #region Injected Dependencies

        private IUnitOfWorkManager UowManager { get; set; }
        private IGameRepository GameRepository { get; set; }
        private IGamePolicy GamePolicy { get; set; }

        #endregion

        #region ctors

        public GameAppService(IUnitOfWorkManager uowManager, 
            IGameRepository gameRepository, 
            IGamePolicy gamePolicy)
        {
            UowManager = uowManager;
            GameRepository = gameRepository;
            GamePolicy = gamePolicy;
        }

        #endregion

        public RetrieveAllPagedResultOutput<GameDto, long> RetrieveAllPagedResult(RetrieveAllGamesPagedResultInput input)
        {
            if (input.IsActive ?? true)
                UowManager.Current.EnableFilter(Filters.IPassivableFilter);

            GameRepository.Includes.Add(r => r.LastModifierUser);
            GameRepository.Includes.Add(r => r.CreatorUser);
            GameRepository.Includes.Add(r => r.GameTasks);

            IQueryable<Game> gamesQuery = GamePolicy.CanRetrieveManyEntities(
                GameRepository.GetAll()
                .WhereIf(!input.GameIds.IsNullOrEmpty(), r => input.GameIds.Contains(r.Id))
                .WhereIf(!String.IsNullOrEmpty(input.Name), r => r.Name.ToLower().Contains(input.Name.ToLower())));

            int totalCount = gamesQuery.Count();
            IReadOnlyList<GameDto> gameDtos = gamesQuery
                .OrderByDescending(r => r.IsActive).ThenBy(r => r.Name)
                .Skip(input.SkipCount).Take(input.MaxResultCount)
                .ToList().MapIList<Game, GameDto>().ToList();

            GameRepository.Includes.Clear();

            return new RetrieveAllPagedResultOutput<GameDto, long>()
            {
                Items = gameDtos,
                TotalCount = totalCount
            };
        }

        public RetrieveAllGamesLikeComboBoxesOutput RetrieveAllGamesLikeComboBoxes(RetrieveAllGamesLikeComboBoxesInput input)
        {
            if (input.IsActive ?? true)
                UowManager.Current.EnableFilter(Filters.IPassivableFilter);

            IReadOnlyList<ComboboxItemDto> gamesLikeComboBoxes = GamePolicy.CanRetrieveManyEntities(
                GameRepository.GetAll()).ToList()
                .Select(r => new ComboboxItemDto(r.Id.ToString(), r.Name)).ToList();

            return new RetrieveAllGamesLikeComboBoxesOutput()
            {
                Items = gamesLikeComboBoxes
            };
        }

        public RetrieveAllOutput<GameDto, long> RetrieveAll(RetrieveAllGamesInput input)
        {
            if (input.IsActive ?? true)
                UowManager.Current.EnableFilter(Filters.IPassivableFilter);

            IList<Game> gameEntities = GamePolicy.CanRetrieveManyEntities( 
                GameRepository.GetAll()
                .WhereIf(!input.GameIds.IsNullOrEmpty(), r => input.GameIds.Contains(r.Id))
                .WhereIf(!String.IsNullOrEmpty(input.Name), r => r.Name.ToLower().Contains(input.Name.ToLower())))
                .ToList();

            IList<GameDto> result = gameEntities.MapIList<Game, GameDto>();

            return new RetrieveAllOutput<GameDto, long>()
            {
                RetrievedEntities = result
            };
        }

        public RetrieveOutput<GameDto, long> Retrieve(RetrieveGameInput input)
        {
            if (input.IsActive ?? true)
                UowManager.Current.EnableFilter(Filters.IPassivableFilter);

            GameRepository.Includes.Add(r => r.GameTasks);
            GameRepository.Includes.Add(r => r.LastModifierUser);
            GameRepository.Includes.Add(r => r.CreatorUser);

            IList<Game> gameEntities = GameRepository.GetAll()
                .WhereIf(input.Id != null, r => r.Id == input.Id)
                .WhereIf(!String.IsNullOrEmpty(input.Name), r => r.Name.ToLower().Contains(input.Name.ToLower()))
                .ToList();

            if (gameEntities.Count != 1) 
                throw new CityQuestItemNotFoundException(CityQuestConsts.CityQuestItemNotFoundExceptionMessageBody, "\"Game\"");

            if (!GamePolicy.CanRetrieveEntity(gameEntities.Single()))
                throw new CityQuestPolicyException(CityQuestConsts.CQPolicyExceptionRetrieveDenied, "\"Game\"");

            GameDto gameEntity = gameEntities.Single().MapTo<GameDto>();

            GameRepository.Includes.Clear();

            return new RetrieveOutput<GameDto, long>()
            {
                RetrievedEntity = gameEntity
            };
        }

        public CreateOutput<GameDto, long> Create(CreateGameInput input)
        {
            Game newGameEntity = input.Entity.MapTo<Game>();

            if (!GamePolicy.CanCreateEntity(newGameEntity))
                throw new CityQuestPolicyException(CityQuestConsts.CQPolicyExceptionCreateDenied, "\"Game\"");

            newGameEntity.IsActive = true;

            GameRepository.Includes.Add(r => r.GameTasks);
            GameRepository.Includes.Add(r => r.LastModifierUser);
            GameRepository.Includes.Add(r => r.CreatorUser);

            GameDto newGameDto = (GameRepository.Insert(newGameEntity)).MapTo<GameDto>();

            GameRepository.Includes.Clear();

            return new CreateOutput<GameDto, long>()
            {
                CreatedEntity = newGameDto
            };
        }

        public UpdateOutput<GameDto, long> Update(UpdateGameInput input)
        {
            Game newGameEntity = input.Entity.MapTo<Game>();

            if (newGameEntity == null)
                throw new CityQuestItemNotFoundException(CityQuestConsts.CityQuestItemNotFoundExceptionMessageBody, "\"Game\"");

            if (!GamePolicy.CanUpdateEntity(newGameEntity))
                throw new CityQuestPolicyException(CityQuestConsts.CQPolicyExceptionUpdateDenied, "\"Game\"");

            GameRepository.Includes.Add(r => r.GameTasks);
            GameRepository.Includes.Add(r => r.LastModifierUser);
            GameRepository.Includes.Add(r => r.CreatorUser);

            GameRepository.Update(newGameEntity);
            GameDto newGameDto = (GameRepository.Get(newGameEntity.Id)).MapTo<GameDto>();

            GameRepository.Includes.Clear();

            return new UpdateOutput<GameDto, long>()
            {
                UpdatedEntity = newGameDto
            };
        }

        public DeleteOutput<long> Delete(DeleteInput<long> input)
        {
            Game gameEntityForDelete = GameRepository.Get(input.EntityId);

            if (gameEntityForDelete == null)
                throw new CityQuestItemNotFoundException(CityQuestConsts.CityQuestItemNotFoundExceptionMessageBody, "\"Game\"");

            if (!GamePolicy.CanDeleteEntity(gameEntityForDelete))
                throw new CityQuestPolicyException(CityQuestConsts.CQPolicyExceptionDeleteDenied, "\"Game\"");

            GameRepository.Delete(gameEntityForDelete);

            return new DeleteOutput<long>()
            {
                DeletedEntityId = input.EntityId
            };
        }

        public ChangeActivityOutput<GameDto, long> ChangeActivity(ChangeActivityInput input)
        {
            GameRepository.Includes.Add(r => r.GameTasks);
            GameRepository.Includes.Add(r => r.LastModifierUser);
            GameRepository.Includes.Add(r => r.CreatorUser);

            Game gameEntity = GameRepository.Get(input.EntityId);

            if (gameEntity == null)
                throw new CityQuestItemNotFoundException(CityQuestConsts.CityQuestItemNotFoundExceptionMessageBody, "\"Game\"");

            if (!GamePolicy.CanChangeActivityForEntity(gameEntity))
                throw new CityQuestPolicyException(CityQuestConsts.CQPolicyExceptionChangeActivityDenied, "\"Game\"");

            gameEntity.IsActive = input.IsActive == null ? !gameEntity.IsActive : (bool)input.IsActive;

            GameDto newGameDto = (gameEntity).MapTo<GameDto>();

            GameRepository.Update(gameEntity);

            GameRepository.Includes.Clear();

            return new ChangeActivityOutput<GameDto, long>()
            {
                Entity = newGameDto
            };
        }
    }
}