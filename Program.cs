using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using HtmlAgilityPack; // Install via NuGet

namespace DecodeSecretMessage
{
    // Represents a single cell in the grid
    public class GridCell
    {
        public int X { get; set; }
        public string Character { get; set; }
        public int Y { get; set; }

        public override string ToString()
        {
            return $"X: {X}, Char: '{Character}', Y: {Y}";
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            string url = "https://docs.google.com/document/d/e/2PACX-1vRPzbNQcx5UriHSbZ-9vmsTow_R6RRe7eyAU60xIF9Dlz-vaHiHNO2TKgDi7jy4ZpTpNqM7EvEcfr_p/pub";
            string htmlContent = await ReadDocumentAsync(url);

            var gridCells = ParseGridCells(htmlContent);

            Console.WriteLine("Grid Cells:");
            if (gridCells.Count == 0)
            {
                Console.WriteLine("No table or data found in the document.");
            }
            else
            {
                foreach (var cell in gridCells)
                {
                    Console.WriteLine(cell);
                }
            }

            // Decode and display the secret message
            string secretMessage = DecodeMessage(gridCells);
            //Console.WriteLine($"\nSecret Message: {secretMessage}");
            Console.WriteLine($"\nSecret Message:");

            // Print the grid of characters with Unicode values and coordinates
            PrintCharacterGrid(gridCells);
        }

        static async Task<string> ReadDocumentAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetStringAsync(url);
            }
        }

        // Parses the first table and returns a list of GridCell objects
        static List<GridCell> ParseGridCells(string html)
        {
            var cellsList = new List<GridCell>();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var table = doc.DocumentNode.SelectSingleNode("//table");
            if (table == null)
                return cellsList;

            var rows = table.SelectNodes(".//tr");
            if (rows == null)
                return cellsList;

            bool isHeader = true;
            foreach (var row in rows)
            {
                var cells = row.SelectNodes(".//th|.//td");
                if (cells == null || cells.Count < 3)
                    continue;

                // Skip header row
                if (isHeader)
                {
                    isHeader = false;
                    continue;
                }

                // Parse values
                if (int.TryParse(cells[0].InnerText.Trim(), out int x) &&
                    int.TryParse(cells[2].InnerText.Trim(), out int y))
                {
                    var character = cells[1].InnerText.Trim();
                    cellsList.Add(new GridCell { X = x, Character = character, Y = y });
                }
            }
            return cellsList;
        }

        // Decodes the secret message by sorting gridCells by X, then Y, and concatenating their characters
        static string DecodeMessage(List<GridCell> gridCells)
        {
            // Sort by X, then Y (left-to-right, top-to-bottom)
            gridCells.Sort((a, b) =>
            {
                int xCompare = a.X.CompareTo(b.X);
                return xCompare != 0 ? xCompare : a.Y.CompareTo(b.Y);
            });

            var message = new System.Text.StringBuilder();
            foreach (var cell in gridCells)
            {
                message.Append(cell.Character);
            }
            return message.ToString();
        }

        // Prints the grid of Unicode characters only, in grid layout
        static void PrintCharacterGrid(List<GridCell> gridCells)
        {
            if (gridCells == null || gridCells.Count == 0)
            {
                Console.WriteLine("No grid data to display.");
                return;
            }

            // Find grid bounds
            int maxX = int.MinValue, maxY = int.MinValue;
            int minX = int.MaxValue, minY = int.MaxValue;
            foreach (var cell in gridCells)
            {
                if (cell.X > maxX) maxX = cell.X;
                if (cell.Y > maxY) maxY = cell.Y;
                if (cell.X < minX) minX = cell.X;
                if (cell.Y < minY) minY = cell.Y;
            }

            // Build a lookup for fast access
            var gridLookup = new Dictionary<(int x, int y), GridCell>();
            foreach (var cell in gridCells)
            {
                gridLookup[(cell.X, cell.Y)] = cell;
            }

            Console.WriteLine("\nGrid of Unicode Characters:");
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (gridLookup.TryGetValue((x, y), out var cell))
                    {
                        Console.Write(cell.Character);
                    }
                    else
                    {
                        Console.Write(" "); // Print space for empty cell
                    }
                }
                Console.WriteLine();
            }
        }

        // Prints the grid of characters, displaying Unicode values and coordinates
        //static void PrintCharacterGrid(List<GridCell> gridCells)
        //{
        //    if (gridCells == null || gridCells.Count == 0)
        //    {
        //        Console.WriteLine("No grid data to display.");
        //        return;
        //    }

        //    // Find grid bounds
        //    int maxX = int.MinValue, maxY = int.MinValue;
        //    int minX = int.MaxValue, minY = int.MaxValue;
        //    foreach (var cell in gridCells)
        //    {
        //        if (cell.X > maxX) maxX = cell.X;
        //        if (cell.Y > maxY) maxY = cell.Y;
        //        if (cell.X < minX) minX = cell.X;
        //        if (cell.Y < minY) minY = cell.Y;
        //    }

        //    // Build a lookup for fast access
        //    var gridLookup = new Dictionary<(int x, int y), GridCell>();
        //    foreach (var cell in gridCells)
        //    {
        //        gridLookup[(cell.X, cell.Y)] = cell;
        //    }

        //    Console.WriteLine("\nGrid of Characters (Unicode and Coordinates):");
        //    for (int y = minY; y <= maxY; y++)
        //    {
        //        for (int x = minX; x <= maxX; x++)
        //        {
        //            if (gridLookup.TryGetValue((x, y), out var cell))
        //            {
        //                int unicode = cell.Character.Length > 0 ? cell.Character[0] : 0;
        //                Console.Write($"[{x},{y}] '{cell.Character}' (U+{unicode:X4})\t");
        //            }
        //            else
        //            {
        //                Console.Write($"[{x},{y}] (empty)\t");
        //            }
        //        }
        //        Console.WriteLine();
        //    }
        //}
    }
}