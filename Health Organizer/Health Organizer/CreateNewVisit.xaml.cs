﻿using Health_Organizer.Common;
using SQLiteWinRT;
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
using Health_Organizer.Database_Connet_Classes;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.ObjectModel;


namespace Health_Organizer
{
    public sealed partial class CreateNewVisit : Page
    {

        private NavigationHelper navigationHelper;
        private DBConnect connection;
        private string decodedImage = null;
        private Database database;
        private int PID = 0;
        ObservableCollection<string> ocString;
        private bool isUpdating = false;
        Boolean check = true;
        int counterComma = 0;
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public CreateNewVisit()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            this.ocString = new ObservableCollection<string>();

            VisitListBox.ItemsSource = this.ocString;
            this.InitializeVisitDetialsComboBox();
        }

        private async void InitializeDB(int pid)
        {
            this.connection = new DBConnect();
            await this.connection.InitializeDatabase(DBConnect.ORG_HOME_DB);
            database = this.connection.GetConnection();

            string query = "SELECT * FROM MedicalDetails WHERE PID = @pid";
            Statement statement = await this.database.PrepareStatementAsync(query);
            statement.BindIntParameterWithName("@pid", pid);
            statement.EnableColumnsProperty();
            while (await statement.StepAsync())
            {
                //Debug.WriteLine(statement.Columns["DateVisited"]);
                this.ocString.Add(statement.Columns["DateVisited"]);
            }

            this.loadPatientDetails(pid);
            if (this.ocString.Count() > 0)
            {
                VisitListBox.SelectedIndex = 0;
            }
        }

        private async void loadPatientDetails(int pid)
        {
            string q = "SELECT * FROM Patient WHERE PID = @pid";
            Statement s = await this.database.PrepareStatementAsync(q);
            s.BindIntParameterWithName("@pid", pid);
            s.EnableColumnsProperty();
            if (await s.StepAsync())
            {
                //Debug.WriteLine(s.Columns["PID"]);
                VisitPatientName.Text = s.Columns["FirstName"] + " " + s.Columns["LastName"];
                VisitPatientPhoto.Source = await ImageMethods.Base64StringToBitmap(s.Columns["Image"]);
            }
        }

        private void InitializeVisitDetialsComboBox()
        {
            this.ClearAllFields();
            //Adding days and years to combobox in form
            for (int i = 0; i < 31; i++)
            {
                VisitDayComboBox.Items.Add(i + 1);
            }

            for (int i = 2000; i <= DateTime.Now.Year; i++)
            {
                VisitYearComboBox.Items.Add(i);
            }

            //Set current date in form
            VisitDayComboBox.SelectedItem = DateTime.Now.Day;
            VisitMonthComboBox.SelectedIndex = DateTime.Now.Month - 1;
            VisitYearComboBox.SelectedItem = DateTime.Now.Year;
        }


        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
            this.PID = Int32.Parse(e.Parameter as string);
            this.InitializeDB(this.PID);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void ClearAllFields()
        {
            VisitSymptoms.Text = "";
            VisitDiseasesDiagnosed.Text = "";
            VisitMedicineGiven.Text = "";
            VisitBloodGlucose.Text = "";
            VisitSystolicBP.Text = "";
            VisitDiastolicBP.Text = "";
            VisitVaccine.Text = "";
            VisitHeightFeet.SelectedItem = null;
            VisitHeightInch.SelectedItem = null;
            VisitWeight.Text = "";
            isUpdating = false;
        }

        private void AddVisitClicked(object sender, RoutedEventArgs e)
        {
            VisitFormCmdbar.IsOpen = false;
            VisitFormBar.IsOpen = true;
            counterComma = 0;
        }

