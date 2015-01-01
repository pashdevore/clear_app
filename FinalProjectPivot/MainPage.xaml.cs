using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Input;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Primitives;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Info;
using FinalProjectPivot.Resources;

using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Markup;
using System.Diagnostics;

namespace FinalProjectPivot
{
    public partial class MainPage : PhoneApplicationPage
    {        
        #region PropertyChanged-------------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// PropertyChanged event handler used within the ObservableCollection for updating the UI when necessary.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method for updating UI when items have changed.
        /// </summary>
        /// <param name="propertyName">The name used to keep track of a property's changes.</param>
        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler temp = PropertyChanged;
            if (temp != null)
            {
                temp(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region ToDoLists Observable Collection---------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// ObservableCollection for the ToDoLists.
        /// </summary>
        private ObservableCollection<ToDoList> toDoLists;

        /// <summary>
        /// ObservableCollection used to keep track of the ToDoLists and update them when necessary.
        /// </summary>
        public ObservableCollection<ToDoList> ToDoLists
        {
            get
            {
                return toDoLists;
            }
            set
            {
                if (toDoLists != value)
                {
                    toDoLists = value;
                    NotifyPropertyChanged("ToDoLists");
                }
            }
        }
        #endregion

        #region MainPage Contructor---------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Reference to the Database context.
        /// </summary>
        private ToDoDataContext database;

        /// <summary>
        /// Contructor for the MainPage.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
            
            //Setup of Touch.FrameReported Events
            Touch.FrameReported += Touch_FrameReported;
            sideMenuActive = false;
            editingToDo = false;
            
            //DB setup and DataContext
            database = App.Database;
            this.DataContext = this;
        }
        #endregion

        #region Navigation Overrides--------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Sets the Tile for the app to display the total number of ToDo items upon exiting the app.
        /// </summary>
        /// <param name="e">Data from the Navigation event.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            int total = TotalToDoItems();
          
            ShellTile.ActiveTiles.First().Update(new IconicTileData()
            {
                Title = "ToDo",
                Count = total,
                WideContent1 = "Tasks Made Simple",
                WideContent2 = "CSCI 152",
                WideContent3 = "Pash DeVore",
                BackgroundColor = new Color { A = 255, R = 0, G = 148, B = 255 }
            });   
        }

        /// <summary>
        /// Sets the initial ToDo list the first time the app is run. Also sets up the color
        /// scheme from the database when entering the app.
        /// </summary>
        /// <param name="e">Data from the Navigation event.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ToDoLists = new ObservableCollection<ToDoList>(from ToDoList tdl in database.ToDoLists select tdl);
            if (ToDoLists.Count == 0)
            {
                ToDoList list = new ToDoList();
                list.PivotIndex = ToDoPivot.Items.Count;
                list.Header = "First";

                ToDoItem first = new ToDoItem();
                first.Content = "Slide right to check off items";
                first.PivotIndex = 0;
                first.Index = 0;

                ToDoItem second = new ToDoItem();
                second.Content = "Slide left to remove items";
                second.PivotIndex = 0;
                second.Index = 1;

                ToDoItem third = new ToDoItem();
                third.Content = "See menu below to add lists and items";
                third.PivotIndex = 0;
                third.Index = 2;

                list.ToDoItems.Add(first);
                list.ToDoItems.Add(second);
                list.ToDoItems.Add(third);

                ToDoLists.Add(list);
                database.ToDoLists.InsertOnSubmit(list);
                database.ToDoItems.InsertOnSubmit(first);
                database.ToDoItems.InsertOnSubmit(second);
                database.ToDoItems.InsertOnSubmit(third);
                database.SubmitChanges();
            }
            ToDoPivot.ItemsSource = ToDoLists;

            switch (ToDoLists.ElementAt(0).BackgroundColor)
            {
                case "CornflowerBlue":
                    colorPicker.SelectedIndex = 0;
                    break;
                case "Teal":
                    colorPicker.SelectedIndex = 1;
                    break;
                case "Orchid":
                    colorPicker.SelectedIndex = 2;
                    break;
                case "MidnightBlue":
                    colorPicker.SelectedIndex = 3;
                    break;
                default:
                    colorPicker.SelectedIndex = 0;
                    break;
            }
            
