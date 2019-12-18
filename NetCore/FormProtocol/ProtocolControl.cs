using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace NetCore.FormProtocol
{
    public struct ProtocolControl
    {
        public static readonly ProtocolControl Empty = default(ProtocolControl);

        public int ID;
        public ControlFlag Flag;
        public string Name;
        public Size Size;
        public Point Point;
        public Padding Padding;
        public Padding Margin;
        public int ParentID;
        public string Value;
        public bool RTL;//right to left control
        public List<int> Sons;

        public static ProtocolControl Create()
        {
            return new ProtocolControl()
            {
                ID = 0,
                Flag = ControlFlag.None,
                Name = string.Empty,
                Size = default(Size),
                Point = default(Point),
                ParentID = 0,
                Value = string.Empty,
                RTL = false,
                Sons = null,
                Padding = default(Padding),
                Margin = default(Padding)
            };
        }

        public static implicit operator Control(ProtocolControl protocolControl)
        {
            Control control = null;
            switch (protocolControl.Flag)
            {
                case ControlFlag.Label:
                    {
                        control = new Label()
                        {
                            Name = protocolControl.Name,
                            Tag = protocolControl.ID,
                            Text = protocolControl.Value,
                            Size = protocolControl.Size,
                            Location = protocolControl.Point,
                            Padding = protocolControl.Padding,
                            Margin = protocolControl.Margin,
                            TextAlign = ContentAlignment.MiddleCenter,
                            RightToLeft = protocolControl.RTL ? RightToLeft.Yes : RightToLeft.No
                        };
                        break;
                    }
            }
            return control;
        }
    }
}
