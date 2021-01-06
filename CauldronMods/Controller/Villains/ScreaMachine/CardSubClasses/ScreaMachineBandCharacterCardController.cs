﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.ScreaMachine
{
    public abstract class ScreaMachineBandCharacterCardController : ScreaMachineUtilityCharacterCardController
    {
        public string AbilityKey { get; }

        public string CardKeyword { get; }

        protected ScreaMachineBandCharacterCardController(Card card, TurnTakerController turnTakerController, string abilityKey, string cardKeyword) : base(card, turnTakerController)
        {
            AbilityKey = abilityKey;
            CardKeyword = cardKeyword;
        }

        protected abstract string AbilityDescription { get; }

        protected abstract IEnumerator ActivateBandAbility();

        public override IEnumerator ActivateAbility(string abilityKey)
        {
            if (abilityKey == AbilityKey)
                return ActivateBandAbility();

            return base.ActivateAbility(abilityKey);
        }

        public override IEnumerable<ActivatableAbility> GetActivatableAbilities(string key = null, TurnTakerController activatingTurnTaker = null)
        {
            if (key is null || key == AbilityKey)
            {
                yield return new ActivatableAbility(TurnTakerController, this, AbilityKey, AbilityDescription, ActivateAbility(AbilityKey), 0, null, activatingTurnTaker, GetCardSource());
            }
        }

        public override void AddSideTriggers()
        {
            if (!Card.IsFlipped)
            {
                AddSideTrigger(AddTrigger<CardEntersPlayAction>(FlipCriteria, ca => base.FlipThisCharacterCardResponse(ca), TriggerType.FlipCard, TriggerTiming.After));
            }
            else
            {
                AddFlippedSideTriggers();
            }
        }

        private bool FlipCriteria(CardEntersPlayAction ga)
        {
            int count = base.FindCardsWhere(new LinqCardCriteria(c => c.IsInPlayAndNotUnderCard && GameController.DoesCardContainKeyword(c, CardKeyword)), GetCardSource()).Count();

            return count >= 3;
        }

        protected abstract void AddFlippedSideTriggers();

        public override MoveCardDestination GetTrashDestination()
        {
            return new MoveCardDestination(TurnTaker.OutOfGame);
        }
    }
}
