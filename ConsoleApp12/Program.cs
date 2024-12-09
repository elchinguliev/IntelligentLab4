using Accord.Imaging.Filters;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        string imagePath = "inteligentlabpic.jpg";

        try
        {
            // Load and preprocess the image
            Image<Rgba32> binaryImage = LoadAndPreprocessImage(imagePath);

            // Segment the digits
            List<Rectangle> digitRegions = SegmentDigits(binaryImage);

            Console.WriteLine($"Found {digitRegions.Count} digit(s).");

            // Extract and visualize digits
            int count = 0;
            foreach (var region in digitRegions)
            {
                if (region.Width > 0 && region.Height > 0) // Ensure the rectangle is valid
                {
                    var digit = binaryImage.Clone(x => x.Crop(region));
                    string digitPath = Path.Combine(Directory.GetCurrentDirectory(), $"digit_{count}.png");
                    digit.Save(digitPath); // Save each digit as an image
                    Console.WriteLine($"Digit {count} saved at {digitPath}");
                    count++;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static Image<Rgba32> LoadAndPreprocessImage(string imagePath)
    {
        // Load the image
        Image<Rgba32> originalImage = Image.Load<Rgba32>(imagePath);

        // Convert to grayscale (luminosity method)
        originalImage.Mutate(ctx => ctx.Grayscale()); //boz renge ceviri
//        Mutate metodu, şəkilin üzərində dəyişikliklər etmək üçün istifadə olunur.
//ctx.Grayscale() metodu, şəkilin rənglərini boz(gri) tonlara çevirmək üçün istifadə edilir
//            .Bu zaman, rəngli şəkil tək bir boz dərəcəsi ilə təmsil olunur
//            və hər pikselin parıltı(luminosity) dəyəri hesablanır.

        // Convert to binary image (black and white)
        originalImage.Mutate(ctx => ctx.BinaryThreshold(0.5f)); //ikili ag qara renge ceviri
//        BinaryThreshold(0.5f) metodu, hər bir pikselin parıltı dəyərinə görə qərar verir:
//Əgər pikselin parıltısı 0.5-dən böyük və ya bərabərdirsə, o zaman piksel ağ(white) olur.
//Əgər pikselin parıltısı 0.5-dən kiçikdirsə, o zaman piksel qaradır(black).
        return originalImage;

    }



    static List<Rectangle> SegmentDigits(Image<Rgba32> binaryImage)
    {
        List<Rectangle> rectangles = new List<Rectangle>();

        // Scan rows and columns to find bounding boxes of connected components
        bool[,] visited = new bool[binaryImage.Width, binaryImage.Height];

        for (int y = 0; y < binaryImage.Height; y++)
        {
            for (int x = 0; x < binaryImage.Width; x++)
            {
                if (!visited[x, y] && binaryImage[x, y].R == 0) // Black pixel
                {
                    // Flood-fill to find the bounding box of the connected component
                    Rectangle rect = FindBoundingBox(binaryImage, x, y, visited);
                    rectangles.Add(rect);
                }
            }
        }

        return rectangles.OrderBy(r => r.X).ToList(); // Sort rectangles left to right
    }

    static Rectangle FindBoundingBox(Image<Rgba32> binaryImage, int startX, int startY, bool[,] visited)


        //Bu funksiya, verilmiş bir ikili şəkil(binary image) içərisindəki əlaqəli komponentin sərhədini 
        //tapmaq üçün Flood-fill(sulama dolumu) metodundan istifadə edir.Bu metod, bir başlanğıc 
        //nöqtəsindən(başlanğıc koordinatları startX və startY) başlayaraq əlaqəli olan bütün "qara" 
        //pikselləri(bu halda, R == 0 olan pikselləri) tapır və bu piksellərin ən sol, ən yuxarı, 
        //ən sağ və ən aşağı nöqtələrini təyin edərək bu komponentin sərhədini(bounding box) müəyyənləşdirir.
        //Aşağıda adım-adım nə baş verdiyi izah edilir:Əsas Məqsəd:
//Bu funksiya, şəkil içərisindəki "qara" obyektlərin(məsələn, rəqəmlər və ya digər qara komponentlər) sərhədlərini 
//        tapmaq üçün istifadə edilir.Bu metod, şəkli skan edərək hər bir əlaqəli komponenti(məsələn, bir rəqəmi)
//        tapır və onun sərhədini müəyyənləşdirir.Bu sərhəd daha sonra həmin komponenti(rəqəmi) kəsmək(crop)
//        və digər əməliyyatlar üçün istifadə edilə bilər.
    {
        int minX = startX, minY = startY, maxX = startX, maxY = startY;

        Queue<(int, int)> queue = new Queue<(int, int)>();
        queue.Enqueue((startX, startY));
        visited[startX, startY] = true;

        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();

            for (int i = 0; i < 4; i++)
            {
                int nx = x + dx[i], ny = y + dy[i];

                if (nx >= 0 && nx < binaryImage.Width && ny >= 0 && ny < binaryImage.Height &&
                    !visited[nx, ny] && binaryImage[nx, ny].R == 0)
                {
                    visited[nx, ny] = true;
                    queue.Enqueue((nx, ny));

                    // Update bounding box
                    minX = Math.Min(minX, nx);
                    minY = Math.Min(minY, ny);
                    maxX = Math.Max(maxX, nx);
                    maxY = Math.Max(maxY, ny);
                }
            }
        }

        return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }
}
