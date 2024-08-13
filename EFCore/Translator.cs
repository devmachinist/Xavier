using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

//namespace to contain the script
namespace Xavier
{
    public class EFToJS
    {
        //default constructor
        public EFToJS()
        {

        }
        //method to translate EF Core values and methods to JavaScript
        public static string TranslateEFToJS(DbContext dbContext)
        {

                string generatedJS = "";
            try
            {
                //declare a string variable to hold the generated JavaScript

                //declare a variable to store the name of the dbcontext
                string dbContextName = dbContext.GetType().Name;

                //begin the generated JavaScript string
                generatedJS += "//JavaScript code generated from EF Core values and methods for the " + dbContextName + "\n";

                //use the dbContext instance to get the type of the class representing the dbContext
                Type dbcType = dbContext.GetType();

                //retrieve the properties of the dbcontext
                PropertyInfo[] dbContextProperties = dbcType.GetProperties();

                //iterate through the dbContext properties
                foreach (PropertyInfo property in dbContextProperties)
                {
                    //generate the setters & getters for each dbContext property
                    generatedJS += GeneratePropertySetterGetterJS(property);
                }

                //generate the EF Core equivalent of the Where() method
                generatedJS += GenerateWhereMethodJS();

                //generate the EF Core equivalent of the SingleOrDefault() method
                generatedJS += GenerateSingleOrDefaultMethodJS();

                //generate the EF Core equivalent of the ToList() method
                generatedJS += GenerateToListMethodJS();

                //generate the EF Core equivalent of the SaveChanges() method
                generatedJS += GenerateSaveChangesMethodJS(dbContext);

                //generate the EF Core equivalent of the Add() method
                generatedJS += GenerateAddMethodJS();

                //generate the EF Core equivalent of the Remove() method
                generatedJS += GenerateRemoveMethodJS();

                //generate the EF Core equivalent of the Update() method
                generatedJS += GenerateUpdateMethodJS();

                generatedJS += @"Array.prototype.Map = function(callback) {
  const result = [];
            for (let item of this)
            {
                result.push(callback(item));
            }
            return result.join('');
        };";

            return generatedJS;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
            dbContext.Dispose();
            return "";
            //print out the generated JavaScript
        }

        //method to generate the setters & getters for each dbContext property
        public static string GeneratePropertySetterGetterJS(PropertyInfo property)
        {
            string generatedJS = "";

            //generate the getter for the current property
            generatedJS += "export function Get" + property.Name + "(){\n";
            generatedJS += "\t return " + property.Name + "; \n}\n\n";

            //generate the setter for the current property
            generatedJS += "export function Set" + property.Name + "(value){\n";
            generatedJS += "\t " + property.Name + " = value; \n}\n\n";

            return generatedJS;
        }

        //method to generate the EF Core equivalent of the Where() method
        public static string GenerateWhereMethodJS()
        {
            string generatedJS = "";

            //generatedJS += "Array.prototype.Where = function(predicate){\n";
            //generatedJS += "\t var result = [];\n";
            //generatedJS += "\t for(var i=0; i < this.length; i++){\n";
            //generatedJS += "\t \t if(predicate(this[i])){\n";
            //generatedJS += "\t \t \t result.push(this[i]);\n";
            //generatedJS += "\t \t }\n";
            //generatedJS += "\t }\n";
            //generatedJS += "\t return result;\n}\n\n";

            return generatedJS;
        }

        public static string GenerateWFirstMethodJS()
        {
            string generatedJS = "";

            //generatedJS += "Array.prototype.First = function(predicate){\n";
            //generatedJS += "\t var result = [];\n";
            //generatedJS += "\t for(var i=0; i < this.length; i++){\n";
            //generatedJS += "\t \t if(predicate(this[i])){\n";
            //generatedJS += "\t \t \t result.push(this[i]);\n";
            //generatedJS += "\t \t }\n";
            //generatedJS += "\t }\n";
            //generatedJS += "\t return result[0];\n}\n\n";

            return generatedJS;
        }

        //method to generate the EF Core equivalent of the SingleOrDefault() Method
        public static string GenerateSingleOrDefaultMethodJS()
        {
            string generatedJS = "";

            //generatedJS += "Array.prototype.SingleOrDefault = function(predicate){\n";
            //generatedJS += "\t for(var i=0; i < this.length; i++){\n";
            //generatedJS += "\t \t if(predicate(this[i])){\n";
            //generatedJS += "\t \t \t return this[i];\n";
            //generatedJS += "\t \t }\n";
            //generatedJS += "\t }\n";
            //generatedJS += "\t return null;\n}\n\n";

            return generatedJS;
        }

        //method to generate the EF Core equivalent of the ToList() Method
        public static string GenerateToListMethodJS()
        {
            string generatedJS = "";

            generatedJS += "Array.prototype.ToList = function(){\n";
            generatedJS += "\t return this;\n}\n\n";

            return generatedJS;
        }

        //method to generate the EF Core equivalent of the SaveChanges() Method
        public static string GenerateSaveChangesMethodJS(DbContext dbContext)
        {
            string generatedJS = "";
            string db = dbContext.Database.ProviderName;
            db = db.Substring(db.LastIndexOf(".") +1, db.Length - db.LastIndexOf(".")-1).ToLower();
            db = db.Contains("lite")? db + "3":db;

            generatedJS += "Array.prototype.SaveChanges = function(){" +
                $"const sql = require('{db}');\n";
            generatedJS += "\t     ChangeTracker.forEach(object => {\r\n " +
                "       Object.keys(object).forEach(key => {\r\n     " +
                "       let value = object[key];\r\n      " +
                "      let query = `update ${key} set value = '${value}'`;\r\n         " +
                "      async () => {\r\n  " +
                "  try {\r\n  " +
                "      // make sure that any items are correctly URL encoded in the connection string\r\n  " +
                "      await sql.connect(connect)\r\n  " +
                "      const result = await sql.query`${query}`\r\n     " +
                "   console.dir(result)\r\n    }" +
                " catch (err) {\r\n        console.log(err)\r\n    }\r\n    " +
                "    }\r\n  " +
                "  });\n})  }\n\n";

            return generatedJS;
        }

        //method to generate the EF Core equivalent of the Add() Method
        public static string GenerateAddMethodJS()
        {
            string generatedJS = "";

            //generatedJS += "Array.prototype.Add = function(entity){\n";
            //generatedJS += "\t this.push(entity);\n}\n\n";

            return generatedJS;
        }

        //method to generate the EF Core equivalent of the Remove() Method
        public static string GenerateRemoveMethodJS()
        {
            string generatedJS = "";

            //generatedJS += "Array.prototype.Remove = function(entity){\n";
            //generatedJS += "\t var index = this.indexOf(entity);\n";
            //generatedJS += "\t this.splice(index,1);\n}\n\n";

            return generatedJS;
        }

        //method to generate the EF Core equivalent of the Update() Method
        public static string GenerateUpdateMethodJS()
        {
            string generatedJS = "";

            //generatedJS += "Array.prototype.Update = function(entity){\n";
            //generatedJS += "\t var index = this.indexOf(entity);\n";
            //generatedJS += "\t this[index] = entity;\n}\n\n";

            return generatedJS;
        }
    }
}