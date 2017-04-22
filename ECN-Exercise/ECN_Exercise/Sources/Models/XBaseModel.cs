using ReactiveUI;

namespace ECN_Exercise.Sources.Models
{
    public class XBaseModel : ReactiveObject
    {
        private int _id;

        public int Id
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
    }
}