        private async void EditVisitClicked(object sender, RoutedEventArgs e)
        {
            counterComma = 0;
            VisitFormCmdbar.IsOpen = false;
            if (VisitListBox.SelectedItem != null)
            {
                VisitFormBar.IsOpen = true;
                string query = "SELECT * FROM MedicalDetails WHERE PID = @pid AND DateVisited = @dv";
                Statement statement = await this.database.PrepareStatementAsync(query);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@dv", VisitListBox.Items[VisitListBox.SelectedIndex].ToString());
                statement.EnableColumnsProperty();
                if (await statement.StepAsync())
                {
                    Debug.WriteLine(statement.Columns["DiastolicBP"] + statement.Columns["PID"]);
                    string[] dv = statement.Columns["DateVisited"].Split('-');
                    string[] height = statement.Columns["Height"].ToString().Split('.');

                    double totalInchHeight = Convert.ToDouble(statement.Columns["Height"]) * 39.3701;
                    double feetHeight = totalInchHeight % 12;
                    double inchHeight = totalInchHeight - feetHeight * 12;

                    VisitDayComboBox.SelectedIndex = VisitDayComboBox.Items.IndexOf(Int32.Parse(dv[2]));
                    VisitMonthComboBox.SelectedIndex = VisitMonthComboBox.Items.IndexOf(dv[1]);
                    VisitYearComboBox.SelectedIndex = VisitYearComboBox.Items.IndexOf(Int32.Parse(dv[0]));
                    VisitSymptoms.Text = statement.Columns["Symptoms"];
                    VisitDiseasesDiagnosed.Text = statement.Columns["DiseaseFound"];

                    int itemFeetHeight = Convert.ToInt32(Math.Round(feetHeight));
                    int itemInchHeight = Convert.ToInt32(Math.Round(inchHeight));

                    if (itemInchHeight == 12)
                    {
                        itemFeetHeight += 1;
                        itemInchHeight = 0;
                    }

                    VisitHeightFeet.SelectedIndex = VisitHeightFeet.Items.IndexOf(itemFeetHeight.ToString());
                    VisitHeightInch.SelectedIndex = VisitHeightInch.Items.IndexOf(itemInchHeight.ToString());
                    VisitWeight.Text = statement.Columns["Weight"];
                    VisitSystolicBP.Text = statement.Columns["SystolicBP"];
                    VisitDiastolicBP.Text = statement.Columns["DiastolicBP"];
                    VisitBloodGlucose.Text = statement.Columns["BloodGlucose"];
                }

                statement.Reset();
                VisitMedicineGiven.IsEnabled = false;
                query = "SELECT Medicine FROM MedicalDetailsMedicine WHERE PID = @pid AND DateVisited = @dv";
                statement = await this.database.PrepareStatementAsync(query);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@dv", VisitListBox.Items[VisitListBox.SelectedIndex].ToString());
                statement.EnableColumnsProperty();
                VisitMedicineGiven.Text = "";
                while (await statement.StepAsync())
                {
                    //Debug.WriteLine(statement.Columns["Medicine"]);
                    VisitMedicineGiven.Text += statement.Columns["Medicine"] + ",";
                }

                statement.Reset();
                VisitVaccine.IsEnabled = false;
                query = "SELECT Vaccine FROM MedicalDetailsVaccine WHERE PID = @pid AND DateVisited = @dv";
                statement = await this.database.PrepareStatementAsync(query);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@dv", VisitListBox.Items[VisitListBox.SelectedIndex].ToString());
                statement.EnableColumnsProperty();
                VisitVaccine.Text = "";
                while (await statement.StepAsync())
                {
                    //Debug.WriteLine(statement.Columns["Vaccine"]);
                    VisitVaccine.Text += statement.Columns["Vaccine"] + ",";                  
                }

                isUpdating = true;
            }
        }

