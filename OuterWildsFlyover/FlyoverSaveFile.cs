using System.Linq;
using System.Collections.Generic;

namespace OuterWildsFlyover
{
    internal class FlyoverSaveFile
    {
        public List<FlyoverProfile> Profiles { get; set; } = new List<FlyoverProfile>();

        public FlyoverProfile GetOrCreateProfile(string profileName)
        {
            FlyoverProfile returnVal = Profiles.FirstOrDefault(p => p.Name == profileName);
            if (returnVal == null)
            {
                returnVal = new FlyoverProfile(profileName);
                if (returnVal.Saves.Count == 0)
                    returnVal.Saves.Add(new FlyoverSave());
                Profiles.Add(returnVal);
            }

            return returnVal;
        }
    }

    internal class FlyoverProfile
    {
        public string Name { get; set; } = "";
        public List<FlyoverSave> Saves { get; set; } = new List<FlyoverSave>();

        public FlyoverProfile() { }
        public FlyoverProfile(string name)
        {
            Name = name;
        }

        public FlyoverSavePoi[] GetCollectedPois()
        {
            if (Saves.Count == 0) return new FlyoverSavePoi[0];
            else return Saves[0].Pois.ToArray();
        }
    }

    internal class FlyoverSave
    {
        public string Id { get; set; } = "default";
        public List<FlyoverSavePoi> Pois { get; set; } = new List<FlyoverSavePoi>();
    }

    internal class FlyoverSavePoi
    {
        public int Id { get; set; }
        public bool IsNew { get; set; }
    }
}
