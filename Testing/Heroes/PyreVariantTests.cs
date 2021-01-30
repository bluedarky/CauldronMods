﻿using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Cauldron.Pyre;

namespace CauldronTests
{
    [TestFixture()]
    public class PyreVariantTests : CauldronBaseTest
    {
        #region PyreHelperFunctions
        private void SetupIncap(TurnTakerController villain)
        {
            SetHitPoints(pyre.CharacterCard, 1);
            DealDamage(villain, pyre, 2, DamageType.Melee);
        }

        protected DamageType DTM => DamageType.Melee;

        protected Card MDP { get { return FindCardInPlay("MobileDefensePlatform"); } }

        protected bool IsIrradiated(Card card)
        {
            var result = card != null && card.NextToLocation.Cards.Any((Card c) => c.Identifier == "IrradiatedMarker");
            if (result && !card.Location.IsHand)
            {
                Assert.Fail($"{card.Title} is irradiated, but is not in a hand!");
            }
            return result;
        }
        protected void AssertIrradiated(Card card)
        {
            Assert.IsTrue(IsIrradiated(card), $"{card.Title} should have been irradiated, but it was not.");
        }
        protected void AssertNotIrradiated(Card card)
        {
            Assert.IsFalse(IsIrradiated(card), $"{card.Title} was irradiated, but it should not be.");
        }
        protected void RemoveCascadeFromGame()
        {
            MoveCards(pyre, new string[] { "RogueFissionCascade", "RogueFissionCascade" }, pyre.TurnTaker.OutOfGame);
        }
        #endregion PyreHelperFunctions
        [Test]
        public void TestExpeditionOblaskPyreLoads()
        {
            SetupGameController("BaronBlade", "Cauldron.Pyre/ExpeditionOblaskPyreCharacter", "Megalopolis");

            // Assert
            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());
            Assert.IsNotNull(pyre);
            Assert.IsInstanceOf(typeof(ExpeditionOblaskPyreCharacterCardController), pyre.CharacterCardController);

            Assert.AreEqual(30, pyre.CharacterCard.HitPoints);
        }
        [Test]
        public void TestExpeditionOblaskPower()
        {
            SetupGameController("BaronBlade", "Cauldron.Pyre/ExpeditionOblaskPyreCharacter", "Legacy", "Tempest", "TheScholar", "Megalopolis");
            StartGame();

            Card ring = PutInHand("TheLegacyRing");
            Card punch = PutInHand("AtomicPunch");
            Card fort = PutInHand("Fortitude");
            Card iron = PutInHand("FleshToIron");

            DecisionSelectCards = new Card[] { ring, punch, fort, iron };
            UsePower(pyre);
            AssertIrradiated(ring);
            AssertIrradiated(punch);
            AssertIrradiated(fort);
            AssertNotIrradiated(iron);
        }
        [Test]
        public void TestExpeditionOblaskIncap1()
        {
            SetupGameController("BaronBlade", "Cauldron.Pyre/ExpeditionOblaskPyreCharacter", "Legacy", "Tempest", "TheScholar", "Megalopolis");
            StartGame();

            Card ring = PutInHand("TheLegacyRing");
            Card punch = PutInHand("AtomicPunch");
            Card fort = PutInHand("Fortitude");
            Card surge = PutInHand("SurgeOfStrength");
            Card iron = PutInHand("FleshToIron");
            Card aqua = PutInHand("AquaticCorrespondence");


            DecisionSelectCards = new Card[] { ring, punch, fort };
            UsePower(pyre);

            DealDamage(baron, pyre, 50, DTM);
            DecisionSelectTurnTakers = new TurnTaker[] { legacy.TurnTaker, legacy.TurnTaker, legacy.TurnTaker, tempest.TurnTaker, scholar.TurnTaker };
            DecisionSelectCards = new Card[] { null, ring, surge, aqua, iron };
            DecisionSelectCardsIndex = 0;

            QuickHandStorage(legacy, tempest, scholar);
            UseIncapacitatedAbility(pyre, 0);
            AssertInHand(ring, surge, iron, aqua);
            QuickHandCheckZero();

            UseIncapacitatedAbility(pyre, 0);
            AssertInTrash(ring, aqua);
            AssertIsInPlay(surge, iron);
            QuickHandCheck(-2, 2, -1);
        }
        [Test]
        public void TestExpeditionOblaskIncap2()
        {
            SetupGameController("BaronBlade", "Cauldron.Pyre/ExpeditionOblaskPyreCharacter", "Legacy", "Tempest", "TheScholar", "Megalopolis");
            StartGame();

            DealDamage(baron, pyre, 50, DTM);

            Card traffic = PlayCard("TrafficPileup");
            Card police = PlayCard("PoliceBackup");
            Card hostage = PlayCard("HostageSituation");

            DecisionSelectCards = new Card[] { hostage, traffic, police };
            UseIncapacitatedAbility(pyre, 1);
            AssertInTrash(hostage, traffic);
            AssertIsInPlay(police);
        }
        [Test]
        public void TestExpeditionOblaskIncap3()
        {
            SetupGameController("BaronBlade", "Cauldron.Pyre/ExpeditionOblaskPyreCharacter", "Legacy", "Tempest", "TheScholar", "Megalopolis");
            StartGame();
            DealDamage(baron, pyre, 50, DTM);

            AssertIncapLetsHeroDrawCard(pyre, 2, legacy, 1);
        }
        [Test]
        public void TestUnstablePyreLoads()
        {
            SetupGameController("BaronBlade", "Cauldron.Pyre/UnstablePyreCharacter", "Megalopolis");

            // Assert
            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());
            Assert.IsNotNull(pyre);
            Assert.IsInstanceOf(typeof(UnstablePyreCharacterCardController), pyre.CharacterCardController);