        private async void DeleteVisitClicked(object sender, RoutedEventArgs e)
        {
            VisitFormCmdbar.IsOpen = false;
            if (VisitListBox.SelectedItem != null)
            {
                string DateVisited = VisitListBox.Items[VisitListBox.SelectedIndex].ToString();
                string deleteQuery = "DELETE FROM MedicalDetails WHERE PID = @pid AND DateVisited = @dv";
                Statement statement = await this.database.PrepareStatementAsync(deleteQuery);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@dv", DateVisited);
                await statement.StepAsync();
                this.ocString.Remove(DateVisited);

                statement.Reset();
                deleteQuery = "DELETE FROM MedicalDetailsVaccine WHERE PID = @pid AND DateVisited = @dv";
                statement = await this.database.PrepareStatementAsync(deleteQuery);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@dv", DateVisited);
                while (await statement.StepAsync())
                {
                    Debug.WriteLine(statement.Columns["Vaccine"]);
                }

                statement.Reset();
                deleteQuery = "DELETE FROM MedicalDetailsMedicine WHERE PID = @pid AND DateVisited = @dv";
                statement = await this.database.PrepareStatementAsync(deleteQuery);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@dv", DateVisited);
                while (await statement.StepAsync())
                {
                    Debug.WriteLine(statement.Columns["Medicine"]);
                }
            }
        }

        private async void VisitSaveClicked(object sender, RoutedEventArgs e)
        {
            check = false;
            if (await this.CheckIfFilled())
            {
                try
                {
                    if (isUpdating)
                    {
                        await this.UpdateDetails();
                    }
                    else
                    {
                        await this.InsertDetails();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                VisitFormBar.IsOpen = false;
            }
            else
            {
                Debug.WriteLine(check);
                if (check && !isUpdating)
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog("Please complete the form before saving it.", "Error!");
                    messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("Okay", null));
                    var dialogResult = await messageDialog.ShowAsync();
                }
            }
        }

        private async Task<int> UpdateDetails()
        {
            double height = ((VisitHeightFeet.SelectedIndex + 1)*12 + VisitHeightInch.SelectedIndex) * 0.0254;
            int weight = Int32.Parse(VisitWeight.Text.ToString());
            string DateVisited = VisitYearComboBox.Items[VisitYearComboBox.SelectedIndex].ToString() + "-" + VisitMonthComboBox.Items[VisitMonthComboBox.SelectedIndex].ToString() + "-" + VisitDayComboBox.Items[VisitDayComboBox.SelectedIndex].ToString();
            double bmi = ExtraModules.CalculateBMI(VisitHeightFeet.SelectedIndex + 1, VisitHeightInch.SelectedIndex, weight);

            string updateQuery = "UPDATE MedicalDetails SET BloodGlucose = @bg , SystolicBP = @sbp , DiastolicBP = @dbp , DiseaseFound = @disease , Height = @height , Weight = @weight , Symptoms = @symptoms , BMI = @bmi  WHERE PID = @pid AND DateVisited = @dv";
            Statement statement = await this.database.PrepareStatementAsync(updateQuery);
            statement.BindIntParameterWithName("@pid", this.PID);
            statement.BindTextParameterWithName("@dv", DateVisited);
            statement.BindIntParameterWithName("@bg", Int32.Parse(VisitBloodGlucose.Text.ToString()));
            statement.BindIntParameterWithName("@dbp", Int32.Parse(VisitDiastolicBP.Text.ToString()));
            statement.BindIntParameterWithName("@sbp", Int32.Parse(VisitSystolicBP.Text.ToString()));
            statement.BindTextParameterWithName("@disease", VisitDiseasesDiagnosed.Text.ToString());
            statement.BindDoubleParameterWithName("@height", height);
            statement.BindIntParameterWithName("@weight", weight);
            statement.BindTextParameterWithName("@symptoms", VisitSymptoms.Text.ToString());
            statement.BindDoubleParameterWithName("@bmi", bmi);

            await statement.StepAsync();

            //statement.Reset();
            //string insertBMI = "UPDATE MedicalDetailsBMI SET BMI = @bmi WHERE Height = @height AND Weight = @weight";
            //statement = await this.database.PrepareStatementAsync(insertBMI);
            //statement.BindDoubleParameterWithName("@height", height);
            //statement.BindIntParameterWithName("@weight", weight);
            //statement.BindDoubleParameterWithName("@bmi", bmi);

            //await statement.StepAsync();

            //statement.Reset();
            //string updateMedicine = "UPDATE MedicalDetailsMedicine SET Medicine = @medicine WHERE PID = @pid AND DateVisited = @dv";

            //foreach (string str in VisitMedicineGiven.Text.Split(','))
            //{
            //    statement = await this.database.PrepareStatementAsync(updateMedicine);
            //    statement.BindIntParameterWithName("@pid", this.PID);
            //    statement.BindTextParameterWithName("@dv", DateVisited);
            //    statement.BindTextParameterWithName("@medicine", str);

            //    await statement.StepAsync();
            //    statement.Reset();
            //}

            //string updateVaccine = "UPDATE MedicalDetailsVaccine SET Vaccine = @vaccine WHERE PID = @pid AND DateVisited = @dv";
            //foreach (string str in VisitVaccine.Text.Split(','))
            //{
            //    statement = await this.database.PrepareStatementAsync(updateVaccine);
            //    statement.BindIntParameterWithName("@pid", this.PID);
            //    statement.BindTextParameterWithName("@dv", DateVisited);
            //    statement.BindTextParameterWithName("@vaccine", str);

            //    await statement.StepAsync();
            //    statement.Reset();
            //}
            this.ClearAllFields();
            isUpdating = false;
            VisitMedicineGiven.IsEnabled = true;
            VisitVaccine.IsEnabled = true;
            this.UpdateEditedDetails();

            return 1;
        }

