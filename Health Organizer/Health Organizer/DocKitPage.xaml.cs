﻿using Health_Organizer.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using Health_Organizer.Data_Model_Classes;
using Health_Organizer.Database_Connet_Classes;
using Health_Organizer.DML_Method_Classes;
using System.Threading.Tasks;
using SQLite;
using Windows.UI.Popups;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Health_Organizer
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class DocKitPage : Page
    {
        DiseasesTable diseaseMethods;
        FirstAidTable firstAidMethods;
        DBConnect connect;
        private bool isUpdating = false, isDiseaseSelected = true;
        private string decodedImage = null;
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        
        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public DocKitPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }
        
        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await this.InitializeDB();
            docKitProgress.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitProgress.IsActive = false;
            docKitCombo.SelectedIndex = 0;
        }

        private async Task InitializeDB()
        {
            connect = new DBConnect();
            await connect.InitializeDatabase();
            diseaseMethods = new DiseasesTable(connect);
            firstAidMethods = new FirstAidTable(connect);
        }

        private void docKitSearchBut(object sender, RoutedEventArgs e)
        {
            if (docKitSearchBox.Visibility != Windows.UI.Xaml.Visibility.Visible)
            {
                docKitSearchBox.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                docKitSearchBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private void docKitComboBox(object sender, SelectionChangedEventArgs e)
        {
            if (docKitCombo.SelectedIndex == 0){
                pageTitle.Text = "Disease List";
                docKitScrollerFirstAid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                docKitScrollerDisease.Visibility = Windows.UI.Xaml.Visibility.Visible;
                isDiseaseSelected = true;
                this.UpdateDiseaseListBox();
            }
            else if (docKitCombo.SelectedIndex == 1)
            {
                pageTitle.Text = "First Aid List";
                docKitScrollerDisease.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                docKitScrollerFirstAid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                isDiseaseSelected = false;
                this.UpdateFirstAidListBox();
            }
        }

/////////////////////This methods are for Buttons in CommandBar at the bottom
        private void docKitAddItem(object sender, RoutedEventArgs e)
        {
            if (isDiseaseSelected)
            {
                docKitDialog.IsOpen = true;
            }
            else 
            {
                docKitDialogFirstAid.IsOpen = true;
            }
            docKitCmdbar.IsOpen = false;
        }

        private async void docKitEditItem(object sender, RoutedEventArgs e)
        {
            docKitCmdbar.IsOpen = false;

            ListBoxItem xItem = docKitListBox.SelectedItem as ListBoxItem;
            if (xItem != null)
            {
                //Load all the values from the DB. Assertion: Value exist in DB since loaded from list.Also set PK to ReadOnly.
                if (isDiseaseSelected)
                {
                    docKitDialog.IsOpen = true;
                    BasicDiseases tempDisease = await diseaseMethods.FindSingleDisease(xItem.Content.ToString());
                    docKitDName.Text = tempDisease.Name;
                    docKitDDescription.Text = tempDisease.Description;
                    docKitDSymptoms.Text = tempDisease.Symptoms;
                    docKitDImage.Text = tempDisease.Name + ".jpg";
                    decodedImage = tempDisease.Image;
                    isUpdating = true;
                    docKitDName.IsReadOnly = true;
                }
                else 
                {
                    docKitDialogFirstAid.IsOpen = true;
                    BasicFirstAid tempFirstAid = await firstAidMethods.FindSingleFirstAid(xItem.Content.ToString());
                    docKitFAName.Text = tempFirstAid.Name;
                    docKitFADescription.Text = tempFirstAid.FirstAid;
                    docKitFASymptoms.Text = tempFirstAid.DoNot;
                    docKitFAImage.Text = tempFirstAid.Name + ".jpg";
                    decodedImage = tempFirstAid.Image;
                    isUpdating = true;
                    docKitFAName.IsReadOnly = true;
                }
            }
        }

        private async void docKitDelItem(object sender, RoutedEventArgs e)
        {
            docKitCmdbar.IsOpen = false;

            //Find the instance of the item which is selected from the DB, then delete it using that instance.
            ListBoxItem xItem = docKitListBox.SelectedItem as ListBoxItem;
            if (xItem != null)
            {
                var messageDialog = new MessageDialog("Are you sure you want to delete details for this disease?", "Confirmation");
                messageDialog.Commands.Add(new  Windows.UI.Popups.UICommand("Yes",null));
                messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("No", null));
                var dialogResult = await messageDialog.ShowAsync();

                if (dialogResult.Label.Equals("Yes"))
                {
                    if (isDiseaseSelected)
                    {
                        BasicDiseases tempDisease = await diseaseMethods.FindSingleDisease(xItem.Content.ToString());
                        await diseaseMethods.DeleteDisease(tempDisease);
                        this.UpdateDiseaseListBox();
                    }
                    else 
                    {
                        BasicFirstAid tempDisease = await firstAidMethods.FindSingleFirstAid(xItem.Content.ToString());
                        await firstAidMethods.DeleteFirstAid(tempDisease);
                        this.UpdateFirstAidListBox();
                    }
                }
            }
        }

////////////////////////This methods are for Updating the View after changes in FB
        private async void UpdateDiseaseListBox()
        {
            docKitProgress.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitProgress.IsActive = true;

            List<BasicDiseases> result = await diseaseMethods.SelectAllDisease();
                
            //This is used to sort the list on the basis of Name value pairs. Also note first we need to clear previous list.
            result.Sort(delegate(BasicDiseases c1, BasicDiseases c2)
            {
                return c1.Name.CompareTo(c2.Name);
            });
            docKitListBox.Items.Clear();

            //Load the Resource Style from themed dictionary for listboxItems
            ResourceDictionary rd = Application.Current.Resources.ThemeDictionaries["Default"] as ResourceDictionary;

            foreach(var i in result)
            {
                //Load A New Item Programatically every time set its style to one required and then display it.
                ListBoxItem xItem = new ListBoxItem();
                xItem.Content = i.Name;
                xItem.Style = rd["ListBoxItemStyle"] as Style;
                docKitListBox.Items.Add(xItem);
            }

            //Disable Edit/Delete Buttons if there are no items in the List.
            if (result.Count() > 0)
            {
                if (docKitListBox.SelectedIndex == -1)
                {
                    docKitListBox.SelectedIndex = 0;
                }

                this.showDiseaseItems();
                docKitDelBut.IsEnabled = true;
                docKitEditBut.IsEnabled = true;
            }
            else 
            {
                this.hideDiseaseItems();
                 docKitDelBut.IsEnabled = false;
                docKitEditBut.IsEnabled = false;
            }        

            docKitProgress.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitProgress.IsActive = false;
        }

        public async void UpdateFirstAidListBox()
        {
            docKitProgress.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitProgress.IsActive = true;

            List<BasicFirstAid> result = await firstAidMethods.SelectAllFirstAids();

            result.Sort(delegate(BasicFirstAid c1, BasicFirstAid c2)
            {
                return c1.Name.CompareTo(c2.Name);
            });

            docKitListBox.Items.Clear();
            ResourceDictionary rd = Application.Current.Resources.ThemeDictionaries["Default"] as ResourceDictionary;

            foreach (var i in result)
            {
                ListBoxItem xItem = new ListBoxItem();
                xItem.Content = i.Name;
                xItem.Style = rd["ListBoxItemStyle"] as Style;
                docKitListBox.Items.Add(xItem);
            }

            if (result.Count() > 0)
            {
                if (docKitListBox.SelectedIndex == -1) 
                {
                    docKitListBox.SelectedIndex = 0;
                }
                this.showFirstAidItems();
                docKitDelBut.IsEnabled = true;
                docKitEditBut.IsEnabled = true;
            }
            else
            {
                this.hideFirstAidItems();
                docKitDelBut.IsEnabled = false;
                docKitEditBut.IsEnabled = false;
            }

            docKitProgress.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitProgress.IsActive = false;
        }

        private void showDiseaseItems()
        {
            docKitSymptoms.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitDescription.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitName.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitImage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitD.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitS.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void showFirstAidItems()
        {
            docKitFirstAidSymptoms.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitFirstAidDescription.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitName.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitFirstAidImage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitFirstAidD.Visibility = Windows.UI.Xaml.Visibility.Visible;
            docKitFirstAidS.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void hideDiseaseItems()
        {
            docKitSymptoms.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitDescription.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitName.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitImage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitD.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitS.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void hideFirstAidItems()
        {
            docKitFirstAidSymptoms.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitFirstAidDescription.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitName.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitFirstAidImage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitFirstAidD.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            docKitFirstAidS.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

///////////////////This are the methods used to update the display immideately after updating
        private async void UpdateDiseaseData(BasicDiseases tempDisease)
        {
            docKitName.Text = tempDisease.Name;
            docKitDescription.Text = "\n" + tempDisease.Description;
            docKitImage.Source = await ImageMethods.Base64StringToBitmap(tempDisease.Image);

            string tempSymptoms = "";
            foreach (var i in tempDisease.Symptoms.Split(','))
            {
                tempSymptoms += "\n• " + i;
            }
            docKitSymptoms.Text = tempSymptoms;
        }

        private async void UpdateFirstAidData(BasicFirstAid tempFirstAid)
        {
            docKitName.Text = tempFirstAid.Name;
            docKitFirstAidDescription.Text = "\n" + tempFirstAid.FirstAid;
            docKitFirstAidImage.Source = await ImageMethods.Base64StringToBitmap(tempFirstAid.Image);

            string tempFirstAidString = "";
            foreach (var i in tempFirstAid.DoNot.Split(','))
            {
                tempFirstAidString += "\n• " + i;
            }
            docKitFirstAidSymptoms.Text = tempFirstAidString;
        }

//////////////////////This methods are for Buttons click events in Dialog Box opened.
        private async void docKitDialogSave(object sender, RoutedEventArgs e)
        {
            if(isDiseaseSelected)
            {
                if (docKitDName.Text.Equals("") || docKitDSymptoms.Text.Equals("") || docKitDDescription.Text.Equals("") || docKitDImage.Text.Equals(""))
                {
                    docKitErrorDescription.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    if (decodedImage != null)
                    {
                        if (isUpdating == true)
                        {
                            //Find that object's instance and change its values
                            ListBoxItem xItem = docKitListBox.SelectedItem as ListBoxItem;
                            BasicDiseases tempDisease = await diseaseMethods.FindSingleDisease(xItem.Content.ToString());
                            tempDisease.Name = docKitDName.Text;
                            tempDisease.Description = docKitDDescription.Text;
                            tempDisease.Image = decodedImage;
                            tempDisease.Symptoms = docKitDSymptoms.Text;

                            await diseaseMethods.UpdateDisease(tempDisease);
                            isUpdating = false;
                            docKitDName.IsReadOnly = false;
                            this.UpdateDiseaseData(tempDisease);
                        }
                        else
                        {
                            await diseaseMethods.InsertDisease(new BasicDiseases() { Name = docKitDName.Text, Description = docKitDDescription.Text, Symptoms = docKitDSymptoms.Text, Image = decodedImage });
                        }

                        this.UpdateDiseaseListBox();
                        ////Find the Item in the List
                        //for (int i = 0; i < docKitListBox.Items.Count(); i++)
                        //{
                        //    ListBoxItem xItem = docKitListBox.Items[i] as ListBoxItem;
                        //    if (xItem.Content.Equals(docKitDName.Text))
                        //    {
                        //        //docKitListBox.SelectedIndex = i;
                        //        docKitListBox.SelectedItem = xItem;
                        //    }
                        //}
                  }
                }
            }
            else 
            {
                if (docKitFAName.Text.Equals("") || docKitFASymptoms.Text.Equals("") || docKitFADescription.Text.Equals("") || docKitFAImage.Text.Equals(""))
                {
                    docKitErrorDescription.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    if (isUpdating == true)
                    {
                        //Find that object's instance and change its values
                        ListBoxItem xItem = docKitListBox.SelectedItem as ListBoxItem;
                        BasicFirstAid tempFirstAid = await firstAidMethods.FindSingleFirstAid(xItem.Content.ToString());
                        tempFirstAid.Name = docKitFAName.Text;
                        tempFirstAid.FirstAid = docKitFADescription.Text;
                        tempFirstAid.Image = decodedImage;
                        tempFirstAid.DoNot = docKitFASymptoms.Text;

                        await firstAidMethods.UpdateFirstAid(tempFirstAid);
                        isUpdating = false;
                        docKitFAName.IsReadOnly = false;
                        this.UpdateFirstAidData(tempFirstAid);
                    }
                    else
                    {

                        await firstAidMethods.InsertFirstAid(new BasicFirstAid() { Name = docKitFAName.Text, FirstAid = docKitFADescription.Text, DoNot = docKitFASymptoms.Text, Image = decodedImage });
                    }
                    this.UpdateFirstAidListBox();
                }
            }
            //After everything is stored/Updated in database we need to reset all the fields.
            this.ClearFormFields();
        }

        private void docKitDialogCancel(object sender, RoutedEventArgs e)
        {
            if (isDiseaseSelected)
            {
                docKitErrorDescription.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else 
            {
                docKitFAErrorDescription.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            this.ClearFormFields();
        }

        private async void docKitDialogBrowse(object sender, RoutedEventArgs e)
        {
            //This is used to Open the FilePicker Browse Menu from which we can select file
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            var file = await picker.PickSingleFileAsync();

            decodedImage = await ImageMethods.ConvertStorageFileToBase64String(file);
            if(isDiseaseSelected)
                docKitDImage.Text = file.Name;
            else
                docKitFAImage.Text = file.Name;
        }

//////////////////////////This method is for the click event in the List Box.
        private async void docKitListItemSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0)
               return;
            
            ListBoxItem xItem = docKitListBox.SelectedItem as ListBoxItem;
            
            //Check whether diseases or firstaid and then display selected Item's details
            if (isDiseaseSelected)
            {
                BasicDiseases tempDisease = await diseaseMethods.FindSingleDisease(xItem.Content.ToString());
                this.UpdateDiseaseData(tempDisease);     
            }
            else 
            {
                BasicFirstAid tempFirstAid = await firstAidMethods.FindSingleFirstAid(xItem.Content.ToString());
                this.UpdateFirstAidData(tempFirstAid);     
            }
        }
       
/////////////////////////This is used to clear all the Dialog Fields
        private void ClearFormFields()
        {
            if (docKitDialog.IsOpen == true)
                docKitDialog.IsOpen = false;
            docKitDName.Text = "";
            docKitDDescription.Text = "";
            docKitDSymptoms.Text = "";
            docKitDImage.Text = "";
            docKitDName.IsReadOnly = false;

            if(docKitDialogFirstAid.IsOpen == true)
                docKitDialogFirstAid.IsOpen = false;
            docKitFAName.Text = "";
            docKitFADescription.Text = "";
            docKitFASymptoms.Text = "";
            docKitFAImage.Text = "";
            decodedImage = null;
            docKitFAName.IsReadOnly = false;
        }
    }
}
