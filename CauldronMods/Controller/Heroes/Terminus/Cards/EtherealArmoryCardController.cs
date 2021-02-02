﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.Terminus
{
    public class EtherealArmoryCardController : TerminusBaseCardController
    {
        /* 
         * Each player may play an ongoing or equipment card now.
         * At the start of your next turn, return each of those cards that is still in play to its player's hand.
         */
        public EtherealArmoryCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;

            // Each player may play an ongoing or equipment card now.
            coroutine = base.GameController.SelectCardsAndDoAction(DecisionMaker, 
                new LinqCardCriteria((Card c) => c.IsRealCard && c.IsHeroCharacterCard && c.IsInPlayAndHasGameText && !c.IsIncapacitatedOrOutOfGame, "hero"), 
                SelectionType.HeroCharacterCard, 
                PlayCardResponse, 
                null, 
                optional: false, 
                0, 
                null, 
                allowAutoDecide: false, 
                null, 
                GetCardSource());
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

        private IEnumerator PlayCardResponse(Card selectHero)
        {
            IEnumerator coroutine;
            List<PlayCardAction> playCardActions = new List<PlayCardAction>();
            OnPhaseChangeStatusEffect onPhaseChangeStatusEffect;

            coroutine = base.GameController.SelectAndPlayCardFromHand(base.FindCardController(selectHero).DecisionMaker, true, storedResults: playCardActions, cardCriteria: new LinqCardCriteria((card) => card.IsOngoing || base.IsEquipment(card)), cardSource: base.GetCardSource());
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            // PROBLEM: THIS IS A ONE-SHOT. IT LEAVES PLAY IMMEDIATELY. THIS WON't WORL.
            if (DidPlayCards(playCardActions))
            {
                var playedCard = playCardActions.FirstOrDefault().CardToPlay;

                onPhaseChangeStatusEffect = new OnPhaseChangeStatusEffect(base.Card, 
                    nameof(this.StartOfTurnResponse), 
                    "At the start of {Terminus} next turn, return " + playedCard.Title + " from play to your hand.", 
                    new TriggerType[] { TriggerType.MoveCard }, 
                    playedCard);
                onPhaseChangeStatusEffect.NumberOfUses = 1;
                onPhaseChangeStatusEffect.BeforeOrAfter = BeforeOrAfter.After;
                onPhaseChangeStatusEffect.TurnTakerCriteria.IsSpecificTurnTaker = base.TurnTaker;
                onPhaseChangeStatusEffect.TurnPhaseCriteria.Phase = Phase.Start;
                onPhaseChangeStatusEffect.TurnPhaseCriteria.TurnTaker = base.TurnTaker;
                onPhaseChangeStatusEffect.TurnIndexCriteria.GreaterThan = base.Game.TurnIndex;
                onPhaseChangeStatusEffect.CardMovedExpiryCriteria.Card = playedCard;

                coroutine = base.AddStatusEffect(onPhaseChangeStatusEffect);
                if (base.UseUnityCoroutines)
                {
                    yield return base.GameController.StartCoroutine(coroutine);
                }
                else
                {
                    base.GameController.ExhaustCoroutine(coroutine);
                }
            }

            yield break;
        }

        public IEnumerator StartOfTurnResponse(PhaseChangeAction action, OnPhaseChangeStatusEffect effect)
        {
            //...return it from play to your hand.
            IEnumerator coroutine;
            TurnTakerController originalCardController = base.FindTurnTakerController(effect.CardMovedExpiryCriteria.Card.Owner);

            coroutine = base.GameController.MoveCard(originalCardController, effect.CardMovedExpiryCriteria.Card, originalCardController.TurnTaker.ToHero().Hand, cardSource: base.GetCardSource());

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
