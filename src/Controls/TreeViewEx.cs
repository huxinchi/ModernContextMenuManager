using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ModernContextMenuManager.Controls
{
    internal partial class TreeViewEx : TreeView
    {
        public TreeViewEx()
        {
            this.DrawMode = TreeViewDrawMode.OwnerDrawText;
        }

        private const int WM_LBUTTONDBLCLK = 0x0203;
        private const int WM_RBUTTONDOWN = 0x0204;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDBLCLK)
            {
                TreeViewHitTestInfo tvhti = HitTest(new Point(unchecked((int)m.LParam)));
                if (tvhti != null && tvhti.Node != null && tvhti.Location == TreeViewHitTestLocations.StateImage)
                {
                    m.Result = IntPtr.Zero;
                    tvhti.Node.Checked = !tvhti.Node.Checked;
                    return;
                }
            }
            else if (m.Msg == WM_RBUTTONDOWN)
            {
                TreeViewHitTestInfo tvhti = HitTest(new Point(unchecked((int)m.LParam)));
                if (tvhti != null)
                    this.SelectedNode = tvhti.Node;
            }
            base.WndProc(ref m);
        }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            var node = e.Node;
            var g = e.Graphics;
            var bounds = e.Bounds;
            TreeNodeStates curState = e.State;

            if (node?.TreeView != null)
            {
                using var brush = new SolidBrush(BackColor);

                if (CheckBoxes && node is TreeNodeEx treeNodeEx && !treeNodeEx.ShowCheckBox)
                {
                    e.DrawDefault = false;

                    bounds.X -= 16;
                    bounds.Width += 16;
                    g.FillRectangle(brush, bounds);
                    bounds.Width -= 16;
                }

                // Simulate default text drawing here
                Font? font = node.NodeFont ?? node.TreeView?.Font;
                Color color = (((curState & TreeNodeStates.Selected) == TreeNodeStates.Selected) && node.TreeView!.Focused) ? SystemColors.HighlightText : (node.ForeColor != Color.Empty) ? node.ForeColor : node.TreeView!.ForeColor;

                // Draw the actual node.
                if ((curState & TreeNodeStates.Selected) == TreeNodeStates.Selected)
                {
                    g.FillRectangle(SystemBrushes.Highlight, bounds);
                    ControlPaint.DrawFocusRectangle(g, bounds, color, SystemColors.Highlight);
                    TextRenderer.DrawText(g, node.Text, font, bounds, color, TextFormatFlags.Default);
                }
                else
                {
                    using var nodeBrush = new SolidBrush(node.BackColor);
                    g.FillRectangle(nodeBrush, bounds);
                    TextRenderer.DrawText(g, node.Text, font, bounds, color, TextFormatFlags.Default);
                }
            }

            base.OnDrawNode(e);
        }
    }

    internal partial class TreeNodeEx : TreeNode
    {
        private bool showCheckBox = true;

        public TreeNodeEx() : base() { }
        public TreeNodeEx(string? text) : base(text) { }
        public TreeNodeEx(string? text, TreeNode[] children) : base(text, children) { }
        public TreeNodeEx(string? text, int imageIndex, int selectedImageIndex) : base(text, imageIndex, selectedImageIndex) { }
        public TreeNodeEx(string? text, int imageIndex, int selectedImageIndex, TreeNode[] children) : base(text, imageIndex, selectedImageIndex, children) { }

        public bool ShowCheckBox
        {
            get => showCheckBox;
            set
            {
                showCheckBox = value;
                this.TreeView?.Invalidate();
            }
        }
    }
}
