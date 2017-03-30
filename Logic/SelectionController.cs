using System.Collections.Generic;
using System.Linq;
using Domain;
using System.Diagnostics;

namespace Logic
{ 
    public class SelectionController
    {       
        public List<RouteNumber> routeNumberList;
        public Selection selection;
        public List<RouteNumber> sortedRouteNumberList;
        ListContainer listContainer = ListContainer.GetInstance();
        
        public SelectionController()
        {
            routeNumberList = new List<RouteNumber>();
            selection = new Selection();
        }

        /// <summary>
        /// 1. Sorts routeNumbers by ID
        /// 2. Sorts offers of each routeNumber by OperationPrice
        /// </summary>
        private void SortRouteNumberList(List<RouteNumber> routeNumberList)
        {
            sortedRouteNumberList = routeNumberList.OrderBy(x => x.RouteID).ToList();

            foreach (RouteNumber routeNumber in sortedRouteNumberList)
            {
                routeNumber.offers = routeNumber.offers.OrderBy(x => x.OperationPrice).ThenBy(x => x.RouteNumberPriority).ToList();

                ///////////////////// Tracing //////////////////////////
                Trace.WriteLine($"\nOffers for route-number {routeNumber.RouteID} are now sorted");

                int index = 0;
                foreach (Offer o in routeNumber.offers)
                {
                    Trace.Indent();

                    if (index == 0)
                    {
                        Trace.WriteLine($"Offer from company '{o.Contractor.CompanyName}' at price {o.OperationPrice}kr per hour (WINNER)");
                    }
                    else
                    {
                        Trace.WriteLine($"Offer from company '{o.Contractor.CompanyName}' at price {o.OperationPrice}kr per hour");
                    }
                    
                    Trace.Unindent();

                    index++;
                }
                //////////////////// Tracing end //////////////////////
            }

        }

        /// <summary>
        /// Uses methods from Selection-class to find winners.
        /// </summary>
        public void SelectWinners()
        {
            routeNumberList = listContainer.routeNumberList;
            SortRouteNumberList(routeNumberList);

            List<Offer> offersToAssign = new List<Offer>();

            selection.CalculateOperationPriceDifferenceForOffers(sortedRouteNumberList);
            int lengthOfSortedRouteNumberList = sortedRouteNumberList.Count();
            for (int i = 0; i < lengthOfSortedRouteNumberList; i++)
            {
                List<Offer> toAddToAssign = selection.FindWinner(sortedRouteNumberList[i]);
                foreach (Offer offer in toAddToAssign)
                {
                    offersToAssign.Add(offer);
                }
            }
            List<Offer> offersThatAreIneligible = selection.AssignWinners(offersToAssign, sortedRouteNumberList);

            bool allRouteNumberHaveWinner = DoAllRouteNumbersHaveWinner(offersThatAreIneligible);
            if (allRouteNumberHaveWinner)
            {
                selection.CheckIfContractorHasWonTooManyRouteNumbers(CreateWinnerList(), sortedRouteNumberList);
                selection.CheckForMultipleWinnersForEachRouteNumber(CreateWinnerList());
                List<Offer> winningOffers = CreateWinnerList();
                foreach (Offer offer in winningOffers)
                {
                    listContainer.outputList.Add(offer);
                }
            }
            // If not all routes have found winners yet
            else
            {
                ContinueUntilAllRouteNumbersHaveWinner(offersThatAreIneligible);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ContinueUntilAllRouteNumbersHaveWinner(List<Offer> offersThatAreIneligible)
        {
            List<Offer> offersThatHaveBeenMarkedIneligible = offersThatAreIneligible;
            List<Offer> offersToAssign = new List<Offer>();

            foreach (Offer offer in offersThatHaveBeenMarkedIneligible)
            {
                foreach (RouteNumber routeNumber in sortedRouteNumberList)
                {
                    if (routeNumber.RouteID == offer.RouteID)
                    {
                        List<Offer> offersToAssignToContractor = selection.FindWinner(routeNumber);
                        foreach (Offer ofr in offersToAssignToContractor)
                        {
                            offersToAssign.Add(ofr);
                        }
                    }
                }
            }
            offersThatHaveBeenMarkedIneligible = selection.AssignWinners(offersToAssign, sortedRouteNumberList);
            bool allRouteNumberHaveWinner = DoAllRouteNumbersHaveWinner(offersThatHaveBeenMarkedIneligible);
            if (allRouteNumberHaveWinner)
            {
                selection.CheckIfContractorHasWonTooManyRouteNumbers(CreateWinnerList(), sortedRouteNumberList);
                selection.CheckForMultipleWinnersForEachRouteNumber(CreateWinnerList());
                foreach (Offer offer in CreateWinnerList())
                {
                    listContainer.outputList.Add(offer);
                }
            } // Sidste punkt
            else
            {                
                ContinueUntilAllRouteNumbersHaveWinner(offersThatHaveBeenMarkedIneligible);
            }
        }

        /// <summary>
        /// 1. Loops through all contractors to find all winning offers
        /// 2. Return a big list of all winning offers
        /// </summary>
        public List<Offer> CreateWinnerList()
        {
            List<Offer> winningOffers = new List<Offer>();

            foreach (Contractor c in listContainer.contractorList)
            {
                foreach (Offer o in c.winningOffers)
                {
                    winningOffers.Add(o);
                }
            }
            return winningOffers;
        }

        /// <summary>
        /// Basically just checks if the list of offers is empty or not.
        /// So if no offers are ineligible = all routes has winners
        /// </summary>
        private bool DoAllRouteNumbersHaveWinner(List<Offer> offersThatAreIneligible)
        {
            if (offersThatAreIneligible.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

