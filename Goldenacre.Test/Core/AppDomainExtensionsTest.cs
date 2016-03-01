﻿using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Goldenacre.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable UnusedVariable

namespace Goldenacre.Test.Core
{
    [TestClass]
    public class AppDomainExtensionsTest
    {
        [TestMethod]
        public void Test_IsAssemblyLoaded_when_valid()
        {
            var result = AppDomain.CurrentDomain.IsAssemblyLoaded("Goldenacre.Test");

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void Test_IsAssemblyLoaded_when_not_valid()
        {
            var result = AppDomain.CurrentDomain.IsAssemblyLoaded("ABC");

            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void Test_GetLoadedAssembly_when_valid()
        {
            var result = AppDomain.CurrentDomain.GetLoadedAssembly("Goldenacre.Test");

            Assert.IsNotNull(result);
        }
    }

    [TestClass]
    public class ImageExtensionsTest
    {
        [TestMethod]
        public void Test_Image_to_grayscale()
        {
            var img = Assembly.GetExecutingAssembly().GetEmbeddedResourceImage("tazmania.jpg");

            var newImg = img.Grayscale();

            var path = Path.GetTempFileName().Replace(".tmp", ".jpg");
            newImg.Save(path);
        }

        [TestMethod]
        public void Test_Image_compress()
        {
            using (var bigImg = Assembly.GetExecutingAssembly().GetEmbeddedResourceImage("tazmania.jpg"))
            using (var smallImg = bigImg.Compress(25))
            {
                using (MemoryStream ms1 = new MemoryStream())
                using (MemoryStream ms2 = new MemoryStream())
                {
                    bigImg.Save(ms1, System.Drawing.Imaging.ImageFormat.Tiff);
                    smallImg.Save(ms2, System.Drawing.Imaging.ImageFormat.Tiff);

                    // make sure the compressed one is actually smaller.
                    Assert.IsTrue(ms2.Length < ms1.Length);
                }
            }
        }

        [TestMethod]
        public void Test_Image_add_text_to_image()
        {
            var img = Assembly.GetExecutingAssembly().GetEmbeddedResourceImage("tazmania.jpg");

            var newImg = img.AddTextToImage("Hello World!");

            var path = Path.GetTempFileName().Replace(".tmp", ".jpg");
            newImg.Save(path);
        }

        [TestMethod]
        public void Test_Image_resize()
        {
            var img = Assembly.GetExecutingAssembly().GetEmbeddedResourceImage("tazmania.jpg");

            var newImg = img.Resize(new Size(200, 200));

            var path = Path.GetTempFileName().Replace(".tmp", ".jpg");
            newImg.Save(path);
        }
    }
}