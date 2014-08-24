using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TweakScale
{
    static class ScaleDatabase
    {
        static private Dictionary<string, ScaleInfo> _scales = new Dictionary<string,ScaleInfo>();
        static private HashSet<string> _hidden = new HashSet<string>();
        
        static public ScaleInfo Lookup(string name)
        {
            if (_scales.ContainsKey(name))
            {
                return _scales[name];
            }
            else
            {
                return ScaleInfo.DefaultValue;
            }
        }

        static public bool IsHidden(string name)
        {
            return _hidden.Contains(name);
        }

        static public void Hide(string name, bool hidden)
        {
            if (hidden)
            {
                _hidden.Add(name);
            }
            else
            {
                _hidden.Remove(name);
            }
            Save();
        }

        static public void Update(string name, ScaleInfo info)
        {
            _scales[name] = info;
            Save();
        }

        static public void Remove(string name)
        {
            _scales.Remove(name);
            Save();
        }

        static public void Save()
        {
            var filename = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ScaleEdit.txt");

            Tools.Logf("Filename: {0}", filename);

            //var filename = "E:\\KSP_Test\\ScaleEdit.txt";

            const string partFormat1 = "@PART[{0}] // {1}\r\n{{\r\n\tMODULE\r\n\t{{\r\n\t\tname = TweakScale\r\n\t\ttype = {2}\r\n\t\tdefaultScale = {3}\r\n\t}}\r\n}}";
            const string partFormat2 = "@PART[{0}] // {1}\r\n{{\r\n\tMODULE\r\n\t{{\r\n\t\tname = TweakScale\r\n\t\ttype = {2}\r\n\t}}\r\n}}";
            const string partFormat3 = "@PART[{0}] // {1}\r\n{{\r\n\tcategory = none\r\n}}";
            using (var sw = new StreamWriter(filename, false))
            {
                foreach (var scale in _scales)
                {
                    var title = PartLoader.getPartInfoByName(scale.Key).title;
                    var cfg = ScaleConfig.AllConfigs.First(a => a.name == scale.Value.type);

                    if (cfg.defaultScale == scale.Value.defaultScale)
                    {
                        sw.WriteLine(string.Format(partFormat2, scale.Key, title, scale.Value.type));
                    }
                    else
                    {
                        sw.WriteLine(string.Format(partFormat1, scale.Key, title, scale.Value.type, scale.Value.defaultScale));
                    }
                }
                foreach (var part in _hidden)
                {
                    var title = PartLoader.getPartInfoByName(part).title;

                    sw.WriteLine(string.Format(partFormat3, part, title));
                }
            }
        }
    }
}
