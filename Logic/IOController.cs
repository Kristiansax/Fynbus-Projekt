using System.Collections.Generic;
using Domain;
using DataAccess;

namespace Logic
{
    public class IOController
    {
        List<RouteNumber> routeNumberList;
        List<Contractor> contractorList;

        public IOController()
        {
            routeNumberList = new List<RouteNumber>();
        }
       
        // Export list of winners to file (see CSVExportToPublicList in DataAccess for more info)
        public void InitializeExportToPublishList(string filePath)
        {
            CSVExportToPublishList ExportToPublishList = new CSVExportToPublishList(filePath);
            ExportToPublishList.CreateFile(); 
        }

        // Export list of winners to file with contact info (see CSVExportToCallList in DataAccess for more info)
        public void InitializeExportToCallingList(string filePath)
        {
            CSVExportToCallList ExportCallList = new CSVExportToCallList(filePath);
            ExportCallList.CreateFile();
        }

        // Import information from file with offers and file with routes (see CSVImport in DataAccess for more info)
        public void InitializeImport(string masterDataFilepath, string routeNumberFilepath)
        {
            CSVImport csvImport = new CSVImport();
            csvImport.ImportContractors(masterDataFilepath);
            csvImport.ImportRouteNumbers();
            csvImport.ImportOffers(routeNumberFilepath);
            contractorList = csvImport.SendContractorListToContainer();
            routeNumberList = csvImport.SendRouteNumberListToContainer();
            ListContainer listContainer = ListContainer.GetInstance();
            listContainer.GetLists(routeNumberList, contractorList);
        }
    }
}
