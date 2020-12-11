using System;
using System.Collections.Generic;
using System.Text;

namespace TestElva
{
    abstract class GeoJson
    {
        public string type { get; set; }
        abstract public void ReduceTheNumberOfPoints(int divider);
    }
    class Point : GeoJson
    {
        public double[] coordinates { get; set; }
        override public void ReduceTheNumberOfPoints(int divider)
        {

        }
    }
    class GeoJsonLineString : GeoJson
    {
        public double[][] coordinates { get; set; }
        override public void ReduceTheNumberOfPoints(int divider)
        {

            int kmax = coordinates.Length / divider;
            if (coordinates.Length % divider != 0)
                kmax++;
            int k = 0;
            double[][] newArray = new double[kmax][];
            for (int j = 0; j < coordinates.Length; j++)
            {
                if (j % divider == 0)
                {
                    //Console.WriteLine(" j:" + j+" k:"+k);
                    newArray[k++] = coordinates[j];
                }
            }
            coordinates = newArray;
        }
    }
    class GeoJsonPolygon : GeoJson
    {
        public double[][][] coordinates { get; set; }
        override public void ReduceTheNumberOfPoints(int divider)
        {
            for(int i=0;i<coordinates.Length;i++)
            {
                int kmax = coordinates[i].Length / divider;
                if (coordinates[i].Length % divider != 0)
                    kmax++;
                int k = 0;
                double[][] newArray = new double[kmax][];
                for (int j = 0; j < coordinates[i].Length; j++)
                {
                    if (j % divider == 0)
                    {
                        //Console.WriteLine(" j:" + j+" k:"+k);
                        newArray[k++] = coordinates[i][j];
                    }
                }
                coordinates[i] = newArray;
            }
        }
    }
    class GeoJsonMultiPolygon : GeoJson
    {
        public double[][][][] coordinates { get; set; }
        public int CountNumberOfPoints()
        {
            int i = 0;
            foreach (var x in coordinates)
            {
                foreach (var y in x)
                {
                    foreach (var z in y)
                    {
                        i++;
                    }

                }
            }
            return i;
        }
        override public void ReduceTheNumberOfPoints(int divider)
        {
            foreach (var x in coordinates)
            {
                for (int t = 0; t < x.Length; t++)
                {
                    int kmax = x[t].Length / divider;
                    if (x[t].Length % divider != 0)
                        kmax++;
                    //Console.WriteLine("t:" + t+ " x[t].length:" + x[t].Length+" kmax:"+kmax);
                    int k = 0;
                    double[][] newArray = new double[kmax][];
                    for (int j = 0; j < x[t].Length; j++)
                    {
                        if (j % divider == 0)
                        {
                            //Console.WriteLine(" j:" + j+" k:"+k);
                            newArray[k++] = x[t][j];
                        }
                    }
                    x[t] = newArray;
                }
            }
        }
    }
}
