using Microsoft.EntityFrameworkCore;
using System;

// compile with C# 8.0
namespace Xavier
{
    public static class TransposeEFToJavascript
    {
        public static string TransposeDatabase(DbContext dbContext, string dbType)
        {
            //use a switch statement to handle different db type
            switch (dbType)
            {
                case "MSSQL":
                    //use mssql provider
                    return TransposeMSSql(dbContext);
                case "SQLite":
                    //use sqlite provider
                    return TransposeSqlite(dbContext);
                case "MySQL":
                    //use mysql provider
                    return TransposeMySql(dbContext);
                default:
                    throw new ArgumentException("Invalid db type");
            }
        }

        public static string TransposeMSSql(DbContext dbContext)
        {
            //generate transposed methods and properties
            //add methods to access and change all objects
            //as if you were using LINQ

            //generate and return javascript code
            return "// Generated MSSQL code...";
        }

        public static string TransposeSqlite(DbContext dbContext)
        {
            //generate transposed methods and properties
            //add methods to access and change all objects
            //as if you were using LINQ

            //generate and return javascript code
            return "// Generated SQLite code...";
        }

        public static string TransposeMySql(DbContext dbContext)
        {
            //generate transposed methods and properties
            //add methods to access and change all objects
            //as if you were using LINQ

            //generate and return javascript code
            return "// Generated MySQL code...";
        }
    }
}