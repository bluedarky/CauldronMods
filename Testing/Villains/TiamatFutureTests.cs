﻿using Cauldron.Tiamat;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CauldronTests
{
    [TestFixture()]
    class TiamatFutureTests : BaseTest
    {
        protected TurnTakerController tiamat { get { return FindVillain("Tiamat"); } }

        private void SetupIncap(TurnTakerController source, Card target)
        {
            SetHitPoints(target, 1);
            DealDamage(source, target, 2, DamageType.Radiant);
        }

        protected void AddCannotDealNextDamageTrigger(TurnTakerController ttc, Card card)
        {
            CannotDealDamageStatusEffect cannotDealDamageStatusEffect = new CannotDealDamageStatusEffect();
            cannotDealDamageStatusEffect.NumberOfUses = 1;
            cannotDealDamageStatusEffect.SourceCriteria.IsSpecificCard = card;
            this.RunCoroutine(this.GameController.AddStatusEffect(cannotDealDamageStatusEffect, true, new CardSource(ttc.CharacterCardController)));
        }

        private void AddShuffleTrashCounterAttackTrigger(TurnTakerController ttc, TurnTaker turnTakerToReshuffleTrash)
        {
            Func<DealDamageAction, bool> criteria = (DealDamageAction dd) => dd.Target == ttc.CharacterCard;
            Func<DealDamageAction, IEnumerator> response = (DealDamageAction dd) => this.GameController.ShuffleTrashIntoDeck(this.GameController.FindTurnTakerController(turnTakerToReshuffleTrash));
            this.GameController.AddTrigger<DealDamageAction>(new Trigger<DealDamageAction>(this.GameController, criteria, response, new TriggerType[] { TriggerType.ShuffleTrashIntoDeck }, TriggerTiming.After, this.GameController.FindCardController(turnTakerToReshuffleTrash.CharacterCard).GetCardSource()));
        }

        private bool IsSpell(Card card)
        {
            return card != null && base.GameController.DoesCardContainKeyword(card, "spell");
        }

        [Test()]
        public void TestFutureTiamatLoad()
        {
            SetupGameController("Cauldron.Tiamat/FutureTiamatCharacter", "Legacy", "Bunker", "Haka", "Megalopolis");
            StartGame();

            Assert.AreEqual(5, this.GameController.TurnTakerControllers.Count());
            Assert.IsNotNull(tiamat);
            AssertNumberOfCardsInPlay(tiamat, 3);
            AssertHitPoints(tiamat, 120);
            //Dragonscales have X HP, where X = {H - 1}.
            AssertMaximumHitPoints(GetCard("NeoscaleCharacter"), 2);
            AssertMaximumHitPoints(GetCard("ExoscaleCharacter"), 2);
            //At the start of the villain turn, flip {Tiamat}'s villain character cards.
            AssertFlipped(tiamat);
        }

        [Test()]
        public void TestScaleHP_5H()
        {
            SetupGameController("Cauldron.Tiamat/FutureTiamatCharacter", "Legacy", "Bunker", "Haka", "Ra", "Unity", "Megalopolis");
            StartGame();

            //Dragonscales have X HP, where X = {H - 1}.
            AssertMaximumHitPoints(GetCard("NeoscaleCharacter"), 4);
            AssertMaximumHitPoints(GetCard("ExoscaleCharacter"), 4);
        }

        [Test()]
        public void TestScaleHP_3H_Advanced()
        {
            SetupGameController(new string[] { "Cauldron.Tiamat/FutureTiamatCharacter", "Legacy", "Bunker", "Haka", "Megalopolis" }, true);
            StartGame();

            //Advanced: X = {H + 1} instead.
            AssertMaximumHitPoints(GetCard("NeoscaleCharacter"), 4);
            AssertMaximumHitPoints(GetCard("ExoscaleCharacter"), 4);
        }

        [Test()]
        public void TestScaleHP_5H_Advanced()
        {
            SetupGameController(new string[] { "Cauldron.Tiamat/FutureTiamatCharacter", "Legacy", "Bunker", "Haka", "Ra", "Unity", "Megalopolis" }, true);
            StartGame();

            //Advanced: X = {H + 1} instead.
            AssertMaximumHitPoints(GetCard("NeoscaleCharacter"), 6);
            AssertMaximumHitPoints(GetCard("ExoscaleCharacter"), 6);
        }

        [Test()]
        public void TestFutureTiamatAdvancedStartOfGame()
        {
            SetupGameController(new string[] { "Cauldron.Tiamat/FutureTiamatCharacter", "Legacy", "Bunker", "Haka", "Megalopolis" }, true);
            StartGame();

            AssertNumberOfCardsInTrash(tiamat, 2);
            foreach (Card c in tiamat.TurnTaker.Trash.Cards)
            {
                Assert.IsTrue(IsSpell(c));
            }
        }

        [Test()]
        public void TestFutureTiamatEndOfTurn()
        {
            SetupGameController("Cauldron.Tiamat/FutureTiamatCharacter", "Legacy", "Bunker", "Haka", "Megalopolis");
            StartGame();

            //At the end of the villain turn, {Tiamat} deals the hero target with the highest HP {H} energy damage. 
            QuickHPStorage(haka, legacy, bunker);
            GoToEndOfTurn(tiamat);
            QuickHPCheck(-3, 0, 0);

            AddCannotDealNextDamageTrigger(tiamat, tiamat.CharacterCard);
            PlayCard("TaMoko");
            PlayCard("SurgeOfStrength");
            //Then, if {Tiamat} deals no damage this turn, each hero target deals itself 3 projectile damage.
            QuickHPStorage(haka, legacy, bunker);
            GoToEndOfTurn(tiamat);
            QuickHPCheck(-2, -4, -3);
        }

        [Test()]
        public void TestExoscale()
        {
            SetupGameController("Cauldron.Tiamat/FutureTiamatCharacter", "TheScholar", "Bunker", "Haka", "Unity", "Megalopolis");
            StartGame();

            Card exo = GetCardInPlay("ExoscaleCharacter");
            SetHitPoints(scholar, 17);

            //When this card is destroyed, 1 hero may draw a card...
            QuickHandStorage(scholar);
            DealDamage(bunker, exo, 3, DamageType.Melee);
            QuickHandCheck(1);
            AssertIsInPlay(exo);
            //Then, flip this card.
            AssertFlipped(exo);

            //At the end of the villain turn...Then flip all ruined scales and restore them to their max HP.
            GoToEndOfTurn(tiamat);
            AssertNotFlipped(exo);
            AssertHitPoints(exo, 3);

            DecisionSelectFunction = 1;

            //When this card is destroyed, 1 hero may...use a power.
            QuickHPStorage(scholar);
            QuickHandStorage(scholar);
            DealDamage(bunker, exo, 3, DamageType.Melee);
            QuickHandCheck(0);
            QuickHPCheck(1);
            //Then, flip this card.
            AssertFlipped(exo);
        }

        [Test()]
        public void TestNeoscale()
        {
            SetupGameController("Cauldron.Tiamat/FutureTiamatCharacter", "Legacy", "Bunker", "Haka", "Unity", "Megalopolis");
            StartGame();

            Card neo = GetCardInPlay("NeoscaleCharacter");
            Card surge = PutInHand("SurgeOfStrength");

            //When this card is destroyed, 1 hero may draw a card...
            QuickHandStorage(legacy);
            DealDamage(bunker, neo, 3, DamageType.Melee);
            QuickHandCheck(1);
            AssertIsInPlay(neo);
            //Then, flip this card.
            AssertFlipped(neo);

            //At the end of the villain turn...Then flip all ruined scales and restore them to their max HP.
            GoToEndOfTurn(tiamat);
            AssertNotFlipped(neo);
            AssertHitPoints(neo, 3);

            DecisionSelectFunction = 1;
            DecisionSelectCard = surge;

            //When this card is destroyed, 1 hero may...play a card. 
            QuickHandStorage(legacy);
            DealDamage(bunker, neo, 3, DamageType.Melee);
            QuickHandCheck(-1);
            AssertIsInPlay(surge);
            //Then, flip this card.
            AssertFlipped(neo);
        }

        [Test()]
        public void TestTiamatFutureBackEffects()
        {
            SetupGameController("Cauldron.Tiamat/FutureTiamatCharacter", "Legacy", "Bunker", "Haka", "Unity", "Megalopolis");
            StartGame();

            //{Tiamat} counts as The Jaws of Winter, The Mouth of the Inferno, and The Eye of the Storm.
            PlayCard("ElementOfIce");
            //Each spell card in the villain trash counts as Element of Ice, Element of Fire, and Element of Lightining.
        }
    }
}
