using Fractals.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

using Point = System.Drawing.Point;

namespace Fractals.Generators
{
   
    public class ThreadTest
    {
        protected byte[,] data = new byte[10, 10];

        protected double[,,] actualData;
        
        protected void Work(Point min, Point max, byte value)
        {
            Debug.WriteLine("Drawing " + value);

            for (int y = min.Y; y < max.Y; y++)
            {
                for (int x = min.X; x < max.X; x++)
                {
                    data[x, y] = value;
                }
            }
        }

        protected void ActualWork(Rect values, Rectangle pixels, double[,,] data)
        {
            double dotSizeX = values.Width / pixels.Width;
            double dotSizeY = values.Height / pixels.Height;

            for (int y = pixels.Top; y < pixels.Height; y++)
            {
                for (int x = pixels.Left; x < pixels.Width; x++)
                {
                    data[x, y, 0] = (x - pixels.X) * dotSizeX + values.Left;
                    data[x, y, 1] = (y - pixels.Y) * dotSizeY + values.Top;
                }
            }
        }

        public List<Task> tasks = new List<Task>();


        private void GoodWork(ValueRect values, PixelRect pixels, double[,,] data)
        {
            double dotSizeX = values.Width / pixels.Width;
            double dotSizeY = values.Height / pixels.Height;

            for (int y = pixels.top; y < pixels.bottom; y++)
            {
                for (int x = pixels.left; x < pixels.right; x++)
                {
                    data[x, y, 0] = (x - pixels.left) * dotSizeX + values.left;
                    data[x, y, 1] = (y - pixels.top) * dotSizeY + values.top;
                }
            }
        }



        public void GoodStart(ValueRect values, PixelRect pixels)
        {
            actualData = new double[pixels.Width, pixels.Height, 2];

            tasks.Clear();

            int fraction = 2;

            List<(ValueRect, PixelRect, double[,,])> args = new List<(ValueRect, PixelRect, double[,,])>();

            for (int y = 0; y < fraction; y++)
            {
                for (int x = 0; x < fraction; x++)
                {
                    args.Add((values.GetFraction(fraction, x, y), pixels.GetFraction(fraction, x, y), actualData));
                    //tasks.Add( Task.Run(() => GoodWork(values.GetFraction(fraction, x, y), pixels.GetFraction(fraction, x, y), actualData)));
                }
            }

            foreach (var arg in args)
                tasks.Add(Task.Run(() => GoodWork(arg.Item1, arg.Item2, arg.Item3)));

            Task.WaitAll(tasks.ToArray());

            Debug.WriteLine("Done!");

            for (int y = 0; y < data.GetLength(1); y++)
            {
                for (int x = 0; x < data.GetLength(0); x++)
                {
                    Debug.Write($"[{actualData[x, y, 0]}, {actualData[x, y, 1]}]\t");
                }
                Debug.WriteLine("");
            }
        }


        public void Start(Rect values, Rectangle pixels)
        {
            actualData = new double[pixels.Width, pixels.Height, 2];

            tasks.Clear();

            //tasks.Add(Task.Run(() => Work(new Point(0, 0), new Point(10, 5), 10)));
            //tasks.Add(Task.Run(() => Work(new Point(0, 5), new Point(10, 10), 20)));

            tasks.Add(Task.Run(() => ActualWork(
                new Rect(values.X, values.Y, values.Width / 2, values.Height / 2),
                new Rectangle(pixels.X, pixels.Y, pixels.Width / 2, pixels.Height / 2),
                actualData)));

            tasks.Add(Task.Run(() => ActualWork(
                new Rect(values.Width / 2, values.Y, values.Width, values.Height / 2),
                new Rectangle(pixels.Width / 2, pixels.Y, pixels.Width, pixels.Height / 2),
                actualData)));

            tasks.Add(Task.Run(() => ActualWork(
                new Rect(values.X, values.Height / 2, values.Width / 2, values.Height),
                new Rectangle(pixels.X, pixels.Height / 2, pixels.Width / 2, pixels.Height),
                actualData)));

            tasks.Add(Task.Run(() => ActualWork(
                new Rect(values.Width / 2, values.Height / 2, values.Width, values.Height),
                new Rectangle(pixels.Width / 2, pixels.Height / 2, pixels.Width, pixels.Height),
                actualData)));

            Task.WaitAll(tasks.ToArray());

            Debug.WriteLine("Done!");

            for (int y = 0; y < data.GetLength(1); y++)
            {
                for (int x = 0; x < data.GetLength(0); x++)
                {
                    Debug.Write($"[{actualData[x, y, 0]}, {actualData[x, y, 1]}]\t");
                }
                Debug.WriteLine("");
            }
        }

    }
}
