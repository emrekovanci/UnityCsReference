// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Reporting;

namespace UnityEditor.Build.Reporting
{
    ///<summary>The StrippingInfo object contains information about which native code modules in the engine are still present in the build, and the reasons why they are still present.</summary>
    public class StrippingInfo : ScriptableObject, ISerializationCallbackReceiver
    {
        ///<summary>The native engine modules that were included in the build.</summary>
        ///<remarks>You can pass each of these to <see cref="StrippingInfo.GetReasonsForIncluding" /> to obtain information about what caused a module to be included in the build.</remarks>
        public IEnumerable<string> includedModules {  get { return modules; } }

        ///<summary>Returns the list of dependencies or reasons that caused the given entity to be included in the build.</summary>
        ///<remarks>The returned list of strings may include names of components, internal engine objects, other modules, or other human-readable reasons. To obtain further information, you can pass each string back into GetReasonsForIncluding again.
        ///
        ///For example, calling GetReasonsForIncluding("Physics Module") may return a list that includes "Rigidbody", and you can then call GetReasonsForIncluding("Rigidbody") to get more information about which Scenes or assets are using the Rigidbody component.</remarks>
        ///<param name="entityName">The name of an engine module, class, or other entity present in the build.</param>
        ///<returns>A list of modules, classes, or other entities that caused the provided entity to be included in the build.</returns>
        public IEnumerable<string> GetReasonsForIncluding(string entityName)
        {
            HashSet<string> deps;
            return dependencies.TryGetValue(entityName, out deps) ? deps : Enumerable.Empty<string>();
        }

        internal const string RequiredByScripts = "Required by Scripts";

        [System.Serializable]
        internal struct SerializedDependency
        {
            [SerializeField]
            public string key;
            [SerializeField]
            public List<string> value;
            [SerializeField]
            public string icon;
            [SerializeField]
            public int size;
        }

        [SerializeField] internal List<SerializedDependency> serializedDependencies;

        [SerializeField] internal List<string> modules = new List<string>();

        // Not needed any more since we have SerializedDependency.size now, but keep it as long as the BuildReport UI uses it.
        [SerializeField] internal List<int> serializedSizes = new List<int>();

        [SerializeField] internal Dictionary<string, HashSet<string>> dependencies = new Dictionary<string, HashSet<string>>();

        [SerializeField] internal Dictionary<string, int> sizes = new Dictionary<string, int>();

        [SerializeField] internal Dictionary<string, string> icons = new Dictionary<string, string>();

        [SerializeField] internal int totalSize = 0;

        void OnEnable()
        {
            SetIcon(RequiredByScripts, "class/MonoScript");
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            serializedDependencies = new List<SerializedDependency>();
            foreach (var dep in dependencies)
            {
                var list = new List<string>();
                foreach (var nc in dep.Value)
                    list.Add(nc);
                SerializedDependency sd;
                sd.key = dep.Key;
                sd.value = list;
                sd.icon = icons.ContainsKey(dep.Key) ? icons[dep.Key] : "class/DefaultAsset";
                sd.size = sizes.ContainsKey(dep.Key) ? sizes[dep.Key] : 0;
                serializedDependencies.Add(sd);
            }
            serializedSizes = new List<int>();
            foreach (var module in modules)
            {
                serializedSizes.Add(sizes.ContainsKey(module) ? sizes[module] : 0);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            dependencies = new Dictionary<string, HashSet<string>>();
            icons = new Dictionary<string, string>();
            sizes = new Dictionary<string, int>();
            for (int i = 0; i < serializedDependencies.Count; i++)
            {
                HashSet<string> depends = new HashSet<string>();
                foreach (var s in serializedDependencies[i].value)
                    depends.Add(s);
                dependencies.Add(serializedDependencies[i].key, depends);
                icons[serializedDependencies[i].key] = serializedDependencies[i].icon;
                sizes[serializedDependencies[i].key] = serializedDependencies[i].size;
            }
            for (int i = 0; i < serializedSizes.Count; i++)
                sizes[modules[i]] = serializedSizes[i];
        }

        internal void RegisterDependency(string obj, string depends)
        {
            if (!dependencies.ContainsKey(obj))
                dependencies[obj] = new HashSet<string>();
            dependencies[obj].Add(depends);
            if (!icons.ContainsKey(depends))
                SetIcon(depends, "class/" + depends);
        }

        internal void AddModule(string module, bool appendModuleToName = true)
        {
            var moduleName = appendModuleToName ? ModuleName(module) : module;
            var packageName = $"com.unity.modules.{module.ToLower()}";
            if (!modules.Contains(moduleName))
                modules.Add(moduleName);
            if (!sizes.ContainsKey(moduleName))
                sizes[moduleName] = 0;

            // Fall back to default icon for unknown modules
            if (!icons.ContainsKey(moduleName))
                SetIcon(moduleName, $"package/{packageName}");
        }

        internal void SetIcon(string dependency, string icon)
        {
            icons[dependency] = icon;
            if (!dependencies.ContainsKey(dependency))
                dependencies[dependency] = new HashSet<string>();
        }

        internal void AddModuleSize(string module, int size)
        {
            if (modules.Contains(module))
                sizes[module] = size;
        }

        internal static StrippingInfo GetBuildReportData(BuildReport report)
        {
            if (report == null)
                return null;
            var allStrippingData = report.GetAppendices<StrippingInfo>();
            if (allStrippingData.Length > 0)
                return allStrippingData[0];

            var newData = ScriptableObject.CreateInstance<StrippingInfo>();
            report.AddAppendix(newData);
            return newData;
        }

        internal static string ModuleName(string module)
        {
            return module + " Module";
        }
    }
}
