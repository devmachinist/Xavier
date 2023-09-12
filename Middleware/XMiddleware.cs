using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Xavier;
using System.ComponentModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Emit;
using System;
using System.Security.Permissions;
using Newtonsoft.Json.Linq;
using NuGet.Packaging;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Xavier
{
    public static class XNodesExtension
    {
        private static Dictionary<string, List<Delegate> > actionLookup { get; set; }
        public static List<MethodInfo> MethodArray { get; set; } = new List<MethodInfo>();
        /// <summary>
        /// Used to Map all Xavier Nodes with @page in their first line to the Name of the xavier file.
        /// Pass in the the pattern of the action ex.{ "    " }, and the location(folderPath) to the .xavier files.
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="pattern"></param>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static IEndpointConventionBuilder MapXavierNodes(this IEndpointRouteBuilder endpoints, string pattern, string folderPath, Memory memory)
        {
            // try {
            return endpoints.Map(pattern, async context =>
             {
                 //parse the request path
                 var filename = $"{context.Request.Path.Value}.xavier";

                 //construct the complete file path
                 var filePath = folderPath + filename;

                 //check if the file exists
                 if (File.Exists(filePath))
                 {

                     //read the content of the file
                     var content = File.ReadAllText(filePath);
                     //check whether the first line is @page
                     if (content.Split(Environment.NewLine).FirstOrDefault() != "@page")
                         return;

                     //get the component name
                     var componentName = context.Request.Path.Value.Split("/").LastOrDefault();
                     var component = (memory.XavierNodes.Where(p => (p as XavierNode).Name == componentName).FirstOrDefault() as XavierNode);
                     var component2 = (memory.XavierNodes.Where(p => (p as XavierNode).Name == componentName).FirstOrDefault());
                     var html = component.Content(memory);
                     var js = component.Scripts;
                     var controllerName = component.Name;
                     var xid = component.Xid;
                     // set the component as the response
                     MemoryStream stream = new MemoryStream();
                     StreamWriter writer = new StreamWriter(stream);
                     var reader = html + "<script async type='module'>"  + component2.GetType().GetMethod("GenerateJavascriptApi").Invoke(component2, new object[] {})+ js+ "</script>";
                     writer.Flush();
                     stream.Position = 0;
                     context.Response.Clear();
                     context.Response.ContentType = component.ContentType;
                     await context.Response.WriteAsync(reader.Replace("@page", ""));
                     
                     

                     endpoints.MapGet(pattern, () => { context.Response.Body.ToString(); });

                    writer.Close();
                 }
                 else
                 {
                     context.Response.Clear();
                     context.Response.ContentType = "text/html";
                     context.Response.StatusCode = 404;
                     await context.Response.WriteAsync(File.ReadAllText(memory.StaticFallback));

                     endpoints.MapGet(pattern, () => { context.Response.Body.ToString(); });
                 }
             });

            //}catch(Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //}
            //    return endpoints.MapGet(pattern, () => { });
        }

        public static string GenerateJavascript(Type theType, string typename)
        {
            string jsModuleName = typename;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("var " + jsModuleName + " = {};");

            //Loop over all of the methods in the type and create a JS function for each
            foreach (var methodInfo in theType.GetMethods())
            {
                sb.AppendLine(jsModuleName+"."+methodInfo.Name + " = async function(parameters){ ");
                sb.AppendLine("try {");
                sb.AppendLine(" var apiCall = await fetch('~/api/" + typename + "/" + methodInfo.Name + "', {method: 'POST', body: JSON.stringify(parameters)});");
                sb.AppendLine(" return await apiCall.json();");
                sb.AppendLine("} catch(err) {");
                sb.AppendLine("console.log(err);");
                sb.AppendLine("} };");
            }

            //Write the output to a string
            return sb.ToString();
        }
        public static void CreateCSharpAPI(Type apiType)
        {
            //get all the public methods of the given type
            MethodInfo[] methods = apiType.GetMethods();

            //run loop for all the methods
            foreach (MethodInfo method in methods)
            {
                //create a new delegate
                Delegate del = Delegate.CreateDelegate(
                    typeof(Action<object, object>),
                    method
                );

                //add to the list of delegates
                List<Delegate> delegates = new List<Delegate>();
                delegates.Add(del);

                //add the delegate to the dictionary
                actionLookup.Add(method.Name, delegates);
            }
        }

    }
}