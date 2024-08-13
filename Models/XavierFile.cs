using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Scripting;
using System.Text.RegularExpressions;
using System.Text;
using System.Text.Json;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.Scripting.Hosting;

namespace Xavier
{
    /// <summary>
    /// This is a Xavier FIle model with all methods and options.
    /// It includes a C# attribute handler to add types as parameters for taking query strings.
    /// </summary>

    public class XavierNode
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Code { get; set; }
        public string dataset { get; set; }
        public string ContentType { get; set; } = "text/html";
        public string Xid { get; set; } = Guid.NewGuid().ToString();
        public string ClassBody(Memory memory) => GenerateJavaScriptClass(this, this.GetType(), memory);
        public string Route { get; set; }
        public State state { get; set; }
        public string HTML { get; set; } = "";
        public string Scripts { get; set; }
        public string PyImports { get; set; } = "import json\n";
        public bool ShouldRender { get; set; } = true;
        public bool NodeJS { get; set; } = false;
        public bool JSClientApi { get; set; } = true;
        public string BaseUrl { get; private set; }
        public bool Authorize { get; set; }
        public Assembly XAssembly { get; set; }
        public string Content(Memory memory) {
            return ExtractPython(RW.WriteVirtualFile(this, File.ReadAllText(Path), GetAssembly(), memory), memory);
        }
        public Assembly GetAssembly() {
            return XAssembly;
        }
        public string GenerateJavascriptApi()
        {
            if (JSClientApi) {
                var typename = this.Name;
                var NSpace = XAssembly.FullName.Split(",")[0];
                var theType = XAssembly.GetType(NSpace + "." + this.Name);

                string jsModuleName = typename;

                StringBuilder sb = new StringBuilder();

                sb.AppendLine("var " + jsModuleName + " = {};");

                //Loop over all of the methods in the type and create a JS function for each
                foreach (var methodInfo in theType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                  BindingFlags.Static | BindingFlags.Instance))
                {
                    sb.AppendLine(jsModuleName + "." + methodInfo.Name + " = async function(parameters){ ");
                    sb.AppendLine("try {");
                    sb.AppendLine(" var apiCall = await fetch('/api/" + typename + "/" + methodInfo.Name + "', {method: 'POST',  headers: {  'Content-Type': 'application/json', }, body: JSON.stringify(parameters)});");
                    sb.AppendLine(" return await apiCall.text();");
                    sb.AppendLine("} catch(err) {");
                    sb.AppendLine("console.log(err);");
                    sb.AppendLine("} };");
                }
                GC.Collect();
                //Write the output to a file
                return sb.ToString();
            }
            else {
                return "";
            }
        }
        public string ExtractPython(string py, Memory memory)
        {
            var sb = new StringBuilder();
            var source = py;
            var type = this.GetType();
            List<string> Blocks = new List<string>();
            var eval = source;
            var startIndex = 0;

            while (source.IndexOf("py{", startIndex) >= 0)
            {
                int start = eval.IndexOf("py{", startIndex);
                int end = eval.IndexOf("}py", startIndex) + 3;
                Blocks.Add(eval.Substring(start, end - start));
                startIndex = end;
            }

            // Create an instance of the .NET Core type

            // Create a scope and context
            var className = type.Name;
            var result = string.Empty;
            var imports = PyImports;
            // Write the python class codea
            result += imports;

            result += $"false = False\ntrue = True\nnull = None\n";
            result += $"class {this.Name}:\n";

            // Get the properties and fields
            foreach (var m in type.GetMembers())
            {
                // For this example, we only care about properties
                if (m.MemberType == System.Reflection.MemberTypes.Property)
                {
                    // Get the property
                    var prop = type.GetProperty(m.Name);

                    if (prop.Name == "XAssembly") { continue; }
                    if (prop.Name == "Controller") { continue; }
                    if (prop.PropertyType == typeof(MethodInfo)) { continue; }
                    // Get the property type
                    if (prop.Name == "Types") { continue; }
                    //if (prop.Name == "Parameters") { continue; }
                    //if (prop.Name == "state") { continue; }
                    if (prop.Name == "Code") { continue; }
                    if (prop.Name == "Properties") { continue; }
                    if (prop.PropertyType == typeof(PropertyInfo)) { continue; }
                    if (prop.PropertyType == typeof(string))
                    {
                        var propType1 = prop.PropertyType;
                        // Get the value of the property
                        var propValue1 = JsonSerializer.Serialize(prop.GetValue(this) ?? "None");

                        // Write the property
                        result += $"\t{m.Name} = {propValue1}\n";
                        continue;
                    }
                    if (prop.PropertyType == typeof(bool))
                    {
                        var propType2 = prop.PropertyType;

                        // Get the value of the property
                        var propValue2 = JsonSerializer.Serialize(prop.GetValue(this) ?? null);

                        // Write the property
                        result += $"\t{m.Name} = {propValue2.ToString()}\n";
                        continue;

                    }

                    var propType = prop.PropertyType;

                    // Get the value of the property
                    var propValue = JsonSerializer.Serialize(prop.GetValue(this) ?? null);

                    // Write the property
                    result += $"\t{m.Name} = json.loads('{propValue}')\n";
                }
            }

            foreach (var block in Blocks)
            {
                try
                {
                    // Run the script
                    var pyscript = new PythonScript(memory.PyEngine);
                    var python = pyscript.RunFromString<string>(result + block, "result");
                    py = py.Replace(block, python);
                } catch (Exception ex) { Console.WriteLine(ex.Message); }
            }

            return py;
        }
        public class PythonScript
        {
            private ScriptEngine _engine { get; set; }

            public PythonScript(ScriptEngine engine)
            {
                _engine = engine;
            }

            public TResult RunFromString<TResult>(string code, string variableName)
            {

                // for easier debugging write it out to a file and call: _engine.CreateScriptSourceFromFile(filePath);
                var scope = _engine.CreateScope();

                _engine.Execute(code.Replace("py{", "").Replace("}py", ""), scope);
                dynamic item = scope.GetVariable<TResult>(variableName);
                return item;
            }
        }
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public string GenerateJavaScriptClass(object node, Type cSharpClass, Memory memory)
        {
            var Node = (node as XavierNode);

            StringBuilder sb = new StringBuilder();
            var propertyRun = 0;
            sb.AppendLine($@"
class {Node.Name.ToUpper()} extends XavierNode {{
    constructor(data){{
        super();");
            Type[] inheritedTypes = XAssembly.GetTypes().Where(t => t.IsSubclassOf(typeof(XavierNode))).ToArray();
            List<Type> checkTypes = new List<Type>();
            foreach (Type inheritedType in inheritedTypes)
            {
                if (inheritedType.Name == this.Name && !checkTypes.Contains(inheritedType))
                {
                    checkTypes.Add(inheritedType);
                    System.Runtime.Remoting.ObjectHandle instance =
          Activator.CreateInstanceFrom(XAssembly.Location,
                                       inheritedType.FullName);
                    Node.Properties.AddRange(instance.Unwrap().GetType().GetProperties().ToList());

                    Node.RemoveDuplicates();

                    var i = instance;

                    foreach (var xprop in instance.Unwrap().GetType().GetProperties().ToList())
                    {
                        var fullprop = xprop.GetValue(instance.Unwrap());
                        if (xprop.Name == "Code")
                        {
                            continue;
                        }
                        else if (xprop.Name == "ClassBody")
                        {
                            continue;
                        }
                        else if (xprop.Name == "Properties")
                        {
                            continue;
                        }
                        else if (xprop.Name == "Path")
                        {
                            sb.AppendLine($@"this.{xprop.Name} = '{xprop.GetValue(instance.Unwrap()).ToString()}';");
                        }
                        else if (xprop.Name == "Scripts")
                        {
                        }
                        else if (xprop.Name == "HTML")
                        {

                        }

                        else if (xprop.Name == "Route")
                        {
                            sb.AppendLine($@"this.{xprop.Name} = {RW.ClearSlashes(xprop.GetValue(instance.Unwrap())?.ToString()) ?? ""};");
                        }
                        else if (xprop.PropertyType.ToString().Contains("List"))
                        {
                            sb.AppendLine($@"this.{xprop.Name} = new ObservableArray(...{JsonSerializer.Serialize(xprop.GetValue(instance.Unwrap())?? Array.Empty<string>())});");
                        }
                        else if (xprop.PropertyType.IsArray)
                        {
                            sb.AppendLine($@"this.{xprop.Name} = new ObservableArray(...{JsonSerializer.Serialize(xprop.GetValue(instance.Unwrap())?? Array.Empty<string>())});");
                        }

                        else
                        {
                            try
                            {
                                sb.AppendLine($@"this.{xprop.Name} = {JsonSerializer.Serialize(xprop.GetValue(instance.Unwrap())) ?? ""};");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message.ToString());
                            }
                        }
                    }
                }
            }
            sb.Append($@"}}
    GetScripts(){{
        return `{this.Scripts}`;
    }}
    GetHTML(){{
        return `{this.Content(memory).Replace("/", "\\/").Replace("`", "\\`")}`; 
    }}
}}");
            GC.Collect();
            return sb.ToString();
        }
        public string ExtractAtLast(string code)
        {
            StringBuilder ScriptBuilder = new StringBuilder();
            Regex ScriptReg = new Regex(@"[\s\S]*?(<script[^>]*>[\s\S]*?</script>)[\s\S]*?");
            MatchCollection Scripts = ScriptReg.Matches(code);


            Regex regex = new Regex(@"(@\w+[-+]{2}|@\w+)");
            MatchCollection matches = regex.Matches(code);
            Type[] inheritedTypes = XAssembly.GetTypes().Where(t => t.IsSubclassOf(typeof(XavierNode))).ToArray();
            List<PropertyInfo[]> properties = new List<PropertyInfo[]>();
            System.Runtime.Remoting.ObjectHandle instance = null;
            foreach (Type inheritedType in inheritedTypes)
            {
                if (inheritedType.GetType().Name == this.Name) { }
                instance =
    Activator.CreateInstanceFrom(XAssembly.Location,
                                 inheritedType.FullName);

                List<PropertyInfo> props = instance.Unwrap().GetType().GetRuntimeProperties().ToList();

                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        foreach (var property in props)
                        {
                            if (match.Groups[1].Value.Replace("@", "") == property.Name && this.Name == property.GetType().Name)
                            {
                                code = code.Replace(match.Value, property.GetValue(instance.Unwrap()).ToString());
                            }
                            else
                            {

                            }
                        }
                    }
                    foreach (Match m in Scripts)
                    {
                        ScriptBuilder.Append(m.Groups[1].Value);
                    }
                    foreach (Match t in Scripts)
                    {
                        code = code.Replace(t.Groups[1].Value, "");
                    }
                    ScriptBuilder = ScriptBuilder.Replace("<script>", "");
                    ScriptBuilder = ScriptBuilder.Replace("</script>", "");

                    this.Scripts = ScriptBuilder.ToString().Replace("/", "\\/").Replace("`", "\\`");
                    GC.Collect();
                    return code;
                }
            }
            foreach (Match m in Scripts)
            {
                ScriptBuilder.Append(m.Groups[1].Value);
            }
            foreach (Match t in Scripts)
            {
                code = code.Replace(t.Groups[1].Value, "");
            }
            ScriptBuilder = ScriptBuilder.Replace("<script>", "");
            ScriptBuilder = ScriptBuilder.Replace("</script>", "");

            this.Scripts = ScriptBuilder.ToString().Replace("/", "\\/").Replace("`", "\\`");
            ScriptBuilder.Clear();
            GC.Collect();
            return code;
        }
        public void GetPropertiesFromClasses()
        {
            // get all types that inherit from XavierNode
            Type[] inheritedTypes = XAssembly.GetTypes().Where(t => t.IsSubclassOf(typeof(XavierNode))).ToArray();

            // get all properties from each type
            foreach (var inheritedType in inheritedTypes)
            {

                if (inheritedType.Name == this.Name)
                {
                    PropertyInfo[] xprops = inheritedType.GetProperties().ToArray();
                    foreach (var xprop in xprops)
                    {
                        if (!Properties.Contains(xprop))
                        {
                            this.Properties.Add(xprop);
                        }
                    }
                }
            }
            // return the properties
        }
        public void RemoveDuplicates()
        {
            // Create a new List to store the information
            List<PropertyInfo> distinctPropertyInfo = new List<PropertyInfo>();

            // Iterate through the list of PropertyInfo
            foreach (PropertyInfo property in Properties)
            {
                //Check if the Distinct List already contains the item
                if (!distinctPropertyInfo.Contains(property))
                {
                    // Add the item to the Distinct List
                    distinctPropertyInfo.Add(property);
                }
            }
            this.Properties = distinctPropertyInfo;
        }
        public class State
        {
            public string Id { get; set; }
            public string Hash { get; set; }
            public bool RenderStatus(string hash)
            {
                if (Hash != hash)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public List<PropertyInfo> ReadProperties()
        {
            List<PropertyInfo> propertyInfos = new List<PropertyInfo>();

            // Get the type of this class

            Type ClassType = this.GetType();

            // Get properties from this class

            PropertyInfo[] properties = ClassType.GetProperties();

            // Loop through all properties and display their values
            foreach (PropertyInfo property in properties)
            {
                if (property != null)
                {
                    if (!Properties.Contains(property))
                    {
                        Properties.Add(property);
                        //Console.WriteLine("Property: {0}, Value: {1}",

                        //                  property.Name,

                        //                  property.GetValue(this));
                    }
                }
            }
            RemoveDuplicates();
            GC.Collect();
            return Properties;
        }
        public string ExtractPropertyDeclaration(object xavier, PropertyInfo propertyInfo, Assembly assembly)
        {
            System.Runtime.Remoting.ObjectHandle instance = null;
            instance = Activator.CreateInstanceFrom(assembly.Location,
                             xavier.GetType().FullName);
            try
            {

                //.GetRuntimeProperty("Name")
                //.GetValue(instance.Unwrap()));
                if (instance.Unwrap()
                           .GetType()
                           .GetRuntimeProperty("Name")
                           .GetValue(instance.Unwrap()) == this.Name)
                {

                    this.Properties
                        .Where(p => p.Name == propertyInfo.Name && p.GetType().Name == instance
                                                                                         .Unwrap()
                                                                                         .GetType()
                                                                                         .GetRuntimeProperty("Name")
                                                                                         .GetValue(instance.Unwrap()))
                                           .First()
                                           .SetValue(instance
                                                    .Unwrap(), this
                                                             .GetType()
                                                             .GetRuntimeProperties()
                                                             .Where(p => p.Name == propertyInfo.Name)
                                                                    .First()
                                                                    .GetValue(this));
                    if (this.Properties.Where(p => p.Name == propertyInfo.Name && p.GetType().Name == instance.Unwrap().GetType().GetRuntimeProperty("Name").GetValue(instance.Unwrap()))?.ToList().Count() < 2)
                    {

                        if (propertyInfo.PropertyType == typeof(string))
                        {
                            if (propertyInfo.Name == "Code" || propertyInfo.Name.Contains("ClassBody") || propertyInfo.Name.Contains("Content"))
                            {

                            }
                            else
                            {
                                string declaration = $"public {propertyInfo.PropertyType.Name} {propertyInfo.Name} {{ get; set; }} = \"{propertyInfo.GetValue(instance.Unwrap())?.ToString()}\";";
                                GC.Collect();
                                return declaration;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return "";
        }
        public string ExtractVariableList(object xavier, Assembly assembly)
        {
            string result = " ";
            foreach (PropertyInfo property in this.Properties)
            {
                //Check if the property is of type ScriptVariable
                //Extract the ScriptVariable from the property
                string scriptVar = ExtractPropertyDeclaration(xavier, property, assembly);

                //Add it to the list
                result += $"{RW.ClearSlashes(scriptVar)}";
            }

            return result;
        }
        public List<ScriptVariable> ExtractScriptVariables()
        {
            List<ScriptVariable> svs = new List<ScriptVariable>();
            foreach (FieldInfo field in this.GetType().GetFields())
            {
                if (field.FieldType == typeof(ScriptVariable))
                {
                    svs.Add((ScriptVariable)field.GetValue(this));
                }
            }
            return svs;
        }
        public class Renderer
        {
            public Memory memory { get; set; }
            public Renderer(Memory mem)
            {
                memory = mem;
            }
            public XavierNode xavierNode { get; set; }
            public string Error { get; set; }
            public string Render(XavierNode sender)
            {
                xavierNode = sender;
                xavierNode.state = xavierNode.state ??
                    new State()
                    {
                        Id = Guid.NewGuid().ToString(),

                    };
                return xavierNode.Content(memory);
            }
            public async Task<string> RenderAsync(XavierNode sender)
            {
                await Task.Run(() =>
                {
                    xavierNode = sender;
                    xavierNode.state = xavierNode.state ??
                        new State()
                        {
                            Id = Guid.NewGuid().ToString(),

                        };
                    return xavierNode.Content(memory);

                });
                return "";
            }
        }
        public XavierNode(XavierNode node)
        {
            XAssembly = node.XAssembly;
            Name = node.Name;
            Code = node.Code;
            Scripts = node.Scripts;
            Path = node.Path;
            dataset = node.dataset;
            Xid = node.Xid;
            Route = node.Route;
            state = node.state;
            PyImports = node.PyImports;
            ShouldRender = node.ShouldRender;
            BaseUrl = node.BaseUrl;
            Authorize = node.Authorize;
        }
        public XavierNode()
        {
            this.Name = this.GetType().Name;
            this.Path = "";
            this.state = new State();
            this.Code = "";


        }
        public XavierNode(string path, Assembly assembly, Memory memory)
        {
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
            Path = path;
            XAssembly = assembly;
            Code = File.ReadAllText(Path);

            this.GetPropertiesFromClasses();

            RemoveDuplicates();

            Renderer renderer = new Renderer(memory);
            this.state = new State();
            renderer.Render(this);
        }
        public IAsyncResult Invoke(Func<Task<Type>> value)
        {
            return value.Invoke();
        }
        // Define an event handler delegate
        public delegate void EventCallback(object sender, EventArgs e);
        // Declare an event of the same type
        public event EventCallback Event;
        /// <summary>
        /// The array of parameters for taking query strings
        /// </summary>
        public Type[] Types { get; set; }
        /// <summary>
        /// The list of properties for the component which is going to be used by other Xavier components
        /// </summary>
        public List<PropertyInfo> Properties { get; set; } = new List<PropertyInfo>();
        /// <summary>
        /// The map of parameters and their value. This is used to pass the parameter values
        /// to other components in a Blazor application.
        /// </summary>
        public IDictionary<string, string> Parameters { get; set; }
        /// <summary>
        /// This is a parameter method which allows the user to pass in Types as parameters
        /// in order to take query stringss.
        /// </summary>
        /// <param name="types">The array of types to use as parameters</param>
        public void SetParameters(Type[] types)
        {
            Types = types;
        }
        /// <summary>
        /// This is an overridable method which allows the user to define
        /// the list of properties to be used by the component.
        /// </summary>
        /// <param name="properties">The list of properties.</param>
        public virtual void SetProperties(List<PropertyInfo> properties)
        {

            Properties = properties;
        }
        public virtual List<PropertyInfo> GetProperties()
        {
            return this.GetType().GetProperties().ToList();
        }
        /// <summary>
        /// This is an overridable method which allows the user to define
        /// the map of parameters and their values.
        /// </summary>
        /// <param name="parameters">The parameters and their values.</param>
        public virtual void SetParameters(IDictionary<string, string> parameters)
        {
            Parameters = parameters;

        }
        /// <summary>
        /// This is an overridable method which allows the
        /// user to handle events.
        /// </summary>
        /// <param name="sender">The object sending the event.</param>
        /// <param name="e">The arguments associated with the event.</param>
        public virtual void OnPageLoaded(object sender, EventArgs e)
        {
            Event?.Invoke(sender, e);
        }
    }
}