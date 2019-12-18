using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CoreThreading.CustomCon
{
    [Serializable]
    public unsafe struct ConsoleConfig : ISerializable, IDeserializationCallback
    {
        public static ConsoleConfig Default = Create();

        [NonSerialized]
        private SerializationInfo _siInfo;
        [DefaultValue(@"..\Log.log")]
        public string LogFile;
        [DefaultValue(false)]
        public bool Commands;
        [DefaultValue(false)]
        public bool AccessClipBoard;
        [DefaultValue(ConsoleKey.End)]
        public ConsoleKey CloseKey;
        [DefaultValue((uint)0)]
        public uint nFont;
        [DefaultValue(true)]
        public bool TrueType;//truetype
        [DefaultValue(ConsoleFontSize._8_X_12)]
        public ConsoleFontSize FontSize;//COORD.X = 0 | COORD.Y = value(INT32)
        [DefaultValue(FontWeight.Regular)]
        public FontWeight FontWeight;
        [DefaultValue("Raster Fonts")]
        public string FontName;
        public void Set(object key, object value)
        {
            this.InitializeSetValue<ConsoleConfig, object, object>(key, value);
        }
        public static ConsoleConfig Create()
        {
            return new ConsoleConfig().InitializeDefaultValues();
        }
        public static implicit operator CONSOLE_FONT_INFO_EX(ConsoleConfig font)
        {
            var retfont = new CONSOLE_FONT_INFO_EX()
            {
                cbSize = (uint)sizeof(CONSOLE_FONT_INFO_EX),
                dwFontSize = COORD.Create(font.FontSize),
                FontFamily = font.TrueType ? 4 : 0,
                FontWeight = (int)font.FontWeight,
                nFont = font.nFont
            };
            Marshal.Copy(font.FontName.ToCharArray(), 0, new IntPtr(retfont.FaceName), font.FontName.Length);
            return retfont;
        }

        public void OnDeserialization(object sender)
        {
            Debug.Assert(this._siInfo == null, "Null value exception", string.Format("{0}"), new ArgumentNullException("info"));
            this.CloseKey = (ConsoleKey)this._siInfo.GetByte("CloseKey");
            this.nFont = this._siInfo.GetUInt32("nFont");
            this.TrueType = this._siInfo.GetBoolean("TrueType");
            this.FontSize = (ConsoleFontSize)this._siInfo.GetInt32("FontSize");
            this.FontWeight = (FontWeight)this._siInfo.GetInt32("FontWeight");
            this.FontName = this._siInfo.GetString("FontName");
            this.AccessClipBoard = this._siInfo.GetBoolean("AccessClipBoard");
            this.Commands = this._siInfo.GetBoolean("Commands");
            this.LogFile = this._siInfo.GetString("LogFile");
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("CloseKey", (byte)this.CloseKey);
            info.AddValue("nFont", this.nFont);
            info.AddValue("TrueType", this.TrueType);
            info.AddValue("FontSize", (int)this.FontSize);
            info.AddValue("FontWeight", (int)this.FontWeight);
            info.AddValue("FontName", this.FontName);
            info.AddValue("AccessClipBoard", this.AccessClipBoard);
            info.AddValue("Commands", this.Commands);
            info.AddValue("LogFile", this.LogFile);
        }
    }
}
