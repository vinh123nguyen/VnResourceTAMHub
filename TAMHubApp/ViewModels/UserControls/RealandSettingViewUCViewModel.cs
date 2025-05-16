using System.Collections.ObjectModel;
using System.Windows.Input;
using TAMHubApp.Data;
using TAMHubApp.Data.Entities;
using TAMHubApp.Infrastructures.Enums;
using TAMHubApp.Infrastructures;
using TAMHubApp.Models;
using TAMHubApp.Services;
using System.Collections;
using TAMHubApp.Infrastructures.Helpers;
using TAMHubApp.Commands;

namespace TAMHubApp.ViewModels.UserControls
{
    public class RealandSettingViewUCViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IUnitOfWork _unitOfWork;
        private ObservableCollection<EnumMultiSelectItem> AllEnumRealand { get; set; }
        public RealandSettingViewUCViewModel(IUnitOfWork unitOfWork, MainViewModel mainViewModel)
        {
            //_mainViewModel = mainViewModel;
            //_unitOfWork = unitOfWork;

            //var listEnum = new List<EnumMultiSelectItem>();
            //for (int i = 0; i <= 31; i++)
            //{
            //    var t = RealandObjectConvert.IOMode(i);
            //    if(string.IsNullOrEmpty(t))
            //        continue;
            //    listEnum.Add(new EnumMultiSelectItem()
            //    {
            //        Id = i,
            //        Name = t
            //    });
            //}

            //AllEnumRealand = new ObservableCollection<EnumMultiSelectItem>(listEnum);
            //ListZKInType = AllEnumRealand;
            //ListZKOutType = AllEnumRealand;

            //SelectedInItemsChangedCommand = new SelectedItemsChangedCommand(UpdateSelectedInItems, CanListenToSelectedItemsChangedExample);
            //SelectedOutItemsChangedCommand = new SelectedItemsChangedCommand(UpdateSelectedOutItems, CanListenToSelectedItemsChangedExample);
            //SaveConfig = new RelayCommand<object>(x => { return true; }, SaveConfigExcute);
            //LoadConfig();

        }
        //private async void SaveConfigExcute(object obj)
        //{
        //    _mainViewModel.IsLoading = true;
        //    var lstConfig = await _unitOfWork.AppSettingRepository.GetManyAsync(x => x.Name.Contains(ConfigEnumConstant.ENUM_INOUT_REALAND.ToString()));

        //    var InKey = lstConfig.FirstOrDefault(x => x.Name == ConfigEnumConstant.ENUM_INOUT_REALAND_IN_KEY.ToString());
        //    var OutKey = lstConfig.FirstOrDefault(x => x.Name == ConfigEnumConstant.ENUM_INOUT_REALAND_OUT_KEY.ToString());
        //    var InItem = lstConfig.Where(x => x.Name == ConfigEnumConstant.ENUM_INOUT_REALAND_IN_ITEM.ToString());
        //    var OutItem = lstConfig.Where(x => x.Name == ConfigEnumConstant.ENUM_INOUT_REALAND_OUT_ITEM.ToString());

        //    _unitOfWork.AppSettingRepository.RemoveRange(InItem);
        //    _unitOfWork.AppSettingRepository.RemoveRange(OutItem);

        //    if (InKey != null)
        //        InKey.Value = EnumIn;
        //    else
        //    {
        //        var a = new AppSetting()
        //        {
        //            Id = Guid.NewGuid(),
        //            Name = ConfigEnumConstant.ENUM_INOUT_REALAND_IN_KEY.ToString(),
        //            Value = EnumIn
        //        };
        //        await _unitOfWork.AppSettingRepository.AddAsync(a);
        //    }
        //    if (OutKey != null)
        //        OutKey.Value = EnumOut;
        //    else
        //    {
        //        var a = new AppSetting()
        //        {
        //            Id = Guid.NewGuid(),
        //            Name = ConfigEnumConstant.ENUM_INOUT_REALAND_OUT_KEY.ToString(),
        //            Value = EnumOut
        //        };
        //        await _unitOfWork.AppSettingRepository.AddAsync(a);
        //    }

        //    if (ListSelectedZKInType.Any())
        //    {
        //        var listAdd = new List<AppSetting>();
        //        foreach (var item in ListSelectedZKInType)
        //        {
        //            listAdd.Add(new AppSetting()
        //            {
        //                Id = Guid.NewGuid(),
        //                Name = ConfigEnumConstant.ENUM_INOUT_REALAND_IN_ITEM.ToString(),
        //                Value = item.Id.ToString()
        //            });
        //        }
        //        await _unitOfWork.AppSettingRepository.AddRangeAsync(listAdd);
        //    }
        //    if (ListSelectedZKOutType.Any())
        //    {
        //        var listAdd = new List<AppSetting>();
        //        foreach (var item in ListSelectedZKOutType)
        //        {
        //            listAdd.Add(new AppSetting()
        //            {
        //                Id = Guid.NewGuid(),
        //                Name = ConfigEnumConstant.ENUM_INOUT_REALAND_OUT_ITEM.ToString(),
        //                Value = item.Id.ToString()
        //            });
        //        }
        //        await _unitOfWork.AppSettingRepository.AddRangeAsync(listAdd);
        //    }
        //    await _unitOfWork.SaveChanageAsync();
        //    await GlobalEnumInOutConverter.InitUpdateEnumConverter("Realand");
        //    _mainViewModel.IsLoading = false;
        //    _mainViewModel.Action_ShowToastNotify(ConstantDisplay.LANG_GLO_ActionSuccess.TranslateString(), ToastNotifyType.Success);
        //}
        //private async void LoadConfig()
        //{
        //    var lstConfig = await _unitOfWork.AppSettingRepository.GetManyAsync(x => x.Name.Contains("ENUM_INOUT_REALAND"));

