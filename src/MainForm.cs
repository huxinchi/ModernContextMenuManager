using ModernContextMenuManager.Controls;
using ModernContextMenuManager.Helpers;
using ModernContextMenuManager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModernContextMenuManager
{
    public partial class MainForm : Form
    {
        private List<AppModel>? apps;
        private PackagedComHelper.ComPackage[]? comPackages;
        private PackagedComHelper.BlockedClsid[]? blockedClsids;

        private TreeView treeView;
        private TextBox searchBox;
        private string oldSearchText = "";

        public MainForm()
        {
            InitializeComponent();
            Text = "Modern Context Menu Manager";
            this.Load += MainForm_Load;

            var layout = new TableLayoutPanel()
            {
                ColumnCount = 1,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Percent,100)
                },
                RowCount = 2,
                RowStyles =
                {
                    new RowStyle(SizeType.AutoSize),
                    new RowStyle(SizeType.AutoSize),
                },
                Dock = DockStyle.Fill,
            };

            layout.SuspendLayout();
            layout.Controls.Add(new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                Controls =
                {
                    new Button()
                    {
                        Text = "Refresh",
                        Margin = new Padding(12,8,8,0),
                        Command = new RelayCommand(async () => await RefreshAsync())
                    },
                    (searchBox = new TextBox()
                    {
                        Width = 120,
                        Margin = new Padding(0,8,8,0),
                    }),
                    new Button()
                    {
                        Text = "Search",
                        Margin = new Padding(0,8,8,0),
                        Command = new RelayCommand(() => Search())
                    }
                }
            }, 0, 0);

            layout.Controls.Add((treeView = new TreeViewEx()
            {
                Scrollable = true,
                CheckBoxes = true,
                Dock = DockStyle.Fill,
            }), 0, 1);

            this.Controls.Add(layout);

            layout.ResumeLayout();
            layout.PerformLayout();

            searchBox.KeyDown += (s, a) =>
            {
                if (a.KeyCode == Keys.Enter)
                {
                    Search();
                }
            };

            treeView.BeforeCheck += (s, a) =>
            {
                if (a.Node?.Tag is AppModel appModel)
                {
                    a.Cancel = true;
                }
                else if (a.Node?.Tag is ClsidCheckModel clsidModel)
                {
                    if (!clsidModel.CanModify) a.Cancel = true;
                }
            };

            treeView.AfterCheck += (s, a) =>
            {
                if (a.Node?.Tag is AppModel appModel)
                {
                    a.Node.Checked = false;
                }
                else if (a.Node?.Tag is ClsidCheckModel clsidModel)
                {
                    if (a.Node.Checked != clsidModel.Enabled)
                    {
                        clsidModel.Enabled = a.Node.Checked;
                    }

                    if (a.Node.Checked != clsidModel.Enabled)
                    {
                        a.Node.Checked = clsidModel.Enabled;
                    }
                }
            };

            treeView.ContextMenuStrip = new ContextMenuStrip()
            {
                Items =
                {
                    new ToolStripMenuItem("Copy")
                    {
                        Command = new RelayCommand(() => CopySelectedNode())
                    }
                }
            };
        }

        private async void MainForm_Load(object? sender, EventArgs e)
        {
            await RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            if (!this.Enabled) return;
            try
            {
                this.Enabled = false;

                searchBox.Text = "";
                oldSearchText = "";
                treeView.Nodes.Clear();

                comPackages = PackagedComHelper.GetAllComPackages();
                blockedClsids = PackagedComHelper.GetBlockedClsids();

                var dict = blockedClsids
                    .DistinctBy(c => c.Clsid)
                    .ToDictionary(c => c.Clsid, c => c.Type);

                var list = new List<AppModel>(comPackages.Length);
                for (int i = 0; i < comPackages.Length; i++)
                {
                    if (comPackages[i].Clsids.Length > 0)
                    {
                        var packageInfo = PackageManager.GetPackageInfoByFullName(comPackages[i].PackageFullName);
                        if (packageInfo != null)
                        {
                            var appInfo = await PackageManager.GetPackageAppInfoAsync(packageInfo.PackageInstallLocation);
                            if (appInfo != null && appInfo.ContextMenuGuids.Count > 0)
                            {
                                list.Add(new AppModel(appInfo, packageInfo, dict));
                            }
                        }
                    }
                }

                apps = list;
                Search(true);
            }
            finally
            {
                this.Enabled = true;
            }
        }

        private void Search(bool force = false)
        {
            if (apps == null) return;

            var searchText = searchBox.Text;
            if (!force && oldSearchText == searchText) return;
            oldSearchText = searchText;

            treeView.Nodes.Clear();
            for (int i = 0; i < apps.Count; i++)
            {
                bool flag = false;
                bool parentFlag1 = !string.IsNullOrEmpty(searchText) && apps[i].DisplayName.Contains(searchText, StringComparison.OrdinalIgnoreCase);
                bool parentFlag2 = parentFlag1 || string.IsNullOrEmpty(searchText);

                var node = new TreeNodeEx(apps[i].DisplayName)
                {
                    Tag = apps[i],
                    ShowCheckBox = false,
                };
                if (parentFlag1)
                {
                    node.BackColor = Color.LightYellow;
                }
                for (int j = 0; j < apps[i].Clsids.Count; j++)
                {
                    var flag2 = SearchGuid(apps[i].Clsids[j].Clsid, searchText);
                    if (parentFlag2 || flag2 || string.IsNullOrEmpty(searchText))
                    {
                        flag = true;

                        var clsid = apps[i].Clsids[j];
                        var childNode = new TreeNodeEx(clsid.Clsid.ToString("B").ToUpperInvariant());
                        childNode.Tag = clsid;
                        childNode.Checked = clsid.Enabled;
                        if (!clsid.CanModify) childNode.ForeColor = SystemColors.Control;
                        if (flag2) childNode.BackColor = Color.LightYellow;

                        node.Nodes.Add(childNode);
                    }
                }
                if (parentFlag2 || flag)
                {
                    treeView.Nodes.Add(node);
                }
            }

            treeView.ExpandAll();
            treeView.Nodes?.OfType<TreeNode>().FirstOrDefault()?.EnsureVisible();

            static bool SearchGuid(Guid _guid, string _search)
            {
                return !string.IsNullOrEmpty(_search)
                    && (_guid.ToString("N").Contains(_search, StringComparison.OrdinalIgnoreCase)
                        || _guid.ToString("B").Contains(_search, StringComparison.OrdinalIgnoreCase));
            }
        }

        private void CopySelectedNode()
        {
            string text = "";
            if (treeView.SelectedNode?.Tag is AppModel appModel)
            {
                text = $"""
                    DisplayName: {appModel.AppInfo.DisplayName}
                    PackageFamilyName: {appModel.PackageInfo.PackageFamilyName}
                    PackageFullName: {appModel.PackageInfo.PackageFullName}
                    PackageInstallLocation: {appModel.PackageInfo.PackageInstallLocation}
                    PackageId.Name: {appModel.PackageInfo.PackageId.Name}
                    PackageId.Version: {appModel.PackageInfo.PackageId.Version}
                    PackageId.Publisher: {appModel.PackageInfo.PackageId.Publisher}
                    PackageId.PublisherId: {appModel.PackageInfo.PackageId.PublisherId}
                    PackageId.Architecture: {appModel.PackageInfo.PackageId.Architecture}
                    """;
            }
            else if (treeView.SelectedNode?.Tag is ClsidCheckModel clsidModel)
            {
                text = clsidModel.Clsid.ToString("B").ToUpperInvariant();
            }

            if (!string.IsNullOrEmpty(text))
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        Clipboard.SetText(text);
                        break;
                    }
                    catch { }
                }
            }
        }
    }
}
