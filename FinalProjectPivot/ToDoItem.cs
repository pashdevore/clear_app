using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Collections.ObjectModel;

namespace FinalProjectPivot
{
    [Table]
    public class ToDoItem : INotifyPropertyChanging, INotifyPropertyChanged
    {
        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler temp = PropertyChanged;
            if (temp != null)
            {
                temp(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region PropertyChanging
        public event PropertyChangingEventHandler PropertyChanging;
        protected void NotifyPropertyChanging(string propertyName)
        {
            PropertyChangingEventHandler temp = PropertyChanging;
            if (temp != null)
            {
                temp(this, new PropertyChangingEventArgs(propertyName));
            }
        }
        #endregion

        #region Contructors
        public ToDoItem()
        {
            content = string.Empty;
            completed = false;
            index = 0;
            itemOpacity = 1.0;
            strikeThroughOpacity = 0.0;
            backgroundColor = "LightGray";
            foregroundColor = "Black";
        }
        #endregion

        #region ID
        private int id;
        [Column(AutoSync = AutoSync.OnInsert, CanBeNull = false, DbType = "INT NOT NULL Identity", IsDbGenerated = true, IsPrimaryKey = true)]
        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                if (id != value)
                {
                    NotifyPropertyChanging("ID");
                    id = value;
                    NotifyPropertyChanged("ID");
                }
            }
        }
        #endregion

        #region Content
        private string content;
        [Column]
        public string Content
        {
            get
            {
                return content;
            }
            set
            {
                if (content != value)
                {
                    NotifyPropertyChanging("Content");
                    content = value;
                    NotifyPropertyChanged("Content");
                }
            }
        }
        #endregion

        #region Completed
        private bool completed;
        [Column]
        public bool Completed
        {
            get
            {
                return completed;
            }
            set
            {
                if (completed != value)
                {
                    NotifyPropertyChanging("Completed");
                    completed = value;
                    NotifyPropertyChanged("Completed");
                }
            }
        }
        #endregion

        #region ItemOpacity
        private double itemOpacity;
        [Column]
        public double ItemOpacity
        {
            get
            {
                return itemOpacity;
            }
            set
            {
                if (itemOpacity != value)
                {
                    NotifyPropertyChanging("ItemOpacity");
                    itemOpacity = value;
                    NotifyPropertyChanged("ItemOpacity");
                }
            }
        }
        #endregion

        #region StrikeThrough Opacity
        private double strikeThroughOpacity;
        [Column]
        public double StrikeThroughOpacity
        {
            get
            {
                return strikeThroughOpacity;
            }
            set
            {
                NotifyPropertyChanging("StrikeThroughOpacity");
                strikeThroughOpacity = value;
                NotifyPropertyChanged("StrikeThroughOpacity");
            }
        }
        #endregion

        #region Index
        private int index;
        [Column]
        public int Index
        {
            get
            {
                return index;
            }
            set
            {
                NotifyPropertyChanging("Index");
                index = value;
                NotifyPropertyChanged("Index");
            }
        }
        #endregion

        #region PivotIndex
        private int pivotIndex;
        [Column]
        public int PivotIndex
        {
            get
            {
                return pivotIndex;
            }
            set
            {
                if (pivotIndex != value)
                {
                    NotifyPropertyChanging("PivotIndex");
                    pivotIndex = value;
                    NotifyPropertyChanged("PivotIndex");
                }
            }
        }
        #endregion

        #region BackgroundRed
        private int backgroundRed;
        [Column]
        public int BackgroundRed
        {
            get
            {
                return backgroundRed;
            }
            set
            {
                if (backgroundRed != value)
                {
                    NotifyPropertyChanging("BackgroundRed");
                    backgroundRed = value;
                    NotifyPropertyChanged("BackgroundRed");
                }
            }
        }
        #endregion

        #region BackgroundGreen
        private int backgroundGreen;
        [Column]
        public int BackgroundGreen
        {
            get
            {
                return backgroundGreen;
            }
            set
            {
                if (backgroundGreen != value)
                {
                    NotifyPropertyChanging("BackgroundGreen");
                    backgroundGreen = value;
                    NotifyPropertyChanged("BackgroundGreen");
                }
            }
        }
        #endregion

        #region BackgroundBlue
        private int backgroundBlue;
        [Column]
        public int BackgroundBlue
        {
            get
            {
                return backgroundBlue;
            }
            set
            {
                if (backgroundBlue != value)
                {
                    NotifyPropertyChanging("BackgroundBlue");
                    backgroundBlue = value;
                    NotifyPropertyChanged("BackgroundBlue");
                }
            }
        }
        #endregion

        #region BackgroundAlpha
        private int backgroundAlpha;
        [Column]
        public int BackgroundAlpha
        {
            get
            {
                return backgroundAlpha;
            }
            set
            {
                if (backgroundAlpha != value)
                {
                    NotifyPropertyChanging("BackgroundAlpha");
                    backgroundAlpha = value;
                    NotifyPropertyChanged("BackgroundAlpha");
                }
            }
        }
        #endregion


        #region BackgroundColor
        private string backgroundColor;
        [Column]
        public string BackgroundColor
        {
            get
            {
                return backgroundColor;
            }
            set
            {
                if (backgroundColor != value)
                {
                    NotifyPropertyChanging("BackgroundColor");
                    backgroundColor = value;
                    NotifyPropertyChanged("BackgroundColor");
                }
            }
        }
        #endregion

        #region ForegroundColor
        private string foregroundColor;
        [Column]
        public string ForegroundColor
        {
            get
            {
                return foregroundColor;
            }
            set
            {
                if (foregroundColor != value)
                {
                    NotifyPropertyChanging("ForegroundColor");
                    foregroundColor = value;
                    NotifyPropertyChanged("ForegroundColor");
                }
            }
        }
        #endregion

        //#region LinkedListID
        //private int linkedListID;
        //[Column]
        //public int LinkedListID
        //{
        //    get
        //    {
        //        return linkedListID;
        //    }
        //    set
        //    {
        //        if (linkedListID != value)
        //        {
        //            NotifyPropertyChanging("LinkedListID");
        //            linkedListID = value;
        //            NotifyPropertyChanged("LinkedListID");
        //        }
        //    }
        //}
        //#endregion

        #region Associated ToDoList
        [Column]
        internal int toDoListID;

        private EntityRef<ToDoList> toDoList;
        [Association(IsForeignKey = true, Storage = "toDoList", ThisKey = "toDoListID", OtherKey = "ID")]
        public ToDoList ToDoList
        {
            get
            {
                return toDoList.Entity;
            }
            set
            {
                NotifyPropertyChanging("ToDoList");
                toDoList.Entity = value;
                if (value != null)
                {
                    toDoListID = value.ID;
                }
                NotifyPropertyChanged("ToDoList");
            }
        }
        #endregion
    }
}