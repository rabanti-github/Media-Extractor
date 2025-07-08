using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using WixSharp;
using WixSharp.UI.Forms;

using WixSharp.UI.WPF;
using WixToolset.Dtf.WindowsInstaller;

namespace SetupProject
{
    /// <summary>
    /// The standard FeaturesDialog.
    /// </summary>
    /// <seealso cref="WixSharp.UI.WPF.WpfDialog" />
    /// <seealso cref="WixSharp.IWpfDialog" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class FeaturesDialog : WpfDialog, IWpfDialog
    {
   
        FeaturesDialogModel model;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeaturesDialog"/> class.
        /// </summary>
        public FeaturesDialog()
        {
            InitializeComponent();

        }

        /// <summary>
        /// This method is invoked by WixSHarp runtime when the custom dialog content is internally fully initialized.
        /// This is a convenient place to do further initialization activities (e.g. localization).
        /// </summary>
        public void Init()
        {
            DataContext = model = new FeaturesDialogModel(ManagedFormHost);
            var name = this.Session()["ProductName"];
            var ver = this.Session()["ProductVersion"];
            this.DialogTitle = $"{name} {ver} Setup";
        }

        private void FeaturesTree_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new System.Action(() =>
            {
                ExpandAll(FeaturesTree);
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        void GoPrev_Click(object sender, RoutedEventArgs e)
            => model.GoPrev();

        void GoNext_Click(object sender, RoutedEventArgs e)
            => model.GoNext();

        void Cancel_Click(object sender, RoutedEventArgs e)
            => model.Cancel();

        void Reset_Click(object sender, RoutedEventArgs e)
            => model.Reset();

        public void ExpandAll(ItemsControl items)
        {
            foreach (object obj in items.Items)
            {
                TreeViewItem item = items.ItemContainerGenerator.ContainerFromItem(obj) as TreeViewItem;
                if (item != null)
                {
                    item.IsExpanded = true;
                    ExpandAll(item);
                }
            }
        }

        private void FeaturesTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var data = (FeatureItem)(e.NewValue as Node)?.Data;
            string key = data.Description;
            if (key.StartsWith("[") && key.EndsWith("]"))
            {
                key = key.Substring(1, key.Length - 2);
            }
            model.SelectedNodeDescription = this.MsiRuntime()?.Localize(key); // localize the feature description
        }
    }

    /// <summary>
    /// ViewModel of the feature tree node.
    /// </summary>
    internal class Node : NotifyPropertyChangedBase
    {
        bool @checked;
        string name;
        bool isEditable = true;
        Session session;

        public ManagedForm Host { get; set; }

        public string Name
        {
            get
            {
                string displayName = name;
                if (name.StartsWith("[") && name.EndsWith("]"))
                {
                    displayName = name.Substring(1, name.Length - 2);
                }

                if (Host != null && Host.Runtime != null)
                {
                    return Host.Runtime.Localize(displayName);
                }
                return displayName;
            }
            set => name = value;
        }

        public ObservableCollection<Node> Nodes { get; set; } = new ObservableCollection<Node>();
        internal bool ignoreChildrenCheck = false;

        public bool Checked
        {
            get => @checked;
            set
            {
                @checked = value;
                // always update view in case if the Checked update was triggered by children even when @checked value is not changed
                NotifyOfPropertyChange(nameof(Checked));
                switch (name)
                {
                    case Constants.FEATURE_DESKTOP_NAME:
                        Constants.AddSecureProperty(session, Constants.SecureProperties.ADD_DESKTOP_ICON, value);
                        break;
                     case Constants.FEATURE_STARTMENU_NAME:
                        Constants.AddSecureProperty(session, Constants.SecureProperties.ADD_STARTMENU_ICON, value);
                        break;
                     case Constants.FEATURE_EXPLORER_NAME:
                        Constants.AddSecureProperty(session, Constants.SecureProperties.REGISTER_EXPLORER, value);
                        break;
                }
            }
        }

        public bool DefaultChecked { get; set; }
        public object Data { get; set; }
        public Node Parent => ((FeatureItem)this.Data).Parent?.ViewModel as Node;

        public bool IsEditable
        {
            get 
            {
                return isEditable;
            }
            set
            {
                isEditable = value;
                NotifyOfPropertyChange(nameof(IsEditable));
            }
        }

        public Node(ManagedForm host)
        {
            Host = host;
            session = host.Session();
        }
    }

    /// <summary>
    /// ViewModel for standard FeaturesDialog.
    /// </summary>
    internal class FeaturesDialogModel : NotifyPropertyChangedBase
    {
        ManagedForm Host;
        ISession session => Host?.Runtime.Session;
        IManagedUIShell shell => Host?.Shell;

        string selectedNodeDescription;
        public ObservableCollection<Node> RootNodes { get; set; } = new ObservableCollection<Node>();

        public FeaturesDialogModel(ManagedForm host)
        {
            this.Host = host;
            BuildFeaturesHierarchy();
           //dialogReference.ExpandAll(dialogReference.FeaturesTree); // expand all nodes by default
        }

        public BitmapImage Banner => session?.GetResourceBitmap("WixSharpUI_Bmp_Banner").ToImageSource() ??
                                     session?.GetResourceBitmap("WixUI_Bmp_Banner").ToImageSource();

        public string SelectedNodeDescription
        {
            get => selectedNodeDescription;
            set { selectedNodeDescription = value; NotifyOfPropertyChange(nameof(SelectedNodeDescription)); }
        }

        /// <summary>
        /// The collection of the features selected by user as the features to be installed.
        /// </summary>
        public static List<string> UserSelectedItems { get; set; }

        /// <summary>
        /// The initial/default set of selected items (features) before user made any selection(s).
        /// </summary>
        static List<string> InitialUserSelectedItems { get; set; }

        public void GoPrev()
        {
            SaveUserSelection();
            shell?.GoPrev();
        }

        /*https://msdn.microsoft.com/en-us/library/aa367536(v=vs.85).aspx
        * ADDLOCAL - list of features to install
        * REMOVE - list of features to uninstall
        * ADDDEFAULT - list of features to set to their default state
        * REINSTALL - list of features to repair*/

        public void GoNext()
        {
            if (Host == null) return;

            bool userChangedFeatures = UserSelectedItems?.JoinBy(",") != InitialUserSelectedItems.JoinBy(",");

            if (userChangedFeatures)
            {
                string itemsToInstall = features.Where(x => (x.ViewModel as Node).Checked)
                                                .Select(x => x.Name)
                                                .JoinBy(",");

                string itemsToRemove = features.Where(x => !(x.ViewModel as Node).Checked)
                                               .Select(x => x.Name)
                                               .JoinBy(",");

                if (itemsToRemove.Any())
                    session["REMOVE"] = itemsToRemove;

                if (itemsToInstall.Any())
                    session["ADDLOCAL"] = itemsToInstall;
            }
            else
            {
                session["REMOVE"] = "";
                session["ADDLOCAL"] = "";
            }

            SaveUserSelection();
            shell.GoNext();
        }

        public void Cancel()
            => shell?.Cancel();

        public void Reset()
        {
            features.ForEach(x => (x.ViewModel as Node).Checked = x.DefaultIsToBeInstalled());
            //(features[0].ViewModel as Node).Checked = true; // enable base feature by default
        }

        FeatureItem[] features;

        void BuildFeaturesHierarchy()
        {
            features = session.Features; // must make a copy of the features as they cannot be modified in the session

            // build the hierarchy tree
            var visibleRootItems = features.Where(x => x.ParentName.IsEmpty())
                                           .OrderBy(x => x.RawDisplay)
                                           .ToArray();

            var itemsToProcess = new Queue<FeatureItem>(visibleRootItems); // features to find the children for

            while (itemsToProcess.Any())
            {
                var item = itemsToProcess.Dequeue();

                // create the view of the feature
                var viewModel = new Node(Host)
                {
                    Name = item.Title,
                    Data = item, // link view to model
                    IsEditable = !item.DisallowAbsent,
                    DefaultChecked = item.DefaultIsToBeInstalled(),
                    Checked = item.DefaultIsToBeInstalled(),
                };

                item.ViewModel = viewModel; // link model to view

                if (item.Parent != null && item.Display != FeatureDisplay.hidden)
                    (item.Parent.ViewModel as Node).Nodes.Add(viewModel); //link child view to parent view

                // even if the item is hidden process all its children so the correct hierarchy is established

                // find all children
                features.Where(x => x.ParentName == item.Name)
                        .ForEach(c =>
                         {
                             c.Parent = item; //link child model to parent model
                             itemsToProcess.Enqueue(c); //schedule for further processing
                         });

                if (UserSelectedItems != null)
                    viewModel.Checked = UserSelectedItems.Contains((viewModel.Data as FeatureItem).Name);
            }

            // add views to the treeView control
            visibleRootItems
                .Where(x => x.Display != FeatureDisplay.hidden)
                .Select(x => (Node)x.ViewModel)
                .ForEach(node => RootNodes.Add(node));

            InitialUserSelectedItems = features.Where(x => (x.ViewModel as Node).Checked)
                                               .Select(x => x.Name)
                                               .OrderBy(x => x)
                                               .ToList();
            
        }

        void SaveUserSelection()
        {
            UserSelectedItems = features.Where(x => x.IsViewChecked())
                                        .Select(x => x.Name)
                                        .OrderBy(x => x)
                                        .ToList();
        }
    }
}