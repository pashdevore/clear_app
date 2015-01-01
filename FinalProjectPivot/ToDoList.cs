using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;

namespace FinalProjectPivot
{
    [Table]
    public class ToDoList : INotifyPropertyChanged, INotifyPropertyChanging
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
        public ToDoList()
        {
            header = "New List";
            pivotIndex = -1;
            backgroundColor = "CornflowerBlue";
            foregroundColor = "Black";
            toDoItemSet = new EntitySet<ToDoItem>(
                new Action<ToDoItem>(this.AttachToItem),
                new Action<ToDoItem>(this.DetachFromItem));
        }

        void AttachToItem(ToDoItem item)
        {
            NotifyPropertyChanging("ToDoItem");
            item.ToDoList = this;
        }
        void DetachFromItem(ToDoItem item)
        {
            NotifyPropertyChanging("ToDoItem");
            item.ToDoList = null;
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

        #region Header
        private string header;
        [Column]
        public string Header
        {
            get
            {
                return header;
            }
            set
            {
                if (header != value)
                {
                    NotifyPropertyChanging("Header");
                    header = value;
                    NotifyPropertyChanged("Header");
                }
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

        #region ToDoItemSet
        EntitySet<ToDoItem> toDoItemSet;
        [Association(Storage = "toDoItemSet", ThisKey = "ID", OtherKey = "toDoListID")]
        public EntitySet<ToDoItem> ToDoItems
        {
            get
            {
                return toDoItemSet;
            }
            set
            {
                toDoItemSet.Assign(value);
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
    }
}
