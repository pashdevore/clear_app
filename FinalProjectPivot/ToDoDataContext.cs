using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Linq;


namespace FinalProjectPivot
{
    public class ToDoDataContext : DataContext
    {
        public ToDoDataContext() : base("Data Source=isostore:/ToDo.sdf") { }

        public Table<ToDoList> ToDoLists;

        public Table<ToDoItem> ToDoItems;
    }
}
