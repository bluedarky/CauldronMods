﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Cauldron.Mythos
{
    public abstract class MythosUtilityCardController : CardController
    {
        protected MythosUtilityCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowSpecialString(() => this.DeckIconList());
            base.SpecialStringMaker.ShowSpecialString(() => this.ThisCardsIcon());
        }

        protected const string MythosClueDeckIdentifier = "MythosClue";
        protected const string MythosDangerDeckIdentifier = "MythosDanger";
        protected const string MythosMadnessDeckIdentifier = "MythosMadness";

        private string GetIconIdentifier(Card c)
        {
            //Temporary method to get the icon of a card until Subdecks are implemented
            string[] clueIdentifiers = { "DangerousInvestigation", "PallidAcademic", "Revelations", "RitualSite", "RustedArtifact", "TornPage" };
            string[] dangerIdentifiers = { "AclastyphWhoPeers", "FaithfulProselyte", "OtherworldlyAlignment", "PreyUponTheMind" };
            string[] madnessIdentifiers = { "ClockworkRevenant", "DoktorVonFaust", "HallucinatedHorror", "WhispersAndLies", "YourDarkestSecrets" };

            string identifier = null;
            if (clueIdentifiers.Contains(c.Identifier))
            {
                identifier = MythosClueDeckIdentifier;
            }
            if (dangerIdentifiers.Contains(c.Identifier))
            {
                identifier = MythosDangerDeckIdentifier;
            }
            if (madnessIdentifiers.Contains(c.Identifier))
            {
                identifier = MythosMadnessDeckIdentifier;
            }
            return identifier;
            /**Remove above when Subdecks are implemented**/
            return c.ParentDeck.Identifier;
        }

        public bool IsTopCardMatching(string type)
        {
            //Advanced: Activate all {MythosDanger} effects.
            if (base.Game.IsAdvanced && type == MythosDangerDeckIdentifier)
            {
                return true;
            }
            return this.GetIconIdentifier(base.TurnTaker.Deck.TopCard) == type;
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            //When cards originate from a Subdeck we need to redirect them to the correct Play Area
            storedResults.Add(new MoveCardDestination(base.TurnTaker.PlayArea));
            yield return null;
            yield break;
        }

        public override MoveCardDestination GetTrashDestination()
        {
            //When cards originate from a Subdeck we need to redirect them to the correct Trash
            return new MoveCardDestination(base.TurnTaker.Trash);
        }

        private string DeckIconList()
        {
            //For special string describing the order of icons in the deck top(1) to bottom
            string output = null;
            int place = 0;
            foreach (Card c in base.TurnTaker.Deck.Cards)
            {
                place++;
                if (output == null)
                {
                    output = "Starting at the top, the order of the deck icons is: ";
                }
                switch (this.GetIconIdentifier(c))
                {
                    case MythosClueDeckIdentifier:
                        output += place + ": {MythosClue}, ";
                        break;

                    case MythosDangerDeckIdentifier:
                        output += place + ": {MythosDanger}, ";
                        break;

                    case MythosMadnessDeckIdentifier:
                        output += place + ": {MythosMadness}, ";
                        break;
                }
            }

            if (output == null)
            {
                output = "There are no cards in the deck.";
            }
            else
            {
                output.Trim(new char[] { ',', ' ' });
                output += ".";
            }
            return output;
        }

        private string ThisCardsIcon()
        {
            //For special string describing the icon on the back of this card
            string output = null;
            switch (this.GetIconIdentifier(this.Card))
            {
                case MythosClueDeckIdentifier:
                    output = "This card has a Blue Clue Icon.";
                    break;

                case MythosDangerDeckIdentifier:
                    output = "This card has a Red Danger Icon.";
                    break;

                case MythosMadnessDeckIdentifier:
                    output = "This card has a Green Madness Icon.";
                    break;
            }
            return output;
        }
    }
}