        private async Task<int> InsertDetails()
        {
            double height = ((VisitHeightFeet.SelectedIndex + 1) * 12 + VisitHeightInch.SelectedIndex) * 0.0254;
            int weight = Int32.Parse(VisitWeight.Text.ToString());
            string DateVisited = VisitYearComboBox.Items[VisitYearComboBox.SelectedIndex].ToString() + "-" + VisitMonthComboBox.Items[VisitMonthComboBox.SelectedIndex].ToString() + "-" + VisitDayComboBox.Items[VisitDayComboBox.SelectedIndex].ToString();
            double bmi = ExtraModules.CalculateBMI(VisitHeightFeet.SelectedIndex + 1, VisitHeightInch.SelectedIndex, weight);

            string insertQuery = "INSERT INTO MedicalDetails (PID, DateVisited, Age, BloodGlucose, SystolicBP, DiastolicBP, DiseaseFound, Height, Weight, Symptoms, BMI) " +
                                 "VALUES (@pid, @dv, @age, @bg, @sbp, @dbp, @disease, @height, @weight, @symptoms, @bmi)";
            Statement statement = await this.database.PrepareStatementAsync(insertQuery);
            statement.BindIntParameterWithName("@pid", this.PID);
            statement.BindTextParameterWithName("@dv", DateVisited);
            statement.BindIntParameterWithName("@age", await this.GetPatientAge(this.PID));
            statement.BindIntParameterWithName("@bg", Int32.Parse(VisitBloodGlucose.Text.ToString()));
            if (!VisitDiastolicBP.Text.ToString().Equals(""))
            {
                statement.BindIntParameterWithName("@dbp", Int32.Parse(VisitDiastolicBP.Text.ToString()));
            }
            if (!VisitSystolicBP.Text.ToString().Equals(""))
            {
                statement.BindIntParameterWithName("@sbp", Int32.Parse(VisitSystolicBP.Text.ToString()));
            }
            statement.BindTextParameterWithName("@disease", VisitDiseasesDiagnosed.Text.ToString());
            statement.BindDoubleParameterWithName("@height", height);
            statement.BindIntParameterWithName("@weight", weight);
            statement.BindTextParameterWithName("@symptoms", VisitSymptoms.Text.ToString());
            statement.BindDoubleParameterWithName("@bmi", bmi);

            await statement.StepAsync();

            //statement.Reset();
            //double bmi = 1.0 * height / weight;
            //string insertBMI = "INSERT INTO MedicalDetailsBMI (Height, Weight, BMI) VALUES (@height, @weight, @bmi)";
            //statement = await this.database.PrepareStatementAsync(insertBMI);
            //statement.BindDoubleParameterWithName("@height", height);
            //statement.BindIntParameterWithName("@weight", weight);
            //statement.BindDoubleParameterWithName("@bmi", bmi);
            //await statement.StepAsync();

            statement.Reset();
            string insertMedicine = "INSERT INTO MedicalDetailsMedicine (PID, DateVisited, Medicine) VALUES (@pid, @dv, @medicine)";

            foreach (string str in VisitMedicineGiven.Text.Split(','))
            {
                statement = await this.database.PrepareStatementAsync(insertMedicine);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@dv", DateVisited);
                statement.BindTextParameterWithName("@medicine", str);

                await statement.StepAsync();
                statement.Reset();
            }

            string insertVaccine = "INSERT INTO MedicalDetailsVaccine (PID, DateVisited, Vaccine) VALUES (@pid, @dv, @vaccine)";

            foreach (string str in VisitVaccine.Text.Split(','))
            {
                statement = await this.database.PrepareStatementAsync(insertVaccine);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@dv", DateVisited);
                statement.BindTextParameterWithName("@vaccine", str);

                await statement.StepAsync();
                statement.Reset();
            }
            this.ocString.Add(DateVisited);
            this.ClearAllFields();
            VisitListBox.SelectedIndex = this.ocString.IndexOf(DateVisited);

            return 1;
        }

