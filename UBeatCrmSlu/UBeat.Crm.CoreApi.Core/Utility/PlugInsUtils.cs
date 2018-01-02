using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Runtime.Loader;

namespace UBeat.Crm.CoreApi.Core.Utility
{
    public class PlugInsUtils
    {
        private static PlugInsUtils _instance;
        public List<string> PlugInFiles = new List<string>();
        public Dictionary<string, string> TypesInFile = new Dictionary<string, string>();
        private PlugInsUtils() {
        }
        public System.Type getTypeWithName(string typename) {
            try
            {

                if (TypesInFile.ContainsKey(typename))
                {
                    Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(TypesInFile[typename]);
                    return assembly.GetType(typename);
                }
            }catch(Exception ex){
            }
            return null;
        }

        public static PlugInsUtils getInstance() {
            if (_instance == null) {
                _instance = new PlugInsUtils();
            }
            return _instance;
        }
        private void init() {
            var config = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("plugin.json")
              .Build();
            List<string> plugins = new List<string>();
            IEnumerator<IConfigurationSection> it = config.GetSection("PluginProjectList").GetChildren().GetEnumerator();
            while (it.MoveNext())
            {
                IConfigurationSection item = it.Current;
                if (item.Value != null && item.Value.Length != 0)
                {
                    plugins.Add(item.Value);
                }
            }
            dynamic type = this.GetType();
            string currentDirectory = Path.GetDirectoryName(type.Assembly.Location);
            DirectoryInfo d = new DirectoryInfo(currentDirectory);

            string filesplite = "\\";
            string env = config.GetSection("Environment").Value;
            if (env == null) env = "";
            if (env.ToUpper().Equals("DEBUG"))
            {
                if (!(d != null
                    && d.Parent != null
                    && d.Parent.Parent != null
                    && d.Parent.Parent.Parent != null
                    && d.Parent.Parent.Parent.Parent != null))
                {
                    return;
                }
                currentDirectory = d.Parent.Parent.Parent.Parent.FullName;
                foreach (string item in plugins)
                {
                    string filePath = "";
                    if (item.EndsWith(".dll"))
                    {
                        filePath = item;
                    }
                    else
                    {
                        filePath = string.Format("{0}{1}{2}{1}obj{1}Debug{1}netcoreapp1.1{1}{2}.dll", currentDirectory, filesplite, item);
                    }
                    FileInfo fileInfo = new FileInfo(filePath);
                    if (fileInfo.Exists == false) continue;
                    PlugInFiles.Add(fileInfo.FullName);
                    Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(fileInfo.FullName);
                    IEnumerator< TypeInfo> typeit  = assembly.DefinedTypes.GetEnumerator();

                    while (typeit.MoveNext()) {
                        TypeInfo t = typeit.Current;
                        TypesInFile.Add(t.FullName, fileInfo.FullName);
                    }

                }
            }
            else
            {
                filesplite = "/";
                foreach (string item in plugins)
                {
                    string filePath = string.Format("{0}{1}{2}.dll", currentDirectory, filesplite, item);
                    FileInfo fileInfo = new FileInfo(filePath);
                    if (fileInfo.Exists == false) continue;
                    PlugInFiles.Add(fileInfo.FullName);
                    Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(fileInfo.FullName);
                    IEnumerator<TypeInfo> typeit = assembly.DefinedTypes.GetEnumerator();

                    while (typeit.MoveNext())
                    {
                        TypeInfo t = typeit.Current;
                        TypesInFile.Add(t.FullName, fileInfo.FullName);
                    }
                }
            }
        }
    }
}