            CurrentGradientSelection();
        }
        #endregion

        #region Manipulation/Touch Events---------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Locks and unlocks the Pivot dependent upon touch events and the location
        /// of the events on the screen.
        /// </summary>
        /// <param name="sender">The sender of the touch event.</param>
        /// <param name="e">The data from the touch event.</param>
        private void Touch_FrameReported(object sender, TouchFrameEventArgs e)
        {
            if (e.GetPrimaryTouchPoint(ToDoPivot).Action == TouchAction.Up)
            {
                ToDoPivot.IsLocked = false;
            }
            else if (e.GetPrimaryTouchPoint(ToDoPivot).Action == TouchAction.Move)
            {
                if(e.GetPrimaryTouchPoint(null).Position.Y < 200) ToDoPivot.IsLocked = false;
                else
                {
                    ToDoPivot.IsLocked = true;
                }
            }
        }

        /// <summary>
        /// Transforms the TextBoxes within the ListBox to allow for removal and completion of items.
        /// Also allows for reorganization of ToDo items within the ListBox.
        /// </summary>
        /// <param name="sender">The sender of the Manipulation event.</param>
        /// <param name="e">The data of the Manipulation event.</param>
        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            ToDoPivot.IsLocked = true;
            AutoCompleteBox tb = sender as AutoCompleteBox;
            TranslateTransform transform = new TranslateTransform();
            tb.RenderTransform = transform;

            //Traversal of TextBox's parent's children
            Grid g = (Grid)tb.Parent;
            UIElementCollection ec = g.Children;
            Rectangle check = (Rectangle)ec.ElementAt(0);
            Rectangle cancel = (Rectangle)ec.ElementAt(1);
            Border border = (Border)ec.ElementAt(3);
            check.RenderTransform = transform;
            cancel.RenderTransform = transform;
            border.RenderTransform = transform;