        private async Task<int> GetPatientAge(int p)
        {
            string query = "SELECT Birthday FROM Patient WHERE PID = @pid";
            Statement statement = await this.database.PrepareStatementAsync(query);
            statement.BindIntParameterWithName("@pid", p);
            statement.EnableColumnsProperty();
            if (await statement.StepAsync())
            {
                Debug.WriteLine(Int32.Parse(statement.Columns["Birthday"].Split('-')[0]));
                int age = DateTime.Now.Year - Int32.Parse(statement.GetTextAt(0).Split('-')[0]);
                return age;
            }
            return -1;
        }

        private async Task<bool> CheckIfFilled()
        {
            VisitDiseasesDiagnosed.ClearValue(BorderBrushProperty);
            VisitSymptoms.ClearValue(BorderBrushProperty);
            VisitMedicineGiven.ClearValue(BorderBrushProperty);
            VisitWeight.ClearValue(BorderBrushProperty);
            VisitHeightFeet.ClearValue(BorderBrushProperty);
            VisitHeightInch.ClearValue(BorderBrushProperty);
            VisitMonthComboBox.ClearValue(BorderBrushProperty);
            VisitDayComboBox.ClearValue(BorderBrushProperty);
            VisitYearComboBox.ClearValue(BorderBrushProperty);

            if (VisitDiseasesDiagnosed.Text.Equals("") || VisitSymptoms.Text.Equals("") ||
                VisitMedicineGiven.Text.Equals("") || VisitWeight.Text.Equals("") || VisitHeightFeet.SelectedItem == null || VisitHeightInch.SelectedItem == null ||
                ((ocString.Contains(VisitYearComboBox.SelectedItem + "-" + VisitMonthComboBox.SelectedItem + "-" + VisitDayComboBox.SelectedItem) && !isUpdating)))
            {
                if ((ocString.Contains(VisitYearComboBox.Items[VisitYearComboBox.SelectedIndex] + "-" + VisitMonthComboBox.Items[VisitMonthComboBox.SelectedIndex] + "-" + VisitDayComboBox.Items[VisitDayComboBox.SelectedIndex])))
                {
                    check = false;
                    var messageDialog = new Windows.UI.Popups.MessageDialog("You cannot select the same date again.", "Error!");
                    messageDialog.Commands.Add(new Windows.UI.Popups.UICommand("Okay", null));
                    var dialogResult = await messageDialog.ShowAsync();
                    VisitDayComboBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                    VisitYearComboBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                    VisitMonthComboBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);

                    return false;
                }
                else
                {
                    check = true;
                }
                if (VisitDiseasesDiagnosed.Text.Equals(""))
                {
                    VisitDiseasesDiagnosed.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (VisitSymptoms.Text.Equals(""))
                {
                    VisitSymptoms.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (VisitMedicineGiven.Text.Equals(""))
                {
                    VisitMedicineGiven.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (VisitWeight.Text.Equals(""))
                {
                    VisitWeight.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                if (VisitHeightFeet.SelectedItem == null)
                {
                    VisitHeightFeet.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);

                }
                if (VisitHeightInch.SelectedItem == null)
                {
                    VisitHeightInch.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        private void VisitCancelClicked(object sender, RoutedEventArgs e)
        {
            this.ClearAllFields();
            VisitMedicineGiven.IsEnabled = true;
            VisitVaccine.IsEnabled = true;
            VisitFormBar.IsOpen = false;
        }

        private void visitSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count() < 0)
                return;


            this.UpdateEditedDetails();
        }

        private async void UpdateEditedDetails()
        {
            if (VisitListBox.SelectedItem != null)
            {
                string query = "SELECT * FROM MedicalDetails WHERE PID = @pid AND DateVisited = @dv";
                Statement statement = await this.database.PrepareStatementAsync(query);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@dv", VisitListBox.Items[VisitListBox.SelectedIndex].ToString());
                statement.EnableColumnsProperty();
                if (await statement.StepAsync())
                {

                    VisitSymptomsPanel.Children.Clear(); 

                    foreach (string str in statement.Columns["Symptoms"].Split(','))
                    {   
                        StackPanel VisitSymptomsStackPanels = new StackPanel();
                        VisitSymptomsStackPanels.Margin = new Thickness(0, 15, 0, 0);
                        VisitSymptomsStackPanels.Orientation = Orientation.Horizontal;

                        TextBlock dot = new TextBlock();
                        dot.Width = 10;
                        dot.FontSize = 15;
                        dot.Text = "•";
                        VisitSymptomsStackPanels.Children.Add(dot);
                        
                        TextBlock vaccineName = new TextBlock();
                        vaccineName.Width = 280;
                        vaccineName.Text = ExtraModules.RemoveStringSpace(str);
                        vaccineName.TextWrapping = TextWrapping.Wrap;
                        vaccineName.FontSize = 15;
                        VisitSymptomsStackPanels.Children.Add(vaccineName);
                        
                        VisitSymptomsPanel.Children.Add(VisitSymptomsStackPanels);
                    }

                    VisitTextDisease.Text = "\n" + statement.Columns["DiseaseFound"];
                    VisitTextBG.Text = statement.Columns["BloodGlucose"];
                    VisitTextBP.Text = statement.Columns["SystolicBP"] + "/" + statement.Columns["DiastolicBP"];

                    double BMIDouble = Convert.ToDouble(statement.Columns["BMI"]);
                    Double BMIRounded3 = Math.Round(BMIDouble, 3);
                    VisitTextBMI.Text = BMIRounded3.ToString();
                    
                    VisitTextWeight.Text = statement.Columns["Weight"];
                    Debug.WriteLine(statement.Columns["Height"]);
                    VisitTextHeight.Text = statement.Columns["Height"];
                }

                statement.Reset();
                query = "SELECT * FROM MedicalDetailsMedicine WHERE PID = @pid AND DateVisited = @dv";
                statement = await this.database.PrepareStatementAsync(query);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@dv", VisitListBox.Items[VisitListBox.SelectedIndex].ToString());
                statement.EnableColumnsProperty();

                VisitTextMedicines.Children.Clear();
                while (await statement.StepAsync())
                {
                    StackPanel VisitMedicineStackPanels = new StackPanel();
                    VisitMedicineStackPanels.Margin = new Thickness(0, 15, 0, 0);
                    VisitMedicineStackPanels.Orientation = Orientation.Horizontal;

                    TextBlock dot = new TextBlock();
                    dot.Width = 10;
                    dot.FontSize = 15;
                    dot.Text = "•";
                    VisitMedicineStackPanels.Children.Add(dot);

                    TextBlock medicineName = new TextBlock();
                    medicineName.Width = 280;
                    medicineName.Text = ExtraModules.RemoveStringSpace(statement.Columns["Medicine"]);
                    medicineName.TextWrapping = TextWrapping.Wrap;
                    medicineName.FontSize = 15;
                    VisitMedicineStackPanels.Children.Add(medicineName);

                    VisitTextMedicines.Children.Add(VisitMedicineStackPanels);
                }

                statement.Reset();
                query = "SELECT * FROM MedicalDetailsVaccine WHERE PID = @pid AND DateVisited = @dv";
                statement = await this.database.PrepareStatementAsync(query);
                statement.BindIntParameterWithName("@pid", this.PID);
                statement.BindTextParameterWithName("@dv", VisitListBox.Items[VisitListBox.SelectedIndex].ToString());
                statement.EnableColumnsProperty();

                VisitTextVaccine.Children.Clear();
                while (await statement.StepAsync())
                {
                    StackPanel VisitVaccineStackPanels = new StackPanel();
                    VisitVaccineStackPanels.Margin = new Thickness(0, 15, 0, 0);
                    VisitVaccineStackPanels.Orientation = Orientation.Horizontal;

                    TextBlock dot = new TextBlock();
                    dot.Width = 10;
                    dot.FontSize = 15;
                    dot.Text = "•";
                    VisitVaccineStackPanels.Children.Add(dot);

                    TextBlock VaccineName = new TextBlock();
                    VaccineName.Width = 280;
                    VaccineName.Text = ExtraModules.RemoveStringSpace(statement.Columns["Vaccine"]);
                    VaccineName.TextWrapping = TextWrapping.Wrap;
                    VaccineName.FontSize = 15;
                    VisitVaccineStackPanels.Children.Add(VaccineName);

                    VisitTextVaccine.Children.Add(VisitVaccineStackPanels);
                }
            }
        }
        //Validation for numeric entries in weight, bp and glucose
        private void numberValidation_decimal(object sender, KeyRoutedEventArgs e)
        {
            if (((uint)e.Key >= (uint)Windows.System.VirtualKey.Number0
          && (uint)e.Key <= (uint)Windows.System.VirtualKey.Number9) || ((uint)e.Key >= (uint)Windows.System.VirtualKey.NumberPad0 && (uint)e.Key <= (uint)Windows.System.VirtualKey.NumberPad9) || (uint)e.Key == (uint)Windows.System.VirtualKey.Tab || (uint)e.Key == (uint)Windows.System.VirtualKey.Decimal)
            {
                e.Handled = false;
            }
            else e.Handled = true;
        }
        private void numberValidation_integer(object sender, KeyRoutedEventArgs e)
        {
            if (((uint)e.Key >= (uint)Windows.System.VirtualKey.Number0
          && (uint)e.Key <= (uint)Windows.System.VirtualKey.Number9) || ((uint)e.Key >= (uint)Windows.System.VirtualKey.NumberPad0 && (uint)e.Key <= (uint)Windows.System.VirtualKey.NumberPad9) || (uint)e.Key == (uint)Windows.System.VirtualKey.Tab)
            {
                e.Handled = false;
            }
            else e.Handled = true;
        }
        private void commaKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (counterComma >= 1 && ((uint)e.Key == 188))
            {
                e.Handled = true;
            }
            else
            {
                e.Handled = false;
            }

            if (((uint)e.Key == 188))
            {
                counterComma++;
            }
            else
            {
                counterComma = 0;
            }

        }

        private void ViewProfileClicked(object sender, RoutedEventArgs e)
        {
            if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(ProfileDetailsPage), this.PID.ToString());
            }
        }

    }
}