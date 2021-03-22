using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Core.Utility;

namespace UBeat.Crm.CoreApi.Utility
{
    public class AssemblyPluginUtils
    {
        private static AssemblyPluginUtils instance = null;
        private List<Assembly> assembles = new List<Assembly>();
        public static AssemblyPluginUtils getInstance()
        {
            if (instance == null)
            {
                instance = new AssemblyPluginUtils();
            }
            return instance;
        }
        private AssemblyPluginUtils()
        {
            LoadUKAssemble();
        }
        private void LoadUKAssemble()
        {

            dynamic type = this.GetType();
            string currentDirectory = Path.GetDirectoryName(type.Assembly.Location);
            System.IO.DirectoryInfo dir = new DirectoryInfo(currentDirectory);
            FileInfo[] files = dir.GetFiles("UBeat.Crm.CoreApi.*.dll");
            foreach (FileInfo f in files)
            {
                string assemblename = f.Name.Substring(0, f.Name.Length - 4);
                var assembly = Assembly.Load(new AssemblyName(assemblename));
                assembles.Add(assembly);
            }
        }
        public Type getUKType(string typename, bool isrec = true)
        {
            foreach (Assembly a in assembles)
            {
                try
                {
                    Type t = a.GetType(typename);
                    if (t != null) return t;
                }
                catch (Exception ex)
                {
                }
            }
            if (isrec)
            {
                return PlugInsUtils.getInstance().getTypeWithName(typename, false);
            }
            return null;
        }
    }
}