        //    var InKey = lstConfig.FirstOrDefault(x => x.Name == ConfigEnumConstant.ENUM_INOUT_REALAND_IN_KEY.ToString());
        //    var OutKey = lstConfig.FirstOrDefault(x => x.Name == ConfigEnumConstant.ENUM_INOUT_REALAND_OUT_KEY.ToString());
        //    var InItem = lstConfig.Where(x => x.Name == ConfigEnumConstant.ENUM_INOUT_REALAND_IN_ITEM.ToString());
        //    var OutItem = lstConfig.Where(x => x.Name == ConfigEnumConstant.ENUM_INOUT_REALAND_OUT_ITEM.ToString());

        //    if (InKey != null)
        //        EnumIn = InKey.Value;

        //    if (OutKey != null)
        //        EnumOut = OutKey.Value;

        //    if (InItem != null && InItem.Any())
        //    {
        //        var selectedIds = InItem.Select(x => x.Value);

        //        var a = AllEnumRealand.Where(x => selectedIds.Contains(x.Id.ToString()));

        //        ListSelectedZKInType = new ObservableCollection<EnumMultiSelectItem>(a);

        //        var dw = AllEnumRealand.Where(x => !selectedIds.Contains(x.Id.ToString())).ToList();
        //        ListZKOutType = new ObservableCollection<EnumMultiSelectItem>(dw);
        //    }
        //    if (OutItem != null && OutItem.Any())
        //    {
        //        var selectedIds = OutItem.Select(x => x.Value);

        //        var a = AllEnumRealand.Where(x => selectedIds.Contains(x.Id.ToString()));

        //        ListSelectedZKOutType = new ObservableCollection<EnumMultiSelectItem>(a);

        //        var dw = AllEnumRealand.Where(x => !selectedIds.Contains(x.Id.ToString())).ToList();
        //        ListZKInType = new ObservableCollection<EnumMultiSelectItem>(dw);
        //    }
        //}

        //private bool CanListenToSelectedItemsChangedExample()
        //{
        //    return true;
        //}

        //private void UpdateSelectedInItems(ICollection collection)
        //{
        //    var listId = collection.Cast<EnumMultiSelectItem>().Select(x => x.Id);
        //    var dw = AllEnumRealand.Where(x => !listId.Contains(x.Id)).ToList();
        //    ListZKOutType = new ObservableCollection<EnumMultiSelectItem>(dw);
        //    ListSelectedZKInType = new ObservableCollection<EnumMultiSelectItem>(collection.Cast<EnumMultiSelectItem>());
        //}
        //private void UpdateSelectedOutItems(ICollection collection)
        //{
        //    var listId = collection.Cast<EnumMultiSelectItem>().Select(x => x.Id);
        //    var dw = AllEnumRealand.Where(x => !listId.Contains(x.Id)).ToList();
        //    ListZKInType = new ObservableCollection<EnumMultiSelectItem>(dw);
        //    ListSelectedZKOutType = new ObservableCollection<EnumMultiSelectItem>(collection.Cast<EnumMultiSelectItem>());
        //}


        //private string _enumIn { get; set; }
        //public string EnumIn { get => _enumIn; set { _enumIn = value; OnPropertyChanged(); } }

        //private string _enumOut { get; set; }
        //public string EnumOut { get => _enumOut; set { _enumOut = value; OnPropertyChanged(); } }

        //private ObservableCollection<EnumMultiSelectItem> _listZKInType { get; set; }
        //public ObservableCollection<EnumMultiSelectItem> ListZKInType { get => _listZKInType; set { _listZKInType = value; OnPropertyChanged(); } }

        //private ObservableCollection<EnumMultiSelectItem> _listSelectedZKInType { get; set; }
        //public ObservableCollection<EnumMultiSelectItem> ListSelectedZKInType { get => _listSelectedZKInType; set { _listSelectedZKInType = value; OnPropertyChanged(); } }

        //private ObservableCollection<EnumMultiSelectItem> _listZKOutType { get; set; }
        //public ObservableCollection<EnumMultiSelectItem> ListZKOutType { get => _listZKOutType; set { _listZKOutType = value; OnPropertyChanged(); } }

        //private ObservableCollection<EnumMultiSelectItem> _listSelectedZKOutType { get; set; }
        //public ObservableCollection<EnumMultiSelectItem> ListSelectedZKOutType { get => _listSelectedZKOutType; set { _listSelectedZKOutType = value; OnPropertyChanged(); } }

        //public ICommand SelectedInItemsChangedCommand { get; }
        //public ICommand SelectedOutItemsChangedCommand { get; }

        //public ICommand SaveConfig { get; }
    }
}
