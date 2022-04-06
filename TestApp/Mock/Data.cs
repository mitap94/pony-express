using Parcels.Models;
using Requests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Models;

namespace Tester.Mock
{
    class Data
    {
        public static readonly List<string> Cities = new List<string>
            { "Kragujevac", "Batocina", "Lapovo", "Jagodina", "Zajecar",
              "Sabac", "Nis", "Aleksinac", "Aleksandrovac", "Krusevac", "Leskovac", "Vranje", "Arilje",
              "Negotin", "Novi Sad", "Vrnjacka Banja", "Arandjelovac", "Subotica", "Zrenjanin", "Markovac",
              "Svilajnac", "Vrsac", "Smederevo", "Pozarevac", "Bajina Basta", "Cajetina", "Prijepolje",
              "Zlatar", "Zlatibor", "Pirot", "Sombor", "Lazarevac", "Valjevo", "Loznica", "Cacak", "Uzice",
              "Gornji Milanovac", "Priboj", "Sjenica", "Novi Pazar", "Prokuplje", "Cuprija", "Paracin", "Vlasotince",
              "Pancevo", "Sremska Mitrovica", "Backa Palanka"};
        
        public static readonly List<string> Names = new List<string>
            { "Aleksandar", "Sasa", "Sanja", "Visnja", "Aleksandra", "Zoran",
              "Djordje", "Dragan", "Milan", "Katarina", "Stefan", "Bojan",
              "Marko", "Vladimir", "Vladica", "Branimir", "Branislav", "Nevena",
              "Dragomir", "Nikola", "Stojan", "Petar", "Dimitrije", "Borivoje",
              "Jovana", "Jelena", "Ana", "Vanja", "Marina", "Mia", "Tea", "Zorana",
              "Svetlana", "Simona", "Nadja", "Milica"};

        public static readonly List<string> Content = new List<string>
            { "igracke", "delovi za racunar", "saksija", "kozmetika", "ventilator",
              "alat", "civiluk", "mis", "slusalice", "elektronika", "retrovizor", "torba",
              "neseser", "ranac", "jakna", "pantalone", "nakit", "bizuterija", "patike",
              "cipele", "cizme", "busilica", "testera", "album za slike", "ram za slike",
              "rokovnik", "pederusa", "cinije", "case", "escajg", "posudje", "mobilni telefon",
              "televizor", "mikser", "carape", "majica", "masinica za sisanje", "povodac",
              "privezak za kljuceve", "deo za auto", "turbina", "EGR ventil", "katalizator",
              "pumpa goriva", "polica", "rukavice", "upaljac", "naocare", "krema", "lance",
              "sat", "pametni sat", "termos", "uticnica", "multimetar" };

        public static readonly List<User> CreatedUserList = new List<User>();

        public static readonly List<Request> PendingRequestList = new List<Request>();

        public static readonly List<Parcel> PendingParcelList = new List<Parcel>();

        public static string GetRandomName()
        {
            return Names[new Random().Next(Names.Count)];
        }

        public static string GetRandomCity()
        {
            return Cities[new Random().Next(Cities.Count)];
        }
        public static string GetRandomContent()
        {
            return Content[new Random().Next(Content.Count)];
        }

        public static double GetRandomWeight()
        {
            double randomValue = new Random().NextDouble();
            if (randomValue == 0.0)
            {
                randomValue = 0.1;
            }

            return randomValue * 10;
        }

        public static User GetRandomUser()
        {
            return CreatedUserList[new Random().Next(CreatedUserList.Count)];
        }

        public static Request GetRandomRequest()
        {
            return PendingRequestList[new Random().Next(PendingRequestList.Count)];
        }

        public static Parcel GetRandomParcel()
        {
            return PendingParcelList[new Random().Next(PendingParcelList.Count)];
        }
    }
}