            Assert.AreEqual(33, pyre.CharacterCard.HitPoints);
        }
        [Test]
        public void TestUnstablePower()
        {
            SetupGameController("BaronBlade", "Cauldron.Pyre/UnstablePyreCharacter", "Legacy", "Tempest", "TheScholar", "Megalopolis");
            StartGame();

            DiscardAllCards(pyre);
            Card rod = PutInHand("FracturedControlRod");
            Card collider = PutInHand("ParticleCollider");
            Card punch = PutInHand("AtomicPunch");

            DecisionSelectTurnTaker = legacy.TurnTaker;
            Card ring = PutInHand("TheLegacyRing");
            Card fort = PutInHand("Fortitude");

            DecisionSelectCards = new Card[] { ring, rod, fort, collider };
            Card cascade = PutInTrash("RogueFissionCascade");
            QuickShuffleStorage(pyre);

            UsePower(pyre);
            AssertInDeck(cascade);
            QuickShuffleCheck(1);
            AssertIrradiated(ring);
            AssertNotIrradiated(fort);
            AssertIsInPlay(rod);
            AssertInHand(punch, collider);

            UsePower(pyre);
            QuickShuffleCheck(0);
            AssertIrradiated(fort);
            AssertIsInPlay(rod, collider);
            AssertInHand(punch);
        }
        [Test]
        public void TestUnstableIncap1()
        {
            SetupGameController("BaronBlade", "Cauldron.Pyre/UnstablePyreCharacter", "Legacy", "Tempest", "TheScholar", "Megalopolis");
            StartGame();

            Card ring = PutInHand("TheLegacyRing");
            DiscardAllCards(pyre);
            DecisionSelectTurnTaker = legacy.TurnTaker;
            DecisionSelectCard = ring;
            UsePower(pyre);
            DealDamage(baron, pyre, 50, DTM);

            SetHitPoints(legacy, 20);
            SetHitPoints(tempest, 20);
            SetHitPoints(scholar, 20);
            DecisionSelectCards = new Card[] { null, ring, tempest.CharacterCard };
            QuickHPStorage(baron, legacy, tempest, scholar);

            UseIncapacitatedAbility(pyre, 0);
            AssertIrradiated(ring);
            QuickHPCheckZero();

            UseIncapacitatedAbility(pyre, 0);
            AssertInTrash(ring);
            QuickHPCheck(0, 0, 4, 0);

        }
        [Test]
        public void TestUnstableIncap2()
        {
            SetupGameController("BaronBlade", "Cauldron.Pyre/UnstablePyreCharacter", "Legacy", "Tempest", "TheScholar", "Megalopolis");
            StartGame();
            DealDamage(baron, pyre, 50, DTM);

            DecisionSelectCard = tempest.CharacterCard;
            UseIncapacitatedAbility(pyre, 1);
            QuickHPStorage(legacy, tempest, scholar);
            DealDamage(legacy, tempest, 1, DTM);
            DealDamage(tempest, legacy, 1, DTM);
            QuickHPCheck(-1, -1, 0);

            GoToStartOfTurn(base.env);
            DealDamage(legacy, tempest, 1, DTM);
            DealDamage(tempest, legacy, 1, DTM);
            QuickHPCheck(-1, 0, 0);

            GoToStartOfTurn(baron);
            DealDamage(legacy, tempest, 1, DTM);
            DealDamage(tempest, legacy, 1, DTM);
            QuickHPCheck(-1, -1, 0);
        }
        [Test]
        public void TestUnstableIncap3()
        {
            SetupGameController("BaronBlade", "Cauldron.Pyre/UnstablePyreCharacter", "Legacy", "Tempest", "TheScholar", "Megalopolis");
            StartGame();
            DealDamage(baron, pyre, 50, DTM);

            AssertIncapLetsHeroDrawCard(pyre, 2, legacy, 1);
        }
        [Test]
        public void TestWastelandRoninPyreLoads()
        {
            SetupGameController("BaronBlade", "Cauldron.Pyre/WastelandRoninPyreCharacter", "Megalopolis");

            // Assert
            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());
            Assert.IsNotNull(pyre);
            Assert.IsInstanceOf(typeof(WastelandRoninPyreCharacterCardController), pyre.CharacterCardController);

            Assert.AreEqual(28, pyre.CharacterCard.HitPoints);
        }
        public void TestWastelandRoninPower()
        {
            SetupGameController("BaronBlade", "Cauldron.Pyre/WastelandRoninPyreCharacter", "Legacy", "Tempest", "TheScholar", "Megalopolis");
            StartGame();
        }
    }
}
