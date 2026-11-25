using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Sharing
{
    public class EmailStringBody
    {
        public static string Generate(string email, string token, string confirmationBaseUrl, string message)
        {
            string encodeToken = Uri.EscapeDataString(token);
            string encodeEmail = Uri.EscapeDataString(email);
            string safeMessage = WebUtility.HtmlEncode(message);
            string confirmationUrl = $"{confirmationBaseUrl}?email={encodeEmail}&code={encodeToken}";
            return $@"
            <html> 
                <head>
                    <style>
                        .button {{
                            border: none;
                            border-radius: 10px;
                            padding: 15px 30px;
                            color: #fff !important;
                            display: inline-block;
                            background: linear-gradient(45deg, #ff7e5f, #feb47b);
                            cursor: pointer;
                            text-decoration: none;
                            box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
                            font-size: 16px;
                            font-weight: bold;
                            font-family: 'Arial', sans-serif;
                        }}
                    </style>
                </head>
                <body>
                    <h1>{safeMessage}</h1>
                    <p>Please click the button below to confirm your email address.</p>
                       <a href=""{confirmationUrl}"" style="" display: inline-block; padding: 15px 30px; background-color: #ff7e5f; color: #ffffff; text-decoration: none; border-radius: 10px; font-weight: bold; ""> Confirm Email </a>    
                </body>
            </html>";
        }
    }
}
