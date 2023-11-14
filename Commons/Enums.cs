namespace task_sync_web.Commons
{
    public partial class Enums
    {
        public enum ModalType
        {
            None = 0, Create = 1, Edit= 2
        }

        public enum CollapseState
        {
            Show = 1, Hide = 2
        }

        public enum GetState
        {
            Default = 1, Search = 2, ExcelOutput
        }

        public enum TaskStyle
        {
            PrimaryItem = 1, SecondaryItem = 2, TertiaryItem
        }

        public enum ActionStyle
        {
            Index = 1, InterruptModal = 2
        }
    }
}