using Logic;
using Microsoft.Win32;
using System.Windows;
using System.Threading;

namespace View
{
    public class MainWindowViewModel
    {
        IOController iOController;
        SelectionController selectionController;
        OpenFileDialog openFileDialog;

        public bool ImportDone { get; set; }
        public bool SelectionDone { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindowViewModel()
        {
            iOController = new IOController();
            selectionController = new SelectionController();
            ImportDone = false;
        }
        
        /// <summary>
        /// Calls the IOController to handle the importing of data from the two selected files.
        /// </summary>
        public void ImportCSV(string masterDataFilepath, string routeNumberFilepath)
        {
            iOController.InitializeImport(masterDataFilepath, routeNumberFilepath);
        }
        
        /// <summary>
        /// Enables you to choose a CSV file on your pc that this method will return.
        /// Multiselecting files has been disabled to ensure you only select one file, as this program only processes 1 file in that particular slot.
        /// </summary>
        public string ChooseCSVFile()
        {
            string filename = "Ingen fil er valgt";
            openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "CVS filer (*.csv)|*.csv|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                filename = openFileDialog.FileName;
                return filename;
            }
            return filename;
        }
        
        /// <summary>
        /// Saves the result as a CSV file with the information required to calling a company, if a winner has been found.
        /// </summary>
        public void SaveCSVCallFile()
        {
            if (SelectionDone == true)
            {
                SaveFileDialog saveDlg = new SaveFileDialog();

                saveDlg.Filter = "CSV filer (*.csv)|*.csv|All files (*.*)|*.*";
                saveDlg.InitialDirectory = @"C:\%USERNAME%\";
                saveDlg.ShowDialog();

                string path = saveDlg.FileName;
                iOController.InitializeExportToCallingList(path);
                MessageBox.Show("Filen er gemt.");
            }
            else
            {
                MessageBox.Show("Du har ikke udvalgt vinderne endnu.. Kør Udvælgelse først!");
            }
        }
        
        /// <summary>
        /// Saves the result as a CSV file and in a format used for publishing, if a winner has been found.
        /// </summary>
        public void SaveCSVPublishFile()
        {
            if (SelectionDone == true)
            {
                SaveFileDialog saveDlg = new SaveFileDialog();

                saveDlg.Filter = "CSV filer (*.csv)|*.csv|All files (*.*)|*.*";
                saveDlg.InitialDirectory = @"C:\%USERNAME%\";
                saveDlg.ShowDialog();

                string path = saveDlg.FileName;

                iOController.InitializeExportToPublishList(path);
                MessageBox.Show("Filen er gemt.");
            }
            else
            {
                MessageBox.Show("Du har ikke udvalgt vinderne endnu.. Kør Udvælgelse først!");
            }
        }
        
        public void InitializeSelection()
        {
            if (ImportDone)
            {
                SelectionDone = true;
            }
            else
            {
                MessageBox.Show("Du skal importere filerne først.");
            }
            selectionController.SelectWinners();
        }
    }
}
