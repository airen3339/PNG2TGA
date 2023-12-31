﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using TGASharpLib;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Path = System.IO.Path;
using Rectangle = System.Drawing.Rectangle;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace PNG2TGA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string OriginalPath
        {
            get => Original_Path.Text;
            set => Original_Path.Text = value;
        }

        public string ConvertPath
        {
            get => Convert_Path.Text;
            set => Convert_Path.Text = value;
        }

        public double ConvertProgressMax
        {
            get => Convert_Progress.Maximum;
            set => Convert_Progress.Maximum = value;
        }

        public MainWindow()
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                // <this-program> input output
                string input = args[1];
                string output = args.Length > 2 ? args[2] : input;
                Console.WriteLine("input = " + input);
                Console.WriteLine("output = " + output);
                // convert files ...
                string[] files = CheckDirectory(input) ?
                    Directory.GetFiles(input, "*.png", SearchOption.AllDirectories) : new string[] { input };
                foreach (string file in files)
                {
                    Console.Write("Converting " + file + "...");
                    Convert_One(file, output);
                    Console.WriteLine("done");
                }
                Environment.Exit(0);
            }
            else
            {
                InitializeComponent();
            }
        }

        private void Original_Reference_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "PNGファイル (*.png)|*.png"
            };

            if (dialog.ShowDialog() == true)
            {
                OriginalPath = dialog.FileName;
                if (ConvertPath.Length != 0) return;
                ConvertPath = CheckDirectory(dialog.FileName) ? dialog.FileName : Path.ChangeExtension(dialog.FileName, "tga");
            }
        }

        private void Convert_reference_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "TGAファイル (*.tga)|*.tga";
            if (dialog.ShowDialog() == true)
            {
                ConvertPath = dialog.FileName;
            }
        }

        private void Convert_One(string from, string to)
        {
            string openPath;
            string savePath;
            openPath = from;
            savePath = to.EndsWith("\\")
                ? to + Path.ChangeExtension(Path.GetFileName(from), ".tga")
                : Path.ChangeExtension(to, ".tga");

            Bitmap original = new Bitmap(openPath);
            Bitmap clone = new Bitmap(original);
            Bitmap newbmp = clone.Clone(new Rectangle(0, 0, clone.Width, clone.Height),
                PixelFormat.Format32bppArgb);
            TGA tga = (TGA)newbmp;
            tga.Save(savePath);
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            string[] files;
            files = CheckDirectory(OriginalPath) ?
                Directory.GetFiles(OriginalPath, "*.png", SearchOption.AllDirectories) : new string[] { OriginalPath };
            ConvertProgressMax = files.Length;
            double progress = Convert_Progress.Value = 0;
            foreach (string file in files)
            {
                Convert_One(file, ConvertPath);
                progress += 1;
                Convert_Progress.Value = progress;
            }

            MessageBox.Show("変換が完了しました。", "変換完了", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        private bool CheckDirectory(string path)
        {
            return (File.GetAttributes(path).HasFlag(FileAttributes.Directory));
        }

        private void Original_Path_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop);
        }

        private void Convert_Path_PreviewDragOver(object sender, System.Windows.DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = e.Data.GetDataPresent(DataFormats.FileDrop);
        }

        private void Original_Path_Drop(object sender, System.Windows.DragEventArgs e)
        {
            string[] drop = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (drop == null) return;
            OriginalPath = drop[0];
            if (CheckDirectory(OriginalPath))
                OriginalPath = OriginalPath + "\\";
            if (ConvertPath.Length != 0) return;
            ConvertPath = CheckDirectory(OriginalPath) ? OriginalPath : Path.ChangeExtension(drop[0], "tga");
        }


        private void Convert_Path_Drop(object sender, System.Windows.DragEventArgs e)
        {
            string[] drop = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (drop == null) return;
            ConvertPath = drop[0];
        }
    }
}
