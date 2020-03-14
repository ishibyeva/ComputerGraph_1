using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;

namespace ComputerGraph_1
{
    class Morphology: Filters
    {
        protected int[,] st_elem;
        protected bool Dil;
        protected Morphology() //элемент - квадрат из единиц 
        {
            st_elem = new int[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
        }
        public Morphology(int[,] st_elem)
        {
            this.st_elem = st_elem;
        }

        protected override Color calculateNewPixelColor(Bitmap Source, int W, int H)
        {
            int Xdiam = st_elem.GetLength(0);//наш структурныый элемент 3х3, радиус по одной клетке
            int Ydiam = st_elem.GetLength(1);
            int BITmaxR = 0; int BITmaxG = 0; int BITmaxB = 0; // черный
            int BITminR = 255; int BITminG = 255; int BITminB = 255;//белый
            for (int j = -Ydiam / 2; j <= Ydiam / 2; j++)
                for (int i = -Xdiam / 2; i <= Xdiam / 2; i++)
                {
                    if (Dil) //если расширение
                    {
                        //дополняем все 1 в структурном элементе 
                        if ((st_elem[i, j] == 1) && (Source.GetPixel(W + i, H + j).R > BITmaxR))
                            BITmaxR = Source.GetPixel(W + i, H + j).R;
                        if ((st_elem[i, j] == 1) && (Source.GetPixel(W + i, H + j).G > BITmaxG))
                            BITmaxG = Source.GetPixel(W + i, H + j).G;
                        if ((st_elem[i, j] == 1) && (Source.GetPixel(W + i, H + j).B > BITmaxB))
                            BITmaxB = Source.GetPixel(W + i, H + j).B;

                    }
                    else //сужение
                    {
                        if ((st_elem[i, j] == 1) && (Source.GetPixel(W + i, H + j).R < BITminR))

                            BITminR = Source.GetPixel(W + i, H + j).R;
                        if ((st_elem[i, j] == 1) && (Source.GetPixel(W + i, H + j).G < BITminG))

                            BITminG = Source.GetPixel(W + i, H + j).G;
                        if ((st_elem[i, j] == 1) && (Source.GetPixel(W + i, H + j).B < BITminB))

                            BITminB = Source.GetPixel(W + i, H + j).B;
                    }
                }
            if (Dil)
                return Color.FromArgb(BITmaxR, BITmaxG, BITmaxB);
            else return Color.FromArgb(BITminR, BITminG, BITminB);

        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            int Xdiam = st_elem.GetLength(0);
            int Ydiam = st_elem.GetLength(1);
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = Xdiam / 2; i < sourceImage.Width - Xdiam / 2; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                //прерывание процесса 
                if (worker.CancellationPending)
                    return null;
                for (int j = Ydiam / 2; j < sourceImage.Height - Ydiam / 2; j++) // применяем эффект сверху вниз, потом в ширину 
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
            }
            return resultImage;
        }
    };
    class Dilation : Morphology //  расширение
    {
        public Dilation()
        {
            Dil = true;
        }
        public Dilation(int[,] st_elem_)
        {
            this.st_elem = st_elem_;
            Dil = true;
        }
    }
    class Erosion : Morphology //  сужение
    {
        public Erosion()
        {
            Dil = false;
        }
        public Erosion(int[,] st_elem_)
        {
            this.st_elem = st_elem_;
            Dil = false;
        }
    }
    class Opening : Morphology // Морфологическое открытие
    {

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int W, int H)
        {
            return sourceImage.GetPixel(W, H);
        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            //сначала сужение, потом расширение
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            Filters Err = new Erosion(st_elem);
            Filters Dila = new Dilation(st_elem);
            Bitmap result = Err.processImage(sourceImage, worker);
            result = Dila.processImage(result, worker);//применили два фильтра 
            return result;
        }
    }
    class Closing : Morphology // Морфологическое открытие
    {

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int W, int H)
        {
            return sourceImage.GetPixel(W, H);
        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            //сначала сужение, потом расширение
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            Filters Err = new Erosion(st_elem);
            Filters Dila = new Dilation(st_elem);
            Bitmap result = Dila.processImage(sourceImage, worker);//применили два фильтра 
            result = Err.processImage(result, worker);

            return result;
        }
    }
    class Grad : Morphology
    {

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int W, int H)
        {
            return sourceImage.GetPixel(W, H);
        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            Filters Err = new Erosion(st_elem);
            Filters Dila = new Dilation(st_elem);
            Bitmap resultD = Dila.processImage(sourceImage, worker);
            Bitmap resultE = Err.processImage(sourceImage, worker);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                //прерывание процесса 
                if (worker.CancellationPending)
                    return null;
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    // изображение с расширением (-) изобржение с эрозией, выделяет границы всего, что есть на изображении
                    // с учетом цвета, потому что при чб некоторые границы пропадают 
                    int newR = Clamp(resultD.GetPixel(i, j).R - resultE.GetPixel(i, j).R, 0, 255);
                    int newG = Clamp(resultD.GetPixel(i, j).G - resultE.GetPixel(i, j).G, 0, 255);
                    int newB = Clamp(resultD.GetPixel(i, j).B - resultE.GetPixel(i, j).B, 0, 255);
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;

        }
    }
}
