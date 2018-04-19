namespace QnAMaker
{
    public class TermObject
    {
        public int Index { get; set; }
        public int OriginalIndex { get; set; }
        public int ListId { get; set; }
        public string Term { get; set; }
    }

    public class ClassificationObject
    {
        public bool ReviewRecommended { get; set; }
        public Category Category1 { get; set; }
        public Category Category2 { get; set; }
        public Category Category3 { get; set; }
    }

    public class Category
    {
        public double Score { get; set; }
    }
}
