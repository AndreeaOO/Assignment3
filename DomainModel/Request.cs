using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DomainModel
{
    public class Request
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public long Date { get; set; }
        public string Body { get; set; }


        private readonly List<string> _LegalMethods = new List<string>(new string[] { "create", "read", "update", "delete", "echo"});

        public bool ValidMethod()
        {
            if (_LegalMethods.Contains(Method.Trim().ToLower()))
                return true;
            return false;
        }


        public bool ValidDate()
        {
            foreach (char c in Date.ToString().Trim())
            {
                if ((c < '0' || c > '9' || Date.ToString().Trim().Length < 10))
                    return false;
            }

            return true;
        }


        public bool ValidBody()
        {
            if (string.IsNullOrEmpty(Body))
                return false;
            if ((Body.Trim()[0] == '{') && (Body.Trim()[Body.Trim().Length - 1] == '}'))
                return true;
            else
                return false;
        }


        public bool ValidPath()
        {
            if (string.IsNullOrEmpty(Method) || string.IsNullOrEmpty(Path))
                return false;
            var p = Path.Trim().ToLower();
            switch (Method.Trim().ToLower())
            {
                case "create":
                    if (!p.Contains("/categories") || p.Contains("/categories/"))
                        return false;                 
                    break;
                case "read":
                    var num = new String(p.Where(Char.IsDigit).ToArray());
                    if(string.IsNullOrEmpty(num) && p.Split(new [] { "categories/" }, StringSplitOptions.None).Length > 1)
                          return false;
                    else if (!p.Contains("/categories") && !p.Contains("/categories/" + num))
                        return false;
                    break;
                case "update":
                case "delete":
                    var n = new String(p.Where(Char.IsDigit).ToArray());
                    if (string.IsNullOrEmpty(n) || string.IsNullOrEmpty(n) || !p.Contains("/categories/" + n))
                        return false;
                    break;              
                default:
                    break;
            }

            return true;
        }


    }
}