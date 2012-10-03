using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Drawing;

namespace Miiror.Utils
{
    internal enum MiiIcon
    {
        ExchangeHover,
        ExchangeNormal,
        CheckHover,
        CheckNormal,
        UnCheckHover,
        UnCheckNormal
    }

    internal static class IconManager
    {
        private static Assembly assembly = Assembly.GetExecutingAssembly();
        private static Dictionary<string, Image> iconList = new Dictionary<string,Image>();

        public static Size IconSize { get { return new Size(32, 32); } }

        public static Image GetIcon(MiiIcon icon)
        {
            string resourcePath = "Miiror.Resources.";

            switch (icon)
            {
                case MiiIcon.CheckHover:
                    resourcePath += "check-mover.png";
                    break;
                case MiiIcon.CheckNormal:
                    resourcePath += "check-mout.png";
                    break;
                case MiiIcon.ExchangeHover:
                    resourcePath += "exchange-mover.png";
                    break;
                case MiiIcon.ExchangeNormal:
                    resourcePath += "exchange-mout.png";
                    break;
                case MiiIcon.UnCheckHover:
                    resourcePath += "uncheck-mover.png";
                    break;
                case MiiIcon.UnCheckNormal:
                    resourcePath += "uncheck-mout.png";
                    break;
            }

            if (!iconList.Keys.Contains(resourcePath))
            {
                Bitmap bitmap = new Bitmap(IconSize.Width, IconSize.Height);
                Graphics g = Graphics.FromImage(bitmap);
                g.Clear(Color.Transparent);
                Image image = Image.FromStream(assembly.GetManifestResourceStream(resourcePath));
                g.DrawImage(image, new Rectangle(4, 4, IconSize.Width - 8, IconSize.Height - 8), new Rectangle(0, 0, IconSize.Width, IconSize.Height), GraphicsUnit.Pixel);
                iconList.Add(resourcePath, bitmap);
                g.Dispose();
            }
            return iconList[resourcePath];
        }
    }
}
