using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;

namespace Xavier
{
    public class Memory
    {
        public List<object> XavierNodes { get; set; } = new List<object>();
        public string? XavierName { get; set; } = "Xavier";
        public ScriptEngine PyEngine { get; set; }
        public string? BaseURI { get; set; }
        public string? JSModule { get; set; }
        public string? EFModule { get; set; }
        public string? StaticRoot { get; set; }
        public string? StaticFallback { get;set; }
        public List<DbContext>? Contexts { get; set; }
        public bool? AddAuthentication { get; set; } = true;
        public string? JSAuth() => $@"";
        public Memory(){
            
        }

        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public async Task Init(string root, string destination, Assembly assembly)
        {
            PyEngine = Python.CreateEngine();
            StaticRoot = root;
            XavierName = destination;

            await Task.Run(async () =>
            {
                StringBuilder sb = new StringBuilder();
                StringBuilder wb = new StringBuilder();

                await this.SearchForXavierNodesAndChildren(root, true, assembly);
                foreach (var xav in XavierNodes)
                {

                var xavier = xav as XavierNode;
                    sb.Append((xav as XavierNode).ClassBody(this));
                    wb.Append($"var {xavier.Name} = new {xavier.Name.ToUpper()}();" +
                        $"if(window.location.pathname === {xavier.Name}.Route && {xavier.Name}.ShouldRender === true || {xavier.Name}.Route === '' && {xavier.Name}.ShouldRender === true){{" +
                $"await {xavier.Name}.renderXidElements(document.body);"+
                $"}}{xavier.Name}.AddListener();" +
                $"");

                }
                

                this.JSModule = $"(async function(){{ {sb.ToString()} {wb.ToString()} {EFToJS.GenerateWhereMethodJS()} {EFToJS.GenerateWFirstMethodJS()} {EFToJS.GenerateSingleOrDefaultMethodJS()} }})()";

                var file = $"{XavierName}.js";
            wb.Clear();
                if (File.Exists(file))
                {
                    if (XavierNodes != null)
                    {
                foreach (var xav in XavierNodes)
                {
                var xavier = xav as XavierNode;
                    wb.Append($"var {xavier.Name} = new {xavier.Name.ToUpper()}();" +
                        $"if(window.location.pathname === {xavier.Name}.Route && {xavier.Name}.ShouldRender === true || {xavier.Name}.Route === '' && {xavier.Name}.ShouldRender === true){{" +
                $"await {xavier.Name}.renderXidElements(document.body);"+
                $"}}");
                }
                        StringBuilder check = new StringBuilder();
                        XavierNodes.ForEach(n => { check.Append((n as XavierNode).ClassBody(this)); });
                        if (File.ReadAllText(file).Length == ($"(async function(){{ { check.ToString()} { wb.ToString()} {EFToJS.GenerateWhereMethodJS()} {EFToJS.GenerateWFirstMethodJS()} {EFToJS.GenerateSingleOrDefaultMethodJS()} }})()").Length)
                        {

                        }
                        else
                        {
                            Console.WriteLine("Streaming changes to " + file);
                            WriteModule();
                        }
                    }

                }
                else
                {
                    Console.WriteLine("Writing Xavier Module named " + file);
                    WriteModule();
                }
                GC.Collect();
            });
        }
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public async Task Init(string root, string destination, List<DbContext> contexts, Assembly assembly)
        {
            StaticRoot = root;
            PyEngine = Python.CreateEngine();
            XavierName = destination;
            await Task.Run(async () =>
            {
                StringBuilder sb = new StringBuilder();
                StringBuilder wb = new StringBuilder();

                await this.SearchForXavierNodesAndChildren(root, true, assembly);
                foreach (var xav in XavierNodes)
                {

                var xavier = xav as XavierNode;
                    sb.Append(xavier.ClassBody(this));
                    wb.Append($"var {xavier.Name} = new {xavier.Name.ToUpper()}();" +
                        $"if(window.location.pathname === {xavier.Name}.Route && {xavier.Name}.ShouldRender === true || {xavier.Name}.Route === '' && {xavier.Name}.ShouldRender === true){{" +
                $"await {xavier.Name}.renderXidElements(document.body);"+
                $"}}");

                }
                this.Contexts = contexts;
                this.JSModule = $"(async function(){{ {sb.ToString()} {wb.ToString()} {EFToJS.GenerateWhereMethodJS()} {EFToJS.GenerateWFirstMethodJS()} {EFToJS.GenerateSingleOrDefaultMethodJS()} }})()";

                try
                {
                    foreach (var c in Contexts)
                    {
                        var efFile = $"{XavierName}.{c.GetType().Name}.js";
                        if (File.Exists(efFile))
                        {
                            if (File.ReadAllText(efFile).Length == TestJavascriptFile(efFile, c).Length)
                            {

                            }
                            else
                            {
                                Console.WriteLine("Streaming changes to " + efFile);
                                WriteJavascriptFile(efFile, c);
                            }
                        }
                        else
                        {

                            Console.WriteLine("Writing EF Core module to " + efFile);
                            WriteJavascriptFile(efFile, c);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                var file = $"{XavierName}.js";
            wb.Clear();

                if (File.Exists(file))
                {
                    if (XavierNodes != null)
                    {
                foreach (var xav in XavierNodes)
                {
                var xavier = xav as XavierNode;
                    wb.Append($"var {xavier.Name} = new {xavier.Name.ToUpper()}();" +
                        $"if(window.location.pathname === {xavier.Name}.Route && {xavier.Name}.ShouldRender === true || {xavier.Name}.Route === '' && {xavier.Name}.ShouldRender === true){{" +
                $"await {xavier.Name}.renderXidElements(document.body);"+
                $"}}");

                }
                        StringBuilder check = new StringBuilder();
                        XavierNodes.ForEach(n => { check.Append((n as XavierNode).ClassBody(this)); });
                        if (File.ReadAllText(file).Length == ($"(async function(){{ { check.ToString()} { wb.ToString()} {EFToJS.GenerateWhereMethodJS()} {EFToJS.GenerateWFirstMethodJS()} {EFToJS.GenerateSingleOrDefaultMethodJS()} }})()").Length)
                        {

                        }
                        else
                        {
                            Console.WriteLine("Streaming changes to " + file);
                            WriteModule();
                        }
                    }

                }
                else
                {
                    Console.WriteLine("Writing Xavier Module named " + file);
                    WriteModule();
                }
            });
        }

        public void WriteModule()
        {
            var file = $"{XavierName}.js";
            File.WriteAllText(file, JSModule);
        }

        public void WriteEF(DbContext context)
        {
            var file = Environment.CurrentDirectory + $"/{XavierName}.{context.GetType().Name}.js";
            var item = EFToJS.TranslateEFToJS(context);

            File.WriteAllText(file, item);
            WriteJavascriptFile(file, context);
        }
        public static string TestJavascriptFile(string fileName, DbContext dbContext)
        {
            StringBuilder sw = new StringBuilder();

            sw.AppendLine("//THIS IS A GENERATED FILE - Do not alter");

            //// create a connection string to connect to the database
            //string connectionString = dbContext.Database.GetDbConnection().ConnectionString;

            //// create a new instance of the sequelize module
            //sw.AppendLine($"const connect = \"{connectionString}\";");

            // loop through the available models
            foreach (var modelType in dbContext.Model.GetEntityTypes())
            {
                try
                {
                    var startIndex = (modelType.Name.LastIndexOf(".") >= 0) ? modelType.Name.LastIndexOf(".") + 1 : 0;
                    // get the model name from the model type
                    string modelName = modelType.Name.Substring(startIndex, modelType.Name.Length - startIndex).Replace("<string>", "");

                    sw.AppendLine($@"export class {modelName.Replace("+", "")} {{
  constructor(){{");

                    // loop through the model properties
                    foreach (var property in modelType.GetProperties())
                    {
                        // get the property name and type
                        string propertyName = property.Name;
                        string propertyType = property.ClrType.Name;
                        sw.AppendLine($"     this.{propertyName}= {{}}");
                    }
                    sw.AppendLine("} }");
                    sw.AppendLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            // loop through the db sets
            foreach (var dbSet in dbContext.GetType().GetProperties())
            {
                // get the db set name
                string dbSetName = dbSet.Name;

                sw.AppendLine($"export const {dbSetName} = [];");
            }
            sw.AppendLine(EFToJS.TranslateEFToJS(dbContext));
            return sw.ToString();
        }
        public async void WriteJavascriptFile(string fileName, DbContext dbContext)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                sw.WriteLine("//THIS IS A GENERATED FILE - Do not alter");

                // create a connection string to connect to the database
                // string connectionString = dbContext.Database.GetDbConnection().ConnectionString;

                // create a new instance of the sequelize module
                // sw.WriteLine($"const connect = \"{connectionString.Replace("\\", "/")}\";");

                // loop through the available models
                foreach (var modelType in dbContext.Model.GetEntityTypes())
                {
                    try
                    {
                        var startIndex = (modelType.Name.LastIndexOf(".") >= 0) ? modelType.Name.LastIndexOf(".") + 1 : 0;
                        // get the model name from the model type
                        string modelName = modelType.Name.Substring(startIndex, modelType.Name.Length - startIndex).Replace("<string>", "");

                        sw.WriteLine($@"export class {modelName.Replace("+", "")} {{
  constructor(){{");

                        // loop through the model properties
                        foreach (var property in modelType.GetProperties())
                        {
                            // get the property name and type
                            string propertyName = property.Name;
                            string propertyType = property.ClrType.Name;

                            sw.WriteLine($"     this.{propertyName}= {{}}");
                        }

                        sw.WriteLine("} }");
                        sw.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                // loop through the db sets
                foreach (var dbSet in dbContext.GetType().GetProperties())
                {
                    // get the db set name
                    string dbSetName = dbSet.Name;

                    sw.WriteLine($"export const {dbSetName} = [];");
                }
                sw.WriteLine(EFToJS.TranslateEFToJS(dbContext));
                await dbContext.DisposeAsync();
                sw.Close();
            }

        }
        public async Task SearchForXavierNodesAndChildren(string searchDir, bool searchSubdirectories, Assembly assembly)
        {
                List<object> XavierNodesAndChildren = new List<object>();
            await Task.Run(() =>
            {
                try
                {
                    // Search the directory for all .xavier files
                    string[] xavierNodes = Directory.GetFiles(searchDir, "*.xavier",

                          (searchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
                    // Check if any .xavier files were found
                    if (xavierNodes.Length > 0)
                    {
                        // If xavier files were found, check if they have any children that inherit from the
                        // XavierNode class
                        for (int i = 0; i < xavierNodes.Length; i++)
                        {
                            string XavierNodePath = xavierNodes[i];
                            var XavierNode = new XavierNode(XavierNodePath, assembly, this);
                            var args = new object[1];
                            var node = new object();
                            args[0] = XavierNode;
                            try
                            {
                                node = Activator.CreateInstance(assembly.GetType(assembly.FullName.Split(",")[0] + "." + XavierNode.Name), args);
                                //Console.WriteLine(XavierNode.Content());
                            }
                            catch (Exception ex)
                            {
                                continue;
                            }
                            if (!(XavierNodes.Where(n => n.GetType() == node.GetType()).Count() > 0))
                            {
                                XavierNodes.Add(node);
                            }
                            // Check if the .xavier file has any .xavier.cs children
                        }
                    }
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }
        public void Dispose()
        {
            XavierNodes.Clear();
            JSModule = null;
        }
    }
}
