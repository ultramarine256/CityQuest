﻿using Abp.Application.Services.Dto;
using Abp.Domain.Uow;
using CityQuest.ApplicationServices.GameModule.Tips.Dtos;
using CityQuest.ApplicationServices.Shared.Dtos.Input;
using CityQuest.ApplicationServices.Shared.Dtos.Output;
using CityQuest.CityQuestPolicy.GameModule.Games;
using CityQuest.CityQuestPolicy.GameModule.Tips;
using CityQuest.Entities.GameModule.Games;
using CityQuest.Entities.GameModule.Games.GameTasks;
using CityQuest.Entities.GameModule.Games.GameTasks.Tips;
using CityQuest.Exceptions;
using CityQuest.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CityQuest.ApplicationServices.GameModule.Tips
{
    [Abp.Authorization.AbpAuthorize]
    public class TipAppService : ITipAppService
    {
        #region Injected Dependencies

        private IUnitOfWorkManager UowManager { get; set; }
        private ITipRepository TipRepository { get; set; }
        private IGameTaskRepository GameTaskRepository { get; set; }
        private ITipPolicy TipPolicy { get; set; }
        private IGamePolicy GamePolicy { get; set; }

        #endregion

        #region ctors

        public TipAppService(IUnitOfWorkManager uowManager,
            ITipRepository tipRepository,
            IGameTaskRepository gameTaskRepository,
            ITipPolicy tipPolicy, 
            IGamePolicy gamePolicy)
        {
            UowManager = uowManager;
            TipRepository = tipRepository;
            GameTaskRepository = gameTaskRepository;
            TipPolicy = tipPolicy;
            GamePolicy = gamePolicy;
        }

        #endregion

        [Abp.Authorization.AbpAuthorize]
        public RetrieveAllPagedResultOutput<TipDto, long> RetrieveAllPagedResult(RetrieveAllTipsPagedResultInput input)
        {
            TipRepository.Includes.Add(r => r.LastModifierUser);
            TipRepository.Includes.Add(r => r.CreatorUser);

            IQueryable<Tip> tipsQuery = TipPolicy.CanRetrieveManyEntities(
                TipRepository.GetAll()
                .WhereIf(!input.TipIds.IsNullOrEmpty(), r => input.TipIds.Contains(r.Id))
                .WhereIf(!String.IsNullOrEmpty(input.Name), r => r.Name.ToLower().Contains(input.Name.ToLower())))
                .OrderBy(r => r.Order);

            int totalCount = tipsQuery.Count();
            IReadOnlyList<TipDto> tipDtos = tipsQuery
                .OrderBy(r => r.Order).ThenBy(r => r.Name)
                .Skip(input.SkipCount).Take(input.MaxResultCount)
                .ToList().MapIList<Tip, TipDto>().ToList();

            TipRepository.Includes.Clear();

            return new RetrieveAllPagedResultOutput<TipDto, long>()
            {
                Items = tipDtos,
                TotalCount = totalCount
            };
        }

        [Abp.Authorization.AbpAuthorize]
        public RetrieveAllTipsLikeComboBoxesOutput RetrieveAllTipsLikeComboBoxes(RetrieveAllTipsLikeComboBoxesInput input)
        {
            IReadOnlyList<ComboboxItemDto> tipsLikeComboBoxes = TipPolicy.CanRetrieveManyEntities(
                TipRepository.GetAll()).ToList().OrderBy(r => r.Order)
                .Select(r => new ComboboxItemDto(r.Id.ToString(), r.Name)).ToList();

            return new RetrieveAllTipsLikeComboBoxesOutput()
            {
                Items = tipsLikeComboBoxes
            };
        }

        [Abp.Authorization.AbpAuthorize]
        public RetrieveAllOutput<TipDto, long> RetrieveAll(RetrieveAllTipInput input)
        {
            TipRepository.Includes.Add(r => r.LastModifierUser);
            TipRepository.Includes.Add(r => r.CreatorUser);

            IList<Tip> tipEntities = TipPolicy.CanRetrieveManyEntities(
                TipRepository.GetAll()
                .WhereIf(!input.TipIds.IsNullOrEmpty(), r => input.TipIds.Contains(r.Id))
                .WhereIf(!String.IsNullOrEmpty(input.Name), r => r.Name.ToLower().Contains(input.Name.ToLower())))
                .OrderBy(r => r.Order)
                .ToList();

            IList<TipDto> result = tipEntities.MapIList<Tip, TipDto>();

            TipRepository.Includes.Clear();

            return new RetrieveAllOutput<TipDto, long>()
            {
                RetrievedEntities = result
            };
        }

        [Abp.Authorization.AbpAuthorize]
        public RetrieveOutput<TipDto, long> Retrieve(RetrieveTipInput input)
        {
            TipRepository.Includes.Add(r => r.LastModifierUser);
            TipRepository.Includes.Add(r => r.CreatorUser);

            IList<Tip> tipEntities = TipRepository.GetAll()
                .WhereIf(input.Id != null, r => r.Id == input.Id)
                .WhereIf(!String.IsNullOrEmpty(input.Name), r => r.Name.ToLower().Contains(input.Name.ToLower()))
                .ToList();

            if (tipEntities.Count != 1)
                throw new CityQuestItemNotFoundException(CityQuestConsts.CityQuestItemNotFoundExceptionMessageBody, "\"Tip\"");

            if (!TipPolicy.CanRetrieveEntity(tipEntities.Single()))
                throw new CityQuestPolicyException(CityQuestConsts.CQPolicyExceptionRetrieveDenied, "\"Tip\"");

            TipDto tipEntity = tipEntities.Single().MapTo<TipDto>();

            TipRepository.Includes.Clear();

            return new RetrieveOutput<TipDto, long>()
            {
                RetrievedEntity = tipEntity
            };
        }

        [Abp.Authorization.AbpAuthorize]
        public CreateOutput<TipDto, long> Create(CreateInput<TipDto, long> input)
        {
            throw new NotSupportedException("This method is implemented but it is not safely to use it.");

            Tip newTipEntity = input.Entity.MapTo<Tip>();

            if (!TipPolicy.CanCreateEntity(newTipEntity))
                throw new CityQuestPolicyException(CityQuestConsts.CQPolicyExceptionCreateDenied, "\"Tip\"");

            TipRepository.Includes.Add(r => r.LastModifierUser);
            TipRepository.Includes.Add(r => r.CreatorUser);

            TipDto newTipDto = (TipRepository.Insert(newTipEntity)).MapTo<TipDto>();

            TipRepository.Includes.Clear();

            return new CreateOutput<TipDto, long>()
            {
                CreatedEntity = newTipDto
            };
        }

        [Abp.Authorization.AbpAuthorize]
        public UpdateOutput<TipDto, long> Update(UpdateInput<TipDto, long> input)
        {
            throw new NotSupportedException("This method is implemented but it is not safely to use it.");

            Tip newTipEntity = input.Entity.MapTo<Tip>();

            if (newTipEntity == null)
                throw new CityQuestItemNotFoundException(CityQuestConsts.CityQuestItemNotFoundExceptionMessageBody, "\"Tip\"");

            if (!TipPolicy.CanUpdateEntity(newTipEntity))
                throw new CityQuestPolicyException(CityQuestConsts.CQPolicyExceptionUpdateDenied, "\"Tip\"");

            TipRepository.Includes.Add(r => r.LastModifierUser);
            TipRepository.Includes.Add(r => r.CreatorUser);

            TipRepository.Update(newTipEntity);
            TipDto newTipDto = (TipRepository.Get(newTipEntity.Id)).MapTo<TipDto>();

            TipRepository.Includes.Clear();

            return new UpdateOutput<TipDto, long>()
            {
                UpdatedEntity = newTipDto
            };
        }

        [Abp.Authorization.AbpAuthorize]
        public DeleteOutput<long> Delete(DeleteInput<long> input)
        {
            throw new NotSupportedException("This method is implemented but it is not safely to use it.");

            Tip tipEntityForDelete = TipRepository.Get(input.EntityId);

            if (tipEntityForDelete == null)
                throw new CityQuestItemNotFoundException(CityQuestConsts.CityQuestItemNotFoundExceptionMessageBody, "\"Tip\"");

            if (!TipPolicy.CanDeleteEntity(tipEntityForDelete))
                throw new CityQuestPolicyException(CityQuestConsts.CQPolicyExceptionDeleteDenied, "\"Tip\"");

            TipRepository.Delete(tipEntityForDelete);

            return new DeleteOutput<long>()
            {
                DeletedEntityId = input.EntityId
            };
        }

        [Abp.Authorization.AbpAuthorize]
        public RetrieveTipsForGameTaskOutput RetrieveTipsForGameTask(RetrieveTipsForGameTaskInput input)
        {
            if (!(input.GameTaskId > 0))
                return new RetrieveTipsForGameTaskOutput() { Tips = new List<TipDto>() };

            GameTaskRepository.Includes.Add(r => r.Game);
            GameTaskRepository.Includes.Add(r => r.Tips);

            GameTask gameTaskEntity = GameTaskRepository.Get(input.GameTaskId);
            Game gameEntity = gameTaskEntity.Game;

            if (gameEntity == null)
                throw new CityQuestItemNotFoundException(CityQuestConsts.CityQuestItemNotFoundExceptionMessageBody, "\"Game\"");

            if (!GamePolicy.CanRetrieveEntity(gameEntity))
                throw new CityQuestPolicyException(CityQuestConsts.CQPolicyExceptionRetrieveDenied, "\"Game\"");

            IList<Tip> tipEntities = gameTaskEntity.Tips.ToList();
            IList<TipDto> tipsForGameTask = tipEntities.MapIList<Tip, TipDto>();

            GameTaskRepository.Includes.Clear();

            return new RetrieveTipsForGameTaskOutput()
            {
                Tips = tipsForGameTask
            };
        }
    }
}