            //Transform of the TextBox
            if (e.CumulativeManipulation.Translation.X > 60)
            {
                transform.X += e.CumulativeManipulation.Translation.X;
                check.Opacity = e.CumulativeManipulation.Translation.X/250;
            }
            else if (e.CumulativeManipulation.Translation.X < -60)
            {
                transform.X += e.CumulativeManipulation.Translation.X;
                cancel.Opacity = e.CumulativeManipulation.Translation.X/-250;
            }
        }

        /// <summary>
        /// Determines what to do upon completion of a Manipulation event. This includes
        /// removal of ToDo items, completion of ToDo items, and movement of ToDo items 
        /// within the ListBox.
        /// </summary>
        /// <param name="sender">The sender of the Manipulation event.</param>
        /// <param name="e">The data of the Manipulation event.</param>
        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            ToDoPivot.IsLocked = false;

            AutoCompleteBox tb = sender as AutoCompleteBox;
            TranslateTransform transform = new TranslateTransform();
            tb.RenderTransform = transform;

            //Traversal of TextBox's parent's children
            Grid g = (Grid)tb.Parent;
            UIElementCollection ec = g.Children;
            Rectangle check = (Rectangle)ec.ElementAt(0);
            Rectangle cancel = (Rectangle)ec.ElementAt(1);
            Border border = (Border)ec.ElementAt(3);
            check.RenderTransform = transform;
            cancel.RenderTransform = transform;
            border.RenderTransform = transform;

            //Removal of ToDoItem from DB
            if (e.TotalManipulation.Translation.X < -250 && e.TotalManipulation.Translation.X >= -500)
            {
                foreach(ToDoItem tdi in ToDoLists.ElementAt(ToDoPivot.SelectedIndex).ToDoItems)
                {
                    if (tdi.Content.Equals(tb.Text))
                    {
                        ToDoLists.ElementAt(ToDoPivot.SelectedIndex).ToDoItems.Remove(tdi);
                        database.ToDoItems.DeleteOnSubmit(tdi);
                        database.SubmitChanges();
                        break;
                    }
                }
            }
            else if (e.TotalManipulation.Translation.X > 250 && e.TotalManipulation.Translation.X <= 500)
            {
                transform.X = 0;
                ToDoItem completed = ToDoLists.ElementAt(ToDoPivot.SelectedIndex).ToDoItems.First(tdi => tdi.Content.Equals(tb.Text));
                if (!completed.Completed)
                {
                    completed.StrikeThroughOpacity = 1.0;
                    completed.ItemOpacity = 0.6;
                    completed.BackgroundColor = "LimeGreen";
                    completed.Completed = true;
                }
                else
                {
                    completed.StrikeThroughOpacity = 0.0;
                    completed.ItemOpacity = 1.0;
                    completed.Completed = false;
                    
                    //changes background of item back to 
                    //previous color within gradient
                    CurrentGradientSelection();
                }
                    database.SubmitChanges();
            }
        }
        #endregion

        #region LayoutRoot Touch Events-----------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// Keeps a reference to whether or not the side menu is active.
        /// </summary>
        private bool sideMenuActive;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayoutRoot_Hold(object sender, System.Windows.Input.GestureEventArgs e){ }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayoutRoot_DoubleTap(object sender, System.Windows.Input.GestureEventArgs e){ }

        /// <summary>
        /// Dismisses the Keyboard upon hitting the return key.
        /// </summary>
        /// <param name="sender">Keyboard sender firing the event.</param>
        /// <param name="e">Data from the event.</param>
        private void LayoutRoot_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Focus();
            }
        }
        #endregion

        #region ApplicationBar Methods------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Adds a new ToDo list to the Pivot with a Header titled 'New List.'
        /// </summary>
        /// <param name="sender">ApplicationBar Button sender firing the event.</param>
        /// <param name="e">Data from the event.</param>
        private void AddPivotItem_Click(object sender, EventArgs e)
        {
            //New ToDoList added to ToDoLists Observable Collection and Database
            //---note that the PivotIndex is set to end of the Pivot (may wish to refactor)---
            ToDoList list = new ToDoList();
            list.PivotIndex = ToDoPivot.Items.Count;
            list.Header = "New List";

            switch (colorPicker.SelectedIndex)
            {
                case 0:
                    list.BackgroundColor = "CornflowerBlue";
                    break;
                case 1:
                    list.BackgroundColor = "Teal";
                    break;
                case 2:
                    list.BackgroundColor = "Orchid";
                    break;
                case 3:
                    list.BackgroundColor = "MidnightBlue";
                    break;
                default:
                    list.BackgroundColor = "CornflowerBlue";
                    break;
            }
            
            //Queue new list for DB entry
            ToDoLists.Add(list);
            database.ToDoLists.InsertOnSubmit(list);

            //New DataTemplate, to serve as ListBox's ItemTemplate, based on resource in MainPage.xaml
            DataTemplate dt = LayoutRoot.Resources["ToDoDataTemplate"] as DataTemplate;

            //New ListBox with properly bound ItemsSource
            ListBox lb = new ListBox();
            lb.ItemTemplate = dt;
            ToDoLists.ElementAt(ToDoPivot.Items.Count-1).ToDoItems = new System.Data.Linq.EntitySet<ToDoItem>();
            lb.ItemsSource = ToDoLists.ElementAt(ToDoPivot.Items.Count-1).ToDoItems;

            database.SubmitChanges();
        }

        /// <summary>
        /// Adds a new ToDo item to the currently selected list.
        /// </summary>
        /// <param name="sender">ApplicationBar Button sender firing the event.</param>
        /// <param name="e">Data from the event.</param>
        private void AddToDoItem_Click(object sender, EventArgs e)
        {
            ToDoItem item = new ToDoItem();
            item.PivotIndex = ToDoPivot.SelectedIndex;
            item.ToDoList = ToDoLists.ElementAt(ToDoPivot.SelectedIndex);
            
            //Item currently added to the end of the ListBox
            item.Index = ToDoLists.ElementAt(ToDoPivot.SelectedIndex).ToDoItems.Count;
            ToDoLists.ElementAt(ToDoPivot.SelectedIndex).ToDoItems.Add(item);
            database.ToDoItems.InsertOnSubmit(item);
            database.SubmitChanges();

            CurrentGradientSelection();
        }

        /// <summary>
        /// Clears all completed events from all lists.
        /// </summary>
        /// <param name="sender">ApplicationBar MenuItem sender firing the event.</param>
        /// <param name="e">Data from the event.</param>
        private void ClearCompleted(object sender, EventArgs e)
        {
            for (int i = 0; i < ToDoLists.ElementAt(ToDoPivot.SelectedIndex).ToDoItems.Count; i++)
            {
                ToDoItem tdi = ToDoLists.ElementAt(ToDoPivot.SelectedIndex).ToDoItems.ElementAt(i);
                if (tdi.Completed)
                {
                    ToDoLists.ElementAt(ToDoPivot.SelectedIndex).ToDoItems.Remove(tdi);
                    database.ToDoItems.DeleteOnSubmit(tdi);
                }
            }

            database.SubmitChanges();
            CurrentGradientSelection();
        }
        #endregion

        #region SideMenu Open/Close---------------------------------------------------------------------------------------------------------------

       /// <summary>
       /// Opens and closes the side menu.
       /// </summary>
       /// <param name="sender">ApplicationBar Button sender firing the event.</param>
       /// <param name="e">Data from the event.</param>
        private void SideMenu_Click(object sender, EventArgs e)
        {
            if (!sideMenuActive && !editingToDo)
            {
                VisualStateManager.GoToState(this, "MenuOpen", true);
                sideMenuActive = !sideMenuActive;
                ToDoPivot.Opacity = 0.4;
                ToDoPivot.IsEnabled = false;
                CurrentGradientSelection();
            }
            else if (sideMenuActive && !editingToDo)
            {
                VisualStateManager.GoToState(this, "MenuClosed", true);
                sideMenuActive = !sideMenuActive;
                ToDoPivot.Opacity = 1.0;
                ToDoPivot.IsEnabled = true;
                CurrentGradientSelection();
            }
        }
        #endregion

        #region ToDoItem ColorGradient Methods----------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// Sets all list's color scheme to the selected item from the ListPicker drop down menu.
        /// </summary>
        /// <param name="sender">The ListPicker sender responsible for the event firing.</param>
        /// <param name="e">Data for the Selection Changed event.</param>
        private void ColorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListPicker lp = sender as ListPicker;
            if (sideMenuActive)
            {
                colorPicker.SelectedIndex = lp.SelectedIndex;
                CurrentGradientSelection();
            }
        }

        /// <summary>
        /// Sets the background colors for each cell within each ToDo list so that a gradient
        /// for the current color scheme is applied properly.
        /// </summary>
        private void CurrentGradientSelection()
        {
            //ColorSelection is 'Winter' color
            if (colorPicker.SelectedIndex == 0)
            {
                //Set SideMenu background color
                SolidColorBrush c = new SolidColorBrush(Color.FromArgb(255, 100, 149, 237));
                canvas.Background = c;

                //Set all ListBoxes' background color
                foreach (ToDoList tdl in ToDoLists)
                {
                    tdl.BackgroundColor = "CornflowerBlue";
                    tdl.ForegroundColor = "Black";
                }

                //Create 2d array with necessary colors for each cell
                List<int[,]> colorArray = DetermineGradient(255, 0, 149, 237, 255, 255, 255, 255);
                
                //loop through all lists in pivot
                for (int i = 0; i < ToDoLists.Count; i++)
                {
                    //make sure that no lists are empty (no todoitems present)
                    if (ToDoLists.ElementAt(i).ToDoItems != null)
                    {
                        //loop through each cell and adjust the background and foreground colors
                        for (int j = 0; j < ToDoLists.ElementAt(i).ToDoItems.Count; j++)
                        {
                            if (!ToDoLists.ElementAt(i).ToDoItems.ElementAt(j).Completed)
                            {
                                ToDoLists.ElementAt(i).ToDoItems.ElementAt(j).BackgroundColor = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", colorArray.ElementAt(i)[j, 0], colorArray.ElementAt(i)[j, 1], colorArray.ElementAt(i)[j, 2], colorArray.ElementAt(i)[j, 3]);
                                ToDoLists.ElementAt(i).ToDoItems.ElementAt(j).ForegroundColor = "Black";
                            }
                        }
                    }
                }
                //Update SideMenu's 'Background Color: ' TextBlock to be easily visible regardless of SideMenu background color
                sideMenuText.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                database.SubmitChanges();
            }
            //ColorSelection is 'Teal' color
            else if (colorPicker.SelectedIndex == 1)
            {
                SolidColorBrush c = new SolidColorBrush(Color.FromArgb(255, 0, 128, 128));
                canvas.Background = c;

                foreach (ToDoList tdl in ToDoLists)
                {
                    tdl.BackgroundColor = "Teal";
                    tdl.ForegroundColor = "Black";
                }

                List<int[,]> colorArray = DetermineGradient(255, 0, 128, 128, 255, 255, 255, 255);

                for (int i = 0; i < ToDoLists.Count; i++)
                {
                    if (ToDoLists.ElementAt(i).ToDoItems != null)
                    {
                        for (int j = 0; j < ToDoLists.ElementAt(i).ToDoItems.Count; j++)
                        {
                            if (!ToDoLists.ElementAt(i).ToDoItems.ElementAt(j).Completed)
                            {
                                ToDoLists.ElementAt(i).ToDoItems.ElementAt(j).BackgroundColor = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", colorArray.ElementAt(i)[j, 0], colorArray.ElementAt(i)[j, 1], colorArray.ElementAt(i)[j, 2], colorArray.ElementAt(i)[j, 3]);
                                ToDoLists.ElementAt(i).ToDoItems.ElementAt(j).ForegroundColor = "Black";
                            }
                        }
                    }
                }
                sideMenuText.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                database.SubmitChanges();
            }
            //ColorSelection is 'Orchid' color
            else if (colorPicker.SelectedIndex == 2)
            {
                SolidColorBrush c = new SolidColorBrush(Color.FromArgb(255, 218, 112, 214));
                canvas.Background = c;

                foreach (ToDoList tdl in ToDoLists)
                {
                    tdl.BackgroundColor = "Orchid";
                    tdl.ForegroundColor = "Black";
                }

                List<int[,]> colorArray = DetermineGradient(255, 218, 112, 214, 255, 255, 255, 255);

                for (int i = 0; i < ToDoLists.Count; i++)
                {
                    if (ToDoLists.ElementAt(i).ToDoItems != null)
                    {
                        for (int j = 0; j < ToDoLists.ElementAt(i).ToDoItems.Count; j++)
                        {
                            if (!ToDoLists.ElementAt(i).ToDoItems.ElementAt(j).Completed)
                            {
                                ToDoLists.ElementAt(i).ToDoItems.ElementAt(j).BackgroundColor = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", colorArray.ElementAt(i)[j, 0], colorArray.ElementAt(i)[j, 1], colorArray.ElementAt(i)[j, 2], colorArray.ElementAt(i)[j, 3]);
                                ToDoLists.ElementAt(i).ToDoItems.ElementAt(j).ForegroundColor = "Black";
                            }
                        }
                    }
                }
                sideMenuText.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                database.SubmitChanges();
            }
            //ColorSelection is 'Midnight' color
            else if (colorPicker.SelectedIndex == 3)
            {
                SolidColorBrush c = new SolidColorBrush(Color.FromArgb(255, 25, 25, 112));
                canvas.Background = c;

                foreach (ToDoList tdl in ToDoLists)
                {
                    tdl.BackgroundColor = "MidnightBlue";
                    tdl.ForegroundColor = "White";
                }

                List<int[,]> colorArray = DetermineGradient(255, 25, 25, 112, 255, 138, 43, 226);

                for (int i = 0; i < ToDoLists.Count; i++)
                {
                    if (ToDoLists.ElementAt(i).ToDoItems != null)
                    {
                        for (int j = 0; j < ToDoLists.ElementAt(i).ToDoItems.Count; j++)
                        {
                            if (!ToDoLists.ElementAt(i).ToDoItems.ElementAt(j).Completed)
                            {
                                ToDoLists.ElementAt(i).ToDoItems.ElementAt(j).BackgroundColor = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", colorArray.ElementAt(i)[j, 0], colorArray.ElementAt(i)[j, 1], colorArray.ElementAt(i)[j, 2], colorArray.ElementAt(i)[j, 3]);
                                ToDoLists.ElementAt(i).ToDoItems.ElementAt(j).ForegroundColor = "White";
                            }
                        }
                    }
                }
                sideMenuText.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                database.SubmitChanges();
            }
        }

        /// <summary>
        /// Determines the necessary background colors for each cell of all ToDo lists 
        /// based upon number of ToDo items and the two Color's A, R, G, and B values 
        /// used for the gradient.
        /// </summary>
        /// <param name="a1">First gradient color's alpha value.</param>
        /// <param name="r1">First gradient color's red value.</param>
        /// <param name="g1">First gradient color's green value.</param>
        /// <param name="b1">First gradient color's blue value.</param>
        /// <param name="a2">Second gradient color's alpha value.</param>
        /// <param name="r2">Second gradient color's red value.</param>
        /// <param name="g2">Second gradient color's green value.</param>
        /// <param name="b2">Second gradient color's blue value.</param>
        /// <returns></returns>
        private List<int[,]> DetermineGradient(int a1, int r1, int g1, int b1, int a2, int r2, int g2, int b2)
        {
            List<int[,]> allPivotItems = new List<int[,]>();
            for (int i = 0; i < ToDoLists.Count; i++)
            {
                
                int total = ToDoLists.ElementAt(i).ToDoItems.Count;
                if (total > 0)
                {
                    int[,] itemColors = new int[total, 4];

                    int deltaR = (r2 - r1) / total;
                    int deltaG = (g2 - g1) / total;
                    int deltaB = (b2 - b1) / total;
                    int deltaA = (a2 - a1) / total;

                    for (int j = 0; j < total; j++)
                    {
                        itemColors[j, 0] = a1 + (j * deltaA);
                        itemColors[j, 1] = r1 + (j * deltaR);
                        itemColors[j, 2] = g1 + (j * deltaG);
                        itemColors[j, 3] = b1 + (j * deltaB);
                    }
                    allPivotItems.Add(itemColors);
                }
            }
            return allPivotItems;
        }
        #endregion

        #region TextBox Events--------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Used for accessing the currently selected AutoCompleteBox.
        /// </summary>
        private ToDoItem currentlySelected;

        /// <summary>
        /// Used for determining whether or not a ListBox item's text is currently being edited.
        /// </summary>
        private bool editingToDo;

        /// <summary>
        /// Selection Changed event which forces the application to Focus() so that holds over TextBoxes will not result
        /// in entering the TextBox and/or the Cursor for the TextBox becoming visible.
        /// </summary>
        /// <param name="sender">The TextBox sender responsible for the event firing.</param>
        /// <param name="e">State information and event data associated with the fired event.</param>
        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!editingToDo)
            {
                this.Focus();
            }
        }

        /// <summary>
        /// Determines the ListBox cell that is currently being edited.
        /// </summary>
        /// <param name="sender">The TextBox sender resposible for the event firing.</param>
        /// <param name="e">State information and event data associated with the fired event.</param>
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            AutoCompleteBox tb = sender as AutoCompleteBox;  
            foreach (ToDoItem tdi in ToDoLists.ElementAt(ToDoPivot.SelectedIndex).ToDoItems)
            {
                if (tdi.Content == tb.Text)
                {
                    currentlySelected = tdi;
                }
            }
        }

       /// <summary>
       /// Determines if text is present and updates the associated ToDo item's
       /// content within the database.
       /// </summary>
       /// <param name="sender">The TextBox sender responsible for the event firing.</param>
       /// <param name="e">State information and event data associated with the fired event.</param>
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            AutoCompleteBox tb = sender as AutoCompleteBox;

            //add EventListener back on to AutoCompletBox
            tb.ManipulationDelta += OnManipulationDelta;
            editingToDo = false;
            if (tb.Text.Equals(string.Empty))
            {
                ToDoLists.ElementAt(ToDoPivot.SelectedIndex).ToDoItems.Remove(currentlySelected);
                database.ToDoItems.DeleteOnSubmit(currentlySelected);
            }
            else if(tb.Text != null && currentlySelected != null)
            {
                currentlySelected.Content = tb.Text;
                //check for instances when white text is turned black so the user can see while editing
                if (currentlySelected.ForegroundColor.Equals("White"))
                {
                    tb.Foreground = new SolidColorBrush(Colors.White);
                }
            }
            database.SubmitChanges();
        }

        /// <summary>
        /// Allows for editing of the ToDo cell text.
        /// </summary>
        /// <param name="sender">The TextBox sender responsible for the event firing.</param>
        /// <param name="e">State information and event data associated with the firing event.</param>
        private void TextBox_Tap(object sender, RoutedEventArgs e)
        {
            AutoCompleteBox tb = sender as AutoCompleteBox;
            tb.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));

            tb.ManipulationDelta -= OnManipulationDelta;
            editingToDo = true;
        }
        #endregion

        #region Helper Methods--------------------------------------------------------------------------------------------------------------------
        
        /// <summary>
        /// Determines total number of ToDo items
        /// </summary>
        /// <returns></returns>
        private int TotalToDoItems()
        {
            int total = 0;
            foreach (ToDoList tdl in ToDoLists)
            {
                total += tdl.ToDoItems.Count;
            }
            return total;
        }
        #endregion

        #region PivotHeader Actions/Events--------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Allows the user to rename the Header's text by entering the new name in a message box that pops up.
        /// </summary>
        /// <param name="sender">Pivot sender responsible for the event firing.</param>
        /// <param name="e">Event data for gesture events.</param>
        private void PivotItem_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (e.GetPosition(null).Y < 200)
            {
                String header = ToDoLists.ElementAt(ToDoPivot.SelectedIndex).Header;
                TextBox tb = new TextBox
                {
                    Text = header,
                    Width = 400,
                    Margin = new Thickness(0,10,0,0),
                };

                CustomMessageBox cmb = new CustomMessageBox()
                {
                    Caption = "Change the 'Title' for the currently selected ToDo list.",
                    Message = "No text will result in removal of the ToDo list. Length of title has no limit,"
                    + " but larger titles may wrap around screen.",
                    Content = tb,
                    LeftButtonContent = "accept",
                    RightButtonContent = "cancel",
                };

                cmb.Dismissed += (o1, e1) =>
                    {
                        switch (e1.Result)
                        {
                            case CustomMessageBoxResult.LeftButton:
                                ToDoList tdl = database.ToDoLists.First(item => item.Header == ToDoLists.ElementAt(ToDoPivot.SelectedIndex).Header);
                                if (tb.Text.Equals(string.Empty))
                                {
                                    //disallows the deletion of all lists
                                    if (ToDoLists.Count == 1) break;
                                    for(int i = 0; i < ToDoLists.ElementAt(ToDoPivot.SelectedIndex).ToDoItems.Count; i++)
                                    {
                                        ToDoItem tdi = ToDoLists.ElementAt(ToDoPivot.SelectedIndex).ToDoItems.ElementAt(i);
                                        ToDoLists.ElementAt(ToDoPivot.SelectedIndex).ToDoItems.Remove(tdi);
                                        database.ToDoItems.DeleteOnSubmit(tdi);
                                    }
                                    ToDoLists.Remove(tdl);
                                    database.ToDoLists.DeleteOnSubmit(tdl);
                                    database.SubmitChanges();
                                }
                                else
                                {
                                    tdl.Header = tb.Text;
                                    ToDoLists.ElementAt(ToDoPivot.SelectedIndex).Header = tb.Text;
                                    database.SubmitChanges();
                                }
                                break;
                            case CustomMessageBoxResult.RightButton:
                                break;
                            case CustomMessageBoxResult.None:
                                break;
                            default:
                                break;
                        }
                    };

                cmb.Show();
            }
        }

        /// <summary>
        /// Resizes the Pivot Header upon renaming
        /// </summary>
        /// <param name="sender">FrameworkElement sender firing event.</param>
        /// <param name="e">Data from the event.</param>
        private void OnHeaderSizeChanged(object sender, SizeChangedEventArgs e)
        {
            FrameworkElement fe = (FrameworkElement)sender;
            while (fe != null)
            {
                if (fe is PivotHeadersControl)
                {
                    fe.InvalidateMeasure();
                    break;
                }
                fe = VisualTreeHelper.GetParent(fe) as FrameworkElement;
            }
        }
        #endregion
    }
}