using Sdl.MultiSelectComboBox.API;
using TAMHubApp.ViewModels;

namespace TAMHubApp.Models
{
    public class EnumMultiSelectItem : BaseViewModel, IItemEnabledAware, IItemGroupAware
    {
        private int _id;
        private string _name;
        private bool _isEnabled;
        private int _selectedOrder;
        private IItemGroup _group;

        public EnumMultiSelectItem()
        {
            _isEnabled = true;
            _selectedOrder = -1;
        }
        public int Id
        {
            get => _id;
            set
            {
                if (_id.Equals(value))
                {
                    return;
                }
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }
        public string Name
        {
            get => _name;
            set
            {
                if (_name != null && string.Compare(_name, value, StringComparison.InvariantCulture) == 0)
                {
                    return;
                }

                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled.Equals(value))
                {
                    return;
                }

                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        public int SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                if (_selectedOrder.Equals(value))
                {
                    return;
                }

                _selectedOrder = value;
                OnPropertyChanged(nameof(SelectedOrder));
            }
        }
        public IItemGroup Group
        {
            get => _group;
            set
            {
                if (_group != null && _group.Equals(value))
                {
                    return;
                }

                _group = value;
                OnPropertyChanged(nameof(Group));
            }
        }
    }
}
