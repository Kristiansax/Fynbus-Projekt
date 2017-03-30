﻿using System;
using System.ComponentModel;
using System.Windows;
using Domain;
using System.Linq;
using System.Collections.Generic;

namespace View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnMasterDataFilePathSelect_Click(object sender, RoutedEventArgs e)
        {
            txtBoxFilePathMasterData.Text = mainWindowViewModel.ChooseCSVFile();
        }
        private void BtnRouteNumberFilePathSelect_Click(object sender, RoutedEventArgs e)
        {
            txtBoxFilePathRouteNumberOffer.Text = mainWindowViewModel.ChooseCSVFile();
        }
        
        /// <summary>
        /// If one or both of the textboxes are empty, it will give you a message box that tells you to import the remaining file(s).
        /// If both textboxes have something in them, it will try to import the files. 
        /// If the import is succesfull, it will tell you so, and then ready the data for sorting/calculation.
        /// </summary>
        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtBoxFilePathMasterData.Text.ToString().Equals("") || txtBoxFilePathRouteNumberOffer.Text.ToString().Equals(""))
                {
                    MessageBox.Show("Vælg venligst begge filer inden import startes");
                }
                else if ((txtBoxFilePathMasterData.Text.ToString().Equals("") && txtBoxFilePathRouteNumberOffer.Text.ToString().Equals("")))
                {
                    MessageBox.Show("Vælg venligst filerne inden import startes");
                }
                else
                {
                    mainWindowViewModel.ImportCSV(txtBoxFilePathMasterData.Text.ToString(), txtBoxFilePathRouteNumberOffer.Text.ToString());
                    MessageBox.Show("Filerne er nu importeret");
                    mainWindowViewModel.ImportDone = true;
                }
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }

        }
        
        /// <summary>
        /// When "Udvælgelse" is clicked, the SelectionController adds the winners to our outputlist.
        /// We then sort the outputlist and counts the amount of won offers, and then shows them sorted by user id.
        /// </summary>
        private void BtnStartSelection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mainWindowViewModel.InitializeSelection();
                ListContainer listContainer = ListContainer.GetInstance();
                List<Offer> outputListByUserID = listContainer.outputList.OrderBy(x => x.UserID).ToList();
                listView.ItemsSource = outputListByUserID;
                foreach(Offer offer in listContainer.outputList)
                {
                    offer.Contractor.CountNumberOfWonOffersOfEachType(listContainer.outputList); 
                }
                MessageBox.Show("Udvælgelsen er nu færdig");
            }
            catch (Exception x)
            {
                PromptWindow promptWindow = new PromptWindow(x.Message);
                promptWindow.Show();
            }
        }
      
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Environment.Exit(1);
        }

        private void btnSavePublic_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mainWindowViewModel.SaveCSVPublishFile();
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }
        }

        private void btnSaveCall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mainWindowViewModel.SaveCSVCallFile();
            }          
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }
        }
    }
}
