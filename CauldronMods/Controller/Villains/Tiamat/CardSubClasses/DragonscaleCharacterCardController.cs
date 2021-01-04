﻿using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Tiamat
{
    public class DragonscaleCharacterCardController : VillainCharacterCardController
    {
        public DragonscaleCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {

        }

        public override IEnumerator BeforeFlipCardImmediateResponse(FlipCardAction flip)
        {
            SelectTurnTakerDecision turnTakerDecision = new SelectTurnTakerDecision(base.GameController, this.DecisionMaker, base.FindTurnTakersWhere((TurnTaker tt) => tt.IsHero), SelectionType.SelectFunction, cardSource: base.GetCardSource());
            //When this card is destroyed, 1 hero may draw a card or [use a power/play a card]. Then, flip this card.
            IEnumerator coroutine = base.GameController.SelectTurnTakerAndDoAction(turnTakerDecision, this.SelectActionResponse);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            yield return base.BeforeFlipCardImmediateResponse(flip);
            yield break;
        }

        private IEnumerator SelectActionResponse(TurnTaker tt)
        {
            Function alternateFunction = null;
            if (this is NeoscaleCharacterCardController)
            {
                //...play a card...
                alternateFunction = new Function(base.DecisionMaker, "Play a card", SelectionType.PlayCard, () => base.SelectAndPlayCardFromHand(base.FindHeroTurnTakerController(tt.ToHero()), false));
            }
            if (this is ExoscaleCharacterCardController)
            {
                //...use a power...
                alternateFunction = new Function(base.DecisionMaker, "Use a power", SelectionType.UsePower, () => base.GameController.SelectAndUsePower(base.FindHeroTurnTakerController(tt.ToHero()), false, cardSource: base.GetCardSource()));
            }

            //...1 hero may draw a card or [use a power/play a card].
            IEnumerable<Function> functions = new Function[]
            {
                new Function(base.DecisionMaker, "Draw a card", SelectionType.DrawCard, () => base.DrawCard(tt.ToHero())),
                alternateFunction
            };
            IEnumerator coroutine = base.SelectAndPerformFunction(this.DecisionMaker, functions, true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            yield break;
        }
    }
}