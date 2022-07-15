namespace KnowledgeMining.UI.Services.Search.Models
{
    public class NodeInfo
    {
        public NodeInfo(int index, string colorId)
        {
            Index = index;
            ColorId = colorId;
            LayerCornerStone = -1;
        }
        public int Index { get; set; }
        public string ColorId { get; set; }
        public int Layer { get; set; }
        public int Distance { get; set; }
        public int ChildCount { get; set; }
        public int LayerCornerStone { get; set; }
    }
}
