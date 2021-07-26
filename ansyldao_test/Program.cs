using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ansyl.dao;
using ansyldao_test.dao;
using ansyldao_test.schoolapp;
using MySql.Data.MySqlClient;

namespace ansyldao_test
{
    class Program
    {
        static void Main(string[] args)
        {
            //  get single item
            GetItem();

            //  get list of items
            GetList();

            //  insert single item
            InsertItem();

            //  delete single item
            DeleteItem();

            //  update single item
            UpdateItem();
        }

        static void GetItem()
        {
            using var ot = new OneTransaction();

            //  get an item
            var term = ot.Get<Term>(1);
            Console.WriteLine(term.TermName);
        }

        static void DeleteItem()
        {
            using var ot = new OneTransaction();

            //  get an item
            var term = ot.Get<Term>(200);
            ot.Delete(term);

            ot.SaveChanges();
        }

        //  insert an item
        static void InsertItem()
        {
            using var ot = new OneTransaction();

            var it = new Term()
                     {
                         TermId = 200,
                         TermName = "Test Me",
                         InsertDate = DateTime.Now,
                         TermNo = 200,
                         IsActive = false,
                         YearName = "3000/3001"
                     };

            ot.Insert(it);

            //  commit change
            ot.SaveChanges();
        }

        //  update an item
        static void UpdateItem()
        {
            using var ot = new OneTransaction();

            var it = ot.Get<Term>(200);
            it.TermName = "3000/3001 Term 1";

            ot.Update(it);

            //  commit change
            ot.SaveChanges();
        }

        static void GetList()
        {
            using var ot = new OneTransaction();

            //  select using List
            var schoolTermIds = ot.List<SchoolTerm>().Select(i => i.TermId);

            var items = ot.List<Term>(e => schoolTermIds.Contains(e.TermId));

            var years = items.Select(i => i.YearName).ToList();

            foreach (var year in years)
            {
                Console.WriteLine(year);
            }
        }
    }
}
