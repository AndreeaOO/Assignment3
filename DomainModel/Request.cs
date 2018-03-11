﻿using System;
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


        private readonly List<string> _LegalMethods = new List<string>(new string[] { "create", "read", "update", "delete", "echo" });

        public bool ValidMethod(string s)
        {
            if (_LegalMethods.Contains(s.Trim().ToLower()))
                return true;
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
                    if (!p.Contains("/categories"))
                        return false;
                    break;
                case "read":
                    var num = new String(p.Where(Char.IsDigit).ToArray());
                    if (!p.Contains("/categories") && !p.Contains("/categories/" + num))
                        return false;
                    break;
                case "update":
                case "delete":
                    var n = new String(p.Where(Char.IsDigit).ToArray());
                    if (string.IsNullOrEmpty(n) || !p.Contains("/categories/" + n))
                        return false;
                    break;              
                default:
                    break;
            }

            return true;
        }


    }
}