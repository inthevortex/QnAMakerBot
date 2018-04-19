using System.Collections.Generic;

namespace QnAMaker
{
    public class Answers
    {
        public string Answer { get; set; }
        public List<string> Questions { get; set; }
        public double Score { get; set; }
    }

    public class QnAResponse
    {
        public List<Answers> Answers { get; set; }
    }
}
