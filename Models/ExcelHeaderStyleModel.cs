using System.Drawing;

namespace task_sync_web.Models
{
    public class ExcelHeaderStyleModel
    {
        public Color FirstColor { get; set; }
        public Color SecondColor { get; set; }
        public int[] FirstColorBackgroundColorColumnNumber { get; set; }
        public int[] SecondColorBackgroundColorColumnNumber { get; set; }

        public ExcelHeaderStyleModel()
        {
            FirstColor = Color.FromArgb(250, 206, 215);
            SecondColor = Color.FromArgb(205, 250, 241);
        }
    }
}