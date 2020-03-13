using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;

namespace ComputerGraph_1
{
    abstract class Filters
    {
        protected abstract Color calculateNewPixelColor(Bitmap Source, int W, int H);

        public virtual Bitmap processImage(Bitmap sourceImage, BackgroundWorker worked)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worked.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worked.CancellationPending)
                {
                    return null;
                }
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;
        }

        public int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
    };

    class InvertFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap Source, int W, int H)
        {

            Color sourceColor = Source.GetPixel(W, H);
            Color result = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.G, 255 - sourceColor.B);

            return result;
        }
    };

    class matrixFilters : Filters
    {
        protected float[,] kernel = null;


        protected matrixFilters() { }

        public matrixFilters(float[,] kernel)
        {
            this.kernel = kernel;
        }

        protected override Color calculateNewPixelColor(Bitmap Source, int W, int H)
        {

            int radX = kernel.GetLength(0) / 2;
            int radY = kernel.GetLength(1) / 2;
            float resR = 0;
            float resG = 0;
            float resB = 0;

            for (int i = -radY; i <= radY; i++)
            {
                for (int j = -radX; j <= radX; j++)
                {
                    int iX = Clamp(W + j, 0, Source.Width - 1);
                    int iY = Clamp(H + i, 0, Source.Height - 1);
                    Color neighbor = Source.GetPixel(iX, iY);
                    resR += neighbor.R * kernel[j + radX, i + radY];
                    resG += neighbor.G * kernel[j + radX, i + radY];
                    resB += neighbor.B * kernel[j + radX, i + radY];
                }
            }

            return Color.FromArgb(Clamp((int)resR, 0, 255), Clamp((int)resG, 0, 255), Clamp((int)resB, 0, 255));
        }
    };
    class BlurFilter : matrixFilters
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
                }
            }
        }

    };

    //Фильтр Гаусса

    class GausFilter : matrixFilters
    {
        public void GKernel(int rad, float sigma)
        {
            int size = 2 * rad + 1;
            kernel = new float[size, size];
            float norm = 0;
            for (int i = -rad; i <= rad; i++)
            {
                for (int j = -rad; j <= rad; j++)
                {
                    kernel[i + rad, j + rad] = (float)(Math.Exp(-(i * i + j * j) / (2 * sigma * sigma)));
                    norm += kernel[i + rad, j + rad];
                }
            }

            // Нормировка ядра 
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= norm;
            }
        }

        public GausFilter()
        {
            GKernel(3, 2);
        }
    };
    class GrayScale : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap Source, int W, int H)
        {
            Color sourceColor = Source.GetPixel(W, H);
            double Int = 0.299 * sourceColor.R + 0.587 * sourceColor.G + 0.114 * sourceColor.B;

            Color result = Color.FromArgb((int)Int, (int)Int, (int)Int);

            return result;
        }

    };
    class Sepia : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap Source, int W, int H)
        {
            Color sourceColor = Source.GetPixel(W, H);
            double k = 30.0;
            double Int = 0.299 * sourceColor.R + 0.587 * sourceColor.G + 0.114 * sourceColor.B;
            int R = (int)(Int + 2.0 * k);
            //R = Clamp(R, W, H);
            int G = (int)(Int + 0.5 * k);
            //G = Clamp(G, W,H);
            int B = (int)(Int - k);
            //B = Clamp(B,W,H);
            Color result = Color.FromArgb(Clamp(R, 0, 255), Clamp(G, 0, 255), Clamp(B, 0, 255));

            return result;
        }

    };
    class bright : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap Source, int W, int H)
        {
            Color sourceColor = Source.GetPixel(W, H);

            Color result = Color.FromArgb(Clamp(sourceColor.R + 20, 0, 255), Clamp(sourceColor.G + 20, 0, 255), Clamp(sourceColor.B + 20, 0, 255));

            return result;
        }
    };
    class Sobel : matrixFilters
    {
        public Sobel()
        {
            int sX = 3;
            int sY = 3;
            kernel = new float[sX, sY];
            kernel[0, 0] = -1;
            kernel[1, 0] = -2;
            kernel[2, 0] = -1;
            for (int i = 0; i < sX; i++)
                for (int j = 1; j < sY; j++)
                {
                    if (j == 1)
                    {
                        kernel[i, j] = 0;
                    }

                    if (j == 2)
                    {
                        kernel[i, j] = -kernel[i, 0];
                    }
                }
        }
    };
    class Sharpness : matrixFilters
    {
        public Sharpness()
        {
            int X = 3;
            int Y = 3;
            kernel = new float[X, Y];
            kernel[0, 0] = 0;
            kernel[1, 0] = -1;
            kernel[2, 0] = 0;
            kernel[0, 1] = -1;
            kernel[1, 1] = 5;
            kernel[2, 1] = -1;
            kernel[0, 2] = 0;
            kernel[1, 2] = -1;
            kernel[2, 2] = 0;
        }

    };
    //тиснение
    class Tiss : matrixFilters
    {
        public Tiss()
        {
            kernel = new float[3, 3] { { 0,1,0 }, { 1,0,-1 }, { 0,-1,0} };
        }
        
    };
    //перенос 
    class Shift : Filters
    {
        double x0, y0;
        public Shift(double x00, double y00)
        {
            x0 = x00;y0 = y00;
        }
        protected override Color calculateNewPixelColor(Bitmap Source, int W, int H)
        {
            Color sourceColor = Source.GetPixel(W, H);
            int nX = Clamp((int)(W+x0),0, Source.Width-1);
            int nY = Clamp((int)(H + y0), 0, Source.Height-1);
            return Source.GetPixel(nX,nY);

        }
    };
    //поворот  
    class Rotation : Filters
    {
        double x0, y0,my;
        public Rotation(double x00, double y00, double my0)
        {
            x0 = x00; y0 = y00; my = my0;
        }
        protected override Color calculateNewPixelColor(Bitmap Source, int W, int H)
        {
            Color sourceColor = Source.GetPixel(W, H);
            double si = Math.Sin(my), co = Math.Cos(my);
            int nX = Clamp((int)((W - x0)*co-(H-y0)*si+x0), 0, Source.Width-1);
            int nY = Clamp((int)((W - x0) * si + (H - y0) * co + y0), 0, Source.Height-1);
            return Source.GetPixel(nX, nY);

        }
    };
    //волны
    class Waves : Filters
    {
        
       
        protected override Color calculateNewPixelColor(Bitmap Source, int W, int H)
        {
            Color sourceColor = Source.GetPixel(W, H);
            int nX = Clamp((int)(W+20*Math.Sin(0.20943951023*W)), 0, Source.Width - 1);
            int nY = Clamp(H, 0, Source.Height - 1);
            return Source.GetPixel(nX, nY);

        }
    };
    //класс морфологии
    class Morphology : Filters
    {
        protected int[,] st_elem;
        protected bool Dil;
        protected Morphology() //элемент - квадрат из единиц 
        {
            st_elem= new int[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
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
            for (int j = -Ydiam/ 2; j <= Ydiam / 2; j++)            for (int i = -Xdiam / 2; i <= Xdiam / 2; i++)
            {
                    if (Dil) //если расширение
                    {
                        //дополняем все 1 в структурном элементе 
                        if ((st_elem[i,j]==1) && (Source.GetPixel(W + i, H + j).R > BITmaxR))
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
                return Color.FromArgb(BITmaxR,BITmaxG,BITmaxB);
            else return Color.FromArgb(BITminR, BITminG, BITminB);

        }
        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            int Xrad = st_elem.GetLength(0);
            int Yrad = st_elem.GetLength(1);
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = Xrad / 2; i < sourceImage.Width - Xrad / 2; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                //прерывание процесса 
                if (worker.CancellationPending)
                    return null;
                for (int j = Yrad / 2; j < sourceImage.Height - Yrad / 2; j++) // применяем эффект сверху вниз, потом в ширину 
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