using System;
using System.Data.SqlClient;

namespace DemoExceptionFilter
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                UpdateData();
            }
            catch (FakeSqlException ex) when (ex.Number == 2627)
            {
                Console.WriteLine("主鍵重複!");
            }
            catch (FakeSqlException ex) when (ex.Number == 1205)
            {
                Console.WriteLine("Deadlock!");
            }
        }

        static void UpdateData()
        {
            throw new FakeSqlException() { Number = 1205 };
        }
    }

    class FakeSqlException : Exception
    {
        public int Number { get; set; }
    }
}
