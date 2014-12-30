namespace LFNet.Common.Threading.Internal
{
    public class CanceledWorkItemsGroup
    {
        private bool _isCanceled;
        public static readonly CanceledWorkItemsGroup NotCanceledWorkItemsGroup = new CanceledWorkItemsGroup();

        public bool IsCanceled
        {
            get
            {
                return this._isCanceled;
            }
            set
            {
                this._isCanceled = value;
            }
        }
    }
}

