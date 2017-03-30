using System;
using System.Collections.Generic;
using System.Linq;
using Domain;
using System.Diagnostics;

namespace Logic
{
    /// <summary>
    /// This class is used to find the winners of each routeNumber at a time.
    /// It is a 'helper-class' for SelectionController and methods in Selection are only called from SelectionController.
    /// </summary>
    public class Selection
    {
        ListContainer listContainer = ListContainer.GetInstance();

        /// <summary>
        /// This method takes a list of sorted route numbers.
        /// Each route number has its offers sorted by operationPrice.
        /// </summary>
        public void CalculateOperationPriceDifferenceForOffers(List<RouteNumber> sortedRouteNumberList)
        {
            const int LAST_OPTION_VALUE = int.MaxValue;
            foreach (RouteNumber routeNumber in sortedRouteNumberList)
            {
                int numbersToCalc = (routeNumber.offers.Count()) - 1;

                // No bids = throw exception
                if (routeNumber.offers.Count == 0)
                {
                    throw new Exception("Der er ingen bud på garantivognsnummer " + routeNumber.RouteID);
                }

                // If only one bid
                else if (routeNumber.offers.Count == 1)
                {
                    routeNumber.offers[0].DifferenceToNextOffer = LAST_OPTION_VALUE;
                }

                // If exectly two bids
                else if (routeNumber.offers.Count == 2)
                {

                    // If bids are at same price, both DifferenceToNextOffer are set to max
                    if (routeNumber.offers[0].OperationPrice == routeNumber.offers[1].OperationPrice)
                    {
                        routeNumber.offers[0].DifferenceToNextOffer = LAST_OPTION_VALUE;
                        routeNumber.offers[1].DifferenceToNextOffer = LAST_OPTION_VALUE;
                    }

                    // The lowest priced offer (at index 0) has its property DifferenceToNextOffer set to the actual difference
                    else
                    {
                        routeNumber.offers[0].DifferenceToNextOffer = routeNumber.offers[1].OperationPrice - routeNumber.offers[0].OperationPrice;
                        routeNumber.offers[1].DifferenceToNextOffer = LAST_OPTION_VALUE;
                    }
                }

                // More than two offers
                // Each offer has its DifferenceToNextOffer set to correct difference, while the last last offer has its difference set to LAST_OPTION_VALUE
                else
                {
                    for (int i = 0; i < numbersToCalc; i++)
                    {
                        float difference = 0;
                        int j = i + 1;
                        if (routeNumber.offers[i].OperationPrice != routeNumber.offers[numbersToCalc].OperationPrice)
                        {
                            while (difference == 0 && j <= numbersToCalc)
                            {
                                difference = routeNumber.offers[j].OperationPrice - routeNumber.offers[i].OperationPrice;
                                j++;
                            }
                        }
                        else
                        {
                            while (i < numbersToCalc)
                            {
                                routeNumber.offers[i].DifferenceToNextOffer = LAST_OPTION_VALUE;
                                i++;
                            }
                        }
                        routeNumber.offers[i].DifferenceToNextOffer = difference;
                    }

                    // Last item here
                    routeNumber.offers[numbersToCalc].DifferenceToNextOffer = LAST_OPTION_VALUE;
                }
            }
        }

        /// <summary>
        /// Name of method is self explanatory.
        /// Read more comments inside method.
        /// </summary>
        public void CheckIfContractorHasWonTooManyRouteNumbers(List<Offer> offersToCheck, List<RouteNumber> sortedRouteNumberList)
        {
            List<Contractor> contractorsToCheck = new List<Contractor>();

            // For each offer it loops through all contractors.
            // If contractors id matches offers id, the contractor is added to contractorsToCheck:List
            foreach (Offer offer in offersToCheck)
            {
                foreach (Contractor contractor in listContainer.contractorList)
                {
                    if (contractor.UserID.Equals(offer.UserID))
                    {
                        bool alreadyOnList = contractorsToCheck.Any(obj => obj.UserID.Equals(contractor.UserID));
                        if (!alreadyOnList)
                        {
                            contractorsToCheck.Add(contractor);
                        }
                    }
                }
            }

            // Loops through contractors to see if they won more viechles than their amount of cars
            foreach (Contractor contractor in contractorsToCheck)
            {
                List<Offer> offers = contractor.CompareNumberOfWonOffersAgainstVehicles();
                if (offers.Count > 0 && offers != null)
                {
                    foreach (Offer offer in contractor.CompareNumberOfWonOffersAgainstVehicles())
                    {
                        // Dont think this bool is every used.
                        // Maybe it should have been used (like above) to only add offers to conflictlist of they are not already added
                        bool alreadyOnList = listContainer.conflictList.Any(item => item.OfferReferenceNumber == offer.OfferReferenceNumber);

                        listContainer.conflictList.Add(offer);
                    }
                    throw new Exception("Denne entreprenør har vundet flere garantivognsnumre, end de har biler til.  Der kan ikke vælges imellem dem, da de har samme prisforskel ned til næste bud. Prioriter venligst buddene i den relevante fil i kolonnen Entreprenør Prioritet");
                }
            }
        }

