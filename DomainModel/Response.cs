using System;
using System.Collections.Generic;
using System.Text;

namespace DomainModel
{
    public class Response
    {
        public string Status { get; set; }
        public string Body { get; set; }
    }


    public static class StatusResponse
    {
        public enum STATUSCODE : int
        {
            OK = 1,
            CREATED = 2,
            UPDATED = 3,
            BADREQUEST = 4,
            NOTFOUND = 5,
            ERROR = 6
        }


        public enum REQUESTERRORFIELD : int
        {
            DEFAULT = 0,
            METHOD = 1,
            PATH = 2,
            DATE = 3,
            PATHRESOURSE = 4,
            ILLEGALMETHOD = 5,
            ILLEGALDATE = 6,
            ILLEGALBODY = 7,
            MISSINGBODY=8
        }


        /// <summary> Fetch the string version of the status code </summary>
        /// <param name="CODE"> The current response's status code type </param>
        public static string GetStatusCodeText(STATUSCODE CODE)
        {
            string txt = (int)CODE + " ";
            switch (CODE)
            {
                case STATUSCODE.OK:
                    txt += "Ok";
                    break;
                case STATUSCODE.CREATED:
                    txt += "Created";
                    break;
                case STATUSCODE.UPDATED:
                    txt += "Updated";
                    break;
                case STATUSCODE.BADREQUEST:
                    txt += "Bad Request";
                    break;
                case STATUSCODE.NOTFOUND:
                    txt += "Not Found";
                    break;
                case STATUSCODE.ERROR:
                    txt += "Error";
                    break;
                default:
                    break;
            }
            return txt;
        }


        /// <summary> Fetch the string version of the status code reason phrase </summary>
        /// <param name="CODE"> The current response's status code type </param>
        public static void GetStatusCodeReasonText(REQUESTERRORFIELD FIELD, ref string code)
        {
            switch (FIELD)
            {
                case REQUESTERRORFIELD.METHOD:
                    code += " - missing method";
                    break;
                case REQUESTERRORFIELD.ILLEGALMETHOD:
                    code += " - illegal method";
                    break;
                case REQUESTERRORFIELD.PATH:
                    code += " - missing path";
                    break;
                case REQUESTERRORFIELD.DATE:
                    code += " - missing date";
                    break;
                case REQUESTERRORFIELD.ILLEGALDATE:
                    code += " - illegal date";
                    break;
                case REQUESTERRORFIELD.PATHRESOURSE:
                    code += " - missing resource";
                    break;
                case REQUESTERRORFIELD.ILLEGALBODY:
                    code += " - illegal body";
                    break;
                case REQUESTERRORFIELD.MISSINGBODY:
                    code += " - missing body";
                    break;
                case REQUESTERRORFIELD.DEFAULT:
                    code += " - missing method, missing path, missing date, missing body";
                    break;
                default:
                    break;
            }
        }

    }


}