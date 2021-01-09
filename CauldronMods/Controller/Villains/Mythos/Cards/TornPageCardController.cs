﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.Mythos
{
    public class TornPageCardController : MythosUtilityCardController
    {
        public TornPageCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {

        }

        private const string FirstTimeActivated = "FirstTimeActivated";


        public override void AddTriggers()
        {
            //The first time each turn that:
            //{MythosDanger} a hero card is drawn...
            base.AddTrigger<DrawCardAction>((DrawCardAction action) => !base.HasBeenSetToTrueThisTurn(FirstTimeActivated) && base.IsTopCardMatching(MythosDangerDeckIdentifier) && action.DidDrawCard, this.DealDamageResponse, new TriggerType[] { TriggerType.DealDamage }, TriggerTiming.After);
            //{MythosMadness} a hero card enters play...
            base.AddTrigger<CardEntersPlayAction>((CardEntersPlayAction action) => !base.HasBeenSetToTrueThisTurn(FirstTimeActivated) && action.CardEnteringPlay.IsHero && action.IsSuccessful, this.DealDamageResponse, new TriggerType[] { TriggerType.DealDamage }, TriggerTiming.After);
            //{MythosClue} a power is used...
            base.AddTrigger<UsePowerAction>((UsePowerAction action) => !base.HasBeenSetToTrueThisTurn(FirstTimeActivated) && action.IsSuccessful, this.DealDamageResponse, new TriggerType[] { TriggerType.DealDamage }, TriggerTiming.After);
            //...this card deals that hero 2 infernal damage and 2 psychic damage.
        }

        private IEnumerator DealDamageResponse(GameAction action)
        {
            IEnumerator coroutine;
            //...this card deals that hero 2 infernal damage and 2 psychic damage.
            Card target = action.CardSource.Card.Owner.CharacterCard;
            if (!target.IsRealCard)
            {
                List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
                coroutine = base.GameController.SelectCardAndStoreResults(this.DecisionMaker, SelectionType.DealDamage, new LinqCardCriteria((Card c) => c.Owner == target.Owner && c.IsHeroCharacterCard), storedResults, false, cardSource: base.GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return GameController.StartCoroutine(coroutine);
                }
                else
                {
                    GameController.ExhaustCoroutine(coroutine);
                }
                target = storedResults.FirstOrDefault().SelectedCard;
            }

            coroutine = base.DealMultipleInstancesOfDamage(new List<DealDamageAction>()
                { 
                    //...2 infernal damage...
                    new DealDamageAction(base.GetCardSource(), new DamageSource(base.GameController, base.Card), null, 2, DamageType.Infernal),
                    //...and 2 psychic damage.
                    new DealDamageAction(base.GetCardSource(), new DamageSource(base.GameController, base.Card), null, 2, DamageType.Psychic)
                }, (Card c) => c == target);
            if (UseUnityCoroutines)
            {
                yield return GameController.StartCoroutine(coroutine);
            }
            else
            {
                GameController.ExhaustCoroutine(coroutine);
            }
            yield break;
        }
    }
}
