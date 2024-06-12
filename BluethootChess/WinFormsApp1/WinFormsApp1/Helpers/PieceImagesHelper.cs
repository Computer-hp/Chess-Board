using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChessLogic;

namespace WindowHelper
{
    public static class PieceImages
    {
        public static Bitmap GetPieceImage(PieceColor color, char name)
        {
            string DIR = color.ToString().ToLower();
            string imagePath = DIR + "\\" + name + ".png";

            Bitmap originalImage = (Bitmap)Image.FromFile(AssetsPathHelper.AssetsPath + imagePath);
            return originalImage;
        }
    }
}

