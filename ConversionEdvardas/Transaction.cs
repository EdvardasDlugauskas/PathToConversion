using System;
using System.Drawing;
using Console=Colorful.Console;

namespace ConversionEdvardas
{
    public class Transaction
    {
        // http://json2csharp.com/
        public DateTime LogTime { get; set; }
        public int TransactionType { get; set; }
        public int CookieId { get; set; }
        public bool CookiesEnabled { get; set; }
        public string Campaign { get; set; }
        public string Media { get; set; }
        public string Banner { get; set; }
        public string TrackingSetup { get; set; }
        public int? ID_LogPoints { get; set; }
        public string LogPointName { get; set; }
        public string UrLfrom { get; set; }
        public string UrLto { get; set; }
        public string ClientSite { get; set; }

        public void AttributeTo(Transaction other)
        {
            try
            {
                Banner = other.Banner;
                Campaign = other.Campaign;
                Media = other.Media;
            }
            catch (NullReferenceException) { }

        }

        public override string ToString()
        {
            return $"|{LogTime,22}|{Data.TransTypeName[TransactionType],-15}|{Campaign,-18}|" +
                   $"{Media,-10}|{Banner,-18}|{LogPointName,-14}|";
        }

        public void Print()
        {
            var color = Color.White;
            switch (TransactionType)
            {
                case Data.Impression:
                    color = Color.Aquamarine;
                    break;
                case Data.Click:
                    color = Color.Aqua;
                    break;
                case Data.TrackingPoint:
                    color = Color.Yellow;
                    break;
            }
            if (LogPointName == "Thank You") color = Color.LawnGreen;

            Console.WriteLine(ToString(), color);
        }


    }
}
