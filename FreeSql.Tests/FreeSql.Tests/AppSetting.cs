using System;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;

namespace AME.Helpers
{


    
    #region COREIDS

    public class COREIDS : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action onChanged = null, bool force = false)
        {
            if (!force && EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        string name;
        public string Name { get => name; set => SetProperty(ref name, value); }
        string coreid;
        public string COREID { get => coreid; set => SetProperty(ref coreid, value); }
        int syncprice = 1;
        public int SyncPrice { get => syncprice; set => SetProperty(ref syncprice, value); }
        bool syncProductName = true;
        public bool SyncProductName { get => syncProductName; set => SetProperty(ref syncProductName, value); }
    }
    public class FTPs : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action onChanged = null, bool force = false)
        {
            if (!force && EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        string name;
        public string Name { get => name; set => SetProperty(ref name, value); }
        string hostName;
        public string HostName { get => hostName; set => SetProperty(ref hostName, value); }
        int port = 21;
        public int Port { get => port; set => SetProperty(ref port, value); }
        string username;
        public string Username { get => username; set => SetProperty(ref username, value); }
        string password;
        public string Password { get => password; set => SetProperty(ref password, value); }
        string token;
        public string Token { get => token; set => SetProperty(ref token, value); }
        bool autoSend;
        public bool AutoSend { get => autoSend; set => SetProperty(ref autoSend, value); }
    }
    public class COREIDS_Sync : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action onChanged = null, bool force = false)
        {
            if (!force && EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        string name;
        public string Name { get => name; set => SetProperty(ref name, value); }
        string coreid;
        public string COREID { get => coreid; set => SetProperty(ref coreid, value); }
        string status;
        public string Status { get => status; set => SetProperty(ref status, value); }
        bool doit;
        public bool Doit { get => doit; set => SetProperty(ref doit, value); }
        string info;
        public string Info { get => info; set => SetProperty(ref info, value); }
        public bool SyncCategories { get; set; } = true;
        public bool SyncSuppliers { get; set; } = true;
        public bool SyncPricePurchase { get; set; } = true;
        public int SyncPrice { get; set; } = 1;
        public bool SyncProductName { get; set; } = true;
    }
    #endregion


     public class AppSettingII : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action onChanged = null, bool force = false)
        {
            if (!force && EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        [JsonProperty, Column(IsPrimary = true, IsIdentity = true)]
        public int ID { get; set; }

        /// <summary>
        /// 配置名称
        /// </summary>
        [DisplayName("配置名称")]
        public string SettingName { get; set; }

        /// <summary>
        /// 配置文件路径
        /// </summary>
        [JsonIgnore]
        [DisplayName("配置文件路径")]
        public string FilePath { get; set; }

        #region WPF
        private bool _ConsoleWrite;
        public bool ConsoleWrite { get => _ConsoleWrite; set { if (SetProperty(ref _ConsoleWrite, value)) SaveAI(); } }


        private int _DebugLevel;
        public int DebugLevel { get => _DebugLevel; set { if (SetProperty(ref _DebugLevel, value)) SaveAI(); } }


        private int _DebugOutput;
        public int DebugOutput { get => _DebugOutput; set { if (SetProperty(ref _DebugOutput, value)) SaveAI(); } }

        private string _cfgs;
        public string cfgs { get => _cfgs; set { if (SetProperty(ref _cfgs, value)) SaveAI(); } }

        private bool autoStartPrintServer;
        public bool AutoStartPrintServer { get => autoStartPrintServer; set { if (SetProperty(ref autoStartPrintServer, value)) SaveAI(); } }

        private bool pdf;
        public bool PDF { get => pdf; set { if (SetProperty(ref pdf, value)) SaveAI(); } }

        private bool _isdebug;
        public bool isdebug { get => _isdebug; set { if (SetProperty(ref _isdebug, value)) SaveAI(); } }

        bool isDark = false;
        public bool IsDark { get => isDark; set { if (SetProperty(ref isDark, value)) SaveAI(); } }

        string mainWindowMax = "Maximized";
        public string MainWindowMax { get => mainWindowMax; set { if (SetProperty(ref mainWindowMax, value)) SaveAI(); } }

        string myFontFamily = "幼圆";
        public string MyFontFamily { get => myFontFamily; set { if (SetProperty(ref myFontFamily, value)) SaveAI(); } }

        int myfontsize = 13;
        public int Myfontsize { get => myfontsize; set { if (SetProperty(ref myfontsize, value)) SaveAI(); } }

        //public List<string> WindowState = new List<string> { "Maximized", "Normal", "Minimized" };

        string cif;
        public string Cif { get => cif; set { if (SetProperty(ref cif, value)) SaveAI(); } }

        string hddid;
        public string Hddid { get => hddid; set { if (SetProperty(ref hddid, value)) SaveAI(); } }

        string regkey;
        public string Regkey { get => regkey; set { if (SetProperty(ref regkey, value)) SaveAI(); } }

        bool loadAllProduct = false;
        public bool LoadAllProduct { get => loadAllProduct; set { if (SetProperty(ref loadAllProduct, value)) SaveAI(); } }

        bool autoDownloadPhoto = false;
        public bool AutoDownloadPhoto { get => autoDownloadPhoto; set { if (SetProperty(ref autoDownloadPhoto, value)) SaveAI(); } }

        int ver;
        public int VER { get => ver; set { if (SetProperty(ref ver, value)) SaveAI(); } }
        #endregion

        //同步app第一版设置
        #region app第一版设置
        string printerIP = "192.168.1.232";
        public string PrinterIP { get => printerIP; set { if (SetProperty(ref printerIP, value)) SaveAI(); } }

        string dsid = "demo";
        public string DSID { get => dsid; set { if (SetProperty(ref dsid, value)) { dsid = dsid.ToLower(); SaveAI(); } } }

        public string localuuid { get; set; }
        public string DSIDsin { get => (string.IsNullOrEmpty(localuuid) ? "" : localuuid + "_") + DSID.Replace(":", "").Replace(",", "").Replace(".", "").Replace("*", ""); }

        string dsuser = "t9";
        public string DSUser { get => dsuser; set { if (SetProperty(ref dsuser, value)) SaveAI(); } }

        string dspassword = "0";
        public string DSPassword { get => dspassword; set { if (SetProperty(ref dspassword, value)) SaveAI(); } }

        string syncTime;
        public string SyncTime { get => syncTime; set { if (SetProperty(ref syncTime, value)) SaveAI(); } }

        string uuid;
        public string UUID { get => uuid; set { if (SetProperty(ref uuid, value)) SaveAI(); } }

        bool isLocalDB = false;
        public bool IsLocalDB { get => isLocalDB; set { if (SetProperty(ref isLocalDB, value)) SaveAI(); } }

        string ip = "192.168.1.100";
        public string IP { get => ip; set { if (SetProperty(ref ip, value)) SaveAI(); } }

        string port = "1200";
        public string PORT { get => port; set { if (SetProperty(ref port, value)) SaveAI(); } }

        bool autologin = false;
        public bool Autologin
        {
            get => autologin; set
            {
                if (SetProperty(ref autologin, value))
                {
                    if (!value) DSPassword = "";
                    SaveAI();
                }
            }
        }

        bool printA4_2 = false;
        public bool PrintA4_2 { get => printA4_2; set { if (SetProperty(ref printA4_2, value)) SaveAI(); } }

        bool printA4_3 = false;
        public bool PrintA4_3 { get => printA4_3; set { if (SetProperty(ref printA4_3, value)) SaveAI(); } }

        string cOREID;
        public string COREID { get => cOREID; set { if (SetProperty(ref cOREID, value)) SaveAI(); } }

        bool useCache = false;
        public bool UseCache { get => useCache; set { if (SetProperty(ref useCache, value)) SaveAI(); } }

        string cachePath;
        public string CachePath { get => cachePath; set { if (SetProperty(ref cachePath, value)) SaveAI(); } }

        public Guid GUID { get; set; } = Guid.NewGuid();

        bool productsShowPhoto = true;
        public bool ProductsShowPhoto { get => productsShowPhoto; set { if (SetProperty(ref productsShowPhoto, value)) SaveAI(); } }

        bool productsSelectKeepShow = false;
        public bool ProductsSelectKeepShow { get => productsSelectKeepShow; set { if (SetProperty(ref productsSelectKeepShow, value)) SaveAI(); } }

        COREIDS[] coreIDs = new COREIDS[] {
            new COREIDS { Name = "1" },
            new COREIDS { Name = "2" },
            new COREIDS { Name = "3" },
            new COREIDS { Name = "4" },
            new COREIDS { Name = "5" },
            new COREIDS { Name = "6" }};
        [JsonProperty, Column(IsIgnore = true)]
        public COREIDS[] COREIDS { get => coreIDs; set { if (SetProperty(ref coreIDs, value)) SaveAI(); } }

        #endregion

        #region 销售窗体设置
        string orderDetailsWindowMax = "Maximized";
        public string OrderDetailsWindowMax { get => orderDetailsWindowMax; set { if (SetProperty(ref orderDetailsWindowMax, value)) SaveAI(); } }

        bool orderDetailsShowMore = false;
        public bool OrderDetailsShowMore { get => orderDetailsShowMore; set { if (SetProperty(ref orderDetailsShowMore, value)) SaveAI(); } }

        bool orderDetailsShowDetailsCard = true;
        public bool OrderDetailsShowDetailsCard { get => orderDetailsShowDetailsCard; set { if (SetProperty(ref orderDetailsShowDetailsCard, value)) SaveAI(); } }

        bool orderDetailsShowCategories = false;
        public bool OrderDetailsShowCategories { get => orderDetailsShowCategories; set { if (SetProperty(ref orderDetailsShowCategories, value)) SaveAI(); } }

        bool orderDetailsShowSuppliers = false;
        public bool OrderDetailsShowSuppliers { get => orderDetailsShowSuppliers; set { if (SetProperty(ref orderDetailsShowSuppliers, value)) SaveAI(); } }

        int orderDetailsImagePreviewWidth = 30;
        public int OrderDetailsImagePreviewWidth { get => orderDetailsImagePreviewWidth; set { if (SetProperty(ref orderDetailsImagePreviewWidth, value)) SaveAI(); } }
        #endregion

        #region 商品窗体设置
        /// <summary>
        /// 管理:商品库存过滤
        /// </summary>
        bool filterStock = false;
        public bool FilterStock { get => filterStock; set { if (SetProperty(ref filterStock, value)) SaveAI(); } }

        string productsWindowMax = "Maximized";
        public string ProductsWindowMax { get => productsWindowMax; set { if (SetProperty(ref productsWindowMax, value)) SaveAI(); } }

        bool productsShowMore = false;
        public bool ProductsShowMore { get => productsShowMore; set { if (SetProperty(ref productsShowMore, value)) SaveAI(); } }

        bool productsShowDetailsCard = true;
        public bool ProductsShowDetailsCard { get => productsShowDetailsCard; set { if (SetProperty(ref productsShowDetailsCard, value)) SaveAI(); } }

        bool productsShowCategories = false;
        public bool ProductsShowCategories { get => productsShowCategories; set { if (SetProperty(ref productsShowCategories, value)) SaveAI(); } }

        bool productsShowSuppliers = false;
        public bool ProductsShowSuppliers { get => productsShowSuppliers; set { if (SetProperty(ref productsShowSuppliers, value)) SaveAI(); } }

        int productImagePreviewWidth = 30;
        public int ProductImagePreviewWidth { get => productImagePreviewWidth; set { if (SetProperty(ref productImagePreviewWidth, value)) SaveAI(); } }
        #endregion

        #region 类别窗体设置
        string categoriesWindowMax = "Maximized";
        public string CategoriesWindowMax { get => categoriesWindowMax; set { if (SetProperty(ref categoriesWindowMax, value)) SaveAI(); } }

        bool showCategories2 = false;
        public bool ShowCategories2 { get => showCategories2; set { if (SetProperty(ref showCategories2, value)) SaveAI(); } }

        bool categoriesShowMore = false;
        public bool CategoriesShowMore { get => categoriesShowMore; set { if (SetProperty(ref categoriesShowMore, value)) SaveAI(); } }

        bool categoriesShowDetailsCard = true;
        public bool CategoriesShowDetailsCard { get => categoriesShowDetailsCard; set { if (SetProperty(ref categoriesShowDetailsCard, value)) SaveAI(); } }

        int categoriesImagePreviewWidth = 30;
        public int CategoriesImagePreviewWidth { get => categoriesImagePreviewWidth; set { if (SetProperty(ref categoriesImagePreviewWidth, value)) SaveAI(); } }

        bool ignoreSecondLevelCategories;
        public bool IgnoreSecondLevelCategories { get => ignoreSecondLevelCategories; set { if (SetProperty(ref ignoreSecondLevelCategories, value)) SaveAI(); } }
        #endregion

        #region 客户窗体设置
        string customersWindowMax = "Maximized";
        public string CustomersWindowMax { get => customersWindowMax; set { if (SetProperty(ref customersWindowMax, value)) SaveAI(); } }

        bool customersShowMore = false;
        public bool CustomersShowMore { get => customersShowMore; set { if (SetProperty(ref customersShowMore, value)) SaveAI(); } }

        bool customersShowDetailsCard = true;
        public bool CustomersShowDetailsCard { get => customersShowDetailsCard; set { if (SetProperty(ref customersShowDetailsCard, value)) SaveAI(); } }

        int customersImagePreviewWidth = 30;
        public int CustomersImagePreviewWidth { get => customersImagePreviewWidth; set { if (SetProperty(ref customersImagePreviewWidth, value)) SaveAI(); } }
        #endregion

        #region 供应商窗体设置
        string suppliersWindowMax = "Maximized";
        public string SuppliersWindowMax { get => suppliersWindowMax; set { if (SetProperty(ref suppliersWindowMax, value)) SaveAI(); } }

        bool suppliersShowMore = false;
        public bool SuppliersShowMore { get => suppliersShowMore; set { if (SetProperty(ref suppliersShowMore, value)) SaveAI(); } }

        bool suppliersShowDetailsCard = true;
        public bool SuppliersShowDetailsCard { get => suppliersShowDetailsCard; set { if (SetProperty(ref suppliersShowDetailsCard, value)) SaveAI(); } }

        int suppliersImagePreviewWidth = 30;
        public int SuppliersImagePreviewWidth { get => suppliersImagePreviewWidth; set { if (SetProperty(ref suppliersImagePreviewWidth, value)) SaveAI(); } }
        #endregion

        #region 用户窗体设置
        string usersWindowMax = "Maximized";
        public string UsersWindowMax { get => usersWindowMax; set { if (SetProperty(ref usersWindowMax, value)) SaveAI(); } }

        bool usersShowMore = false;
        public bool UsersShowMore { get => usersShowMore; set { if (SetProperty(ref usersShowMore, value)) SaveAI(); } }

        bool usersShowDetailsCard = true;
        public bool UsersShowDetailsCard { get => usersShowDetailsCard; set { if (SetProperty(ref usersShowDetailsCard, value)) SaveAI(); } }

        int usersImagePreviewWidth = 30;
        public int UsersImagePreviewWidth { get => usersImagePreviewWidth; set { if (SetProperty(ref usersImagePreviewWidth, value)) SaveAI(); } }
        #endregion

        #region 图片窗体设置
        string photosWindowMax = "Maximized";
        public string PhotosWindowMax { get => photosWindowMax; set { if (SetProperty(ref photosWindowMax, value)) SaveAI(); } }

        int photosImagePreviewWidth = 30;
        public int PhotosImagePreviewWidth { get => photosImagePreviewWidth; set { if (SetProperty(ref photosImagePreviewWidth, value)) SaveAI(); } }
        #endregion

        #region 单据列表窗体设置
        string orderListWindowMax = "Maximized";
        public string OrderListWindowMax { get => orderListWindowMax; set { if (SetProperty(ref orderListWindowMax, value)) SaveAI(); } }

        bool orderListShowMore = false;
        public bool OrderListShowMore { get => orderListShowMore; set { if (SetProperty(ref orderListShowMore, value)) SaveAI(); } }

        bool orderListShowDetailsCard = true;
        public bool OrderListShowDetailsCard { get => orderListShowDetailsCard; set { if (SetProperty(ref orderListShowDetailsCard, value)) SaveAI(); } }
        #endregion

        #region 小程序/VIP客户
        string miniOrderDetailsOrderBy = "BarCode";
        public string MiniOrderDetailsOrderBy { get => miniOrderDetailsOrderBy; set { if (SetProperty(ref miniOrderDetailsOrderBy, value)) SaveAI(); } }
        bool miniOrderDetailsShowBarcode;
        public bool MiniOrderDetailsShowBarcode { get => miniOrderDetailsShowBarcode; set { if (SetProperty(ref miniOrderDetailsShowBarcode, value)) SaveAI(); } }

        /// <summary>
        /// 销售:商品库存过滤
        /// </summary>
        bool filterStockVIP = false;
        public bool FilterStockVIP { get => filterStockVIP; set { if (SetProperty(ref filterStockVIP, value)) SaveAI(); } }

        /// <summary>
        /// 销售:商品状态过滤
        /// </summary>
        bool filterStatusVIP = false;
        public bool FilterStatusVIP { get => filterStatusVIP; set { if (SetProperty(ref filterStatusVIP, value)) SaveAI(); } }

        /// <summary>
        /// 销售:商品状态过滤字符串形式
        /// </summary>
        string filterStatusVIPstr;
        public string FilterStatusVIPstr { get => filterStatusVIPstr; set { if (SetProperty(ref filterStatusVIPstr, value)) SaveAI(); } }

        bool forceOneNewOrder = false;
        public bool ForceOneNewOrder { get => forceOneNewOrder; set { if (SetProperty(ref forceOneNewOrder, value)) SaveAI(); } }

        bool inputNumber2 = false;
        public bool InputNumber2 { get => inputNumber2; set { if (SetProperty(ref inputNumber2, value)) SaveAI(); } }

        /// <summary>
        /// 销售父类排序
        /// </summary>
        string categories2OrderBy;
        public string Categories2OrderBy { get => categories2OrderBy; set { if (SetProperty(ref categories2OrderBy, value)) SaveAI(); } }

        /// <summary>
        /// 销售类别排序
        /// </summary>
        string categoriesOrderBy = "CategoryID";
        public string CategoriesOrderBy { get => categoriesOrderBy; set { if (SetProperty(ref categoriesOrderBy, value)) SaveAI(); } }

        /// <summary>
        /// 销售商品排序
        /// </summary>
        string productOrderBy;
        public string ProductOrderBy { get => productOrderBy; set { if (SetProperty(ref productOrderBy, value)) SaveAI(); } }


        /// <summary>
        /// 自定义首页
        /// </summary>
        string vipIndex;
        public string VipIndex { get => vipIndex; set { if (SetProperty(ref vipIndex, value)) SaveAI(); } }

        /// <summary>
        /// 用户资料锁定
        /// </summary>
        bool lockVIPDatas = false;
        public bool LockVIPDatas { get => lockVIPDatas; set { if (SetProperty(ref lockVIPDatas, value)) SaveAI(); } }

        /// <summary>
        /// 禁止单位[件]
        /// </summary>
        bool disabledOrderSingleQuantity;
        public bool DisabledOrderSingleQuantity { get => disabledOrderSingleQuantity; set { if (SetProperty(ref disabledOrderSingleQuantity, value)) SaveAI(); } }

        /// <summary>
        /// 指定游客账号ID
        /// </summary>
        string guestID;
        public string GuestID { get => guestID; set { if (SetProperty(ref guestID, value)) SaveAI(); } }

        /// <summary>
        /// 默认注册email后缀
        /// </summary>
        string email;
        public string Email { get => email; set { if (SetProperty(ref email, value)) SaveAI(); } }

        /// <summary>
        /// 默认Code
        /// </summary>
        string code;
        [DisplayName("默认Code")]
        public string Code { get => code; set { if (SetProperty(ref code, value)) SaveAI(); } }

        /// <summary>
        /// 配货单列表排序
        /// </summary>
        string pickupListOrderBy = "PostalCode,OrderID";
        public string PickupListOrderBy { get => pickupListOrderBy; set { if (SetProperty(ref pickupListOrderBy, value)) SaveAI(); } }

        /// <summary>
        /// 配货单据排序
        /// </summary>
        string pickupOrderBy = "BarCode";
        public string PickupOrderBy { get => pickupOrderBy; set { if (SetProperty(ref pickupOrderBy, value)) SaveAI(); } }

        /// <summary>
        /// 使用紧凑型左类别条
        /// </summary>
        bool useLiteCategoriesBar;
        public bool UseLiteCategoriesBar { get => useLiteCategoriesBar; set { if (SetProperty(ref useLiteCategoriesBar, value)) SaveAI(); } }

        #endregion

        #region 日志
        string nlog_All;
        public string Nlog_All { get => nlog_All; set { if (SetProperty(ref nlog_All, value)) SaveAI(); } }
        string nlog_warn;
        public string Nlog_warn { get => nlog_warn; set { if (SetProperty(ref nlog_warn, value)) SaveAI(); } }
        #endregion

        #region 新加配置
        string urlPhoto;
        public string UrlPhoto { get => urlPhoto; set { if (SetProperty(ref urlPhoto, value)) SaveAI(); } }
        string unsyncPriceTag;
        public string UnsyncPriceTag { get => unsyncPriceTag; set { if (SetProperty(ref unsyncPriceTag, value)) SaveAI(); } }
        bool syncPricePurchase = true;
        public bool SyncPricePurchase { get => syncPricePurchase; set { if (SetProperty(ref syncPricePurchase, value)) SaveAI(); } }
        bool syncStatus;
        public bool SyncStatus { get => syncStatus; set { if (SetProperty(ref syncStatus, value)) SaveAI(); } }

        /// <summary>
        /// 旧版app图片路径,用于导入
        /// </summary>
        string appPhotoUrl;
        public string AppPhotoUrl { get => appPhotoUrl; set { if (SetProperty(ref appPhotoUrl, value)) SaveAI(); } }

        /// <summary>
        /// 显示APP面板
        /// </summary>
        bool showAppPanel;
        public bool ShowAppPanel { get => showAppPanel; set { if (SetProperty(ref showAppPanel, value)) SaveAI(); } }

        bool autoUsercode;
        public bool AutoUsercode { get => autoUsercode; set { if (SetProperty(ref autoUsercode, value)) SaveAI(); } }


        /// <summary>
        /// H5的Url
        /// </summary>
        string h5url = "https://menu.freepos.es/";
        public string H5url { get => h5url; set { if (SetProperty(ref h5url, value)) SaveAI(); } }
        bool loadH5url;
        public bool LoadH5url { get => loadH5url; set { if (SetProperty(ref loadH5url, value)) SaveAI(); } }

        /// <summary>
        /// 监控输出SQL语句
        /// </summary>
        bool useMonitorCommand;
        public bool UseMonitorCommand { get => useMonitorCommand; set { if (SetProperty(ref useMonitorCommand, value)) SaveAI(); } }


        #region Winform版设置

        bool productEditor20;
        public bool ProductEditor20 { get => productEditor20; set { if (SetProperty(ref productEditor20, value)) SaveAI(); } }
        /// <summary>
        /// 打开内测功能
        /// </summary>
        bool enableBataFunction;
        public bool EnableBataFunction { get => enableBataFunction; set { if (SetProperty(ref enableBataFunction, value)) SaveAI(); } }

        /// <summary>
        /// 类别供应商显示序号
        /// </summary>
        bool categoriesDisplayID = true;
        public bool CategoriesDisplayID { get => categoriesDisplayID; set { if (SetProperty(ref categoriesDisplayID, value)) SaveAI(); } }

        /// <summary>
        /// 导入单据过滤数量0的行
        /// </summary>
        bool importOrderSkipQuantityZero;
        public bool ImportOrderSkipQuantityZero { get => importOrderSkipQuantityZero; set { if (SetProperty(ref importOrderSkipQuantityZero, value)) SaveAI(); } }

        /// <summary>
        /// 导入单据过滤库存0的行
        /// </summary>
        bool importOrderSkipStockZero;
        public bool ImportOrderSkipStockZero { get => importOrderSkipStockZero; set { if (SetProperty(ref importOrderSkipStockZero, value)) SaveAI(); } }

        /// <summary>
        /// 打印通道3重定向到小票
        /// </summary>
        bool printA4_3_to_Ticket;
        public bool PrintA4_3_to_Ticket { get => printA4_3_to_Ticket; set { if (SetProperty(ref printA4_3_to_Ticket, value)) SaveAI(); } }

        /// <summary>
        /// 精度计算:使用格式化2位后再累加
        /// </summary>
        bool taxDetalisCalculationAccuracyMethod2 = true;
        public bool TaxDetalisCalculationAccuracyMethod2 { get => taxDetalisCalculationAccuracyMethod2; set { if (SetProperty(ref taxDetalisCalculationAccuracyMethod2, value)) SaveAI(); } }

        /// <summary>
        /// 测试功能:强制同步类别ID,供应商ID, 用于误删除商品表恢复数据
        /// </summary>
        bool forceSyncCategoryIDSupplierID;
        public bool ForceSyncCategoryIDSupplierID { get => forceSyncCategoryIDSupplierID; set { if (SetProperty(ref forceSyncCategoryIDSupplierID, value)) SaveAI(); } }

        /// <summary>
        /// 使用极速版
        /// </summary>
        bool useORM = true;
        public bool UseORM { get => useORM; set { if (SetProperty(ref useORM, value)) SaveAI(); } }

        /// <summary>
        /// 使用多发票序列
        /// </summary>
        bool useMoreFacturaSeriel;
        public bool UseMoreFacturaSeriel { get => useMoreFacturaSeriel; set { if (SetProperty(ref useMoreFacturaSeriel, value)) SaveAI(); } }

        /// <summary>
        /// 显示独立结账提示窗体
        /// </summary>
        bool useExCheckOutDSP;
        public bool UseExCheckOutDSP { get => useExCheckOutDSP; set { if (SetProperty(ref useExCheckOutDSP, value)) SaveAI(); } }

        /// <summary>
        /// 结账前提示输入邮编
        /// </summary>
        bool analysisOrderPostcode;
        public bool AnalysisOrderPostcode { get => analysisOrderPostcode; set { if (SetProperty(ref analysisOrderPostcode, value)) SaveAI(); } }

        /// <summary>
        /// 只读的单元格显示大字体
        /// </summary>
        bool cellReadonlyToGreen;
        public bool CellReadonlyToGreen { get => cellReadonlyToGreen; set { if (SetProperty(ref cellReadonlyToGreen, value)) SaveAI(); } }

        /// <summary>
        /// 接口历史文件地址
        /// </summary>
        string[] interfacePath = new string[] { "", "", "", "", "", "", "", "", "", "" };
        public string[] InterfacePath { get => interfacePath; set { if (SetProperty(ref interfacePath, value)) SaveAI(); } }

        /// <summary>
        /// 客户评级
        /// </summary>
        bool customerStar;
        public bool CustomerStar { get => customerStar; set { if (SetProperty(ref customerStar, value)) SaveAI(); } }

        /// <summary>
        /// 客户供应商删除限制
        /// </summary>
        bool customerDisableDel = true;
        public bool CustomerDisableDel { get => customerDisableDel; set { if (SetProperty(ref customerDisableDel, value)) SaveAI(); } }

        /// <summary>
        /// 导出资料自动传送
        /// </summary>
        bool autoSend;
        public bool AutoSend { get => autoSend; set { if (SetProperty(ref autoSend, value)) SaveAI(); } }

        /// <summary>
        /// FTP资料
        /// </summary>
        FTPs[] _FTPs = new FTPs[] {
            new FTPs { Name = "1.Directialogistica" },
            new FTPs { Name = "2.Saveb2b" },
            new FTPs { Name = "3" },
            new FTPs { Name = "4" },
            new FTPs { Name = "5" },
            new FTPs { Name = "6" }};
        [JsonProperty, Column(IsIgnore = true)]
        public FTPs[] FTPs { get => _FTPs; set { if (SetProperty(ref _FTPs, value)) SaveAI(); } }

        /// <summary>
        /// 导出单据使用名称替换条码
        /// </summary>
        bool expOrderWithName;
        public bool ExpOrderWithName { get => expOrderWithName; set { if (SetProperty(ref expOrderWithName, value)) SaveAI(); } }

        /// <summary>
        /// 预热数据连接
        /// </summary>
        bool warmupDataConnection;
        public bool WarmupDataConnection { get => warmupDataConnection; set { if (SetProperty(ref warmupDataConnection, value)) SaveAI(); } }

        /// <summary>
        /// 销售单内部查找包含名称模糊查找
        /// </summary>
        bool orderSearchIncName;
        public bool OrderSearchIncName { get => orderSearchIncName; set { if (SetProperty(ref orderSearchIncName, value)) SaveAI(); } }

        /// <summary>
        /// 销售单商品重复提示
        /// </summary>
        bool orderItemDuplicateReminder;
        public bool OrderItemDuplicateReminder { get => orderItemDuplicateReminder; set { if (SetProperty(ref orderItemDuplicateReminder, value)) SaveAI(); } }

        /// <summary>
        /// VP过滤VO单据
        /// </summary>
        bool vPfiltersVO;
        public bool VPfiltersVO { get => vPfiltersVO; set { if (SetProperty(ref vPfiltersVO, value)) SaveAI(); } }

        /// <summary>
        /// VP过滤VO单据并过滤金额
        /// </summary>
        bool vPfiltersVOwithAmount;
        public bool VPfiltersVOwithAmount { get => vPfiltersVOwithAmount; set { if (SetProperty(ref vPfiltersVOwithAmount, value)) SaveAI(); } }

        /// <summary>
        /// 导出单据附加发票号码到备注
        /// </summary>
        bool expOrderWithInvoiceID;
        public bool ExpOrderWithInvoiceID { get => expOrderWithInvoiceID; set { if (SetProperty(ref expOrderWithInvoiceID, value)) SaveAI(); } }


        /// <summary>
        /// 优先用webapi或者本机连接
        /// </summary>
        bool vipUseWebAPI;
        public bool VipUseWebAPI { get => vipUseWebAPI; set { if (SetProperty(ref vipUseWebAPI, value)) SaveAI(); } }

        /// <summary>
        /// 比对-销售数量完整后不能再更改
        /// </summary>
        bool comparisonOrderCompleteReadonly;
        public bool ComparisonOrderCompleteReadonly { get => comparisonOrderCompleteReadonly; set { if (SetProperty(ref comparisonOrderCompleteReadonly, value)) SaveAI(); } }

        /// <summary>
        /// 比对-超数量提醒
        /// </summary>
        bool comparisonOrderSuperAlert;
        public bool ComparisonOrderSuperAlert { get => comparisonOrderSuperAlert; set { if (SetProperty(ref comparisonOrderSuperAlert, value)) SaveAI(); } }

        /// <summary>
        /// 比对-点货数量正确才打勾
        /// </summary>
        bool comparisonOnlyQtqCorrctCheck;
        public bool ComparisonOnlyQtqCorrctCheck { get => comparisonOnlyQtqCorrctCheck; set { if (SetProperty(ref comparisonOnlyQtqCorrctCheck, value)) SaveAI(); } }

        /// <summary>
        /// 比对-进价加税
        /// </summary>
        bool comparisonPurchasePriceAddTax;
        public bool ComparisonPurchasePriceAddTax { get => comparisonPurchasePriceAddTax; set { if (SetProperty(ref comparisonPurchasePriceAddTax, value)) SaveAI(); } }
        #endregion

        /// <summary>
        /// 比对-售价为零,进价相同,自动使用系统售价
        /// </summary>
        bool comparisonUnitpriceAuto1;
        public bool ComparisonUnitpriceAuto1 { get => comparisonUnitpriceAuto1; set { if (SetProperty(ref comparisonUnitpriceAuto1, value)) SaveAI(); } }

        /// <summary>
        /// 比对-数量默认为追加
        /// </summary>
        bool comparisonDefaultAdditionalQuantity;
        public bool ComparisonDefaultAdditionalQuantity { get => comparisonDefaultAdditionalQuantity; set { if (SetProperty(ref comparisonDefaultAdditionalQuantity, value)) SaveAI(); } }

        /// <summary>
        /// 销售-计算重量总计
        /// </summary>
        bool salesCalcWeightTotal;
        public bool SalesCalcWeightTotal { get => salesCalcWeightTotal; set { if (SetProperty(ref salesCalcWeightTotal, value)) SaveAI(); } }

        /// <summary>
        /// 测试年结送优惠券/月结送优惠券,宽松条件方式
        /// </summary>
        bool testMonthlyOff;
        public bool TestMonthlyOff { get => testMonthlyOff; set { if (SetProperty(ref testMonthlyOff, value)) SaveAI(); } }


        /// <summary>
        /// 比对-调用供应商的销售价公式
        /// </summary>
        bool comparisonUseSuppliersSalesPriceFormula = true;
        public bool ComparisonUseSuppliersSalesPriceFormula { get => comparisonUseSuppliersSalesPriceFormula; set { if (SetProperty(ref comparisonUseSuppliersSalesPriceFormula, value)) SaveAI(); } }

        /// <summary>
        /// 比对-毛利率公式II => 毛利率 =（1－不含税进价/不含税售价）×100%
        /// </summary>
        bool comparisonGrossFormulaII = true;
        public bool ComparisonGrossFormulaII { get => comparisonGrossFormulaII; set { if (SetProperty(ref comparisonGrossFormulaII, value)) SaveAI(); } }

        /// <summary>
        /// 比对-四舍五入
        /// </summary>
        bool comparisonRounding = true;
        public bool ComparisonRounding { get => comparisonRounding; set { if (SetProperty(ref comparisonRounding, value)) SaveAI(); } }

        /// <summary>
        /// 打开单据不关闭挂单列表
        /// </summary>
        bool doNotCloseListOfOrders;
        public bool DoNotCloseListOfOrders { get => doNotCloseListOfOrders; set { if (SetProperty(ref doNotCloseListOfOrders, value)) SaveAI(); } }

        /// <summary>
        /// 销售-启用Lote
        /// </summary>
        bool salesEnableLote;
        [DisplayName("销售-启用Lote")]
        public bool SalesEnableLote { get => salesEnableLote; set { if (SetProperty(ref salesEnableLote, value)) SaveAI(); } }

        /// <summary>
        /// 精度计算:发票含税单据合计直接使用商品合计,不反算税率合计
        /// </summary>
        bool orderSubtotalUseOldMethod;
        [DisplayName("精度计算:发票含税单据合计直接使用商品合计,不反算税率合计")]
        public bool OrderSubtotalUseOldMethod { get => orderSubtotalUseOldMethod; set { if (SetProperty(ref orderSubtotalUseOldMethod, value)) SaveAI(); } }

        /// <summary>
        /// 小票:打印Json格式收货地址
        /// </summary>
        bool ticketWithJsonRemark;
        [DisplayName("小票:打印Json格式收货地址")]
        public bool TicketWithJsonRemark { get => ticketWithJsonRemark; set { if (SetProperty(ref ticketWithJsonRemark, value)) SaveAI(); } }

        /// <summary>
        /// 小票:打印客户资料地址II
        /// </summary>
        bool ticketClientDataII;
        [DisplayName("小票:打印客户资料地址II")]
        public bool TicketClientDataII { get => ticketClientDataII; set { if (SetProperty(ref ticketClientDataII, value)) SaveAI(); } }

        /// <summary>
        /// 小票:打印备注
        /// </summary>
        bool ticketWithRemark;
        [DisplayName("小票:打印备注")]
        public bool TicketWithRemark { get => ticketWithRemark; set { if (SetProperty(ref ticketWithRemark, value)) SaveAI(); } }

        /// <summary>
        /// 小票:打印部分备注
        /// </summary>
        bool ticketWithRemarkLite;
        [DisplayName("小票:打印部分备注")]
        public bool TicketWithRemarkLite { get => ticketWithRemarkLite; set { if (SetProperty(ref ticketWithRemarkLite, value)) SaveAI(); } }

        /// <summary>
        /// 大票:打印Json格式收货地址
        /// </summary>
        bool a4WithJsonRemark;
        [DisplayName("大票:打印Json格式收货地址")]
        public bool A4WithJsonRemark { get => a4WithJsonRemark; set { if (SetProperty(ref a4WithJsonRemark, value)) SaveAI(); } }

        /// <summary>
        /// 大票:打印收货地址(客户资料地址II)
        /// </summary>
        bool a4WithClientDataII;
        [DisplayName("大票:打印收货地址(客户资料地址II)")]
        public bool A4WithClientDataII { get => a4WithClientDataII; set { if (SetProperty(ref a4WithClientDataII, value)) SaveAI(); } }

        /// <summary>
        /// 小程序客户名称自动转拼音
        /// </summary>
        bool autoAppCustomerNameTransToPinyin;
        [DisplayName("自动")]
        public bool AutoAppCustomerNameTransToPinyin { get => autoAppCustomerNameTransToPinyin; set { if (SetProperty(ref autoAppCustomerNameTransToPinyin, value)) SaveAI(); } }

        /// <summary>
        /// 更新客户资料到原始小程序单据
        /// </summary>
        bool autoAppSaveInfoII2miniOrders;
        [DisplayName("更新客户资料到原始小程序单据")]
        public bool AutoAppSaveInfoII2miniOrders { get => autoAppSaveInfoII2miniOrders; set { if (SetProperty(ref autoAppSaveInfoII2miniOrders, value)) SaveAI(); } }

        /// <summary>
        /// 小程序单据窗口最大化
        /// </summary>
        bool formMiniWebAPPWindowMax = true;
        [DisplayName("窗口最大化")]
        public bool FormMiniWebAPPWindowMax { get => formMiniWebAPPWindowMax; set { if (SetProperty(ref formMiniWebAPPWindowMax, value)) SaveAI(); } }

        /// <summary>
        /// 小程序单据自动调入详单
        /// </summary>
        bool formMiniWebAppAutoLoadOrderDetails = true;
        [DisplayName("详单")]
        public bool FormMiniWebAppAutoLoadOrderDetails { get => formMiniWebAppAutoLoadOrderDetails; set { if (SetProperty(ref formMiniWebAppAutoLoadOrderDetails, value)) SaveAI(); } }

        /// <summary>
        /// 使用会员卡优惠券窗口自动关闭
        /// </summary>
        bool formUsingCouponAutoClose = true;
        [DisplayName("自动关闭")]
        public bool FormUsingCouponAutoClose { get => formUsingCouponAutoClose; set { if (SetProperty(ref formUsingCouponAutoClose, value)) SaveAI(); } }
        #endregion

        /// <summary>
        /// 提醒非正常商品状态
        /// </summary>
        bool alertProductStatus;
        [DisplayName("提醒非正常商品状态")]
        public bool AlertProductStatus { get => alertProductStatus; set { if (SetProperty(ref alertProductStatus, value)) SaveAI(); } }

        /// <summary>
        /// 新打印模块
        /// </summary>
        bool newPrintModel;
        [DisplayName("新小票打印模块")]
        public bool NewPrintModel { get => newPrintModel; set { if (SetProperty(ref newPrintModel, value)) SaveAI(); } }

        public void SaveAI()
        {
            
        }
    }
}