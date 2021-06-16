namespace AL.Data.Classes
{
    public class ClassesDatum : DatumBase<Class>
    {
        public Class Mage { get; set; }
        public Class Merchant { get; set; }
        public Class Paladin { get; set; }
        public Class Priest { get; set; }
        public Class Ranger { get; set; }
        public Class Rogue { get; set; }
        public Class Warrior { get; set; }
    }
}