        /// <summary>
        /// Returns a list of offer (sorted) from a given routeNumber
        /// </summary>
        public List<Offer> FindWinner(RouteNumber routeNumber)
        {
            List<Offer> winningOffers = new List<Offer>();
            List<Offer> listOfOffersWithLowestPrice = new List<Offer>();
            int lengthOfOffers = routeNumber.offers.Count();
            float lowestEligibleOperationPrice = 0;
            bool cheapestNotFound = true;

            for (int i = 0; i < lengthOfOffers; i++)
            {
                if (routeNumber.offers[i].IsEligible && cheapestNotFound)
                {
                    lowestEligibleOperationPrice = routeNumber.offers[i].OperationPrice;
                    cheapestNotFound = false;
                }
            }

            foreach (Offer offer in routeNumber.offers)
            {
                if (offer.IsEligible && offer.OperationPrice == lowestEligibleOperationPrice)
                {
                    listOfOffersWithLowestPrice.Add(offer);
                }
            }

            int count = 0;
            foreach (Offer offer in listOfOffersWithLowestPrice) // Checking if offers with same price are prioritized
            {
                if (offer.RouteNumberPriority != 0)
                {
                    count++;
                }
            }
            if (count != 0) //if routenumberpriority found 

            {
                List<Offer> listOfPriotizedOffers = new List<Offer>();
                foreach (Offer offer in listOfOffersWithLowestPrice)
                {
                    if (offer.RouteNumberPriority > 0)
                    {
                        listOfPriotizedOffers.Add(offer);
                    }
                }

                listOfPriotizedOffers = listOfPriotizedOffers.OrderBy(x => x.RouteNumberPriority).ToList();
                winningOffers.Add(listOfPriotizedOffers[0]);
            }
            else
            {
                foreach (Offer offer in listOfOffersWithLowestPrice)
                {
                    winningOffers.Add(offer);
                }
            }

            return winningOffers;
        }

        /// <summary>
        /// 
        /// </summary>
        public List<Offer> AssignWinners(List<Offer> offersToAssign, List<RouteNumber> sortedRouteNumberList) // sortedRouteNumberList NOT USED!
        {
            List<Offer> offersThatHaveBeenMarkedIneligible = new List<Offer>(); // NOT USED!
            List<Contractor> contractorsToCheck            = new List<Contractor>();
            List<Offer> ineligibleOffersAllContractors     = new List<Offer>();

            foreach (Offer offer in offersToAssign)
            {
                if (offer.IsEligible)
                {
                    listContainer.contractorList.Find(x => x.UserID == offer.UserID).AddWonOffer(offer);
                    contractorsToCheck.Add(offer.Contractor);
                }
            }

            int lengthOfContractorList = contractorsToCheck.Count();
            for (int i = 0; i < lengthOfContractorList; i++)
            {
                contractorsToCheck[i].CompareNumberOfWonOffersAgainstVehicles();
                List<Offer> ineligibleOffersOneContractor = contractorsToCheck[i].ReturnIneligibleOffers();
                ineligibleOffersAllContractors.AddRange(ineligibleOffersOneContractor);
                contractorsToCheck[i].RemoveIneligibleOffersFromWinningOffers();
            }
            
            return ineligibleOffersAllContractors;
        }

        /// <summary>
        /// Does what is says. It checks wether a route numbers has multiple winners.
        /// The program cannot decide which offer to win, if they are at same price.
        /// It then asks the user of the program to go and prioritize an offer.
        /// </summary>
        public void CheckForMultipleWinnersForEachRouteNumber(List<Offer> winnerList)
        {
            int length = winnerList.Count;
            for (int i = 0; i < length; i++)
            {
                for (int j = i + 1; j < length; j++)
                {
                    if (winnerList[i].RouteID == winnerList[j].RouteID)
                    {
                        foreach (Offer offer in winnerList)
                        {
                            if (offer.RouteID == winnerList[i].RouteID)
                            {
                                listContainer.conflictList.Add(offer);
                            }
                        }
                        throw new Exception("Dette garantivognsnummer har flere mulige vindere. Der kan ikke vælges mellem dem, da de har samme prisforskel ned til næste bud. Prioriter venligst buddene i den relevante fil i kolonnen Garantivognsnummer Prioritet.");
                    }
                }
            }
        }
    }
}
