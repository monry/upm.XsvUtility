namespace Monry.XsvUtility.Fixture
{
    public struct ItemIndexed
    {
        [XsvColumn(0)]
        public string Hash { get; set; }
        [XsvColumn(1)]
        public int Size { get; set; }
    }

    public struct ItemNamed
    {
        [XsvColumn("hash")]
        public string Hash { get; set; }
        [XsvColumn("size")]
        public int Size { get; set; }
    }
}