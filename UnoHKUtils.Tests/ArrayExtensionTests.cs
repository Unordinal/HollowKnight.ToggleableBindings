#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnoHKUtils.Extensions;

namespace UnoHKUtils.Tests
{
    [TestClass]
    public class ArrayExtensionTests
    {
        [TestMethod]
        public void ArrayInserting()
        {
            int[] numbers = { 1, 2, 3, 4, 5, 6, 7 };

            Debug.WriteLine("Single Inserting");
            int numberToInsert = 42;
            Debug.WriteLine(string.Join(", ", numbers));

            Debug.WriteLine("Insert at start");
            var singleTest1 = numbers.Insert(0, numberToInsert);
            Debug.WriteLine(string.Join(", ", singleTest1));
            Assert.IsTrue(singleTest1[0] == numberToInsert);

            Debug.WriteLine("Insert at end");
            var singleTest2 = numbers.Insert(numbers.Length - 1, numberToInsert);
            Debug.WriteLine(string.Join(", ", singleTest2));
            Assert.IsTrue(singleTest2[singleTest2.Length - 2] == numberToInsert);

            Debug.WriteLine("Insert after end");
            var singleTest3 = numbers.Insert(numbers.Length, numberToInsert);
            Debug.WriteLine(string.Join(", ", singleTest3));
            Assert.IsTrue(singleTest3[singleTest3.Length - 1] == numberToInsert);

            Debug.WriteLine("Insert between");
            int numberThatWasPushed = numbers[3];
            var singleTest4 = numbers.Insert(3, numberToInsert);
            Debug.WriteLine(string.Join(", ", singleTest4));
            Assert.IsTrue(singleTest4[3] == numberToInsert);
            Assert.IsTrue(singleTest4[4] == numberThatWasPushed);

            Debug.WriteLine(null);

            Debug.WriteLine("Range Inserting");
            int[] range1 = { 14, 24, 34 };
            int[] range2 = { 96, 97, 98, 99 };
            Debug.WriteLine(string.Join(", ", numbers));

            Debug.WriteLine("Insert at start");
            var rangeTest1 = numbers.InsertRange(0, range1);
            Debug.WriteLine(string.Join(", ", rangeTest1));
            Assert.IsTrue(Compare(rangeTest1, 0, range1, 0, range1.Length));

            Debug.WriteLine("Insert at end");
            var rangeTest2 = numbers.InsertRange(numbers.Length - 1, range1);
            Debug.WriteLine(string.Join(", ", rangeTest2));
            Assert.IsTrue(Compare(rangeTest2, rangeTest2.Length - range1.Length - 1, range1, 0, range1.Length));
            
            Debug.WriteLine("Insert after end");
            var rangeTest3 = numbers.InsertRange(numbers.Length, range2);
            Debug.WriteLine(string.Join(", ", rangeTest3));
            Assert.IsTrue(Compare(rangeTest3, rangeTest3.Length - range2.Length, range2, 0, range2.Length));
            
            Debug.WriteLine("Insert between");
            var rangeTest4 = numbers.InsertRange(3, range2);
            Debug.WriteLine(string.Join(", ", rangeTest4));
            Assert.IsTrue(Compare(rangeTest4, 3, range2, 0, range2.Length));
        }

        [TestMethod]
        public void ArrayInsertingBenchmark()
        {
            const int Iterations = 10000;
            int[] numbers10 = Enumerable.Range(0, 10).ToArray();
            int[] numbers1000 = Enumerable.Range(0, 10000).ToArray();

            Debug.WriteLine("Array of 10");
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
            {
                numbers10.Insert(5, i);
            }
            sw.Stop();
            Debug.WriteLine($"Took {sw.Elapsed}");

            Debug.WriteLine("Array of 100000");
            sw.Restart();
            for (int i = 0; i < Iterations; i++)
            {
                numbers1000.Insert(5000, i);
            }
            sw.Stop();
            Debug.WriteLine($"Took {sw.Elapsed}");
            
            Debug.WriteLine("Array of 10, Range");
            sw.Restart();
            for (int i = 0; i < Iterations; i++)
            {
                numbers10.InsertRange(5, i, i, i, i, i);
            }
            sw.Stop();
            Debug.WriteLine($"Took {sw.Elapsed}");
            
            Debug.WriteLine("Array of 10000, Range");
            sw.Restart();
            for (int i = 0; i < Iterations; i++)
            {
                numbers1000.InsertRange(5000, i, i, i, i, i);
            }
            sw.Stop();
            Debug.WriteLine($"Took {sw.Elapsed}");
        }

        private bool Compare<T>(T[] first, int firstIndex, T[] second, int secondIndex, int length)
        {
            if (first.Length < length || second.Length < length)
                return false;

            for (int i = 0; i < length; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(first[firstIndex + i], second[secondIndex + i]))
                    return false;
            }

            return true;
        }
    }
}
